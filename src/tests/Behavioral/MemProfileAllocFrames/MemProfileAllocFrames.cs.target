namespace go;

using fmt = fmt_package;
using runtime = runtime_package;
using sort = sort_package;
using strings = strings_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct T {
    internal nint a, b, c, d;
    internal ж<T> next;
}

internal static slice<byte> sinkBytes;
internal static slice<nint> sinkInts;
internal static ж<T> sinkT;
internal static ж<T> sinkLit;

internal static partial void allocMake() {
    sinkBytes = new slice<byte>((1 << (int)(16)));
}

internal static partial void allocGrow() {
    var s = new slice<nint>(0, 4);
    for (nint i = 0; i < 64; i++) {
        s = append(s, i);
    }
    sinkInts = s;
}

internal static partial void allocNew() {
    sinkT = @new<T>();
}

internal static partial void allocLiteral() {
    sinkLit = Ꮡ(new T(a: 1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string mainAllocˢ = "main.alloc"u8;

internal static partial void Main() {
    runtime.MemProfileRate = 1;
    allocMake();
    allocGrow();
    allocNew();
    allocLiteral();
    runtime.GC();

    var (n, _) = runtime.MemProfile(default!, true);
    var records = new slice<runtime.MemProfileRecord>(n + 64, () => new());
    (n, _) = runtime.MemProfile(records, true);

    var seen = new map<@string, bool>{};
    slice<@string> lines = default!;
    foreach (var (_, vᴛ1) in records.slice(0, n)) {
        var r = vᴛ1.ΔClone();

        slice<@string> names = default!;
        foreach (var (_, pc) in r.Stack()) {
            var f = runtime.FuncForPC(pc - 1);
            if (f == nil) {
                names = append(names, "?"u8);
                continue;
            }
            @string name = f.Name();
            names = append(names, name);
            if (strings.HasPrefix(name, mainAllocˢ)) {
                @string line = strings.Join(names, " < "u8);
                if (!seen[line]) {
                    seen[line] = true;
                    lines = append(lines, line);
                }
                break;
            }
        }
    }
    sort.Strings(lines);
    foreach (var (_, l) in lines) {
        fmt.Println(l);
    }
}

} // end main_package
