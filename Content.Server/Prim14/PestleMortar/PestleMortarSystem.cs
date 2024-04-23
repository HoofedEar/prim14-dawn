using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Kitchen.Components;
using Content.Shared.Popups;
using Content.Shared.Random;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Server.Prim14.PestleMortar;

public sealed class PestleMortarSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly RandomHelperSystem _randomHelper = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PestleMortarComponent, InteractUsingEvent>(OnPestleMortarInteractUsing);
        SubscribeLocalEvent<PestleMortarComponent, UseInHandEvent>(OnPestleMortarUseInHand);
        SubscribeLocalEvent<PestleMortarComponent, GetVerbsEvent<AlternativeVerb>>(AddVerbs);
    }

    private void OnPestleMortarUseInHand(EntityUid owner, PestleMortarComponent component, UseInHandEvent args)
    {
        // Extract stuff from entity inside inputContainer, transfer that to SolutionContainer
        var inputContainer = _containerSystem.EnsureContainer<Container>(owner, SharedPestleMortar.InputContainerId);
        var success = false;
        foreach (var item in inputContainer.ContainedEntities.ToList())
        {
            if (!TryComp<ExtractableComponent>(item, out _))
                continue;

            var solution = CompOrNull<ExtractableComponent>(item)?.JuiceSolution;
            if (solution == null)
                continue;

            if (!_solutionContainerSystem.TryGetFitsInDispenser(owner, out var containerSoln,
                    out var containerSolution))
                continue;

            if (solution.Volume > containerSolution.AvailableVolume)
            {
                _popupSystem.PopupEntity(Loc.GetString("pestle-and-mortar-too-full"), owner, args.User);
                return;
            }

            QueueDel(item);

            _solutionContainerSystem.TryAddSolution(containerSoln.Value, solution);
            success = true;
        }

        if (success)
            _popupSystem.PopupEntity(Loc.GetString("pestle-and-mortar-grind"), owner, args.User);

        args.Handled = true;
    }

    private void DropAllItems(EntityUid uid, GetVerbsEvent<AlternativeVerb> verbs)
    {
        var inputContainer = _containerSystem.EnsureContainer<Container>(uid, SharedPestleMortar.InputContainerId);
        var success = false;
        foreach (var item in inputContainer.ContainedEntities.ToList())
        {
            _containerSystem.Remove(item, inputContainer);
            _randomHelper.RandomOffset(item, 0.4f);
            success = true;
        }

        _popupSystem.PopupEntity(
            success
                ? Loc.GetString("pestle-and-mortar-empty-verb")
                : Loc.GetString("pestle-and-mortar-empty-verb-fail"), uid, verbs.User);
    }

    private void OnPestleMortarInteractUsing(EntityUid uid, PestleMortarComponent component, InteractUsingEvent args)
    {
        var inputContainer =
            _containerSystem.EnsureContainer<Container>(args.Target, SharedPestleMortar.InputContainerId);

        if (!HasComp<ExtractableComponent>(args.Used))
        {
            _popupSystem.PopupEntity(Loc.GetString("pestle-and-mortar-cannot-put-entity-message"), uid, args.User);
            return;
        }

        if (inputContainer.ContainedEntities.Count >= 2)
        {
            _popupSystem.PopupEntity(Loc.GetString("pestle-and-mortar-too-full-entity"), uid, args.User);
            return;
        }

        if (!_containerSystem.Insert(args.Used, inputContainer))
            return;

        args.Handled = true;
    }

    private void AddVerbs(EntityUid uid, PestleMortarComponent component, GetVerbsEvent<AlternativeVerb> verbs)
    {
        if (!_actionBlocker.CanInteract(verbs.User, uid))
        {
            return;
        }


        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("pestle-and-mortar-empty"),
            Act = () => DropAllItems(uid, verbs)
        };
        verbs.Verbs.Add(verb);
    }
}
