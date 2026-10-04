namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedFloat tenth => 0.1;

internal static UntypedFloat twoTo53 => 9007199254740992.0;

internal static UntypedFloat third => /* 1.0 / 3 */ 0.3333333333333333;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object float3201Tenthˢ = (@string)"float32(0.1) <= tenth:"u8;
private static readonly object float3201Tenthˢ2 = (@string)"float32(0.1) == tenth:"u8;
private static readonly object float3213Thirdˢ = (@string)"float32(1/3) == third:"u8;
private static readonly object int642531TwoTo53ˢ = (@string)"int64(2^53+1) > twoTo53:"u8;
private static readonly object int642531TwoTo53ˢ2 = (@string)"int64(2^53+1) != twoTo53:"u8;
private static readonly object uint642531TwoTo53ˢ = (@string)"uint64(2^53+1) > twoTo53:"u8;
private static readonly object int2531TwoTo531ˢ = (@string)"int(2^53+1) >= twoTo53+1:"u8;
private static readonly object float32Arithmeticˢ = (@string)"float32 arithmetic:"u8;
private static readonly object float32VsLiteralˢ = (@string)"float32 vs literal:"u8;
private static readonly object float64VsConstantˢ = (@string)"float64 vs constant:"u8;

internal static void Main() {
    var f = (float32)0.1F;
    fmt.Println(float3201Tenthˢ, f <= (float32)tenth);
    fmt.Println(float3201Tenthˢ2, f == (float32)tenth);
    fmt.Println(float3213Thirdˢ, (float32)(1.0F / 3F) == (float32)third);
    var big = 9007199254740993L;
    fmt.Println(int642531TwoTo53ˢ, big > (int64)twoTo53);
    fmt.Println(int642531TwoTo53ˢ2, big != (int64)twoTo53);
    var ubig = ((uint64)1 << (int)(53)) + 1;
    fmt.Println(uint642531TwoTo53ˢ, ubig > (uint64)twoTo53);
    nint n = unchecked((nint)(9007199254740993L));
    fmt.Println(int2531TwoTo531ˢ, n >= unchecked((nint)(9007199254740993L)));
    fmt.Println(float32Arithmeticˢ, f * (float32)tenth);
    fmt.Println(float32VsLiteralˢ, f <= 0.1F);
    fmt.Println(float64VsConstantˢ, (float64)f <= tenth);
}

} // end main_package
