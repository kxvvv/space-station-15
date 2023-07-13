using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Serialization;
using Content.Server.Alien;

namespace Content.Shared.Alien;

public abstract class SharedFaceHuggingSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FaceHuggingComponent, FaceHuggingActionEvent>(OnFaceHuggingAction);
    }

    protected void OnStartup(EntityUid uid, FaceHuggingComponent component, ComponentStartup args)
    {
        if (component.FaceHuggingAction != null)
            _actionsSystem.AddAction(uid, component.FaceHuggingAction, null);
    }

    protected void OnFaceHuggingAction(EntityUid uid, FaceHuggingComponent component, FaceHuggingActionEvent args)
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
                    _doAfterSystem.TryStartDoAfter(new DoAfterArgs(uid, component.FaceHuggingTime, new FaceHuggingDoAfterEvent(), uid, target: target, used: uid)
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

public sealed class FaceHuggingActionEvent : EntityTargetActionEvent { }

[Serializable, NetSerializable]
public sealed class FaceHuggingDoAfterEvent : SimpleDoAfterEvent { }
