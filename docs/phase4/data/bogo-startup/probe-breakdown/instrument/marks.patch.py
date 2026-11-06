# Scratch instrument v2 (never pushed): phase marks. Needs initpkg.patch.py applied first.
import os, re
W=os.environ['TREE']+'/src/core/'
def edit(p, pairs):
    s=open(p,newline='').read(); nl='\r\n' if '\r\n' in s else '\n'
    for old,new in pairs:
        o=old.replace('\n',nl); n=new.replace('\n',nl)
        assert s.count(o)==1, (p, old[:60]); s=s.replace(o,n)
    open(p,'w',newline='').write(s)
edit(W+'golib/builtin.cs',[('internal static class P1InitTrace','public static class P1InitTrace'),
 ('internal static void Add(int depth','public static void Add(int depth'),
 ('    internal static int Depth;','    internal static int Depth;\n    public static long Now() => System.Diagnostics.Stopwatch.GetTimestamp();\n    public static void Mark(string label, long t0) { if (Out is not null) Add(-1, label, t0, Now()); }')])
edit(W+'golib/runtime/TypeExtensions.ExtensionMethodRegistry.cs',[
 ('''        if (Volatile.Read(ref s_extensionMethods) is { } cached)
            return cached;
''','''        if (Volatile.Read(ref s_extensionMethods) is { } cached)
            return cached;
        long p1t0 = go.P1InitTrace.Now();
'''),
 ('''            if (Volatile.Read(ref s_extensionMethodsGeneration) == generation)
                return published;''','''            if (Volatile.Read(ref s_extensionMethodsGeneration) == generation)
            {
                go.P1InitTrace.Mark("EXTSCAN", p1t0);
                return published;
            }'''),
 ('''        return scanned;
    }

    internal static void ClearTypeCaches''','''        go.P1InitTrace.Mark("EXTSCAN-GAVEUP", p1t0);
        return scanned;
    }

    internal static void ClearTypeCaches''')])
edit(W+'golib/runtime/TypeExtensions.GoMethodSets.cs',[
 ('''        return s_goMethodSetCandidates.GetOrAdd((valueElement, valueIsPointer), static key =>
        {
            (Type valueElement, bool valueIsPointer) = key;''','''        return s_goMethodSetCandidates.GetOrAdd((valueElement, valueIsPointer), static key =>
        {
            long p1t0 = go.P1InitTrace.Now();
            try {
            (Type valueElement, bool valueIsPointer) = key;''')])
edit(W+'golib/runtime/TypeExtensions.GoMethodSets.cs',[
 ('''            return candidates;
        });
    }''','''            return candidates;
            } finally { go.P1InitTrace.Mark("MSET", p1t0); }
        });
    }''')])
edit(W+'testing/TestHost.cs',[
 ('''        PreloadManagedReferenceClosure(Assembly.GetEntryAssembly());''','''        go.P1InitTrace.Mark("RUN-ENTER", go.P1InitTrace.Now());
        long p1pre = go.P1InitTrace.Now();
        PreloadManagedReferenceClosure(Assembly.GetEntryAssembly());
        go.P1InitTrace.Mark("PRELOAD", p1pre);'''),
 ('''        try
        {
            registry.TestMain(new StandardBox<testing_package.M>(m));''','''        go.P1InitTrace.Mark("TESTMAIN-CALL", go.P1InitTrace.Now());
        try
        {
            registry.TestMain(new StandardBox<testing_package.M>(m));''')])
print('marks patched')
