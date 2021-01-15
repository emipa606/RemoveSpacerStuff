using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RemoveSpacerStuff
{
    public class Settings : ModSettings
    {
        // Token: 0x06000063 RID: 99 RVA: 0x00004630 File Offset: 0x00002830
        public void DoWindowContents(Rect canvas)
        {
            var gap = 8f;
            var listing_Standard = new Listing_Standard
            {
                ColumnWidth = canvas.width
            };
            listing_Standard.Begin(canvas);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("Limit items", ref LimitItems, "Removes all items tagged with tech-level spacer");
            listing_Standard.CheckboxLabeled("Limit research", ref LimitResearch, "Removes all research-projects tagged with tech-level spacer");
            listing_Standard.CheckboxLabeled("Limit factions", ref LimitFactions, "Removes all factions tagged with tech-level spacer");
            listing_Standard.CheckboxLabeled("Limit pawnkinds", ref LimitPawns, "Removes all pawnkinds tagged with tech-level spacer");
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("Log removed items", ref LogRemovals, "Logs all removed items at game-start");
            listing_Standard.Gap(gap);
            listing_Standard.Label("NOTE: Any changes require a restart to have effect");
            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref LimitItems, "LimitItems", true, false);
            Scribe_Values.Look(ref LimitResearch, "LimitResearch", true, false);
            Scribe_Values.Look(ref LimitFactions, "LimitFactions", true, false);
            Scribe_Values.Look(ref LimitPawns, "LimitPawns", true, false);
            Scribe_Values.Look(ref LogRemovals, "LogRemovals", false, false);
        }

        public bool LimitItems = true;
        public bool LimitResearch = true;
        public bool LimitFactions = true;
        public bool LimitPawns = true;
        public bool LogRemovals = false;
    }
}
