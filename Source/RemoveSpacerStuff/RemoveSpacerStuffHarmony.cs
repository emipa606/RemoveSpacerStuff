using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoveSpacerStuff;

[StaticConstructorOnStartup]
public static class RemoveModernStuffHarmony
{
    static RemoveModernStuffHarmony()
    {
        var harmony = new Harmony("Mlie.RemoveSpacerStuff");
        harmony.Patch(AccessTools.Method(typeof(ThingSetMaker), "Generate", new[] { typeof(ThingSetMakerParams) }),
            new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(ItemCollectionGeneratorGeneratePrefix)));
        //Log.Message("AddToTradeables");
        harmony.Patch(AccessTools.Method(typeof(TradeDeal), "AddToTradeables"),
            new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(PostCacheTradeables)));
        //Log.Message("CanGenerate");
        harmony.Patch(AccessTools.Method(typeof(ThingSetMakerUtility), nameof(ThingSetMakerUtility.CanGenerate)),
            null, new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(ThingSetCleaner)));
        harmony.Patch(AccessTools.Method(typeof(FactionManager), "FirstFactionOfDef", new[] { typeof(FactionDef) }),
            new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(FactionManagerFirstFactionOfDefPrefix)));

        harmony.Patch(
            AccessTools.Method(typeof(BackCompatibility), "FactionManagerPostLoadInit", Array.Empty<Type>()),
            new HarmonyMethod(typeof(RemoveModernStuffHarmony),
                nameof(BackCompatibilityFactionManagerPostLoadInitPrefix)));
    }

    public static void ThingSetCleaner(ThingDef thingDef, ref bool __result)
    {
        __result &= !RemoveSpacerStuff.things.Contains(thingDef);
    }

    public static bool PostCacheTradeables(Thing t)
    {
        return !RemoveSpacerStuff.things.Contains(t.def);
    }

    public static void ItemCollectionGeneratorGeneratePrefix(ref ThingSetMakerParams parms)
    {
        if (parms.techLevel is null or > RemoveSpacerStuff.MAX_TECHLEVEL)
        {
            parms.techLevel = RemoveSpacerStuff.MAX_TECHLEVEL;
        }
    }

    public static bool FactionManagerFirstFactionOfDefPrefix(ref FactionDef facDef)
    {
        return !ModStuff.Settings.LimitFactions || facDef is not { techLevel: > RemoveSpacerStuff.MAX_TECHLEVEL };
    }

    public static bool BackCompatibilityFactionManagerPostLoadInitPrefix()
    {
        return !ModStuff.Settings.LimitFactions;
    }
}