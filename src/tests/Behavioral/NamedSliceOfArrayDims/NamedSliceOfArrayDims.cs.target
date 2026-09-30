namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[16]byte")] partial struct UUID;

[GoType("[]UUID")] partial struct UUIDs;

public static slice<@string> Strings(this UUIDs us) {
    var @out = new slice<@string>(len(us));
    foreach (var (i, vᴛ1) in us) {
        var u = vᴛ1.Clone();

        @out[i] = fmt.Sprintf("%x"u8, u[..2]);
    }
    return @out;
}

[GoType("[]array<nint>")] partial struct Rows;

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
    fmt.Println(GoReflect.WithElemDims(new UUIDs(new UUID[]{a.Clone(), b.Clone()}.slice()), 16).Strings());
    var us = GoReflect.WithElemDims(new UUIDs(new UUID[]{a.Clone(), b.Clone()}.slice()), 16);
    fmt.Println(len(us), us.Strings());
    var made = GoReflect.WithElemDims(new UUIDs(2, 4), 16);
    fmt.Println(len(made), cap(made), made.Strings());
    fmt.Println(GoReflect.WithElemDims(us[1..], 16).Strings());
    var rows = GoReflect.WithElemDims(new Rows(new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.slice()), 3);
    fmt.Println(rows.Sum(), GoReflect.WithElemDims(new Rows(new slice<array<nint>>(1, () => new(3))), 3).Sum());
    fmt.Println(len(GoReflect.WithElemDims(new UUIDs(new UUID[]{}.slice()), 16)), len(GoReflect.WithElemDims(new UUIDs(0), 16)), GoReflect.WithElemDims(new UUIDs(new UUID[]{}.slice()), 16).Strings(), GoReflect.WithElemDims(new Rows(new array<nint>[]{}.slice()), 3).Sum(), len(GoReflect.WithElemDims(new Rows(new slice<array<nint>>(0, () => new(3), 2)), 3)));
}

} // end main_package
