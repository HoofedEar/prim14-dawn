using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.StepTrigger.Systems;

namespace Content.Server.Tiles;

public sealed class WaterTileSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WaterTileComponent, StepTriggeredOffEvent>(OnWaterStepTriggered);
        SubscribeLocalEvent<WaterTileComponent, StepTriggerAttemptEvent>(OnWaterStepTriggerAttempt);
    }

    private void OnWaterStepTriggerAttempt(EntityUid uid, WaterTileComponent component, ref StepTriggerAttemptEvent args)
    {
        if (!HasComp<FlammableComponent>(args.Tripper))
            return;

        args.Continue = true;
    }

    private void OnWaterStepTriggered(EntityUid uid, WaterTileComponent component, ref StepTriggeredOffEvent args)
    {
        var otherUid = args.Tripper;

        if (TryComp<FlammableComponent>(otherUid, out var flammable))
        {
            // Extinguish them if they're on fire
            _flammable.Extinguish(otherUid, flammable);
        }

        if (TryComp<TemperatureComponent>(otherUid, out var temperature))
        {
            // Cool them down
            _temperature.ChangeHeat(otherUid, component.CoolingRate, true, temperature);
        }
    }
}
