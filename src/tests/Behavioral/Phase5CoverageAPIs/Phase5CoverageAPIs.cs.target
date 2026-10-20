namespace go;

using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using coverage = runtime.coverage_package;
using runtime;

partial class main_package {

internal static void Main() {
    @string dir = os.TempDir();
    fmt.Println("WriteCounters:", coverage.WriteCounters(Δio.Discard));
    fmt.Println("WriteCountersDir:", coverage.WriteCountersDir(dir));
    fmt.Println("WriteMeta:", coverage.WriteMeta(Δio.Discard));
    fmt.Println("WriteMetaDir:", coverage.WriteMetaDir(dir));
    fmt.Println("ClearCounters:", coverage.ClearCounters());
}

} // end main_package
