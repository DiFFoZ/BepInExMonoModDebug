using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace BepInExMonoModDebug.Patches;

[HarmonyPatch]
internal class Patch_StackTraceUtilities
{
    [HarmonyPatch(typeof(StackTraceUtility), "ExtractFormattedStackTrace")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ShowILOffset(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var fileLineNumberMethod = AccessTools.Method(typeof(StackFrame), nameof(StackFrame.GetFileLineNumber));

        matcher.SearchForward(c => c.Calls(fileLineNumberMethod))
            .Set(OpCodes.Call, new Func<StackFrame, string>(GetFileLineOrILOffset).Method);

        matcher.Advance(1);
        matcher.RemoveInstructions(3);

        return matcher.InstructionEnumeration();
    }

    internal static string GetFileLineOrILOffset(StackFrame frame)
    {
        var line = frame.GetFileLineNumber();
        if (line > 0)
        {
            return line.ToString();
        }

        return "IL_" + frame.GetILOffset().ToString("x5");
    }
}
