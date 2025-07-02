using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoveSpacerStuff;

[HarmonyPatch(typeof(TradeDeal), "AddToTradeables")]
public static class TradeDeal_AddToTradeables
{
    public static bool Prefix(Thing t)
    {
        return !RemoveSpacerStuff.Things.Contains(t.def);
    }
}