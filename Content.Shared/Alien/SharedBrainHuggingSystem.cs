using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Serialization;
using Content.Server.Alien;
using Content.Shared.Stunnable;

namespace Content.Shared.Alien;

public abstract class SharedBrainHuggingSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrainHuggingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BrainHuggingComponent, BrainSlugActionEvent>(OnBrainSlugAction);
        SubscribeLocalEvent<BrainHuggingComponent, DominateVictimActionEvent>(OnDominateVictimAction);
        SubscribeLocalEvent<BrainHuggingComponent, ReleaseSlugActionEvent>(OnReleaseSlugAction);
    }

    protected void OnStartup(EntityUid uid, BrainHuggingComponent component, ComponentStartup args)
    {
        if (component.BrainSlugAction != null)
        {
            _actionsSystem.AddAction(uid, component.BrainSlugAction, null);
        }
    }

    protected void OnBrainSlugAction(EntityUid uid, BrainHuggingComponent component, BrainSlugActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var target = args.Target;

        if (TryComp(target, out MobStateComponent? targetState))
        {

            switch (targetState.CurrentState)
            {
                case MobState.Alive:
                case MobState.Critical:
                    _popupSystem.PopupEntity(Loc.GetString("Slug is sucking on your brain!"), uid, uid);
                    _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, component.BrainSlugTime, new BrainHuggingDoAfterEvent(), uid, target: target, used: uid)
                    {
                        BreakOnTargetMove = false,
                        BreakOnUserMove = true,
                    });
                    break;
                default:
                    _popupSystem.PopupEntity(Loc.GetString("The target is dead!"), uid, uid);
                    break;
            }

            return;
        }
    }

    protected void OnReleaseSlugAction(EntityUid uid, BrainHuggingComponent comp, ReleaseSlugActionEvent args)
    {
        var target = args.Target;
        if (TryComp(target, out MobStateComponent? targetState))
        {
            switch (targetState.CurrentState)
            {
                case MobState.Alive:
                case MobState.Critical:
                    _popupSystem.PopupEntity(Loc.GetString("Slug is trying to get out of your head!"), uid, uid);
                    _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, comp.BrainRealeseTime, new ReleaseSlugDoAfterEvent(), uid, target: target, used: uid)
                    {
                        BreakOnTargetMove = false,
                        BreakOnUserMove = true,
                    });
                    break;

                default:
                    break;
            }
        }
    }

    protected void OnDominateVictimAction(EntityUid uid, BrainHuggingComponent comp, DominateVictimActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var target = args.Target;

        _popupSystem.PopupEntity(Loc.GetString("Your limbs are stiff!"), uid, uid);
        _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(comp.ParalyzeTime), true);
    }
}

public sealed class BrainSlugActionEvent : EntityTargetActionEvent { }

public sealed class DominateVictimActionEvent : EntityTargetActionEvent { }

public sealed class ReleaseSlugActionEvent : EntityTargetActionEvent { }

[Serializable, NetSerializable]
public sealed class ReleaseSlugDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed class BrainHuggingDoAfterEvent : SimpleDoAfterEvent { }

//public sealed class BrainInfestDoAfterEvent : SimpleDoAfterEvent { }
