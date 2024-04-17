using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Prim14.Bushes;

[Serializable, NetSerializable]
public enum BushVisuals
{
    Ready,
    Empty
}

[Serializable, NetSerializable]
public sealed partial class BushCollectingDoAfterEvent : SimpleDoAfterEvent;
