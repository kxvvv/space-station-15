using Content.Shared.Actions.ActionTypes;
using Content.Shared.Alien;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Damage;
using Content.Shared.Chemistry.Reagent;

namespace Content.Server.Alien;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedFaceHuggingSystem))]
public sealed class FaceHuggingComponent : Component
{
    [DataField("facehuggingActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
    public string FaceHuggingActionId = "FaceHugging";

    [DataField("facehuggingAction")]
    public EntityTargetAction? FaceHuggingAction;

    [ViewVariables(VVAccess.ReadWrite), DataField("soundFaceHugging")]
    public SoundSpecifier? SoundFaceHugging = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };

    [DataField("facehuggingTime")]
    public float FaceHuggingTime = 3f;

    [DataField("damage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = default!;

    [DataField("ichorChemical", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string IchorChemical = "Ichor";

    [ViewVariables(VVAccess.ReadWrite), DataField("healRate")]
    public float HealRate = 15f;
}
