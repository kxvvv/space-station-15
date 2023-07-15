using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Utility;
using Content.Shared.Alien;
using Content.Shared.DoAfter;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Server.Chat.Systems;
using Content.Server.Speech.Components;
using Content.Server.Mind;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;

namespace Content.Server.Alien
{
    public sealed class BrainSlugSystem : SharedBrainHuggingSystem
    {
        [Dependency] private SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly MindSystem _mind = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<BrainHuggingComponent, ComponentStartup>(OnStartup);


            SubscribeLocalEvent<BrainHuggingComponent, BrainSlugJumpActionEvent>(OnJumpBrainSlug);
            SubscribeLocalEvent<BrainHuggingComponent, ThrowDoHitEvent>(OnBrainSlugDoHit);

            SubscribeLocalEvent<BrainHuggingComponent, BrainSlugActionEvent>(OnBrainSlugAction);
            SubscribeLocalEvent<BrainHuggingComponent, BrainHuggingDoAfterEvent>(BrainHuggingOnDoAfter);

            SubscribeLocalEvent<BrainHuggingComponent, DominateVictimActionEvent>(OnDominateVictimAction);

            SubscribeLocalEvent<BrainHuggingComponent, TormentHostActionEvent>(OnTormentHostAction);

            SubscribeLocalEvent<BrainHuggingComponent, AssumeControlActionEvent>(OnAssumeControlAction);
            SubscribeLocalEvent<BrainHuggingComponent, AssumeControlDoAfterEvent>(AssumeControlDoAfter);

            SubscribeLocalEvent<SlugInsideComponent, ReleaseControlActionEvent>(OnReleaseControlAction);

            SubscribeLocalEvent<BrainHuggingComponent, ReleaseSlugActionEvent>(OnReleaseSlugAction);
            SubscribeLocalEvent<BrainSlugComponent, ReleaseSlugDoAfterEvent>(ReleaseSlugDoAfter);
        }


        protected void OnStartup(EntityUid uid, BrainHuggingComponent component, ComponentStartup args)
        {
            if (component.ActionBrainSlugJump != null)
                _actionsSystem.AddAction(uid, component.ActionBrainSlugJump, null);

            if (component.BrainSlugAction != null)
                _actionsSystem.AddAction(uid, component.BrainSlugAction, null);
        }


        private void OnBrainSlugDoHit(EntityUid uid, BrainHuggingComponent component, ThrowDoHitEvent args)
        {

            if (TryComp(args.Target, out SlugInsideComponent? sluginside))
            {
                return;
            }

            _entityManager.AddComponent<SlugInsideComponent>(args.Target);

            TryComp(uid, out BrainSlugComponent? defcomp);
            if (defcomp == null)
            {
                return;
            }


            if (!HasComp<HumanoidAppearanceComponent>(args.Target))
                return;




            var host = args.Target;



            defcomp.GuardianContainer = host.EnsureContainer<ContainerSlot>("GuardianContainer");


            defcomp.GuardianContainer.Insert(uid);
            DebugTools.Assert(defcomp.GuardianContainer.Contains(uid));



            defcomp.EquipedOn = args.Target;

            _popup.PopupEntity(Loc.GetString("Something jumped on you!"),
                args.Target, args.Target, PopupType.LargeCaution);
        }



        private void OnJumpBrainSlug(EntityUid uid, BrainHuggingComponent component, BrainSlugJumpActionEvent args)
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
            if (component.SoundBrainSlugJump != null)
            {
                _audioSystem.PlayPvs(component.SoundBrainSlugJump, uid, component.SoundBrainSlugJump.Params);
            }
        }


        private void OnBrainSlugAction(EntityUid uid, BrainHuggingComponent component, BrainSlugActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            if (hugcomp == null)
            {
                return;
            }

            if (TryComp(target, out MobStateComponent? targetState))
            {

                switch (targetState.CurrentState)
                {
                    case MobState.Alive:
                    case MobState.Critical:
                        _popup.PopupEntity(Loc.GetString("Slug is sucking on your brain!"), uid, uid);
                        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, hugcomp.BrainSlugTime, new BrainHuggingDoAfterEvent(), uid, target: target, used: uid)
                        {
                            BreakOnTargetMove = false,
                            BreakOnUserMove = true,
                        });
                        break;
                    default:
                        _popup.PopupEntity(Loc.GetString("The target is dead!"), uid, uid);
                        break;
                }

                return;
            }
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
                    _actionsSystem.AddAction(uid, component.DominateVictimAction, null);

                if (component.ReleaseSlugAction != null)
                    _actionsSystem.AddAction(uid, component.ReleaseSlugAction, null);

                if (component.TormentHostSlugAction != null)
                    _actionsSystem.AddAction(uid, component.TormentHostSlugAction, null);

                if (component.AssumeControlAction != null)
                    _actionsSystem.AddAction(uid, component.AssumeControlAction, null);

                if (component.ActionBrainSlugJump != null)
                    _actionsSystem.RemoveAction(uid, component.ActionBrainSlugJump, null);

                if (component.BrainSlugAction != null)
                    _actionsSystem.RemoveAction(uid, component.BrainSlugAction, null);


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

        private void OnDominateVictimAction(EntityUid uid, BrainHuggingComponent comp, DominateVictimActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            if (hugcomp == null)
            {
                return;
            }


            _popup.PopupEntity(Loc.GetString("Your limbs are stiff!"), uid, uid);
            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(hugcomp.ParalyzeTime), true);
        }


        private void OnTormentHostAction(EntityUid uid, BrainHuggingComponent comp, TormentHostActionEvent args)
        {
            var target = args.Target;
            if (TryComp(target, out VocalComponent? scream))
            {
                if (scream != null)
                {
                    _popup.PopupEntity(Loc.GetString("YOU FEEL HELLISH PAIN, YOU WILL BE TURNED INSIDE OUT AND ROLLED ON THE FLOOR!"), target, target, PopupType.LargeCaution);
                    _chat.TryEmoteWithChat(target, scream.ScreamId);
                }
            }
        }

        private void OnAssumeControlAction(EntityUid uid, BrainHuggingComponent component, AssumeControlActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            var target = args.Target;

            TryComp(uid, out BrainHuggingComponent? hugcomp);
            if (hugcomp == null)
            {
                return;
            }

            _popup.PopupEntity(Loc.GetString("You feel like a slug inside your head wants to take over your nervous system!"), target, target, PopupType.LargeCaution);
            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, hugcomp.AssumeControlTime, new AssumeControlDoAfterEvent(), uid, target: target, used: uid)
            {
                BreakOnTargetMove = false,
                BreakOnUserMove = true,
            });

        }


        private void AssumeControlDoAfter(EntityUid uid, BrainHuggingComponent comp, AssumeControlDoAfterEvent args)
        {
            var target = args.Target;
            if (target == null)
            {
                return;
            }

            if (TryComp(target, out SlugInsideComponent? targetcomp))
            {
                targetcomp.Slug = uid;
                if (TryComp(target, out SlugInsideComponent? ttt))
                {
                    if (_entityManager.TryGetComponent<ActorComponent?>(target, out var actorComponent))
                    {
                        var userid = actorComponent.PlayerSession.UserId;
                        targetcomp.NetParent = userid;
                    }
                }
                targetcomp.Parent = target.Value;
            }

            _mind.TryGetMind(uid, out var mind);

            if (mind != null)
                _mind.TransferTo(mind, args.Target);


            if (targetcomp != null)
                if (targetcomp.ReleaseControlName != null)
                {
                    var theAction = new InstantAction(_proto.Index<InstantActionPrototype>(targetcomp.ReleaseControlName));
                    _actionsSystem.AddAction(target.Value, theAction, null);
                }
        }

        private void OnReleaseControlAction(EntityUid uid, SlugInsideComponent comp, ReleaseControlActionEvent args)
        {

            if (TryComp(uid, out SlugInsideComponent? slug))
            {
                _mind.TryGetMind(uid, out var slugmind);
                if (slugmind != null)
                    _mind.TransferTo(slugmind, slug.Slug);
            }
            if (TryComp(uid, out SlugInsideComponent? parrent))
            {
                _mind.TryGetMind(parrent.NetParent, out var parrentmind);
                if (parrentmind != null)
                    _mind.TransferTo(parrentmind, parrent.Parent);
            }
            if (comp.ReleaseControlName != null)
            {
                var theAction = new InstantAction(_proto.Index<InstantActionPrototype>(comp.ReleaseControlName));
                _actionsSystem.RemoveAction(uid, theAction, null);
            }

            _popup.PopupEntity(Loc.GetString("The slug got out of your nervous system."), uid, uid);

        }

        private void OnReleaseSlugAction(EntityUid uid, BrainHuggingComponent comp, ReleaseSlugActionEvent args)
        {
            TryComp(uid, out BrainSlugComponent? defcomp);
            if (defcomp == null)
            {
                return;
            }

            var target = defcomp.EquipedOn;



            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, comp.BrainSlugTime, new ReleaseSlugDoAfterEvent(), uid, target: target, used: uid)
            {
                BreakOnTargetMove = false,
                BreakOnUserMove = true,
            });
        }


        private void ReleaseSlugDoAfter(EntityUid uid, BrainSlugComponent component, ReleaseSlugDoAfterEvent args)
        {
            TryComp(uid, out BrainHuggingComponent? hugcomp);
            if (hugcomp == null)
            {
                return;
            }


            component.GuardianContainer.Remove(uid);
            DebugTools.Assert(!component.GuardianContainer.Contains(uid));


            if (TryComp(args.Target, out SlugInsideComponent? slugcomp))
            {
                var target = args.Target;
                if (target != null && slugcomp != null)
                    _entityManager.RemoveComponent<SlugInsideComponent>(target.Value);
            }


            if (hugcomp.ReleaseSlugAction != null)
                _actionsSystem.RemoveAction(uid, hugcomp.ReleaseSlugAction);

            if (hugcomp.DominateVictimAction != null)
                _actionsSystem.RemoveAction(uid, hugcomp.DominateVictimAction);

            if (hugcomp.TormentHostSlugAction != null)
                _actionsSystem.RemoveAction(uid, hugcomp.TormentHostSlugAction, null);

            if (hugcomp.AssumeControlAction != null)
                _actionsSystem.RemoveAction(uid, hugcomp.AssumeControlAction, null);

            if (hugcomp.ActionBrainSlugJump != null)
                _actionsSystem.AddAction(uid, hugcomp.ActionBrainSlugJump, null);

            if (hugcomp.BrainSlugAction != null)
                _actionsSystem.AddAction(uid, hugcomp.BrainSlugAction, null);
        }


        public override void Update(float frameTime)
        {
            base.Update(frameTime);



            foreach (var comp in EntityQuery<BrainSlugComponent>())
            {
                comp.Accumulator += frameTime;

                if (comp.Accumulator <= comp.DamageFrequency)
                    continue;

                comp.Accumulator = 0;

                if (comp.EquipedOn is not { Valid: true } targetId)
                    continue;
                if (TryComp(targetId, out MobStateComponent? mobState))
                {
                    if (mobState.CurrentState is MobState.Dead)
                    {
                        return;
                    }
                }

                _popup.PopupEntity(Loc.GetString("You feel as if something is stirring inside you."), targetId, targetId);
            }
        }

    }
}
