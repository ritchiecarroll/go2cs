namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct rank /*num:nint*/;

internal static rank rankLow => /* iota */ 0;
internal static rank rankMid => 1;
internal static rank rankHigh => 2;

partial struct code /*num:uint8*/;

internal static code codeA => /* iota */ 0;
internal static code codeB => 1;

internal static slice<@string> rankNames = new golib.SparseArray<@string>{
    [(int)rankLow] = "low"u8,
    [(int)rankMid] = "mid"u8,
    [(int)rankHigh] = "high"u8
}.slice();

internal static slice<@string> codeNames = new golib.SparseArray<@string>{
    [codeA] = "a"u8,
    [codeB] = "b"u8
}.slice();

partial struct errno /*num:uintptr*/;

partial struct kindT /*num:nuint*/;

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [1] = "one"u8,
    [3] = "three"u8
}.slice();

internal static errno errBase => /* 1 << 10 */ 1024;
internal static errno eBig => /* errBase + 1 */ 1025;
internal static errno eAcces => /* errBase + 2 */ 1026;

internal static slice<@string> errNames = new golib.SparseArray<@string>{
    [1] = "big"u8,
    [2] = "acces"u8
}.slice();

internal static void Main() {
    fmt.Println(rankNames[rankLow], rankNames[rankMid], rankNames[rankHigh]);
    fmt.Println(codeNames[codeA], codeNames[codeB]);
    fmt.Println(len(rankNames));
    fmt.Println(errNames[eBig - errBase], errNames[eAcces - errBase]);
    fmt.Println(asciiSpace[(rune)'\t'], asciiSpace[(rune)'\n'], asciiSpace[(rune)' '], asciiSpace[(rune)'A'], len(asciiSpace));
    var data = new byte[]{10, 20, 30}.slice();
    int64 cur = 2;
    uint32 u32 = 1;
    fmt.Println(data[(nint)(cur)], data[u32]);
    fmt.Println(kindNames[((kindT)3)], len(kindNames));
}

internal static array<uint8> asciiSpace = new array<uint8>(256){[(rune)'\t'] = 1, [(rune)'\n'] = 1, [(rune)'\v'] = 1, [(rune)'\f'] = 1, [(rune)'\r'] = 1, [(rune)' '] = 1};

} // end main_package
