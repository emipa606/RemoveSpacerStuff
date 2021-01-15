using System.Text;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace RemoveSpacerStuff
{
    [StaticConstructorOnStartup]
    public static class RemoveSpacerStuff
    {
        public const TechLevel MAX_TECHLEVEL = TechLevel.Industrial;
        private static int removedDefs;
        private static readonly StringBuilder DebugString = new StringBuilder();

        public static IEnumerable<ThingDef> things = new List<ThingDef>();

        static RemoveSpacerStuff()
        {
            DebugString.AppendLine("RemoveSpacerStuff - Start Removal Log");
            DebugString.AppendLine("Tech Max Level = " + MAX_TECHLEVEL.ToString());

            removedDefs = 0;
            IEnumerable<ResearchProjectDef> projects = new List<ResearchProjectDef>();
            if (ModStuff.Settings.LimitResearch)
            {
                projects = DefDatabase<ResearchProjectDef>.AllDefs.Where(rpd => rpd.techLevel > MAX_TECHLEVEL);
            }
            var extraDefsToRemove = new List<string>();

            if (ModStuff.Settings.LimitItems)
            {
                things = new HashSet<ThingDef>(DefDatabase<ThingDef>.AllDefs.Where(td =>
                td.techLevel > MAX_TECHLEVEL ||
                extraDefsToRemove.Contains(td.defName) ||
                (td.researchPrerequisites?.Any(rpd => projects.Contains(rpd)) ?? false) || new string[]
                {

                }.Contains(td.defName)));
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

            //var recipeDefsToRemove = DefDatabase<RecipeDef>.AllDefs.Where(rd =>
            //    rd.products.Any(tcc => things.Contains(tcc.thingDef)) ||
            //    rd.AllRecipeUsers.All(td => things.Contains(td)) ||
            //    projects.Contains(rd.researchPrerequisite)).Cast<Def>().ToList();
            //recipeDefsToRemove?.RemoveAll(x =>
            //    x.defName == "ExtractMetalFromSlag" ||
            //    x.defName == "SmeltWeapon" ||
            //    x.defName == "DestroyWeapon" ||
            //    x.defName == "OfferingOfPlants_Meagre" ||
            //    x.defName == "OfferingOfPlants_Decent" ||
            //    x.defName == "OfferingOfPlants_Sizable" ||
            //    x.defName == "OfferingOfPlants_Worthy" ||
            //    x.defName == "OfferingOfPlants_Impressive" ||
            //    x.defName == "OfferingOfMeat_Meagre" ||
            //    x.defName == "OfferingOfMeat_Decent" ||
            //    x.defName == "OfferingOfMeat_Sizable" ||
            //    x.defName == "OfferingOfMeat_Worthy" ||
            //    x.defName == "OfferingOfMeat_Impressive" ||
            //    x.defName == "OfferingOfMeals_Meagre" ||
            //    x.defName == "OfferingOfMeals_Decent" ||
            //    x.defName == "OfferingOfMeals_Sizable" ||
            //    x.defName == "OfferingOfMeals_Worthy" ||
            //    x.defName == "OfferingOfMeals_Impressive" ||
            //    x.defName == "ROMV_ExtractBloodVial" ||
            //    x.defName == "ROMV_ExtractBloodPack"
            //    );
            //RemoveStuffFromDatabase(typeof(DefDatabase<RecipeDef>), recipeDefsToRemove);

            DebugString.AppendLine("ResearchProjectDef Removal List");
            RemoveStuffFromDatabase(typeof(DefDatabase<ResearchProjectDef>), projects.Cast<Def>());

            DebugString.AppendLine("Scenario Part Removal List");
            FieldInfo getThingInfo =
                typeof(ScenPart_ThingCount).GetField("thingDef", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (ScenarioDef def in DefDatabase<ScenarioDef>.AllDefs)
            {
                foreach (ScenPart sp in def.scenario.AllParts)
                {
                    if (sp is ScenPart_ThingCount && things.Contains((ThingDef)getThingInfo?.GetValue(sp)))
                    {
                        def.scenario.RemovePart(sp);
                        DebugString.AppendLine("- " + sp.Label + " " + ((ThingDef)getThingInfo?.GetValue(sp)).label +
                                               " from " + def.label);
                    }
                }
            }

            foreach (ThingCategoryDef thingCategoryDef in DefDatabase<ThingCategoryDef>.AllDefs)
            {
                thingCategoryDef.childThingDefs.RemoveAll(things.Contains);
            }

            DebugString.AppendLine("Stock Generator Part Cleanup");
            foreach (TraderKindDef tkd in DefDatabase<TraderKindDef>.AllDefs)
            {
                for (var i = tkd.stockGenerators.Count - 1; i >= 0; i--)
                {
                    StockGenerator stockGenerator = tkd.stockGenerators[i];

                    switch (stockGenerator)
                    {
                        case StockGenerator_SingleDef sd when things.Contains(Traverse.Create(sd).Field("thingDef")
                            .GetValue<ThingDef>()):
                            ThingDef def = Traverse.Create(sd).Field("thingDef")
                                .GetValue<ThingDef>();
                            tkd.stockGenerators.Remove(stockGenerator);
                            DebugString.AppendLine("- " + def.label + " from " + tkd.label +
                                                   "'s StockGenerator_SingleDef");
                            break;
                        case StockGenerator_MultiDef md:
                            Traverse thingListTraverse = Traverse.Create(md).Field("thingDefs");
                            List<ThingDef> thingList = thingListTraverse.GetValue<List<ThingDef>>();
                            var removeList = thingList.FindAll(things.Contains);
                            removeList?.ForEach(x =>
                                DebugString.AppendLine("- " + x.label + " from " + tkd.label +
                                                       "'s StockGenerator_MultiDef"));
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


            //DebugString.AppendLine("IncidentDef Removal List");

            //var removedDefNames = new List<string>
            //{
            //    "Disease_FibrousMechanites",
            //    "Disease_SensoryMechanites",
            //    "ResourcePodCrash",
            //    "PsychicSoothe",
            //    "RefugeePodCrash",
            //    "RansomDemand",
            //    "MeteoriteImpact",
            //    "PsychicDrone",
            //    "ShortCircuit",
            //    "ShipChunkDrop",
            //    "OrbitalTraderArrival",
            //    "StrangerInBlackJoin",
            //    "DefoliatorShipPartCrash",
            //    "PsychicEmanatorShipPartCrash",
            //    "MechCluster",
            //    "AnimalInsanityMass"
            //};

            //IEnumerable<IncidentDef> incidents = from IncidentDef incident in DefDatabase<IncidentDef>.AllDefs
            //                                     where removedDefNames.Contains(incident.defName)
            //                                     select incident;


            //foreach (IncidentDef incident in incidents)
            //{
            //    incident.targetTags?.Clear();
            //    incident.baseChance = 0f;
            //    incident.allowedBiomes?.Clear();
            //    incident.earliestDay = int.MaxValue;
            //}

            //RemoveStuffFromDatabase(typeof(DefDatabase<IncidentDef>), incidents.Cast<Def>());

            //DebugString.AppendLine("Replaced Ancient Asphalt Road / Ancient Asphalt Highway with Stone Road");
            //RoadDef[] targetRoads = { RoadDefOf.AncientAsphaltRoad, RoadDefOf.AncientAsphaltHighway };
            //RoadDef originalRoad = DefDatabase<RoadDef>.GetNamed("StoneRoad");

            //List<string> fieldNames = AccessTools.GetFieldNames(typeof(RoadDef));
            //fieldNames.Remove("defName");
            //foreach (FieldInfo fi in fieldNames.Select(name => AccessTools.Field(typeof(RoadDef), name)))
            //{
            //    object fieldValue = fi.GetValue(originalRoad);
            //    foreach (RoadDef targetRoad in targetRoads) fi.SetValue(targetRoad, fieldValue);
            //}

            //DebugString.AppendLine("Special Hediff Removal List");
            //RemoveStuffFromDatabase(typeof(DefDatabase<HediffDef>), (hediffs = new[] { HediffDefOf.Gunshot }).Cast<Def>());

            //DebugString.AppendLine("RaidStrategyDef Removal List");
            //RemoveStuffFromDatabase(typeof(DefDatabase<RaidStrategyDef>),
            //    DefDatabase<RaidStrategyDef>.AllDefs
            //        .Where(rs => typeof(ScenPart_ThingCount).IsAssignableFrom(rs.workerClass)).Cast<Def>());

            ////            ItemCollectionGeneratorUtility.allGeneratableItems.RemoveAll(match: things.Contains);
            ////
            ////            foreach (Type type in typeof(ItemCollectionGenerator_Standard).AllSubclassesNonAbstract())
            ////                type.GetMethod(name: "Reset")?.Invoke(obj: null, parameters: null);

            DebugString.AppendLine("ThingDef Removal List");
            RemoveStuffFromDatabase(typeof(DefDatabase<ThingDef>), things.ToArray());

            DebugString.AppendLine("ThingSetMaker Reset");
            ThingSetMakerUtility.Reset();

            //DebugString.AppendLine("TraitDef Removal List");
            //RemoveStuffFromDatabase(typeof(DefDatabase<TraitDef>),
            //    //                                                                   { nameof(TraitDefOf.Prosthophobe), "Prosthophile" } ?
            //    DefDatabase<TraitDef>.AllDefs
            //        .Where(td => new[] { nameof(TraitDefOf.BodyPurist), "Transhumanist" }.Contains(td.defName))
            //        .Cast<Def>());

            DebugString.AppendLine("Designators Resolved Again");
            MethodInfo resolveDesignatorsAgain = typeof(DesignationCategoryDef).GetMethod("ResolveDesignators",
                BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (DesignationCategoryDef dcd in DefDatabase<DesignationCategoryDef>.AllDefs)
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
                        (pkd.race.techLevel > MAX_TECHLEVEL || pkd.defaultFactionType?.techLevel > MAX_TECHLEVEL))
                    .Cast<Def>());
            }

            if (ModStuff.Settings.LimitFactions)
            {
                DebugString.AppendLine("FactionDef Removal List");
                RemoveStuffFromDatabase(typeof(DefDatabase<FactionDef>),
                    DefDatabase<FactionDef>.AllDefs.Where(fd => !fd.isPlayer && fd.techLevel > MAX_TECHLEVEL).Cast<Def>());
            }
            if (ModStuff.Settings.LogRemovals)
            {
                Log.Message(DebugString.ToString());
            }
            else
            {
                Log.Message("Removed " + removedDefs + " spacer defs");
            }

            PawnWeaponGenerator.Reset();
            PawnApparelGenerator.Reset();

            Debug.Log(DebugString.ToString());
            DebugString = new StringBuilder();
        }


        private static void RemoveStuffFromDatabase(Type databaseType, [NotNull] IEnumerable<Def> defs)
        {
            IEnumerable<Def> enumerable = defs as Def[] ?? defs.ToArray();
            if (!enumerable.Any())
            {
                return;
            }

            Traverse rm = Traverse.Create(databaseType).Method("Remove", enumerable.First());
            foreach (Def def in enumerable)
            {
                removedDefs++;
                DebugString.AppendLine("- " + def.label);
                rm.GetValue(def);
            }
        }
    }
}