using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Temperature.Systems;
using Content.Shared.Placeable;
using Robust.Shared.Physics.Events;

namespace Content.Server.Prim14.EntityHeaterPowerless;

public sealed class EntityHeaterPowerlessSystem: EntitySystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityHeaterPowerlessComponent, StartCollideEvent>(OnStartCollide);
    }

    public override void Update(float deltaTime)
    {
        var query = EntityQueryEnumerator<EntityHeaterPowerlessComponent, ItemPlacerComponent>();
        while (query.MoveNext(out var uid, out var comp, out var placer))
        {
            // don't divide by total entities since its a big grill
            // excess would just be wasted in the air but that's not worth simulating
            // if you want a heater thermomachine just use that...
            var energy = 30.0f;
            foreach (var ent in placer.PlacedEntities)
            {
                _temperature.ChangeHeat(ent, energy);
            }
        }
    }

    private void OnStartCollide(EntityUid uid, EntityHeaterPowerlessComponent comp, ref StartCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (TryComp<FlammableComponent>(otherUid, out var flammable))
        {
            // Apply the fury of a thousand suns
            var multiplier = flammable.FireStacks == 0f ? 5f : 1f;
            _flammable.AdjustFireStacks(otherUid, 0.50f * multiplier, flammable);
            _flammable.Ignite(otherUid, uid, flammable);
        }
    }
}
