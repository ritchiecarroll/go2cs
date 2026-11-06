# SCRATCH INSTRUMENT, NEVER COMMITTED: phase timers in golib for the AOT structural-assert seat.
# Writes "name\tcount\tms" rows to $P1MARK_OUT at process exit. Revert: git checkout HEAD -- src/core/golib
import sys
W=sys.argv[1]; nl='\r\n'
def edit(rel, pairs):
    p=W+'/'+rel; s=open(p,newline='',encoding='utf-8-sig').read(); bom=open(p,'rb').read(3)==b'\xef\xbb\xbf'
    for a,b in pairs:
        a=a.replace('\n',nl); b=b.replace('\n',nl)
        assert s.count(a)==1,(rel,a[:60]); s=s.replace(a,b)
    open(p,'w',newline='',encoding='utf-8-sig' if bom else 'utf-8').write(s)
open(W+'/src/core/golib/P1Marks.cs','w').write('''namespace go.golib;
using System.Diagnostics;
// SCRATCH INSTRUMENT (P1 AOT seat), never committed.
internal static class P1Marks
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, long[]> s_acc = new();
    static P1Marks()
    {
        string? path = System.Environment.GetEnvironmentVariable("P1MARK_OUT");
        if (!string.IsNullOrEmpty(path))
            System.AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                var sb = new System.Text.StringBuilder();
                foreach (var kv in s_acc) sb.Append(kv.Key).Append('\\t').Append(kv.Value[0]).Append('\\t').Append((kv.Value[1] * 1000.0 / Stopwatch.Frequency).ToString("F3")).Append('\\n');
                System.IO.File.WriteAllText(path, sb.ToString());
            };
    }
    public static long T() => Stopwatch.GetTimestamp();
    public static void Add(string name, long start)
    {
        long d = Stopwatch.GetTimestamp() - start;
        long[] a = s_acc.GetOrAdd(name, static _ => new long[2]);
        System.Threading.Interlocked.Increment(ref a[0]);
        System.Threading.Interlocked.Add(ref a[1], d);
    }
}
''')
edit('src/core/golib/runtime/TypeExtensions.ExtensionMethodRegistry.cs',[
('''        foreach (Type type in ExtensionHostTypes(assembly) ?? assembly.GetTypes())
        foreach (MethodInfo extensionMethod in DeclaredExtensionMethods(type))
            extensionMethods.Add((extensionMethod, extensionMethod.GetExtensionTargetType()));
''','''        long p1t = P1Marks.T();
        IEnumerable<Type> p1hosts = ExtensionHostTypes(assembly) ?? (IEnumerable<Type>)assembly.GetTypes();
        P1Marks.Add(p1hosts is Type[] ? "scan.GetTypes" : "scan.metadata", p1t);
        foreach (Type type in p1hosts)
        {
            p1t = P1Marks.T();
            bool p1shape = !(!type.IsSealed || type.IsNested || type.IsGenericType);
            P1Marks.Add("scan.filter.flags", p1t);
            List<MethodInfo> p1methods = [];
            if (p1shape)
            {
                P1Marks.Add(type.IsAbstract ? "scan.filter.shape.static" : "scan.filter.shape.NONSTATIC", P1Marks.T());
                p1t = P1Marks.T();
                MethodInfo[] p1all = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                P1Marks.Add("scan.filter.GetMethods", p1t);
                foreach (MethodInfo p1m in p1all)
                {
                    p1t = P1Marks.T();
                    bool p1ext = p1m.IsDefined(typeof(ExtensionAttribute), false);
                    P1Marks.Add("scan.filter.IsDefined", p1t);
                    if (p1ext) p1methods.Add(p1m);
                }
            }
            foreach (MethodInfo extensionMethod in p1methods)
            {
                p1t = P1Marks.T();
                Type p1target = extensionMethod.GetExtensionTargetType();
                P1Marks.Add("scan.targetType", p1t);
                extensionMethods.Add((extensionMethod, p1target));
            }
        }
'''),
('''    internal static (MethodInfo, Type)[] GetExtensionMethods()
    {
''','''    internal static (MethodInfo, Type)[] GetExtensionMethods()
    {
        long p1g = P1Marks.T();
        try { return P1GetExtensionMethods(); } finally { P1Marks.Add("GetExtensionMethods", p1g); }
    }

    private static (MethodInfo, Type)[] P1GetExtensionMethods()
    {
'''),
])
edit('src/core/golib/runtime/TypeExtensions.GoMethodSets.cs',[
('''            (Type valueElement, bool valueIsPointer) = key;
''','''            (Type valueElement, bool valueIsPointer) = key;
            long p1k = P1Marks.T();
            try {
'''),
('''                ResolveReceiverElement(receiverType, out Type receiverElement, out bool receiverIsPointer);
''','''                long p1r = P1Marks.T();
                ResolveReceiverElement(receiverType, out Type receiverElement, out bool receiverIsPointer);
                P1Marks.Add("key.resolveReceiver", p1r);
'''),
('''                if (!valueIsPointer && (receiverIsPointer || IsPointerSetByRefReceiver(method, receiver)))
                    continue;

                if (!ReceiverElementMatches(receiverElement, valueElement))
                    continue;
''','''                p1r = P1Marks.T();
                bool p1skip = !valueIsPointer && (receiverIsPointer || IsPointerSetByRefReceiver(method, receiver));
                P1Marks.Add("key.pointerSetTest", p1r);
                if (p1skip)
                    continue;

                p1r = P1Marks.T();
                bool p1match = ReceiverElementMatches(receiverElement, valueElement);
                P1Marks.Add("key.receiverMatch", p1r);
                if (!p1match)
                    continue;
'''),
('''            return candidates;
        });
''','''            return candidates;
            } finally { P1Marks.Add("key.total", p1k); }
        });
'''),
])
edit('src/core/golib/runtime/TypeExtensions.GoMethodSets.cs',[
('''    public static bool StructurallyImplements(this Type valueType, Type interfaceType)
    {
''','''    public static bool StructurallyImplements(this Type valueType, Type interfaceType)
    {
        long p1s = P1Marks.T();
        try { return P1StructurallyImplements(valueType, interfaceType); } finally { P1Marks.Add("StructurallyImplements", p1s); }
    }

    private static bool P1StructurallyImplements(Type valueType, Type interfaceType)
    {
'''),
])
edit('src/core/golib/AdapterBinder.cs',[
('''    public static bool TryCreate(object dynamicValue, Type interfaceType, [NotNullWhen(true)] out object? shell)
    {
        return TryCreate(dynamicValue, dynamicValue.GetType(), interfaceType, out shell);
    }
''','''    public static bool TryCreate(object dynamicValue, Type interfaceType, [NotNullWhen(true)] out object? shell)
    {
        long p1b = global::go.golib.P1Marks.T();
        try { return TryCreate(dynamicValue, dynamicValue.GetType(), interfaceType, out shell); } finally { global::go.golib.P1Marks.Add("AdapterBinder.TryCreate", p1b); }
    }
'''),
])
print('marks applied')
