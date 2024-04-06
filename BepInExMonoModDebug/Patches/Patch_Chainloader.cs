using BepInEx.Bootstrap;
using HarmonyLib;

namespace BepInExMonoModDebug.Patches;
internal static class Patch_Chainloader
{
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Initialize))]
    [HarmonyPostfix]
    public static void ChainloaderInitialized()
    {
        BepInExMonoModDebugPatcher.Harmony.PatchAll(typeof(Patch_Chainloader).Assembly);
    }
}
