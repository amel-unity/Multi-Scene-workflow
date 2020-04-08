using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidAmmoDisplay : AmmoDisplay
{
    public LiquidContainer Container;
    public float MinLiquidAmount;
    public float MaxLiquidAmount;
    
    public override void UpdateAmount(int current, int max)
    {
        Container.ChangeLiquidAmount(Mathf.Lerp(MinLiquidAmount, MaxLiquidAmount, current/(float)max));
    }
}
