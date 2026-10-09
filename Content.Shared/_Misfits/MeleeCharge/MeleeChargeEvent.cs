// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Shared/_Misfits/MeleeCharge/MeleeChargeEvent.cs

using Content.Shared.Actions;


namespace Content.Shared._Misfits.MeleeCharge;


public sealed partial class MeleeChargeEvent : WorldTargetActionEvent
{
    [DataField]
    public float Speed = 10f;

    [DataField]
    public float Range = 3.5f;

}
