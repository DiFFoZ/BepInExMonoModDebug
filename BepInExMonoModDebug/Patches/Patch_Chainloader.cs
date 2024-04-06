using System;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace BepInExMonoModDebug.Patches;
internal static class Patch_Chainloader
{
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Initialize))]
    [HarmonyPostfix]
    public static void ChainloaderInitialized()
    {
        try
        {
            BepInExMonoModDebugPatcher.Harmony.PatchAll(typeof(Patch_Chainloader).Assembly);
        }
        catch (Exception ex)
        {
            BepInExMonoModDebugPatcher.Logger.LogWarning("Failed to patch, probably can be ignored\n" + ex);
        }
    }
}
