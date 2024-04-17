using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server.Prim14.Bush;

[RegisterComponent]
[Access(typeof(BushSystem))]
public sealed partial class BushComponent : Component
{
    /// <summary>
    /// Entity that is spawned when a player picks from the bush
    /// </summary>
    [ViewVariables]
    [DataField("loot")]
    public string? Loot;

    /// <summary>
    /// How many should be spawned?
    /// </summary>
    [ViewVariables]
    [DataField("quantity")]
    public int? Quantity = 1;

    /// <summary>
    /// Time it takes to respawn the loot
    /// </summary>
    [ViewVariables]
    [DataField("respawnTime")]
    public float? RespawnTime = 30f;

    /// <summary>
    /// The sound played when interacting with the bush
    /// </summary>
    [ViewVariables]
    [DataField("interactSound")]
    public SoundSpecifier? InteractSound;

    [ViewVariables]
    [DataField("rustleSound")]
    public SoundSpecifier RustleSound = new SoundPathSpecifier("/Audio/Effects/plant_rustle.ogg");

    [ViewVariables]
    public float Accumulator;

    [ViewVariables]
    public bool Ready = true;
}
