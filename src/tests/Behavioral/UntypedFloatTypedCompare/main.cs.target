namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedFloat tenth => 0.1;

internal static UntypedFloat twoTo53 => 9007199254740992.0;

internal static UntypedFloat third => /* 1.0 / 3 */ 0.3333333333333333;

internal static void Main() {
    var f = (float32)0.1F;
    fmt.Println("float32(0.1) <= tenth:", f <= (float32)tenth);
    fmt.Println("float32(0.1) == tenth:", f == (float32)tenth);
    fmt.Println("float32(1/3) == third:", (float32)(1.0F / 3F) == (float32)third);

    var big = 9007199254740993L;
    fmt.Println("int64(2^53+1) > twoTo53:", big > (int64)twoTo53);
    fmt.Println("int64(2^53+1) != twoTo53:", big != (int64)twoTo53);

    var ubig = ((uint64)1 << (int)(53)) + 1;
    fmt.Println("uint64(2^53+1) > twoTo53:", ubig > (uint64)twoTo53);

    nint n = unchecked((nint)(9007199254740993L));
    fmt.Println("int(2^53+1) >= twoTo53+1:", n >= unchecked((nint)(9007199254740993L)));

    fmt.Println("float32 arithmetic:", f * (float32)tenth);
    fmt.Println("float32 vs literal:", f <= 0.1F);
    fmt.Println("float64 vs constant:", (float64)f <= tenth);
}

} // end main_package
