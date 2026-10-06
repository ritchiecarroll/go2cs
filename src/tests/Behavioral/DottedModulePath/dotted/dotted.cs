namespace go.example.com.dotted;

using runtime = runtime_package;
using System.Runtime.CompilerServices;

partial class dotted_package {

[GoType] partial struct T {
    public nint N;
}

public static nint F() {
    return 7;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noCallerˢ = "no caller"u8;

public static partial @string Where() {
    var (pc, _, _, ok) = runtime.Caller(0);
    if (!ok) {
        return noCallerˢ;
    }
    return runtime.FuncForPC(pc).Name();
}

} // end dotted_package
