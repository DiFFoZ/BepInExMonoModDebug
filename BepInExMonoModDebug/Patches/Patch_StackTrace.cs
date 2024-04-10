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

        // Workaround was made to fix lethallib, remove it when PR will be merged
        // https://github.com/EvaisaDev/LethalLib/pull/74

        matcher.SearchForward(c => c.Calls(method))
            .ThrowIfInvalid("Failed to find call GetFileLineNumber")
            .InsertAndAdvance(new CodeInstruction(OpCodes.Dup)) // copy stack frame
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Nop), new(OpCodes.Nop)) // workaround for lethallib (it removes 2 ins)
            .SetInstruction(new CodeInstruction(OpCodes.Pop)) // replaces box (deletes lethallib value)
            .Advance(1)
            .Insert(new CodeInstruction(OpCodes.Call, new Func<StackFrame, string>(Patch_StackTraceUtilities.GetFileLineOrILOffset).Method));

        return matcher.InstructionEnumeration();
    }
}
