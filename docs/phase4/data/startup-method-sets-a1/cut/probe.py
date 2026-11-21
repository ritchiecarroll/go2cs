# MEASUREMENT ONLY: minimal B3 probe hooks (hosts read, methods read, Collect time, main mark) over a golib tree.
import sys, shutil
T=sys.argv[1]; g=T+'/src/core/golib/'
shutil.copy(sys.argv[2], g+'B3Probe.cs')
def edit(path, pairs):
    s=open(path,newline='').read()
    for old,new in pairs:
        old=old.replace('\n','\r\n'); new=new.replace('\n','\r\n')
        assert s.count(old)==1,(path,old[:70]); s=s.replace(old,new)
    open(path,'w',newline='').write(s)
edit(g+'runtime/TypeExtensions.ExtensionMethodRegistry.cs',[
 ("                HostReadProbeForTest?.Invoke(type);\n                methods = ReadExtensionMethods(type);\n",
  "                HostReadProbeForTest?.Invoke(type);\n                long b3 = B3Probe.Enter();\n                methods = ReadExtensionMethods(type);\n                B3Probe.Leave(\"ExtensionHost.Methods(read)\", b3, methods.Length, type.FullName);\n"),
 ("        List<(MethodInfo method, Type receiver)> found = [];\n        ExtensionScan scan = WalkExtensionHosts();\n",
  "        List<(MethodInfo method, Type receiver)> found = [];\n        long b3 = B3Probe.Enter();\n        int b3h = 0;\n        ExtensionScan scan = WalkExtensionHosts();\n"),
 ("                    continue;\n\n                foreach ((MethodInfo method, Type receiver, Assembly? home) in host.Methods)\n",
  "                    continue;\n\n                b3h++;\n\n                foreach ((MethodInfo method, Type receiver, Assembly? home) in host.Methods)\n"),
 ("            if (attempt == MaxScanAttempts || Volatile.Read(ref s_extensionMethodsGeneration) == scan.Generation)\n                return found;\n",
  "            if (attempt == MaxScanAttempts || Volatile.Read(ref s_extensionMethodsGeneration) == scan.Generation)\n            {\n                B3Probe.Leave(\"HomedIn.hostsPassed\", b3, b3h);\n                B3Probe.Leave(\"HomedIn.methodsFound\", b3, found.Count);\n                return found;\n            }\n\n            b3h = 0;\n"),
])
edit(g+'runtime/TypeExtensions.GoMethodSets.cs',[
 ("    internal static List<MethodInfo> CollectGoMethodSetCandidates(Type valueElement, bool valueIsPointer, IReadOnlyList<(MethodInfo, Type)>? source)\n    {\n",
  "    internal static List<MethodInfo> CollectGoMethodSetCandidates(Type valueElement, bool valueIsPointer, IReadOnlyList<(MethodInfo, Type)>? source)\n    {\n        long b3 = B3Probe.Enter();\n        List<MethodInfo> b3r = CollectGoMethodSetCandidatesB3(valueElement, valueIsPointer, source);\n        B3Probe.Leave(\"CollectGoMethodSetCandidates\", b3, b3r.Count, $\"{(valueIsPointer ? \"*\" : \"\")}{valueElement}\");\n        return b3r;\n    }\n\n    private static List<MethodInfo> CollectGoMethodSetCandidatesB3(Type valueElement, bool valueIsPointer, IReadOnlyList<(MethodInfo, Type)>? source)\n    {\n"),
])
edit(g+'runtime/MainThread.cs',[
 ("    public static void Run(Action main)\n    {\n        ArgumentNullException.ThrowIfNull(main);\n","    public static void Run(Action main)\n    {\n        ArgumentNullException.ThrowIfNull(main);\n        B3Probe.Mark(\"main-enter\");\n"),
])
print("probe applied to", T)
