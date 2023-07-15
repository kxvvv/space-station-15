using Robust.Shared.Containers;

namespace Content.Server.Alien;


[RegisterComponent]
public sealed class BrainSlugComponent : Component
{
    public EntityUid Parent;

    public EntityUid EquipedOn;

    [ViewVariables(VVAccess.ReadWrite)] public ContainerSlot GuardianContainer = default!;

    [DataField("damageFrequency"), ViewVariables(VVAccess.ReadWrite)]
    public float DamageFrequency = 50;

    [ViewVariables] public float Accumulator = 0;
}
