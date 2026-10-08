namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct UUID /*[16]byte*/;

partial struct UUIDs /*[]UUID*/;

public static slice<@string> Strings(this UUIDs us) {
    var @out = new slice<@string>(len(us));
    foreach (var (i, vᴛ1) in us) {
        var u = vᴛ1.Clone();

        @out[i] = fmt.Sprintf("%x"u8, u[..2]);
    }
    return @out;
}

partial struct Rows /*[]array<nint>*/;

public static nint Sum(this Rows r) {
    nint total = 0;
    foreach (var (_, vᴛ1) in r) {
        var row = vᴛ1.Clone();

        foreach (var (_, v) in row.ΔRangeSnapshot()) {
            total += v;
        }
    }
    return total;
}

internal static void Main() {
    var (a, b) = (new UUID(new byte[]{0xab, 0x01}.array(16)), new UUID(new byte[]{0xcd, 0x02}.array(16)));
    fmt.Println(new UUIDs(GoReflect.WithElemDims(new UUID[]{a.Clone(), b.Clone()}.slice(), 16)).Strings());
    var us = new UUIDs(GoReflect.WithElemDims(new UUID[]{a.Clone(), b.Clone()}.slice(), 16));
    fmt.Println(len(us), us.Strings());
    var made = new UUIDs(2, 4);
    fmt.Println(len(made), cap(made), made.Strings());
    fmt.Println(us[1..].Strings());
    var rows = new Rows(GoReflect.WithElemDims(new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.slice(), 3));
    fmt.Println(rows.Sum(), new Rows(GoReflect.WithElemDims(new slice<array<nint>>(1, () => new(3)), 3)).Sum());
    fmt.Println(len(new UUIDs(GoReflect.WithElemDims(new UUID[]{}.slice(), 16))), len(new UUIDs(0)), new UUIDs(GoReflect.WithElemDims(new UUID[]{}.slice(), 16)).Strings(), new Rows(GoReflect.WithElemDims(new array<nint>[]{}.slice(), 3)).Sum(), len(new Rows(GoReflect.WithElemDims(new slice<array<nint>>(0, () => new(3), 2), 3))));
}

} // end main_package
