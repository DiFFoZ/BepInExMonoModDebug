using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace BepInExMonoModDebug;
internal static class CecilEmitter
{
    public static void Dump(MethodDefinition md, string dumpPath, MethodBase? original = null)
    {
        var name = SanitizeTypeName(md.GetID(withType: false, simple: true));
        var originalName = (original?.Name ?? md.Name).Replace('.', '_');

        using var module = ModuleDefinition.CreateModule(name,
            new ModuleParameters
            {
                Kind = ModuleKind.Dll,
                ReflectionImporterProvider = MMReflectionImporter.ProviderNoDefault
            });

        var td = new TypeDefinition("", originalName,
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class)
        {
            BaseType = module.TypeSystem.Object
        };

        module.Types.Add(td);

        MethodDefinition? clone = null;

        Relinker relinker = (mtp, ctx) =>
        {
            if (mtp == md)
                return clone!;
            if (mtp is MethodReference mr)
            {
                if (mr.FullName == md.FullName
                 && mr.DeclaringType.FullName == md.DeclaringType.FullName
                 && mr.DeclaringType.Scope.Name == md.DeclaringType.Scope.Name)
                    return clone!;
            }
            return module.ImportReference(mtp);
        };

        clone =
            new MethodDefinition(original?.Name ?? "_" + md.Name.Replace(".", "_"), md.Attributes, module.TypeSystem.Void)
            {
                MethodReturnType = md.MethodReturnType,
                Attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
                ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed,
                DeclaringType = td,
                HasThis = false,
                ReturnType = md.ReturnType.Relink(relinker, clone)
            };
        td.Methods.Add(clone);

        foreach (var param in md.Parameters)
            clone.Parameters.Add(param.Clone().Relink(relinker, clone));

        var body = clone.Body = md.Body.Clone(clone);

        foreach (var variable in clone.Body.Variables)
            variable.VariableType = variable.VariableType.Relink(relinker, clone);

        foreach (var handler in clone.Body.ExceptionHandlers.Where(handler => handler.CatchType != null))
            handler.CatchType = handler.CatchType.Relink(relinker, clone);

        foreach (var instr in body.Instructions)
        {
            var operand = instr.Operand;
            operand = operand switch
            {
                ParameterDefinition param => clone.Parameters[param.Index],
                ILLabel label => label.Target,
                IMetadataTokenProvider mtp => mtp.Relink(relinker, clone),
                _ => operand
            };

            instr.Operand = operand;
        }

        if (md.HasThis)
        {
            TypeReference type = md.DeclaringType;
            if (type.IsValueType)
                type = new ByReferenceType(type);
            clone.Parameters.Insert(0,
                new ParameterDefinition("<>_this", ParameterAttributes.None, type.Relink(relinker, clone)));
        }

        if (!Directory.Exists(dumpPath))
        {
            Directory.CreateDirectory(dumpPath);
        }

        using var stream = new FileStream(Path.Combine(dumpPath, $"{module.Name}.dll"), FileMode.Create, FileAccess.Write);
        module.Write(stream);
    }

    private static string SanitizeTypeName(string typeName)
    {
        return new StringBuilder(typeName)
            .Replace(":", "_")
            .Replace(" ", "_")
            .Replace("<", "{")
            .Replace(">", "}")
            .ToString();
    }
}
