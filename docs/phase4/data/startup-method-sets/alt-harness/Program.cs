// Measurement only (B3 phase 1): prices three sources of "the extension methods whose receiver is type E"
// over a converted program's assembly closure. Never shipped.
//   alt DIR MODE KEYS...   MODE = scan | meta | names | dump ; KEYS = "asm|type full name"
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

string dir = args[0], mode = args[1];
const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
AppDomain.CurrentDomain.AssemblyResolve += (_, e) => { string p = Path.Combine(dir, new AssemblyName(e.Name).Name + ".dll"); return File.Exists(p) ? Assembly.LoadFrom(p) : null; };
int split = Array.IndexOf(args, "--");
if (split > 0) Run(args[2..split], "warm-up ");
Run(split > 0 ? args[(split + 1)..] : args[2..], "");

void Run(string[] keys, string label)
{
List<(Assembly asm, Type type)> asked = [];
foreach (string k in keys) { string[] p = k.Split('|'); Assembly a = Assembly.LoadFrom(Path.Combine(dir, p[0] + ".dll")); asked.Add((a, a.GetType(p[1], true)!)); }
Assembly[] homes = [.. asked.Select(x => x.asm).Distinct()];
HashSet<Type> want = [.. asked.Select(x => x.type)];
List<(Assembly, Type)> hostList = mode is "scan" or "dump" or "holder" or "census" ? [.. homes.SelectMany(h => HostsOf(h).Select(t => (h, t)))] : [];
if (mode is "holder" or "census")
{
    List<(Assembly, Type)> sel = [];
    foreach (IGrouping<Assembly, (Assembly a, Type t)> g in hostList.GroupBy(x => x.Item1))
    {
        int marked = 0, markedM = 0, plain = 0, plainM = 0, hit = 0;
        foreach ((Assembly a, Type t) in g)
        {
            Type? recv = t.GetCustomAttributesData().Where(d => d.AttributeType.Name is "GoMethodHostAttribute" or "GoPromotedHostAttribute").Select(d => d.ConstructorArguments[0].Value as Type).FirstOrDefault();
            int n = mode == "census" ? t.GetMethods(Flags).Count(m => m.IsDefined(typeof(ExtensionAttribute), false)) : 0;
            if (recv is null) { plain++; plainM += n; sel.Add((a, t)); if (mode == "census") Console.WriteLine($"   unmarked {t.FullName} {n}"); }
            else { marked++; markedM += n; if (want.Contains(recv)) { hit++; sel.Add((a, t)); } }
        }
        if (mode == "census") Console.WriteLine($"{g.Key.GetName().Name}: marked hosts {marked} ({markedM} methods), unmarked hosts {plain} ({plainM} methods), marked for the asked types {hit}");
    }
    hostList = sel;
}
double idx = 0; long t0 = Stopwatch.GetTimestamp(); int read = 0, kept = 0, hosts = 0;
static Type Elem(Type r) { if (r.IsByRef) r = r.GetElementType()!; if (r.IsGenericType && r.GetGenericTypeDefinition().FullName == "go.ж`1") r = r.GetGenericArguments()[0]; return r.IsGenericType ? r.GetGenericTypeDefinition() : r; }
List<string> dump = [];
if (mode is "scan" or "dump" or "holder")
{
        foreach ((Assembly a, Type host) in hostList)
        {
            hosts++;
            foreach (MethodInfo m in host.GetMethods(Flags))
            {
                if (!m.IsDefined(typeof(ExtensionAttribute), false)) continue;
                read++;
                Type e = Elem(m.GetParameters()[0].ParameterType);
                if (want.Contains(e)) { kept++; if (mode == "dump") dump.Add($"{a.GetName().Name}|{e.FullName}|{host.FullName}|{m.Name}"); }
            }
        }
}
else if (mode == "meta")
{
    foreach (Assembly a in homes)
    {
        unsafe
        {
            long ti = Stopwatch.GetTimestamp();
            a.TryGetRawMetadata(out byte* blob, out int len);
            MetadataReader r = new(blob, len);
            Dictionary<string, List<int>> index = [];
            foreach (TypeDefinitionHandle th in r.TypeDefinitions)
            {
                TypeDefinition td = r.GetTypeDefinition(th);
                if (!td.GetDeclaringType().IsNil || (td.Attributes & TypeAttributes.Sealed) == 0 || !HasExt(r, td.GetCustomAttributes())) continue;
                hosts++;
                foreach (MethodDefinitionHandle mh in td.GetMethods())
                {
                    MethodDefinition md = r.GetMethodDefinition(mh);
                    if (!HasExt(r, md.GetCustomAttributes())) continue;
                    read++;
                    string? key = FirstParam(r, md.Signature);
                    if (key is null) continue;
                    if (!index.TryGetValue(key, out List<int>? l)) index[key] = l = [];
                    l.Add(MetadataTokens.GetToken(mh));
                }
            }
            idx += Stopwatch.GetElapsedTime(ti).TotalMilliseconds;
            foreach ((Assembly asm, Type type) in asked)
            {
                if (asm != a) continue;
                string key = type.IsNested ? type.DeclaringType!.FullName + "+" + type.Name : type.FullName!;
                if (!index.TryGetValue(key, out List<int>? toks)) continue;
                foreach (int tok in toks) { MethodBase m = a.ManifestModule.ResolveMethod(tok)!; _ = m.GetParameters()[0].ParameterType; kept++; }
            }
        }
    }
}
else if (mode == "names")
{
    // names file: asm|type|host|method, as a generated per-type list would hold them
    foreach (IGrouping<string, string[]> g in File.ReadLines(Path.Combine(AppContext.BaseDirectory, "names.txt")).Select(l => l.Split('|')).Where(p => keys.Contains(p[0] + "|" + p[1])).GroupBy(p => p[0] + "|" + p[2] + "|" + p[1]))
    {
        string[] f = g.First();
        Assembly a = asked.First(x => x.asm.GetName().Name == f[0]).asm;
        Type host = a.GetType(f[2], true)!; hosts++;
        Type e = a.GetType(f[1], true)!;
        _ = e.GetCustomAttributesData().Count;
        foreach (string name in g.Select(p => p[3]).Distinct())
            foreach (MemberInfo mi in host.GetMember(name, MemberTypes.Method, Flags))
            {
                read++;
                MethodInfo m = (MethodInfo)mi;
                if (m.IsDefined(typeof(ExtensionAttribute), false) && Elem(m.GetParameters()[0].ParameterType) == e) kept++;
            }
    }
}
double ms = Stopwatch.GetElapsedTime(t0).TotalMilliseconds;
if (mode == "dump") File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "names.txt"), dump);
Console.WriteLine($"{label}{mode}\t{ms:F1} ms\thomes {homes.Length}\thosts {hosts}\tmethods read {read}\tkept {kept}\tindex part {idx:F1} ms");

}

static bool HasExt(MetadataReader r, CustomAttributeHandleCollection c)
{
    foreach (CustomAttributeHandle h in c)
    {
        EntityHandle ctor = r.GetCustomAttribute(h).Constructor;
        if (ctor.Kind != HandleKind.MemberReference) continue;
        EntityHandle parent = r.GetMemberReference((MemberReferenceHandle)ctor).Parent;
        if (parent.Kind != HandleKind.TypeReference) continue;
        TypeReference tr = r.GetTypeReference((TypeReferenceHandle)parent);
        if (r.StringComparer.Equals(tr.Name, "ExtensionAttribute") && r.StringComparer.Equals(tr.Namespace, "System.Runtime.CompilerServices")) return true;
    }
    return false;
}

// The receiver's element as "namespace.Outer+Name", read from the signature blob with no type load.
static string? FirstParam(MetadataReader r, BlobHandle sig)
{
    BlobReader b = r.GetBlobReader(sig);
    SignatureHeader h = b.ReadSignatureHeader();
    if (h.IsGeneric) b.ReadCompressedInteger();
    if (b.ReadCompressedInteger() == 0) return null;
    SkipType(ref b);                                   // return type
    return ReadElem(r, ref b);
}

static string? ReadElem(MetadataReader r, ref BlobReader b)
{
    SignatureTypeCode c = b.ReadSignatureTypeCode();
    while (c is SignatureTypeCode.RequiredModifier or SignatureTypeCode.OptionalModifier) { b.ReadTypeHandle(); c = b.ReadSignatureTypeCode(); }
    switch (c)
    {
        case SignatureTypeCode.ByReference: return ReadElem(r, ref b);
        case SignatureTypeCode.TypeHandle: return Name(r, b.ReadTypeHandle());
        case SignatureTypeCode.GenericTypeInstance:
            b.ReadSignatureTypeCode();                 // class or valuetype
            string? def = Name(r, b.ReadTypeHandle());
            int n = b.ReadCompressedInteger();
            if (def == "go.ж`1") return ReadElem(r, ref b);
            return def;
        default: return null;
    }
}

static string? Name(MetadataReader r, EntityHandle h)
{
    if (h.Kind == HandleKind.TypeDefinition)
    {
        TypeDefinition td = r.GetTypeDefinition((TypeDefinitionHandle)h);
        string n = r.GetString(td.Name), ns = r.GetString(td.Namespace);
        TypeDefinitionHandle d = td.GetDeclaringType();
        if (!d.IsNil) return Name(r, d) + "+" + n;
        return ns.Length == 0 ? n : ns + "." + n;
    }
    if (h.Kind == HandleKind.TypeReference)
    {
        TypeReference tr = r.GetTypeReference((TypeReferenceHandle)h);
        string n = r.GetString(tr.Name), ns = r.GetString(tr.Namespace);
        if (tr.ResolutionScope.Kind == HandleKind.TypeReference) return Name(r, (EntityHandle)tr.ResolutionScope) + "+" + n;
        return ns.Length == 0 ? n : ns + "." + n;
    }
    return null;
}

static void SkipType(ref BlobReader b)
{
    SignatureTypeCode c = b.ReadSignatureTypeCode();
    switch (c)
    {
        case SignatureTypeCode.RequiredModifier: case SignatureTypeCode.OptionalModifier: b.ReadTypeHandle(); SkipType(ref b); break;
        case SignatureTypeCode.ByReference: case SignatureTypeCode.SZArray: case SignatureTypeCode.Pointer: case SignatureTypeCode.Pinned: SkipType(ref b); break;
        case SignatureTypeCode.TypeHandle: b.ReadTypeHandle(); break;
        case SignatureTypeCode.GenericTypeInstance: b.ReadSignatureTypeCode(); b.ReadTypeHandle(); for (int n = b.ReadCompressedInteger(); n > 0; n--) SkipType(ref b); break;
        case SignatureTypeCode.GenericTypeParameter: case SignatureTypeCode.GenericMethodParameter: b.ReadCompressedInteger(); break;
        case SignatureTypeCode.Array: SkipType(ref b); b.ReadCompressedInteger(); for (int n = b.ReadCompressedInteger(); n > 0; n--) b.ReadCompressedInteger(); for (int n = b.ReadCompressedInteger(); n > 0; n--) b.ReadCompressedSignedInteger(); break;
        case SignatureTypeCode.FunctionPointer: { SignatureHeader h = b.ReadSignatureHeader(); if (h.IsGeneric) b.ReadCompressedInteger(); int n = b.ReadCompressedInteger(); SkipType(ref b); for (; n > 0; n--) SkipType(ref b); break; }
        default: break;
    }
}

static IEnumerable<Type> HostsOf(Assembly a)
{
    foreach (Type t in a.GetTypes())
        if (t.IsSealed && t.IsAbstract && !t.IsNested && t.IsDefined(typeof(ExtensionAttribute), false)) yield return t;
}
