namespace go.modernc.org.sqlite.lib;

using libc = go.modernc.org.libc_package;
using go.modernc.org;

partial class sqlite3_package {

internal static void _piFunc(ж<libc.TLS> tls, uintptr context, int32 argc, uintptr argv) {
    _ = argv;
    Xsqlite3_result_double(tls, context, (float64)3.141592653589793D);
}

} // end sqlite3_package
