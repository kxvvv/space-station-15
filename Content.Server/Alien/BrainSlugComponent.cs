using Content.Shared.Actions.ActionTypes;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Alien;


[RegisterComponent]
public sealed class BrainSlugComponent : Component
{
    [DataField("actionBrainSlugJump", required: true)]
    public WorldTargetAction ActionBrainSlugJump = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("brainSlugHuggingAction")]
    public string BrainSlugHuggingAction = "BrainSlugHuggingAction";

    [DataField("paralyzeTime"), ViewVariables(VVAccess.ReadWrite)]
    public float ParalyzeTime = 3f;

    [DataField("chansePounce"), ViewVariables(VVAccess.ReadWrite)]
    public static int ChansePounce = 33;

    [DataField("damage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = default!;

    public bool IsDeath = false;

    public EntityUid EquipedOn;

    [ViewVariables] public float Accumulator = 0;

    [DataField("damageFrequency"), ViewVariables(VVAccess.ReadWrite)]
    public float DamageFrequency = 5;

    [ViewVariables(VVAccess.ReadWrite), DataField("soundBrainSlugJump")]
    public SoundSpecifier? SoundBrainSlugJump = new SoundPathSpecifier("/Audio/Animals/brainslug_scream.ogg");

}
