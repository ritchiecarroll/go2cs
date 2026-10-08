global using secondWriteOps_testFnc = object;
global using secondWriteOps_fileMaker = object;
global using secondAliases_hdr = go.main_package.Header;

namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object secondWriteOpsˢ = (@string)"secondWriteOps:"u8;

internal partial struct secondWriteOps_readOp /*dyn*/ {
    internal nint cnt;
}

internal static void secondWriteOps() {
    var tests = new secondWriteOps_testFnc[]{new secondWriteOps_readOp(1), new secondWriteOps_readOp(2)}.slice();
    var maker = ((secondWriteOps_fileMaker)new secondWriteOps_readOp(9));
    nint sum = 0;
    foreach (var (_, t) in tests) {
        {
            var (op, ok) = t._<secondWriteOps_readOp>(ᐧ); if (ok) {
                sum += op.cnt;
            }
        }
    }
    {
        var (op, ok) = maker._<secondWriteOps_readOp>(ᐧ); if (ok) {
            sum += op.cnt;
        }
    }
    fmt.Println(secondWriteOpsˢ, len(tests), sum);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object secondAliasesˢ = (@string)"secondAliases:"u8;

internal static void secondAliases() {
    var h = new secondAliases_hdr(Name: "link.txt"u8, Size: 0);
    fmt.Println(secondAliasesˢ, h, new Header(Name: "raw"u8, Size: 7));
}

} // end main_package
