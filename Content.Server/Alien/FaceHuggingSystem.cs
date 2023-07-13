using Content.Server.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Alien;
using Content.Server.Mind;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;

namespace Content.Server.Alien;

public sealed class FaceHuggingSystem : SharedFaceHuggingSystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggingComponent, FaceHuggingDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(EntityUid uid, FaceHuggingComponent component, FaceHuggingDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        else if (args.Args.Target != null)
        {

            var target = args.Target;
            _mindSystem.TryGetMind(uid, out var mind);

            if (mind == null || target == null)
                return;

            _popup.PopupEntity(Loc.GetString("Facehugger exhausts itself and settles in your body!"), uid, uid, PopupType.LargeCaution);

            _mindSystem.TransferTo(mind, target);
            _inventory.TryUnequip(target.Value, "mask", true, true);
            _damageableSystem.TryChangeDamage(uid, component.Damage);

            if (TryComp(target, out MobStateComponent? mobState))
            {
               if (mobState.CurrentState == MobState.Critical)
                {
                    var ichorInjection = new Solution(component.IchorChemical, component.HealRate);
                    ichorInjection.ScaleSolution(5.0f);
                    _bloodstreamSystem.TryAddToChemicals(target.Value, ichorInjection);
                }
            }
        }



        _audioSystem.PlayPvs(component.SoundFaceHugging, uid);
    }
}

