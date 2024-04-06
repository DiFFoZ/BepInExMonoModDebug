using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;

namespace BepInExMonoModDebug.Patches;
[HarmonyPatch(typeof(StackTrace))]
internal static class Patch_StackTrace
{
    [HarmonyPatch("AddFrames")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddFrameWithIL(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var method = AccessTools.Method(typeof(StackFrame), nameof(StackFrame.GetFileLineNumber));

        matcher.SearchForward(c => c.Calls(method))
            .ThrowIfInvalid("Failed to find call GetFileLineNumber")
            .Set(OpCodes.Call, new Func<StackFrame, string>(Patch_StackTraceUtilities.GetFileLineOrILOffset).Method);

        matcher.Advance(1);
        matcher.RemoveInstructions(1);

        return matcher.InstructionEnumeration();
    }
}
