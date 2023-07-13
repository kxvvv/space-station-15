using Content.Shared.Actions.ActionTypes;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Alien;

[Access(typeof(FaceHuggerSystem))]
[RegisterComponent]
public sealed class FaceHuggerComponent : Component
{
    [DataField("actionFaceHuggerJump", required: true)]
    public WorldTargetAction ActionFaceHuggerJump = new();

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

    [ViewVariables(VVAccess.ReadWrite), DataField("soundFaceHuggerJump")]
    public SoundSpecifier? SoundFaceHuggerJump = new SoundPathSpecifier("/Audio/Animals/facehugger_scream.ogg");

}
