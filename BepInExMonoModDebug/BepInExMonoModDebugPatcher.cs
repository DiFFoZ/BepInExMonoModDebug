using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInExMonoModDebug.Patches;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace BepInExMonoModDebug;

public static class BepInExMonoModDebugPatcher
{
    internal static Configuration Configuration { get; private set; } = null!;
    internal static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource(nameof(BepInExMonoModDebugPatcher));
    internal static string DumpsDirectory { get; } = Path.Combine(Paths.BepInExRootPath, "dumps");
    internal static Harmony Harmony { get; } = new(nameof(BepInExMonoModDebugPatcher));

    // Cannot be renamed, method name is important
    private static void Finish()
    {
        Configuration = new Configuration(new(Path.Combine(Paths.ConfigPath, nameof(BepInExMonoModDebugPatcher) + ".cfg"), false));

        if (!Directory.Exists(DumpsDirectory))
        {
            Directory.CreateDirectory(DumpsDirectory);
        }
        else
        {
            DeleteOldDumpFiles();
        }

        ILHook.OnDetour += OnDetour;
        Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", typeof(DebugDMDGenerator).FullName);

        Harmony.PatchAll(typeof(Patch_Chainloader));
    }

    private static bool OnDetour(ILHook hook, MethodBase @base, ILContext.Manipulator manipulator)
    {
        DebugDMDGenerator.ShouldDump = true;
        return true;
    }

    private static void DeleteOldDumpFiles()
    {
        foreach (var dumpFile in Directory.EnumerateFiles(DumpsDirectory))
        {
            try
            {
                File.Delete(dumpFile);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to delete a file {dumpFile}\n{ex}");
            }
        }
    }

    // cannot be removed, BepInEx checks it
    public static IEnumerable<string> TargetDLLs { get; } = [];

    // cannot be removed, BepInEx checks it
    // https://github.com/BepInEx/BepInEx/blob/v5-lts/BepInEx.Preloader/Patching/AssemblyPatcher.cs#L67
    public static void Patch(AssemblyDefinition _)
    {
    }
}
