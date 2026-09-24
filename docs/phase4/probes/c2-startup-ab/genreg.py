# Writes zz_regprobe.g.cs into each closure module's project directory: one module initializer that
# registers every distinct regular "..."u8 literal of the module's compiled .cs files (GoTargetOS=linux).
import os, re, sys, glob
# usage: genreg.py <scratch src root> <file of closure assembly names> <report.tsv> [eager|lazy]
#   eager: a [ModuleInitializer] that registers every literal (arm B).
#   lazy:  a [ModuleInitializer] that registers only the module's range and a registrar delegate (arm L).
# Regular "..."u8 literals only: verbatim @"..."u8 literals are skipped (about 17 lines, under 1%).
src, asmfile, report = sys.argv[1], sys.argv[2], sys.argv[3]
mode = sys.argv[4] if len(sys.argv) > 4 else 'eager'
asms = [a.strip() for a in open(asmfile) if a.strip()]
proj = {}
for cs in glob.glob(f"{src}/core/**/*.csproj", recursive=True) + glob.glob(f"{src}/tests/Behavioral/Su*/*.csproj"):
    if cs.endswith('.tests.csproj'): continue
    t = open(cs, encoding='utf-8', errors='replace').read()
    m = re.search(r'<AssemblyName>([^<]+)</AssemblyName>', t)
    name = m.group(1) if m else os.path.basename(cs)[:-7]
    proj.setdefault(name, (cs, t))
lit = re.compile(r'(?<![@$"\w])"((?:[^"\\\n]|\\.)*)"u8')
tot = 0; rows = []
for a in asms:
    if a in ('golib', 'unsafe') or a not in proj:
        rows.append(f"{a}\tSKIP"); continue
    cs, t = proj[a]; d = os.path.dirname(cs)
    files = [f for f in glob.glob(f"{d}/*.cs") if not re.search(r'(_test\.cs|package_test_info\.cs|go2cs_test_host\.cs|zz_regprobe\.g\.cs)$', f)]
    if '$(GoTargetOS)/*.cs' in t:
        files += [f for f in glob.glob(f"{d}/linux/*.cs") if not f.endswith('_test.cs')]
    seen = []; ss = set()
    for f in files:
        for m in lit.finditer(open(f, encoding='utf-8', errors='replace').read()):
            v = m.group(1)
            if v and v not in ss: ss.add(v); seen.append(v)
    with open(f"{d}/zz_regprobe.g.cs", 'w', encoding='utf-8') as o:
        o.write("// start-up probe (C2): registers this module's u8 literals (" + mode + ")\nnamespace go;\n\ninternal static class ᴛRegProbe\n{\n")
        if mode == 'lazy':
            o.write("    [global::System.Runtime.CompilerServices.ModuleInitializer]\n    internal static void Init() => global::go.LiteralTable.RegisterLazyModule(typeof(ᴛRegProbe).Module, Register);\n\n    internal static void Register()\n    {\n")
        else:
            o.write("    [global::System.Runtime.CompilerServices.ModuleInitializer]\n    internal static void Register()\n    {\n")
        for v in seen: o.write(f'        global::go.LiteralTable.Register("{v}"u8);\n')
        o.write("    }\n}\n")
    tot += len(seen); rows.append(f"{a}\t{len(seen)}")
open(report, 'w').write("\n".join(rows) + f"\nTOTAL\t{tot}\n")
print("total", tot)
