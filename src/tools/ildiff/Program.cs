// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ildiff: compares the METHOD BODIES of two builds of the same assemblies and names every method whose IL
// differs. It is the claim instrument for an emission change declared "IL-identical": such a change must read
// 0 differing methods, and a change that is not must read differences only at the methods it names.
//
// Usage: ildiff <baseDir> <seatDir> [--list N]
//   Every *.dll under baseDir is paired with the same relative path under seatDir.
//
// What counts as the IL of a method: its instructions with every token operand RESOLVED to the name it denotes
// (type, member, signature, user string), its local variable types, its exception regions, and its max stack.
// Metadata row numbers are never compared, so a change that only reorders rows reads identical. Build both sides
// optimized (-p:Optimize=true): a debug build's nops follow the source's line structure, which a pure layout change
// moves without changing what runs.

using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

namespace go2cs.ildiff;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 2 || args.Length % 2 != 0)
        {
            Console.Error.WriteLine("usage: ildiff <baseDir> <seatDir> [--list N] [--show <method substring>] [--roots <dir>;<dir>]");
            return 2;
        }

        string baseDir = Path.GetFullPath(args[0]), seatDir = Path.GetFullPath(args[1]);
        int list = 200;

        // --show: print both normalized bodies of each differing method whose key contains the substring.
        string? show = null;

        for (int i = 2; i < args.Length; i += 2)
        {
            switch (args[i])
            {
                case "--list":
                    list = int.Parse(args[i + 1]);
                    break;
                case "--show":
                    show = args[i + 1];
                    break;
                case "--roots":
                    // The source trees the two sides were built from. A string operand that starts with one (a
                    // [CallerFilePath] the compiler embedded) reads with the root replaced, so the comparison does not
                    // depend on where a tree lives.
                    Names.Roots = args[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(root => root.TrimEnd('\\', '/') + Path.DirectorySeparatorChar).ToArray();
                    break;
                default:
                    Console.Error.WriteLine($"unknown option {args[i]}");
                    return 2;
            }
        }

        int assemblies = 0, methods = 0, differing = 0, onlyBase = 0, onlySeat = 0, missingSeat = 0;
        List<string> report = [];

        foreach (string basePath in Directory.EnumerateFiles(baseDir, "*.dll", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            string relative = Path.GetRelativePath(baseDir, basePath);
            string seatPath = Path.Combine(seatDir, relative);

            if (!File.Exists(seatPath))
            {
                missingSeat++;
                report.Add($"MISSING-IN-SEAT {relative}");
                continue;
            }

            Dictionary<string, string>? left = Bodies(basePath), right = Bodies(seatPath);

            if (left is null || right is null)
                continue; // not a managed assembly with metadata

            assemblies++;

            foreach ((string method, string body) in left)
            {
                if (!right.TryGetValue(method, out string? other))
                {
                    onlyBase++;
                    report.Add($"ONLY-IN-BASE {relative} {method}");
                    continue;
                }

                methods++;

                if (body != other)
                {
                    differing++;
                    report.Add($"DIFF {relative} {method}");

                    if (show is not null && method.Contains(show, StringComparison.Ordinal))
                        Console.WriteLine($"--- base {method}\n{body}--- seat {method}\n{other}");
                }
            }

            foreach (string method in right.Keys.Where(method => !left.ContainsKey(method)))
            {
                onlySeat++;
                report.Add($"ONLY-IN-SEAT {relative} {method}");
            }
        }

        foreach (string line in report.Take(list))
            Console.WriteLine(line);

        if (report.Count > list)
            Console.WriteLine($"... {report.Count - list} more");

        Console.WriteLine($"ILDIFF assemblies={assemblies} methods={methods} differing={differing} only-in-base={onlyBase} only-in-seat={onlySeat} missing-in-seat={missingSeat}");

        return differing + onlyBase + onlySeat + missingSeat == 0 ? 0 : 1;
    }

    // Every method with a body, keyed by its declaring type, name and decoded signature, mapped to its normalized
    // IL text. Null for a file that is not a managed assembly.
    private static Dictionary<string, string>? Bodies(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using PEReader pe = new(stream);

        if (!pe.HasMetadata)
            return null;

        MetadataReader md = pe.GetMetadataReader();
        Names names = new(md);
        Dictionary<string, string> bodies = new(StringComparer.Ordinal);

        foreach (TypeDefinitionHandle typeHandle in md.TypeDefinitions)
        {
            TypeDefinition type = md.GetTypeDefinition(typeHandle);
            string typeName = names.TypeDef(typeHandle);

            foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
            {
                MethodDefinition method = md.GetMethodDefinition(methodHandle);

                if (method.RelativeVirtualAddress == 0)
                    continue;

                string key = $"{typeName}::{md.GetString(method.Name)}{names.MethodSignature(method.Signature)}";
                MethodBodyBlock body = pe.GetMethodBody(method.RelativeVirtualAddress);

                // Two methods of one key (a compiler-generated name reused with a different arity) keep both.
                for (int suffix = 2; bodies.ContainsKey(key); suffix++)
                    key = $"{key}#{suffix}";

                bodies[key] = Normalize(body, names);
            }
        }

        return bodies;
    }

    private static readonly Dictionary<short, OpCode> s_opCodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opCode => opCode.Value);

    private static string Normalize(MethodBodyBlock body, Names names)
    {
        StringBuilder text = new();
        text.Append("maxstack ").Append(body.MaxStack).Append(" init ").Append(body.LocalVariablesInitialized).Append('\n');

        if (!body.LocalSignature.IsNil)
            text.Append("locals ").Append(names.Locals(body.LocalSignature)).Append('\n');

        foreach (ExceptionRegion region in body.ExceptionRegions)
        {
            text.Append($"region {region.Kind} try {region.TryOffset}+{region.TryLength} handler {region.HandlerOffset}+{region.HandlerLength}");

            if (region.Kind == ExceptionRegionKind.Catch)
                text.Append(" catch ").Append(names.Entity(region.CatchType));
            else if (region.Kind == ExceptionRegionKind.Filter)
                text.Append(" filter ").Append(region.FilterOffset);

            text.Append('\n');
        }

        BlobReader il = body.GetILReader();

        while (il.RemainingBytes > 0)
        {
            int offset = il.Offset;
            short value = il.ReadByte();

            if (value == 0xFE)
                value = unchecked((short)(0xFE00 | il.ReadByte()));

            if (!s_opCodes.TryGetValue(value, out OpCode opCode))
            {
                text.Append($"IL_{offset:x4}: <unknown 0x{value:x}>\n");
                break;
            }

            text.Append($"IL_{offset:x4}: {opCode.Name}");

            switch (opCode.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineBrTarget:
                    text.Append(' ').Append(il.ReadSByte());
                    break;
                case OperandType.ShortInlineI:
                    text.Append(' ').Append(opCode == OpCodes.Ldc_I4_S ? il.ReadSByte() : il.ReadByte());
                    break;
                case OperandType.ShortInlineVar:
                    text.Append(' ').Append(il.ReadByte());
                    break;
                case OperandType.InlineVar:
                    text.Append(' ').Append(il.ReadUInt16());
                    break;
                case OperandType.InlineBrTarget:
                case OperandType.InlineI:
                    text.Append(' ').Append(il.ReadInt32());
                    break;
                case OperandType.InlineI8:
                    text.Append(' ').Append(il.ReadInt64());
                    break;
                case OperandType.ShortInlineR:
                    text.Append(' ').Append(BitConverter.SingleToInt32Bits(il.ReadSingle()));
                    break;
                case OperandType.InlineR:
                    text.Append(' ').Append(BitConverter.DoubleToInt64Bits(il.ReadDouble()));
                    break;
                case OperandType.InlineString:
                    text.Append(" \"").Append(names.UserString(il.ReadInt32())).Append('"');
                    break;
                case OperandType.InlineSwitch:
                    int count = il.ReadInt32();
                    text.Append(' ').Append(count);

                    for (int i = 0; i < count; i++)
                        text.Append(' ').Append(il.ReadInt32());
                    break;
                case OperandType.InlineSig:
                    text.Append(' ').Append(names.StandaloneSignature(il.ReadInt32()));
                    break;
                default:
                    // InlineField, InlineMethod, InlineTok, InlineType: a token naming an entity.
                    text.Append(' ').Append(names.Token(il.ReadInt32()));
                    break;
            }

            text.Append('\n');
        }

        return text.ToString();
    }
}

// Names of metadata entities, independent of their row numbers.
internal sealed class Names(MetadataReader md) : ISignatureTypeProvider<string, object?>
{
    private readonly MetadataReader m_md = md;

    public string TypeDef(TypeDefinitionHandle handle)
    {
        TypeDefinition type = m_md.GetTypeDefinition(handle);
        string name = $"{m_md.GetString(type.Namespace)}.{m_md.GetString(type.Name)}";
        TypeDefinitionHandle declaring = type.GetDeclaringType();

        return declaring.IsNil ? name : $"{TypeDef(declaring)}/{m_md.GetString(type.Name)}";
    }

    public string TypeRef(TypeReferenceHandle handle)
    {
        TypeReference type = m_md.GetTypeReference(handle);
        string name = $"{m_md.GetString(type.Namespace)}.{m_md.GetString(type.Name)}";

        return type.ResolutionScope.Kind == HandleKind.TypeReference ? $"{TypeRef((TypeReferenceHandle)type.ResolutionScope)}/{m_md.GetString(type.Name)}" : name;
    }

    public string MethodSignature(BlobHandle signature)
    {
        BlobReader reader = m_md.GetBlobReader(signature);
        MethodSignature<string> decoded = new SignatureDecoder<string, object?>(this, m_md, null).DecodeMethodSignature(ref reader);
        string generic = decoded.GenericParameterCount > 0 ? $"`{decoded.GenericParameterCount}" : "";

        return $"{generic}({string.Join(",", decoded.ParameterTypes)}):{decoded.ReturnType}";
    }

    public string Locals(StandaloneSignatureHandle handle)
    {
        StandaloneSignature signature = m_md.GetStandaloneSignature(handle);

        return string.Join(",", signature.DecodeLocalSignature(this, null));
    }

    public string StandaloneSignature(int token)
    {
        StandaloneSignature signature = m_md.GetStandaloneSignature((StandaloneSignatureHandle)MetadataTokens.EntityHandle(token));
        MethodSignature<string> decoded = signature.DecodeMethodSignature(this, null);

        return $"sig({string.Join(",", decoded.ParameterTypes)}):{decoded.ReturnType}";
    }

    // The source roots --roots names; see Program.Main.
    public static string[] Roots = [];

    public string UserString(int token)
    {
        string value = m_md.GetUserString((UserStringHandle)MetadataTokens.Handle(token));

        foreach (string root in Roots)
        {
            if (value.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return "<root>" + Path.DirectorySeparatorChar + value[root.Length..];
        }

        return value;
    }

    public string Token(int token) => Entity(MetadataTokens.EntityHandle(token));

    public string Entity(EntityHandle handle) => handle.Kind switch
    {
        HandleKind.TypeDefinition => TypeDef((TypeDefinitionHandle)handle),
        HandleKind.TypeReference => TypeRef((TypeReferenceHandle)handle),
        HandleKind.TypeSpecification => m_md.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(this, null),
        HandleKind.MethodDefinition => MethodDef((MethodDefinitionHandle)handle),
        HandleKind.FieldDefinition => FieldDef((FieldDefinitionHandle)handle),
        HandleKind.MemberReference => MemberRef((MemberReferenceHandle)handle),
        HandleKind.MethodSpecification => MethodSpec((MethodSpecificationHandle)handle),
        _ => handle.Kind.ToString()
    };

    private string MethodDef(MethodDefinitionHandle handle)
    {
        MethodDefinition method = m_md.GetMethodDefinition(handle);

        return $"{TypeDef(method.GetDeclaringType())}::{m_md.GetString(method.Name)}{MethodSignature(method.Signature)}";
    }

    private string FieldDef(FieldDefinitionHandle handle)
    {
        FieldDefinition field = m_md.GetFieldDefinition(handle);

        return $"{TypeDef(field.GetDeclaringType())}::{m_md.GetString(field.Name)}";
    }

    private string MemberRef(MemberReferenceHandle handle)
    {
        MemberReference member = m_md.GetMemberReference(handle);
        string parent = Entity(member.Parent);
        string name = m_md.GetString(member.Name);

        return member.GetKind() == MemberReferenceKind.Method
            ? $"{parent}::{name}{MethodSignature(member.Signature)}"
            : $"{parent}::{name}:{member.DecodeFieldSignature(this, null)}";
    }

    private string MethodSpec(MethodSpecificationHandle handle)
    {
        MethodSpecification spec = m_md.GetMethodSpecification(handle);

        return $"{Entity(spec.Method)}<{string.Join(",", spec.DecodeSignature(this, null))}>";
    }

    // ISignatureTypeProvider: render each signature element as text.
    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => TypeDef(handle);
    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => TypeRef(handle);
    public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
        reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    public string GetSZArrayType(string elementType) => elementType + "[]";
    public string GetArrayType(string elementType, ArrayShape shape) => $"{elementType}[{new string(',', shape.Rank - 1)}]";
    public string GetByReferenceType(string elementType) => elementType + "&";
    public string GetPointerType(string elementType) => elementType + "*";
    public string GetPinnedType(string elementType) => elementType + " pinned";
    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => $"{genericType}<{string.Join(",", typeArguments)}>";
    public string GetGenericTypeParameter(object? genericContext, int index) => $"!{index}";
    public string GetGenericMethodParameter(object? genericContext, int index) => $"!!{index}";
    public string GetFunctionPointerType(MethodSignature<string> signature) => $"fnptr({string.Join(",", signature.ParameterTypes)}):{signature.ReturnType}";
    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => $"{unmodifiedType} {(isRequired ? "modreq" : "modopt")}({modifier})";
}
