using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

// Dumps the public API surface of Assembly-CSharp to a text file so the plugin
// can be written against real type / member names instead of guesses.
internal static class Dump
{
    private static readonly HashSet<string> Interesting =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "Player", "PlayerStats", "PlayerStats_Profile", "PlayerInventory",
            "PlayerEquipment", "PlayerAppearance_Profile", "PlayerAppearanceStruct",
            "PlayerProfileData", "ItemObject", "ItemData", "ItemDropEntity",
            "ItemStorageManager", "ShopkeepManager", "Time_Manager", "TimeManager",
            "GameManager", "GameData", "ChatBehaviour", "SettingsManager",
            "Enemy", "Enemy_", "NetworkSceneManager", "SpawnManager",
        };

    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: Dump <assembly> <outfile> [ilType]");
            return 2;
        }

        string assemblyPath = args[0];
        string outPath = args[1];
        string ilType = args.Length > 2 ? args[2] : null;

        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
        resolver.AddSearchDirectory(
            Path.Combine(Path.GetDirectoryName(assemblyPath), "..", "..", "BepInEx", "core"));

        using (var asm = AssemblyDefinition.ReadAssembly(assemblyPath,
                   new ReaderParameters { AssemblyResolver = resolver, ReadSymbols = false }))
        using (var writer = new StreamWriter(outPath, false, new UTF8Encoding(false)))
        {
            writer.WriteLine("# assembly: " + asm.Name.FullName);
            writer.WriteLine("# types: " + asm.MainModule.Types.Count);

            foreach (var type in asm.MainModule.Types.OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                DumpType(writer, type, 0, ilType);
            }
        }

        Console.WriteLine("ok -> " + outPath);
        return 0;
    }

    private static void DumpType(TextWriter w, TypeDefinition type, int depth, string ilType)
    {
        string pad = new string(' ', depth * 2);
        w.WriteLine();
        w.WriteLine(pad + "TYPE " + type.FullName
                    + (type.BaseType != null ? " : " + type.BaseType.FullName : "")
                    + " [" + type.Attributes + "]");

        foreach (var iface in type.Interfaces)
        {
            w.WriteLine(pad + "  IFACE " + iface.InterfaceType.FullName);
        }

        foreach (var field in type.Fields)
        {
            w.WriteLine(pad + "  FIELD " + field.Attributes + " " + field.FieldType.FullName + " " + field.Name
                        + (field.HasConstant ? " = " + Format(field.Constant) : ""));
        }

        foreach (var prop in type.Properties)
        {
            w.WriteLine(pad + "  PROP " + prop.PropertyType.FullName + " " + prop.Name);
        }

        foreach (var method in type.Methods)
        {
            w.WriteLine(pad + "  METHOD " + method.Attributes + " " + method.ReturnType.FullName + " "
                        + method.Name + "(" + string.Join(", ", method.Parameters.Select(p => p.ParameterType.FullName + " " + p.Name).ToArray()) + ")");

            bool wantIl = ilType != null &&
                          (type.FullName == ilType || type.FullName.EndsWith("." + ilType, StringComparison.Ordinal));
            if (wantIl && method.HasBody)
            {
                foreach (var ins in method.Body.Instructions)
                {
                    w.WriteLine(pad + "    IL " + ins);
                }
            }
        }

        foreach (var nested in type.NestedTypes)
        {
            DumpType(w, nested, depth + 1, ilType);
        }
    }

    private static string Format(object value)
    {
        if (value == null) return "null";
        if (value is string) return "\"" + value + "\"";
        return value.ToString();
    }
}
