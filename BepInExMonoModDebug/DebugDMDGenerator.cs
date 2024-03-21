using System;
using System.IO;
using System.Linq;
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
            BepInExMonoModDebugPatcher.Logger.LogError("Failed to generate debug file: " + ex);

            ToggleDump(false);
            return DMDGenerator<DMDEmitDynamicMethodGenerator>.Generate(dmd, context);
        }

        try
        {
            var methodName = GetFullMethodName(dmd.OriginalMethod);

            var files = Directory.GetFiles(BepInExMonoModDebugPatcher.DumpsDirectory);

            var fileToDelete = files.FirstOrDefault(f => Path.GetFileName(f).StartsWith(methodName, StringComparison.OrdinalIgnoreCase));
            if (fileToDelete != null)
            {
                File.Delete(fileToDelete);
            }

            // get the last file in dumps
            var lastFileWritten = files
                .OrderByDescending(File.GetLastWriteTime)
                .First();

            File.Move(lastFileWritten, Path.Combine(Path.GetDirectoryName(lastFileWritten), methodName + ".dll"));
        }
        catch (Exception ex) // even if exception occurred (no permission to write/read) we really want to continue the patching
        {
            BepInExMonoModDebugPatcher.Logger.LogError("Failed to create dump file: " + ex);
        }

        return generatedMethod;
    }

    private static string GetFullMethodName(MethodBase @base)
    {
        var sb = new StringBuilder();

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

        for (var i = sb.Length - 1; i >= 0; i--)
        {
            var c = sb[i];
            if (Array.IndexOf(Path.GetInvalidFileNameChars(), c) != -1)
            {
                sb.Remove(i, 1);
            }
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
