using System.Reflection;
using HarmonyLib;
using Verse;

namespace RemoveSpacerStuff;

[StaticConstructorOnStartup]
public static class RemoveModernStuffHarmony
{
    static RemoveModernStuffHarmony()
    {
        new Harmony("Mlie.RemoveSpacerStuff").PatchAll(Assembly.GetExecutingAssembly());
    }
}