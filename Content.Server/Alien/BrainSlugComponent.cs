using Content.Shared.Damage;
using Robust.Shared.Containers;

namespace Content.Server.Alien;


[RegisterComponent]
public sealed class BrainSlugComponent : Component
{
    public EntityUid EquipedOn;

    [ViewVariables(VVAccess.ReadWrite)] public ContainerSlot GuardianContainer = default!;

    [DataField("damageFrequency"), ViewVariables(VVAccess.ReadWrite)]
    public float DamageFrequency = 5;

    [ViewVariables] public float Accumulator = 0;
}
