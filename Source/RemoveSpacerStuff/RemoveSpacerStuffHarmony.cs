using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RemoveSpacerStuff
{
    [StaticConstructorOnStartup]
    public static class RemoveModernStuffHarmony
    {
        static RemoveModernStuffHarmony()
        {
            var harmony = new Harmony(id: "Mlie.RemoveSpacerStuff");
            harmony.Patch(original: AccessTools.Method(type: typeof(ThingSetMaker), name: "Generate", parameters: new[] { typeof(ThingSetMakerParams) }), prefix: new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(ItemCollectionGeneratorGeneratePrefix)), postfix: null);
            //Log.Message("AddToTradeables");
            harmony.Patch(AccessTools.Method(typeof(TradeDeal), "AddToTradeables"), new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(PostCacheTradeables)), null);
            //Log.Message("CanGenerate");
            harmony.Patch(AccessTools.Method(typeof(ThingSetMakerUtility), nameof(ThingSetMakerUtility.CanGenerate)), null, new HarmonyMethod(typeof(RemoveModernStuffHarmony), nameof(ThingSetCleaner)));
            
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
            if (!parms.techLevel.HasValue || parms.techLevel > RemoveSpacerStuff.MAX_TECHLEVEL)
                parms.techLevel = RemoveSpacerStuff.MAX_TECHLEVEL;
        }

    }
}