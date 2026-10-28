namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt marker => 0xFFFD;

partial struct words /*[]uint16*/;

internal static void Main() {
    var aᴷ = new stackAppend<uint16>();
    var wᴷ = new stackAppend<uint16>();
    var intsᴷ = new stackAppend<nint>();
    var anysᴷ = new stackAppend<any>();

    slice<uint16> a = default;
    a = append(aᴷ, a, (uint16)(marker));
    a = append(a, (uint16)7, (uint16)8);
    uint16 v = 9;
    a = append(a, v);
    fmt.Println(len(a), a[0], a[1], a[2], a[3]);

    words w = default;
    w = append(wᴷ, w, (uint16)(marker), (uint16)1);
    fmt.Println(w[0], w[1]);

    slice<nint> ints = default;
    ints = append(intsᴷ, ints, (nint)(-1));
    ints = append(ints, (nint)(+2));
    ints = append(ints, (nint)(~0));
    fmt.Println(ints[0], ints[1], ints[2]);

    slice<any> anys = default;
    var data = new byte[]{7, 8, 9}.slice();
    anys = append(anys.slice(0, len(anys), len(anys)), (any)(data));
    anys = append(anysᴷ, anys, (any)5);
    fmt.Println(len(anys), len(anys[0]._<slice<byte>>()), anys[1]);
}

} // end main_package
