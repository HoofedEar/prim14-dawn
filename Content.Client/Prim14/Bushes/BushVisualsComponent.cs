namespace Content.Client.Prim14.Bushes;

[RegisterComponent]
public sealed partial class BushVisualsComponent : Component
{
    [DataField("stateReady")]
    public string? StateReady;

    [DataField("stateEmpty")]
    public string? StateEmpty;
}
