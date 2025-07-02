using UnityEngine;
using Verse;

namespace RemoveSpacerStuff;

public class Settings : ModSettings
{
    public bool LimitFactions = true;

    public bool LimitItems = true;
    public bool LimitPawns = true;
    public bool LimitResearch = true;

    public bool LogRemovals;

    public void DoWindowContents(Rect canvas)
    {
        const float gap = 8f;
        var listingStandard = new Listing_Standard
        {
            ColumnWidth = canvas.width
        };
        listingStandard.Begin(canvas);
        listingStandard.Gap(gap);
        listingStandard.CheckboxLabeled("RSS.LimitItems".Translate(), ref LimitItems,
            "RSS.LimitItems.Desc".Translate());
        listingStandard.CheckboxLabeled("RSS.LimitResearch".Translate(), ref LimitResearch,
            "RSS.LimitResearch.Desc".Translate());
        listingStandard.CheckboxLabeled("RSS.LimitFactions".Translate(), ref LimitFactions,
            "RSS.LimitFactions.Desc".Translate());
        listingStandard.CheckboxLabeled("RSS.LimitPawnKinds".Translate(), ref LimitPawns,
            "RSS.LimitPawnKinds.Desc".Translate());
        listingStandard.Gap(gap);
        listingStandard.CheckboxLabeled("RSS.LogRemovals".Translate(), ref LogRemovals,
            "RSS.LogRemovals.Desc".Translate());
        listingStandard.Gap(gap);
        listingStandard.Label("RSS.RestartInfo".Translate());
        if (ModStuff.CurrentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("RSS.CurrentModVersion".Translate(ModStuff.CurrentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref LimitItems, "LimitItems", true);
        Scribe_Values.Look(ref LimitResearch, "LimitResearch", true);
        Scribe_Values.Look(ref LimitFactions, "LimitFactions", true);
        Scribe_Values.Look(ref LimitPawns, "LimitPawns", true);
        Scribe_Values.Look(ref LogRemovals, "LogRemovals");
    }
}