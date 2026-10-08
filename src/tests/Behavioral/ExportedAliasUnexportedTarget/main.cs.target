namespace go;

using fmt = fmt_package;
using aliaslib = ExportedAliasUnexportedTarget.aliaslib_package;
using ExportedAliasUnexportedTarget;

partial class main_package {

partial struct Logger {
    internal aliaslibꓸMutexWrap mu;
    internal slice<@string> lines;
}

public static void Log(this ж<Logger> Ꮡl, @string line) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var l = ref Ꮡl.DerefOrNull();

        Ꮡl.of(Logger.Ꮡmu).Lock();
        ᒐd1 = true;
        l.lines = append(l.lines, line);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡl.of(Logger.Ꮡmu).Unlock(); ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string firstˢ = "first"u8;
private static readonly @string secondˢ = "second"u8;
private static readonly object lockRoundTripOkˢ = (@string)"lock round trip ok"u8;

internal static void Main() {
    ref var l = ref heap(new Logger(), out var Ꮡl);
    Ꮡl.Log(firstˢ);
    l.mu.Disable();
    Ꮡl.Log(secondˢ);
    fmt.Println(len(l.lines), l.lines[0], l.lines[1]);
    ref var w = ref heap(new aliaslibꓸMutexWrap(), out var Ꮡw);
    Ꮡw.Lock();
    Ꮡw.Unlock();
    fmt.Println(lockRoundTripOkˢ);
    fmt.Println(aliaslib.Compare(1, 2), aliaslib.Compare(2, 2), aliaslib.Compare(3, 2));
}

} // end main_package
