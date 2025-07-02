using HarmonyLib;
using RimWorld;

namespace RemoveSpacerStuff;

[HarmonyPatch(typeof(ThingSetMaker), nameof(ThingSetMaker.Generate), typeof(ThingSetMakerParams))]
public static class ThingSetMaker_Generate
{
    public static void Prefix(ref ThingSetMakerParams parms)
    {
        if (parms.techLevel is null or > RemoveSpacerStuff.MaxTechlevel)
        {
            parms.techLevel = RemoveSpacerStuff.MaxTechlevel;
        }
    }
}