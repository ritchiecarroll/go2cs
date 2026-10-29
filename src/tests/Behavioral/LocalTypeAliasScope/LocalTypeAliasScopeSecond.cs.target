global using secondWriteOps_testFnc = object;
global using secondWriteOps_fileMaker = object;
global using secondAliases_hdr = go.main_package.Header;

namespace go;

using fmt = fmt_package;

partial class main_package {

internal partial struct secondWriteOps_readOp /*dyn*/ {
    internal nint cnt;
}

internal static void secondWriteOps() {
    var tests = slice<secondWriteOps_testFnc>([new secondWriteOps_readOp(1), new secondWriteOps_readOp(2)]);
    var maker = (secondWriteOps_fileMaker)new secondWriteOps_readOp(9);

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

    fmt.Println("secondWriteOps:", len(tests), sum);
}

internal static void secondAliases() {
    var h = new secondAliases_hdr(Name: "link.txt", Size: 0);

    fmt.Println("secondAliases:", h, new Header(Name: "raw", Size: 7));
}

} // end main_package
