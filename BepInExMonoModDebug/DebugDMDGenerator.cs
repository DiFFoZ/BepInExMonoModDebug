using System.Reflection;
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

    public override MethodInfo _Generate(DynamicMethodDefinition dmd, object context)
    {
        if (BepInExMonoModDebugPatcher.Configuration.ShouldSaveDumpFile.Value)
        {
            var generatedMethod = DMDEmitMethodBuilderGenerator.Generate(dmd, context);

            if (ShouldDump)
            {
                ShouldDump = false;
                DumpMethodToFile(dmd);
            }

            return generatedMethod;
        }

        ShouldDump = false;
        return DMDEmitMethodBuilderGenerator.Generate(dmd, context);
    }

    private static void DumpMethodToFile(DynamicMethodDefinition dmd)
    {
        if (dmd.OriginalMethod == null)
        {
            return;
        }

        // DMDEmitMethodBuilderGenerator dump doesn't work on Unity (causing hard crash)
        CecilEmitter.Dump(dmd.Definition, BepInExMonoModDebugPatcher.DumpsDirectory, dmd.OriginalMethod);
    }
}
