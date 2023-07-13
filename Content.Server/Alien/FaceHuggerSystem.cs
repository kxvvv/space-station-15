using System.Linq;
using Content.Server.Actions;
using Content.Server.NPC.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;

namespace Content.Server.Alien
{
    public sealed class FaceHuggerSystem : EntitySystem
    {
        [Dependency] private SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
        [Dependency] private readonly SharedCombatModeSystem _combat = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly ActionsSystem _action = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<FaceHuggerComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<FaceHuggerComponent, MeleeHitEvent>(OnMeleeHit);
            SubscribeLocalEvent<FaceHuggerComponent, ThrowDoHitEvent>(OnFaceHuggerDoHit);
            SubscribeLocalEvent<FaceHuggerComponent, GotEquippedEvent>(OnGotEquipped);
            SubscribeLocalEvent<FaceHuggerComponent, GotUnequippedEvent>(OnGotUnequipped);
            SubscribeLocalEvent<FaceHuggerComponent, GotEquippedHandEvent>(OnGotEquippedHand);
            SubscribeLocalEvent<FaceHuggerComponent, MobStateChangedEvent>(OnMobStateChanged);
            SubscribeLocalEvent<FaceHuggerComponent, BeingUnequippedAttemptEvent>(OnUnequipAttempt);
            SubscribeLocalEvent<FaceHuggerComponent, FaceHuggerJumpActionEvent>(OnJumpFaceHugger);
        }

        private void OnStartup(EntityUid uid, FaceHuggerComponent component, ComponentStartup args)
        {
            _action.AddAction(uid, component.ActionFaceHuggerJump, null);
        }

        private void OnFaceHuggerDoHit(EntityUid uid, FaceHuggerComponent component, ThrowDoHitEvent args)
        {
            if (component.IsDeath)
                return;
            //if (HasComp<FleshCultistComponent>(args.Target))
            //    return;
            if (!HasComp<HumanoidAppearanceComponent>(args.Target))
                return;
            if (TryComp(args.Target, out MobStateComponent? mobState))
            {
                if (mobState.CurrentState is not MobState.Alive)
                {
                    return;
                }
            }
            _inventory.TryGetSlotEntity(args.Target, "head", out var headItem);
            if (HasComp<IngestionBlockerComponent>(headItem))
                return;

            var equipped = _inventory.TryEquip(args.Target, uid, "mask", true);
            if (!equipped)
                return;

            component.EquipedOn = args.Target;

            _popup.PopupEntity(Loc.GetString("FaceHugger 1"),
                args.Target, args.Target, PopupType.LargeCaution);

            _popup.PopupEntity(Loc.GetString("FaceHugger 2",
                    ("entity", args.Target)),
                uid, uid, PopupType.LargeCaution);

            _popup.PopupEntity(Loc.GetString("FaceHugger 3",
                ("entity", args.Target)), args.Target, Filter.PvsExcept(uid), true, PopupType.Large);

            EntityManager.RemoveComponent<CombatModeComponent>(uid);
            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(component.ParalyzeTime), true);
            _damageableSystem.TryChangeDamage(args.Target, component.Damage, origin: args.User);
        }

        private void OnGotEquipped(EntityUid uid, FaceHuggerComponent component, GotEquippedEvent args)
        {
            if (args.Slot != "mask")
                return;
            component.EquipedOn = args.Equipee;
            EntityManager.RemoveComponent<CombatModeComponent>(uid);
        }

        private void OnUnequipAttempt(EntityUid uid, FaceHuggerComponent component, BeingUnequippedAttemptEvent args)
        {
            if (args.Slot != "mask")
                return;
            if (component.EquipedOn != args.Unequipee)
                return;
            //if (HasComp<FleshCultistComponent>(args.Unequipee))
            //    return;
            _popup.PopupEntity(Loc.GetString("FaceHugger 4"),
                args.Unequipee, args.Unequipee, PopupType.Large);
            args.Cancel();
        }

        private void OnGotEquippedHand(EntityUid uid, FaceHuggerComponent component, GotEquippedHandEvent args)
        {
            //if (HasComp<FleshPudgeComponent>(args.User))
            //    return;
            //if (HasComp<FleshCultistComponent>(args.User))
            //    return;
            if (component.IsDeath)
                return;
            // _handsSystem.TryDrop(args.User, uid, checkActionBlocker: false);
            _damageableSystem.TryChangeDamage(args.User, component.Damage);
            _popup.PopupEntity(Loc.GetString("FaceHugger 5"),
                args.User, args.User);
        }

        private void OnGotUnequipped(EntityUid uid, FaceHuggerComponent component, GotUnequippedEvent args)
        {
            if (args.Slot != "mask")
                return;
            component.EquipedOn = new EntityUid();
            var combatMode = EntityManager.AddComponent<CombatModeComponent>(uid);
            _combat.SetInCombatMode(uid, true, combatMode);
            EntityManager.AddComponent<NPCMeleeCombatComponent>(uid);
        }

        private void OnMeleeHit(EntityUid uid, FaceHuggerComponent component, MeleeHitEvent args)
        {
            if (!args.HitEntities.Any())
                return;

            foreach (var entity in args.HitEntities)
            {
                if (!HasComp<HumanoidAppearanceComponent>(entity))
                    return;

                if (TryComp(entity, out MobStateComponent? mobState))
                {
                    if (mobState.CurrentState is not MobState.Alive)
                    {
                        return;
                    }
                }

                _inventory.TryGetSlotEntity(entity, "head", out var headItem);
                if (HasComp<IngestionBlockerComponent>(headItem))
                    return;

                var random = new Random();
                var shouldEquip = random.Next(1, 101) <= FaceHuggerComponent.ChansePounce;
                if (!shouldEquip)
                    return;

                var equipped = _inventory.TryEquip(entity, uid, "mask", true);
                if (!equipped)
                    return;

                component.EquipedOn = entity;

                _popup.PopupEntity(Loc.GetString("FaceHugger 6"),
                    entity, entity, PopupType.LargeCaution);

                _popup.PopupEntity(Loc.GetString("FaceHugger 7", ("entity", entity)),
                    uid, uid, PopupType.LargeCaution);

                _popup.PopupEntity(Loc.GetString("FaceHugger 8",
                    ("entity", entity)), entity, Filter.PvsExcept(entity), true, PopupType.Large);
                EntityManager.RemoveComponent<CombatModeComponent>(uid);
                _stunSystem.TryParalyze(entity, TimeSpan.FromSeconds(component.ParalyzeTime), true);
                _damageableSystem.TryChangeDamage(entity, component.Damage, origin: entity);
                break;
            }
        }

        private static void OnMobStateChanged(EntityUid uid, FaceHuggerComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState == MobState.Dead)
            {
                component.IsDeath = true;
            }
        }

        public sealed class FaceHuggerJumpActionEvent : WorldTargetActionEvent
        {

        };

        private void OnJumpFaceHugger(EntityUid uid, FaceHuggerComponent component, FaceHuggerJumpActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var xform = Transform(uid);
            var mapCoords = args.Target.ToMap(EntityManager);
            Logger.Info(xform.MapPosition.ToString());
            Logger.Info(mapCoords.ToString());
            var direction = mapCoords.Position - xform.MapPosition.Position;
            Logger.Info(direction.ToString());

            _throwing.TryThrow(uid, direction, 7F, uid, 10F);
            if (component.SoundFaceHuggerJump != null)
            {
                _audioSystem.PlayPvs(component.SoundFaceHuggerJump, uid, component.SoundFaceHuggerJump.Params);
            }
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var comp in EntityQuery<FaceHuggerComponent>())
            {
                comp.Accumulator += frameTime;

                if (comp.Accumulator <= comp.DamageFrequency)
                    continue;

                comp.Accumulator = 0;

                if (comp.EquipedOn is not { Valid: true } targetId)
                    continue;
                //if (HasComp<FleshCultistComponent>(comp.EquipedOn))
                //    return;
                if (TryComp(targetId, out MobStateComponent? mobState))
                {
                    if (mobState.CurrentState is not MobState.Alive)
                    {
                        _inventory.TryUnequip(targetId, "mask", true, true);
                        comp.EquipedOn = new EntityUid();
                        return;
                    }
                }
                _damageableSystem.TryChangeDamage(targetId, comp.Damage);
                _popup.PopupEntity(Loc.GetString("FaceHugger 10"),
                    targetId, targetId, PopupType.LargeCaution);
                _popup.PopupEntity(Loc.GetString("FaceHugger 11",
                    ("entity", targetId)), targetId, Filter.PvsExcept(targetId), true);
            }
        }
    }
}
