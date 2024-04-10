using System;
using System.IO;
using System.Reflection;
using System.Text;
using MonoMod.Utils;

namespace BepInExMonoModDebug;
internal sealed class DebugDMDGenerator : DMDGenerator<DebugDMDGenerator>
{
    /// <summary>
    /// Should dump patch.
    /// </summary>
    /// <remarks>
    /// Under the method generation MonoMod calls it atleast twice (for IL copy and generating the method itself).
    /// </remarks>
    internal static bool ShouldDump { get; set; }

    protected override MethodInfo _Generate(DynamicMethodDefinition dmd, object context)
    {
        if (!ShouldDump || dmd.OriginalMethod == null)
        {
            ToggleDump(false);
            return DMDGenerator<DMDCecilGenerator>.Generate(dmd, context);
        }

        ShouldDump = false;

        ToggleDump(true);
        MethodInfo generatedMethod;
        try
        {
            generatedMethod = DMDGenerator<DMDCecilGenerator>.Generate(dmd, context);
        }
        catch (Exception ex)
        {
            BepInExMonoModDebugPatcher.Logger.LogWarning("Failed to generate debug file\n" + ex);

            ToggleDump(false);
            return DMDGenerator<DMDEmitDynamicMethodGenerator>.Generate(dmd, context);
        }

        DumpMethodToFile(dmd);

        return generatedMethod;
    }

    private static void DumpMethodToFile(DynamicMethodDefinition dmd)
    {
        if (!BepInExMonoModDebugPatcher.Configuration.ShouldSaveDumpFile.Value)
        {
            return;
        }

        try
        {
            var methodName = GetFullMethodName(dmd.OriginalMethod) + ".dll";
            var dumpName = dmd.GetDumpName("Cecil") + ".dll";

            var fileToDelete = Path.Combine(BepInExMonoModDebugPatcher.DumpsDirectory, methodName);
            if (fileToDelete != null)
            {
                File.Delete(fileToDelete);
            }

            var dumpFile = Path.Combine(BepInExMonoModDebugPatcher.DumpsDirectory, dumpName);
            File.Move(dumpFile, fileToDelete);
        }
        catch (Exception ex) // even if exception occurred (no permission to write/read) we really want to continue the patching
        {
            BepInExMonoModDebugPatcher.Logger.LogError("Failed to create dump file: " + ex);
        }
    }

    private static string GetFullMethodName(MethodBase @base)
    {
        var sb = new StringBuilder(256);

        var declaringType = @base.DeclaringType;
        if (declaringType != null)
        {
            var @namespace = declaringType.Namespace;
            if (!string.IsNullOrEmpty(@namespace))
            {
                sb.Append(@namespace).Append('.');
            }

            sb.Append(declaringType.Name);
        }

        if (sb.Length > 0)
        {
            sb.Append('_');
        }

        sb.Append(@base.Name);

        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            sb.Replace(invalidChar, '_');
        }

        return sb.ToString();
    }

    private void ToggleDump(bool toggle)
    {
        if (!BepInExMonoModDebugPatcher.Configuration.ShouldSaveDumpFile.Value)
        {
            return;
        }

        Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", toggle ? BepInExMonoModDebugPatcher.DumpsDirectory : null!);
    }
}
