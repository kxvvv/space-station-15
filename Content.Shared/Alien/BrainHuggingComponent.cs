using Content.Shared.Actions.ActionTypes;
using Content.Shared.Alien;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Damage;
using Content.Shared.Chemistry.Reagent;

namespace Content.Server.Alien;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedBrainHuggingSystem))]
public sealed class BrainHuggingComponent : Component
{
    [DataField("brainSlugActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
    public string BrainSlugActionId = "BrainSlug";

    [DataField("dominateVictimActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
    public string DominateVictimActionId = "DominateVictim";

    [DataField("releaseSlugActionId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityTargetActionPrototype>))]
    public string ReleaseSlugActionId = "ReleaseSlug";


    [DataField("brainSlugAction")]
    public EntityTargetAction? BrainSlugAction;

    [DataField("dominateVictimAction")]
    public EntityTargetAction? DominateVictimAction;

    [DataField("releaseSlugAction")]
    public EntityTargetAction? ReleaseSlugAction;



    [ViewVariables(VVAccess.ReadWrite), DataField("soundBrainHugging")]
    public SoundSpecifier? SoundBrainHugging = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };

    [DataField("paralyzeTime"), ViewVariables(VVAccess.ReadWrite)]
    public float ParalyzeTime = 3f;

    [DataField("brainslugTime")]
    public float BrainSlugTime = 3f;

    [DataField("brainreleaseTime")]
    public float BrainRealeseTime = 3f;

    [DataField("damage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = default!;

    [DataField("ichorChemical", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string IchorChemical = "Ichor";

    [ViewVariables(VVAccess.ReadWrite), DataField("healRate")]
    public float HealRate = 15f;
}
