using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Serialization;
using Content.Server.Alien;

namespace Content.Shared.Alien;

public abstract class SharedBrainHuggingSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrainHuggingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BrainHuggingComponent, BrainSlugActionEvent>(OnBrainSlugAction);
    }

    protected void OnStartup(EntityUid uid, BrainHuggingComponent component, ComponentStartup args)
    {
        if (component.BrainSlugAction != null)
            _actionsSystem.AddAction(uid, component.BrainSlugAction, null);
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
                    _popupSystem.PopupEntity(Loc.GetString("Facehugger is starting to devour your brain!"), uid, uid);
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
}

public sealed class BrainSlugActionEvent : EntityTargetActionEvent { }

[Serializable, NetSerializable]
public sealed class BrainHuggingDoAfterEvent : SimpleDoAfterEvent { }
