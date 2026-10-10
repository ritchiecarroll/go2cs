namespace go;

using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using coverage = runtime.coverage_package;
using runtime;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object writeCountersˢ = (@string)"WriteCounters:"u8;
private static readonly object writeCountersDirˢ = (@string)"WriteCountersDir:"u8;
private static readonly object writeMetaˢ = (@string)"WriteMeta:"u8;
private static readonly object writeMetaDirˢ = (@string)"WriteMetaDir:"u8;
private static readonly object clearCountersˢ = (@string)"ClearCounters:"u8;

internal static void Main() {
    @string dir = os.TempDir();
    fmt.Println(writeCountersˢ, coverage.WriteCounters(Δio.Discard));
    fmt.Println(writeCountersDirˢ, coverage.WriteCountersDir(dir));
    fmt.Println(writeMetaˢ, coverage.WriteMeta(Δio.Discard));
    fmt.Println(writeMetaDirˢ, coverage.WriteMetaDir(dir));
    fmt.Println(clearCountersˢ, coverage.ClearCounters());
}

} // end main_package
