namespace Content.Server.Tiles;

[RegisterComponent, Access(typeof(WaterTileSystem))]
public sealed partial class WaterTileComponent : Component
{
    /// <summary>
    /// How many fire stacks are applied per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("coolingRate")]
    public float CoolingRate = -20f;
}
