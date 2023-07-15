using Robust.Shared.Network;

namespace Content.Server.Alien;


[RegisterComponent]
public sealed class SlugInsideComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Slug;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Parent;

    [ViewVariables(VVAccess.ReadWrite)]
    public NetUserId NetParent;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("ReleaseControlName")]
    public string ReleaseControlName = "ReleaseControlAction"; // release control
};
