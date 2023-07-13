using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Alien;
using Content.Server.Mind;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Content.Shared.CombatMode;

namespace Content.Server.Alien;

public sealed class BrainHuggingSystem : SharedBrainHuggingSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrainHuggingComponent, BrainHuggingDoAfterEvent>(BrainHuggingOnDoAfter);
        SubscribeLocalEvent<BrainHuggingComponent, ReleaseSlugDoAfterEvent>(ReleaseSlugOnDoAfter);
    }

    private void BrainHuggingOnDoAfter(EntityUid uid, BrainHuggingComponent component, BrainHuggingDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        else if (args.Args.Target != null)
        {
            var target = args.Target;
            if (target == null)
            {
                return;
            }

            if (component.DominateVictimAction != null)
            {
                _actionsSystem.AddAction(uid, component.DominateVictimAction, null);
            }
            if (component.ReleaseSlugAction != null)
            {
                _actionsSystem.AddAction(uid, component.ReleaseSlugAction, null);
            }


            //var target = args.Target;
            //_mindSystem.TryGetMind(uid, out var mind);

            //if (mind == null || target == null)
            //    return;

            //_popup.PopupEntity(Loc.GetString("Facehugger exhausts itself and settles in your body!"), uid, uid, PopupType.LargeCaution);

            //_mindSystem.TransferTo(mind, target);
            //_inventory.TryUnequip(target.Value, "mask", true, true);
            //_damageableSystem.TryChangeDamage(uid, component.Damage);

            if (TryComp(target, out MobStateComponent? mobState))
            {
                if (mobState.CurrentState == MobState.Critical)
                {
                    _popup.PopupEntity(Loc.GetString("Brain Slug is trying save your body!"), target.Value, target.Value);
                    var ichorInjection = new Solution(component.IchorChemical, component.HealRate);
                    ichorInjection.ScaleSolution(5.0f);
                    _bloodstreamSystem.TryAddToChemicals(target.Value, ichorInjection);
                }
            }
        }



        _audioSystem.PlayPvs(component.SoundBrainHugging, uid);
    }


    private void ReleaseSlugOnDoAfter(EntityUid uid, BrainHuggingComponent component, ReleaseSlugDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
        {
            return;
        }

        else if (args.Args.Target != null)
        {
            var target = args.Target;

            if (target == null)
            {
                return;
            }

            //EntityManager.AddComponent<CombatModeComponent>(uid);


            _popup.PopupEntity(Loc.GetString("A slug jumped out of our ear!"), target.Value, target.Value);
            _inventory.TryUnequip(target.Value, "mask", true, true);



            if (component.ReleaseSlugAction != null)
            {
                _actionsSystem.RemoveAction(uid, component.ReleaseSlugAction);
            }
            if (component.DominateVictimAction != null)
            {
                _actionsSystem.RemoveAction(uid, component.DominateVictimAction);
            }

        }
    }
}

