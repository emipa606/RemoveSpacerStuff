using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RemoveSpacerStuff
{
    public class ModStuff : Mod
    {
        public ModStuff(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "Remove Spacer Stuff";
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }

        public static Settings Settings;

    }
}