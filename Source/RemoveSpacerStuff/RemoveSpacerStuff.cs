using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RemoveSpacerStuff;

[StaticConstructorOnStartup]
public static class RemoveSpacerStuff
{
    public const TechLevel MAX_TECHLEVEL = TechLevel.Industrial;
    private static int removedDefs;
    private static readonly StringBuilder DebugString = new StringBuilder();

    public static readonly IEnumerable<ThingDef> things = new List<ThingDef>();

    static RemoveSpacerStuff()
    {
        DebugString.AppendLine("RemoveSpacerStuff - Start Removal Log");
        DebugString.AppendLine($"Tech Max Level = {MAX_TECHLEVEL}");

        removedDefs = 0;
        IEnumerable<ResearchProjectDef> projects = new List<ResearchProjectDef>();
        if (ModStuff.Settings.LimitResearch)
        {
            projects = DefDatabase<ResearchProjectDef>.AllDefs.Where(rpd => rpd.techLevel > MAX_TECHLEVEL);
        }


        if (ModStuff.Settings.LimitItems)
        {
            things = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(td =>
                td.techLevel > MAX_TECHLEVEL ||
                (td.researchPrerequisites?.Any(rpd => projects.Contains(rpd)) ?? false)));
        }

        DebugString.AppendLine("RecipeDef Removal List");


        foreach (var thing in from thing in things where thing.tradeTags != null select thing)
        {
            var tags = thing.tradeTags.ToArray();
            foreach (var tag in tags)
            {
                if (tag.StartsWith("CE_AutoEnableCrafting"))
                {
                    thing.tradeTags.Remove(tag);
                }
            }
        }

        DebugString.AppendLine("ResearchProjectDef Removal List");
        RemoveStuffFromDatabase(typeof(DefDatabase<ResearchProjectDef>), projects);

        DebugString.AppendLine("Scenario Part Removal List");
        var getThingInfo =
            typeof(ScenPart_ThingCount).GetField("thingDef", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var def in DefDatabase<ScenarioDef>.AllDefs)
        {
            foreach (var sp in def.scenario.AllParts)
            {
                if (!(sp is ScenPart_ThingCount) || !things.Contains((ThingDef)getThingInfo?.GetValue(sp)))
                {
                    continue;
                }

                def.scenario.RemovePart(sp);
                DebugString.AppendLine(
                    $"- {sp.Label} {((ThingDef)getThingInfo?.GetValue(sp))?.label} from {def.label}");
            }
        }

        foreach (var thingCategoryDef in DefDatabase<ThingCategoryDef>.AllDefs)
        {
            thingCategoryDef.childThingDefs.RemoveAll(things.Contains);
        }

        DebugString.AppendLine("Stock Generator Part Cleanup");
        foreach (var tkd in DefDatabase<TraderKindDef>.AllDefs)
        {
            for (var i = tkd.stockGenerators.Count - 1; i >= 0; i--)
            {
                var stockGenerator = tkd.stockGenerators[i];

                switch (stockGenerator)
                {
                    case StockGenerator_SingleDef sd when things.Contains(Traverse.Create(sd).Field("thingDef")
                        .GetValue<ThingDef>()):
                        var def = Traverse.Create(sd).Field("thingDef")
                            .GetValue<ThingDef>();
                        tkd.stockGenerators.Remove(stockGenerator);
                        DebugString.AppendLine($"- {def.label} from {tkd.label}'s StockGenerator_SingleDef");
                        break;
                    case StockGenerator_MultiDef md:
                        var thingListTraverse = Traverse.Create(md).Field("thingDefs");
                        var thingList = thingListTraverse.GetValue<List<ThingDef>>();
                        var removeList = thingList.FindAll(things.Contains);
                        removeList.ForEach(x =>
                            DebugString.AppendLine($"- {x.label} from {tkd.label}'s StockGenerator_MultiDef"));
                        thingList.RemoveAll(things.Contains);

                        if (thingList.NullOrEmpty())
                        {
                            tkd.stockGenerators.Remove(stockGenerator);
                        }
                        else
                        {
                            thingListTraverse.SetValue(thingList);
                        }

                        break;
                }
            }
        }

        DebugString.AppendLine("ThingDef Removal List");
        RemoveStuffFromDatabase(typeof(DefDatabase<ThingDef>), things.ToArray());

        DebugString.AppendLine("ThingSetMaker Reset");
        ThingSetMakerUtility.Reset();

        DebugString.AppendLine("Designators Resolved Again");
        var resolveDesignatorsAgain = typeof(DesignationCategoryDef).GetMethod("ResolveDesignators",
            BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var dcd in DefDatabase<DesignationCategoryDef>.AllDefs)
        {
            resolveDesignatorsAgain?.Invoke(dcd, null);
        }

        if (ModStuff.Settings.LimitPawns)
        {
            DebugString.AppendLine("PawnKindDef Removal List");
            RemoveStuffFromDatabase(typeof(DefDatabase<PawnKindDef>),
                DefDatabase<PawnKindDef>.AllDefs
                    .Where(pkd =>
                        (!pkd.defaultFactionType?.isPlayer ?? false) &&
                        (pkd.race.techLevel > MAX_TECHLEVEL || pkd.defaultFactionType?.techLevel > MAX_TECHLEVEL)));
        }

        if (ModStuff.Settings.LimitFactions)
        {
            DebugString.AppendLine("FactionDef Removal List");
            RemoveStuffFromDatabase(typeof(DefDatabase<FactionDef>),
                DefDatabase<FactionDef>.AllDefs.Where(fd => !fd.isPlayer && fd.techLevel > MAX_TECHLEVEL));
            if (ModLister.RoyaltyInstalled)
            {
                var incident = DefDatabase<IncidentDef>.GetNamedSilentFail("CaravanArrivalTributeCollector");
                if (incident != null)
                {
                    RemoveStuffFromDatabase(typeof(DefDatabase<IncidentDef>),
                        new List<Def> { incident });
                }
            }
        }

        Log.Message(ModStuff.Settings.LogRemovals ? DebugString.ToString() : $"Removed {removedDefs} spacer defs");

        PawnWeaponGenerator.Reset();
        PawnApparelGenerator.Reset();

        Debug.Log(DebugString.ToString());
        DebugString = new StringBuilder();
    }


    private static void RemoveStuffFromDatabase(Type databaseType, IEnumerable<Def> defs)
    {
        IEnumerable<Def> enumerable = defs as Def[] ?? defs.ToArray();
        if (!enumerable.Any())
        {
            return;
        }

        var rm = Traverse.Create(databaseType).Method("Remove", enumerable.First());
        foreach (var def in enumerable)
        {
            removedDefs++;
            DebugString.AppendLine($"- {def.label}");
            rm.GetValue(def);
        }
    }
}