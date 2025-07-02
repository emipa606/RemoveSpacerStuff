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
    public const TechLevel MaxTechlevel = TechLevel.Industrial;
    private static int removedDefs;
    private static readonly StringBuilder debugString = new();

    public static readonly IEnumerable<ThingDef> Things = new List<ThingDef>();

    static RemoveSpacerStuff()
    {
        debugString.AppendLine("RemoveSpacerStuff - Start Removal Log");
        debugString.AppendLine($"Tech Max Level = {MaxTechlevel}");

        removedDefs = 0;
        IEnumerable<ResearchProjectDef> projects = new List<ResearchProjectDef>();
        if (ModStuff.Settings.LimitResearch)
        {
            projects = DefDatabase<ResearchProjectDef>.AllDefs.Where(rpd => rpd.techLevel > MaxTechlevel);
        }


        if (ModStuff.Settings.LimitItems)
        {
            Things = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(td =>
                td.techLevel > MaxTechlevel ||
                (td.researchPrerequisites?.Any(rpd => projects.Contains(rpd)) ?? false)));
        }

        debugString.AppendLine("RecipeDef Removal List");


        foreach (var thing in from thing in Things where thing.tradeTags != null select thing)
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

        debugString.AppendLine("ResearchProjectDef Removal List");
        removeStuffFromDatabase(typeof(DefDatabase<ResearchProjectDef>), projects);

        debugString.AppendLine("Scenario Part Removal List");
        var getThingInfo =
            typeof(ScenPart_ThingCount).GetField("thingDef", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var def in DefDatabase<ScenarioDef>.AllDefs)
        {
            foreach (var sp in def.scenario.AllParts)
            {
                if (sp is not ScenPart_ThingCount || !Things.Contains((ThingDef)getThingInfo?.GetValue(sp)))
                {
                    continue;
                }

                def.scenario.RemovePart(sp);
                debugString.AppendLine(
                    $"- {sp.Label} {((ThingDef)getThingInfo?.GetValue(sp))?.label} from {def.label}");
            }
        }

        foreach (var thingCategoryDef in DefDatabase<ThingCategoryDef>.AllDefs)
        {
            thingCategoryDef.childThingDefs.RemoveAll(Things.Contains);
        }

        debugString.AppendLine("Stock Generator Part Cleanup");
        foreach (var tkd in DefDatabase<TraderKindDef>.AllDefs)
        {
            for (var i = tkd.stockGenerators.Count - 1; i >= 0; i--)
            {
                var stockGenerator = tkd.stockGenerators[i];

                switch (stockGenerator)
                {
                    case StockGenerator_SingleDef sd when Things.Contains(Traverse.Create(sd).Field("thingDef")
                        .GetValue<ThingDef>()):
                        var def = Traverse.Create(sd).Field("thingDef")
                            .GetValue<ThingDef>();
                        tkd.stockGenerators.Remove(stockGenerator);
                        debugString.AppendLine($"- {def.label} from {tkd.label}'s StockGenerator_SingleDef");
                        break;
                    case StockGenerator_MultiDef md:
                        var thingListTraverse = Traverse.Create(md).Field("thingDefs");
                        var thingList = thingListTraverse.GetValue<List<ThingDef>>();
                        var removeList = thingList.FindAll(Things.Contains);
                        removeList.ForEach(x =>
                            debugString.AppendLine($"- {x.label} from {tkd.label}'s StockGenerator_MultiDef"));
                        thingList.RemoveAll(Things.Contains);

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

        debugString.AppendLine("ThingDef Removal List");
        removeStuffFromDatabase(typeof(DefDatabase<ThingDef>), Things.ToArray());

        debugString.AppendLine("ThingSetMaker Reset");
        ThingSetMakerUtility.Reset();

        debugString.AppendLine("Designators Resolved Again");
        var resolveDesignatorsAgain = typeof(DesignationCategoryDef).GetMethod("ResolveDesignators",
            BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var dcd in DefDatabase<DesignationCategoryDef>.AllDefs)
        {
            resolveDesignatorsAgain?.Invoke(dcd, null);
        }

        if (ModStuff.Settings.LimitPawns)
        {
            debugString.AppendLine("PawnKindDef Removal List");
            removeStuffFromDatabase(typeof(DefDatabase<PawnKindDef>),
                DefDatabase<PawnKindDef>.AllDefs
                    .Where(pkd =>
                        (!pkd.defaultFactionDef?.isPlayer ?? false) &&
                        (pkd.race.techLevel > MaxTechlevel || pkd.defaultFactionDef?.techLevel > MaxTechlevel)));
        }

        if (ModStuff.Settings.LimitFactions)
        {
            debugString.AppendLine("FactionDef Removal List");
            removeStuffFromDatabase(typeof(DefDatabase<FactionDef>),
                DefDatabase<FactionDef>.AllDefs.Where(fd => !fd.isPlayer && fd.techLevel > MaxTechlevel));
            if (ModLister.RoyaltyInstalled)
            {
                var incident = DefDatabase<IncidentDef>.GetNamedSilentFail("CaravanArrivalTributeCollector");
                if (incident != null)
                {
                    removeStuffFromDatabase(typeof(DefDatabase<IncidentDef>),
                        new List<Def> { incident });
                }
            }
        }

        Log.Message(ModStuff.Settings.LogRemovals ? debugString.ToString() : $"Removed {removedDefs} spacer defs");

        PawnWeaponGenerator.Reset();
        PawnApparelGenerator.Reset();

        Debug.Log(debugString.ToString());
        debugString = new StringBuilder();
    }


    private static void removeStuffFromDatabase(Type databaseType, IEnumerable<Def> defs)
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
            debugString.AppendLine($"- {def.label}");
            rm.GetValue(def);
        }
    }
}