namespace go;

using fmt = fmt_package;
using os = os_package;
using runtime = runtime_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static bool lines = os.Getenv("GO2CS_SLICE_LITERAL_LINES") == "1"u8;

internal static partial nint line() {
    var (_, _, l, _) = runtime.Caller(1);
    return l;
}

partial struct pair {
    internal nint a, b;
}

internal static array<nint> arrayFirst = new nint[]{line(),
    line(),
    line()
}.array();

internal static slice<nint> sliceAfterArray = slice<nint>([line(),
    line(),
    line()
]);

internal static slice<pair> structs = slice<pair>([new(line(), line()),
    new(
        line(),
        line()
    )
]);

internal static slice<slice<nint>> nested = slice<slice<nint>>([slice<nint>([line(), line()]),
    slice<nint>([line(),
        line()])
]);

internal static slice<ж<pair>> pointers = slice<ж<pair>>([Ꮡ(new pair(line(), line())),
    Ꮡ(new pair(
        line(),
        line()))
]);

internal static void show(@string label, params ꓸꓸꓸnint valuesʗp) {
    var values = valuesʗp.slice();

    if (lines) {
        fmt.Println(label, values);
    }
}

internal static void Main() {
    var local = slice<nint>([line(),
        line()
    ]);

    var localStructs = slice<pair>([new(line(), line()),
        new(
            line(),
            line()
        )
    ]);

    show("arrayFirst"u8, arrayFirst[..].ꓸꓸꓸ);
    show("sliceAfterArray"u8, sliceAfterArray.ꓸꓸꓸ);
    show("structs", structs[0].a, structs[0].b, structs[1].a, structs[1].b);
    show("nested", nested[0][0], nested[0][1], nested[1][0], nested[1][1]);
    show("pointers", pointers[0].a, pointers[0].b, pointers[1].a, pointers[1].b);
    show("local"u8, local.ꓸꓸꓸ);
    show("localStructs", localStructs[0].a, localStructs[0].b, localStructs[1].a, localStructs[1].b);

    fmt.Println("lengths:", len(arrayFirst), len(sliceAfterArray), len(structs), len(nested), len(pointers), len(local), len(localStructs));
}

} // end main_package
