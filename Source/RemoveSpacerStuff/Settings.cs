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
        var gap = 8f;
        var listing_Standard = new Listing_Standard
        {
            ColumnWidth = canvas.width
        };
        listing_Standard.Begin(canvas);
        listing_Standard.Gap(gap);
        listing_Standard.CheckboxLabeled("RSS.LimitItems".Translate(), ref LimitItems,
            "RSS.LimitItems.Desc".Translate());
        listing_Standard.CheckboxLabeled("RSS.LimitResearch".Translate(), ref LimitResearch,
            "RSS.LimitResearch.Desc".Translate());
        listing_Standard.CheckboxLabeled("RSS.LimitFactions".Translate(), ref LimitFactions,
            "RSS.LimitFactions.Desc".Translate());
        listing_Standard.CheckboxLabeled("RSS.LimitPawnKinds".Translate(), ref LimitPawns,
            "RSS.LimitPawnKinds.Desc".Translate());
        listing_Standard.Gap(gap);
        listing_Standard.CheckboxLabeled("RSS.LogRemovals".Translate(), ref LogRemovals,
            "RSS.LogRemovals.Desc".Translate());
        listing_Standard.Gap(gap);
        listing_Standard.Label("RSS.RestartInfo".Translate());
        if (ModStuff.currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("RSS.CurrentModVersion".Translate(ModStuff.currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
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