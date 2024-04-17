using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Prim14.Bushes;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Prim14.Bush;

public sealed class BushSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BushComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<BushComponent, BushReadyEvent>(OnBushReady);
        SubscribeLocalEvent<BushComponent, BushCollectingDoAfterEvent>(BushPick);
    }

    private void OnInteractHand(EntityUid uid, BushComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        Rustle(uid, component, args.User);

        if (!component.Ready)
        {
            var msg = Loc.GetString("bushes-interact-no-berries");
            _popupSystem.PopupEntity(msg, uid, args.User);
            return;
        }

        TryBushPick(uid, component, args);

        args.Handled = true;
    }

    private void TryBushPick(EntityUid uid, BushComponent component, InteractHandEvent args)
    {
        if (!component.Ready)
            return;


        var doAfterEventArgs = new DoAfterArgs(_entityManager, args.User, 0f, new BushCollectingDoAfterEvent(), uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
    }

    private void BushPick(EntityUid uid, BushComponent component, BushCollectingDoAfterEvent args)
    {
        if (!component.Ready)
            return;

        var loot = Spawn(component.Loot);
        _handsSystem.PickupOrDrop(args.User, loot);
        UpdateAppearance(uid, true, false);
        component.Ready = false;
        Dirty(uid, component);
    }

    private void Rustle(EntityUid uid, BushComponent? component = null, EntityUid? user = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _audio.PlayPvs(component.RustleSound, uid, AudioParams.Default.WithVariation(0.25f));
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BushComponent>();
        while (query.MoveNext(out var uid, out var bush))
        {
            if (bush.Ready)
                continue;
            if (bush.Accumulator < bush.RespawnTime)
            {
                bush.Accumulator += frameTime;
                continue;
            }

            bush.Accumulator = 0;
            var ev = new BushReadyEvent(bush);
            RaiseLocalEvent(uid, ev);
        }
    }

    private void UpdateAppearance(EntityUid uid, bool isEmpty, bool isReady)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        _appearance.SetData(uid, BushVisuals.Empty, isEmpty, appearance);
        _appearance.SetData(uid, BushVisuals.Ready, isReady, appearance);
    }

    private void OnBushReady(EntityUid uid, BushComponent component, BushReadyEvent args)
    {
        component.Ready = true;
        UpdateAppearance(uid, false, true);
    }

    private sealed class BushReadyEvent(BushComponent bush) : EntityEventArgs
    {
        public BushComponent Bush { [UsedImplicitly] get; } = bush;
    }
}
