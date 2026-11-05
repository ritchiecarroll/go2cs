// ilcompare: compares every method body of every assembly in two build-output trees (same relative
// paths). A body is IDENTICAL when its bytes match; EQUIVALENT when, after local-slot renumbering by
// first use and branch targets read as instruction indices, the instruction streams, the multiset of
// local types and the exception regions all match; otherwise DIFFERENT, and it is listed by name.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

var (baseDir, newDir) = (args[0], args[1]);
var ops = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (OpCode)f.GetValue(null)!)
    .ToDictionary(o => (ushort)o.Value);
int asmCount = 0, identical = 0, equivalent = 0, different = 0, missing = 0;
var diffs = new List<string>();
foreach (var bf in Directory.EnumerateFiles(baseDir, "*.dll", SearchOption.AllDirectories))
{
    var rel = Path.GetRelativePath(baseDir, bf);
    var nf = Path.Combine(newDir, rel);
    if (!File.Exists(nf)) { missing++; continue; }
    asmCount++;
    using var bp = new PEReader(File.OpenRead(bf));
    using var np = new PEReader(File.OpenRead(nf));
    var bm = bp.GetMetadataReader();
    var nm = np.GetMetadataReader();
    var bmeth = bm.MethodDefinitions.ToList();
    var nmeth = nm.MethodDefinitions.ToList();
    if (bmeth.Count != nmeth.Count) { different++; diffs.Add($"{rel}: method count {bmeth.Count} vs {nmeth.Count}"); continue; }
    for (int i = 0; i < bmeth.Count; i++)
    {
        var bd = bm.GetMethodDefinition(bmeth[i]);
        var nd = nm.GetMethodDefinition(nmeth[i]);
        string name = Name(bm, bd);
        if (name != Name(nm, nd)) { different++; diffs.Add($"{rel}: method {i} {name} vs {Name(nm, nd)}"); continue; }
        if (bd.RelativeVirtualAddress == 0 && nd.RelativeVirtualAddress == 0) continue;
        var bb = bp.GetMethodBody(bd.RelativeVirtualAddress);
        var nb = np.GetMethodBody(nd.RelativeVirtualAddress);
        var bil = bb.GetILBytes()!; var nil = nb.GetILBytes()!;
        bool sameLocals = Locals(bm, bb).SequenceEqual(Locals(nm, nb));
        if (bil.AsSpan().SequenceEqual(nil) && sameLocals && Regions(bb).SequenceEqual(Regions(nb))) { identical++; continue; }
        var (bs, boff) = Canon(bil, bm); var (ns, noff) = Canon(nil, nm);
        bool eq = bs.SequenceEqual(ns)
            && Locals(bm, bb).OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(Locals(nm, nb).OrderBy(x => x, StringComparer.Ordinal))
            && Regions(bb, boff).SequenceEqual(Regions(nb, noff));
        if (!eq && args.Length > 2 && (rel + ":" + name).Contains(args[2]))
        {
            Console.WriteLine($"--- {rel}: {name}  base {bs.Count} instrs, new {ns.Count}");
            int k = 0; while (k < Math.Min(bs.Count, ns.Count) && bs[k] == ns[k]) k++;
            for (int q = 0; q < Math.Min(Math.Max(bs.Count, ns.Count), k + 80); q++)
                Console.WriteLine($"   {q}: {(q < bs.Count ? bs[q] : "")}  ||  {(q < ns.Count ? ns[q] : "")}");
            Console.WriteLine("   locals base: " + string.Join(",", Locals(bm, bb)));
            Console.WriteLine("   locals new:  " + string.Join(",", Locals(nm, nb)));
        }
        if (eq) equivalent++;
        else { different++; diffs.Add($"{rel}: {name}"); }
    }
}
Console.WriteLine($"assemblies {asmCount} (missing in new: {missing}); bodies identical {identical}, equivalent {equivalent}, different {different}");
foreach (var d in diffs) Console.WriteLine("DIFFERENT " + d);

string Name(MetadataReader m, MethodDefinition d)
{
    var t = m.GetTypeDefinition(d.GetDeclaringType());
    return $"{m.GetString(t.Namespace)}.{m.GetString(t.Name)}::{m.GetString(d.Name)}";
}

static IEnumerable<string> Locals(MetadataReader m, MethodBodyBlock b)
{
    if (b.LocalSignature.IsNil) return Array.Empty<string>();
    var sig = m.GetStandaloneSignature(b.LocalSignature);
    var blob = m.GetBlobReader(sig.Signature);
    return sig.DecodeLocalSignature(new Prov(), null);
}

IEnumerable<string> Regions(MethodBodyBlock b, Dictionary<int, int>? off = null)
{
    int map(int o) => off != null && off.TryGetValue(o, out var i) ? i : o;
    return b.ExceptionRegions.Select(r => $"{r.Kind}:{map(r.TryOffset)}+{map(r.TryOffset + r.TryLength)}:{map(r.HandlerOffset)}+{map(r.HandlerOffset + r.HandlerLength)}:{r.CatchType.GetHashCode()}");
}

string Tok(MetadataReader m, int token)
{
    if ((token >> 24) == 0x70) return "str:" + m.GetUserString(MetadataTokens.UserStringHandle(token & 0xFFFFFF));
    try
    {
        var h = MetadataTokens.EntityHandle(token);
        switch (h.Kind)
        {
            case HandleKind.MethodDefinition: { var d = m.GetMethodDefinition((MethodDefinitionHandle)h); return "md:" + TypeName(m, d.GetDeclaringType()) + "::" + m.GetString(d.Name) + "/" + MSig(d.DecodeSignature(new Prov(), null)); }
            case HandleKind.FieldDefinition: { var d = m.GetFieldDefinition((FieldDefinitionHandle)h); return "fd:" + TypeName(m, d.GetDeclaringType()) + "::" + m.GetString(d.Name); }
            case HandleKind.TypeDefinition: return "td:" + TypeName(m, (TypeDefinitionHandle)h);
            case HandleKind.TypeReference: { var r = m.GetTypeReference((TypeReferenceHandle)h); return "tr:" + m.GetString(r.Namespace) + "." + m.GetString(r.Name); }
            case HandleKind.TypeSpecification: return "ts:" + m.GetTypeSpecification((TypeSpecificationHandle)h).DecodeSignature(new Prov(), null);
            case HandleKind.MemberReference: { var r = m.GetMemberReference((MemberReferenceHandle)h); return "mr:" + Parent(m, r.Parent) + "::" + m.GetString(r.Name) + "/" + (r.GetKind() == MemberReferenceKind.Method ? MSig(r.DecodeMethodSignature(new Prov(), null)) : r.DecodeFieldSignature(new Prov(), null)); }
            case HandleKind.MethodSpecification: { var s2 = m.GetMethodSpecification((MethodSpecificationHandle)h); return "ms:" + Tok(m, MetadataTokens.GetToken(s2.Method)) + "<" + string.Join(",", s2.DecodeSignature(new Prov(), null)) + ">"; }
            case HandleKind.StandaloneSignature: return "sig:" + Convert.ToHexString(m.GetBlobBytes(m.GetStandaloneSignature((StandaloneSignatureHandle)h).Signature));
        }
    }
    catch { }
    return "tok:" + token.ToString("X8");
}

static string MSig(MethodSignature<string> g) => g.ReturnType + "(" + string.Join(",", g.ParameterTypes) + ")" + g.GenericParameterCount;

string TypeName(MetadataReader m, TypeDefinitionHandle h)
{
    var t = m.GetTypeDefinition(h);
    var outer = t.GetDeclaringType();
    return (outer.IsNil ? m.GetString(t.Namespace) : TypeName(m, outer) + "/") + "." + m.GetString(t.Name);
}

string Parent(MetadataReader m, EntityHandle p) => p.Kind switch
{
    HandleKind.TypeDefinition => TypeName(m, (TypeDefinitionHandle)p),
    HandleKind.TypeReference => m.GetString(m.GetTypeReference((TypeReferenceHandle)p).Namespace) + "." + m.GetString(m.GetTypeReference((TypeReferenceHandle)p).Name),
    HandleKind.TypeSpecification => m.GetTypeSpecification((TypeSpecificationHandle)p).DecodeSignature(new Prov(), null),
    _ => p.Kind.ToString()
};

// Canonical instruction stream: locals renumbered by first use, branch targets as instruction indices.
(List<string>, Dictionary<int, int>) Canon(byte[] il, MetadataReader md)
{
    var raw = new List<(int off, OpCode op, long operand, int[] targets)>();
    for (int i = 0; i < il.Length;)
    {
        int start = i;
        ushort v = il[i++];
        if (v == 0xFE) v = (ushort)(0xFE00 | il[i++]);
        var op = ops[v];
        long operand = 0; int[] tg = Array.Empty<int>();
        switch (op.OperandType)
        {
            case OperandType.InlineNone: break;
            case OperandType.ShortInlineBrTarget: { int d = (sbyte)il[i++]; tg = new[] { i + d }; break; }
            case OperandType.InlineBrTarget: { int d = BitConverter.ToInt32(il, i); i += 4; tg = new[] { i + d }; break; }
            case OperandType.ShortInlineI: case OperandType.ShortInlineVar: operand = il[i++]; break;
            case OperandType.InlineVar: operand = BitConverter.ToUInt16(il, i); i += 2; break;
            case OperandType.InlineI8: case OperandType.InlineR: operand = BitConverter.ToInt64(il, i); i += 8; break;
            case OperandType.InlineSwitch:
            {
                int n = BitConverter.ToInt32(il, i); i += 4; int baseOff = i + 4 * n;
                tg = new int[n];
                for (int k = 0; k < n; k++) { tg[k] = baseOff + BitConverter.ToInt32(il, i); i += 4; }
                break;
            }
            default: operand = BitConverter.ToInt32(il, i); i += 4; break;
        }
        raw.Add((start, op, operand, tg));
    }
    // an instruction's index counts only the instructions that are not `nop` (a Debug brace), so a target that
    // lands on a nop reads as the next real instruction
    var idx = new Dictionary<int, int>();
    int real = 0;
    for (int k = 0; k < raw.Count; k++) { idx[raw[k].off] = real; if (raw[k].op.Name != "nop") real++; }
    idx[il.Length] = real;
    var slot = new Dictionary<long, int>();
    var outp = new List<string>();
    foreach (var (off, op, operand, tg) in raw)
    {
        string n = op.Name!;
        long? local = n switch
        {
            "ldloc.0" or "stloc.0" => 0, "ldloc.1" or "stloc.1" => 1, "ldloc.2" or "stloc.2" => 2, "ldloc.3" or "stloc.3" => 3,
            "ldloc.s" or "stloc.s" or "ldloca.s" or "ldloc" or "stloc" or "ldloca" => operand,
            _ => null
        };
        if (local is long l)
        {
            if (!slot.TryGetValue(l, out int c)) slot[l] = c = slot.Count;
            string kind = n.StartsWith("ldloca") ? "ldloca" : n.StartsWith("ldloc") ? "ldloc" : "stloc";
            outp.Add($"{kind} L{c}");
        }
        else if (tg.Length > 0)
        {
            string bn = n.EndsWith(".s") ? n[..^2] : n;
            outp.Add($"{bn} " + string.Join(",", tg.Select(t => idx.TryGetValue(t, out var q) ? q : -1)));
        }
        else if (op.OperandType is OperandType.InlineMethod or OperandType.InlineField or OperandType.InlineType
                 or OperandType.InlineTok or OperandType.InlineString or OperandType.InlineSig)
            outp.Add($"{n} {Tok(md, (int)operand)}");
        else if (n == "nop") { }
        else outp.Add($"{n} {operand}");
    }
    return (outp, idx);
}

sealed class Prov : ISignatureTypeProvider<string, object?>
{
    public string GetArrayType(string e, ArrayShape s) => e + "[" + s.Rank + "]";
    public string GetByReferenceType(string e) => e + "&";
    public string GetFunctionPointerType(MethodSignature<string> s) => "fnptr";
    public string GetGenericInstantiation(string g, ImmutableArray<string> a) => g + "<" + string.Join(",", a) + ">";
    public string GetGenericMethodParameter(object? c, int i) => "!!" + i;
    public string GetGenericTypeParameter(object? c, int i) => "!" + i;
    public string GetModifiedType(string m, string u, bool r) => u;
    public string GetPinnedType(string e) => e + " pinned";
    public string GetPointerType(string e) => e + "*";
    public string GetPrimitiveType(PrimitiveTypeCode t) => t.ToString();
    public string GetSZArrayType(string e) => e + "[]";
    public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle h, byte k) { var t = r.GetTypeDefinition(h); return r.GetString(t.Namespace) + "." + r.GetString(t.Name); }
    public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle h, byte k) { var t = r.GetTypeReference(h); return r.GetString(t.Namespace) + "." + r.GetString(t.Name); }
    public string GetTypeFromSpecification(MetadataReader r, object? c, TypeSpecificationHandle h, byte k) => r.GetTypeSpecification(h).DecodeSignature(this, c);
}
