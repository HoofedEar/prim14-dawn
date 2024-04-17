using Content.Shared.Prim14;
using Content.Shared.Prim14.Bushes;
using Robust.Client.GameObjects;

namespace Content.Client.Prim14.Bushes;

public sealed class BushVisualsSystem : VisualizerSystem<BushVisualsComponent>
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    protected override void OnAppearanceChange(EntityUid uid, BushVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp(uid, out SpriteComponent? sprite) ||
            !_appearance.TryGetData(uid, BushVisuals.Ready, out bool isOn) ||
            !_appearance.TryGetData(uid, BushVisuals.Empty, out bool isRunning))
            return;
        var state = isRunning ? component.StateReady : component.StateEmpty;
        sprite.LayerSetVisible(BushVisualLayers.Ready, isOn);
        sprite.LayerSetState(BushVisualLayers.Empty, state);
    }
}

public enum BushVisualLayers : byte
{
    Ready,
    Empty
}
