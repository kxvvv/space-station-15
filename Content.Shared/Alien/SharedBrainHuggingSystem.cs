using Content.Shared.DoAfter;
using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.Alien;

public abstract class SharedBrainHuggingSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;
}

public sealed class BrainSlugJumpActionEvent : WorldTargetActionEvent { }

public sealed class BrainSlugActionEvent : EntityTargetActionEvent { }

public sealed class DominateVictimActionEvent : EntityTargetActionEvent { }

public sealed class ReleaseSlugActionEvent : EntityTargetActionEvent { }

public sealed class TormentHostActionEvent : EntityTargetActionEvent { }


[Serializable, NetSerializable]
public sealed class ReleaseSlugDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed class ReleaseDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed class BrainHuggingDoAfterEvent : SimpleDoAfterEvent { }
