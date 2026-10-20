namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    foreach (var (_, v) in new nint[]{5, 99, 999, 1234, -5, -999}.slice()) {
        fmt.Printf("k =% 4d;\n"u8, v);
    }

    fmt.Printf("[% d] [% d] [%+d] [%+d]\n"u8, (nint)(7), (nint)(-7), (nint)(7), (nint)(-7));

    fmt.Printf("[%4d] [%-4d] [%04d] [%04d] [%+4d]\n"u8, (nint)(42), (nint)(42), (nint)(42), (nint)(-42), (nint)(7));

    fmt.Printf("[% 04d] [%+04d]\n"u8, (nint)(5), (nint)(5));

    fmt.Printf("[%6s] [%-6s] [%6v]\n"u8, "ab", "ab", (nint)(12));

    fmt.Printf("[%.1f] [%0.1f] [%6.2f] [%-7.2f] [%.0f]\n"u8, 45.678D, 45.678D, 3.14159D, 3.14159D, 2.71D);

    fmt.Printf("[%2d] [%2s]\n"u8, (nint)(12345), "hello");

    fmt.Printf("[% d] [% 6s]\n"u8, (nint)(-3), "ab");

    var zero = 0.0D;
    var posInf = 1.0D / zero;
    var negInf = -1.0D / zero;
    var nan = zero / zero;
    var posInf32 = (float32)posInf;
    var negInf32 = (float32)negInf;

    fmt.Println(posInf, negInf, nan);
    fmt.Println(posInf32, negInf32);
    fmt.Printf("[%v] [%v] [%g] [%g]\n"u8, posInf, negInf, posInf, negInf);
    fmt.Printf("[%e] [%e] [%f] [%f]\n"u8, posInf, negInf, posInf, negInf);
    fmt.Printf("[%.2f] [%.2f] [%8.2f] [%-8.2f]\n"u8, posInf, negInf, posInf, negInf);
    fmt.Printf("[%+v] [%+f] [% f] [% f]\n"u8, posInf, posInf, posInf, negInf);
    fmt.Printf("[%08f] [%08f] [%08f]\n"u8, posInf, negInf, nan);
    fmt.Printf("[%v] [%g] [%e] [%f]\n"u8, posInf32, negInf32, posInf32, negInf32);
    fmt.Println(fmt.Sprint(posInf), fmt.Sprint(negInf));
    fmt.Printf("[%v] [%f] [%+f] [% f]\n"u8, nan, nan, nan, nan);

    var bits = (uint64)9218868437227405311UL;
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, bits, bits, bits, bits);
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, (nint)(255), (nint)(255), (nint)(255), (nint)(255));
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, (nint)(-255), (nint)(-255), (nint)(-255), (nint)(-255));
    fmt.Printf("[%x] [%#x] [%x] [%#x]\n"u8, (nint)(0), (nint)(0), (nint)(15), (nint)(15));

    fmt.Printf("[%8x] [%-8x] [%08x] [%20x]\n"u8, (nint)(255), (nint)(255), (nint)(255), bits);
    fmt.Printf("[%#8x] [%#-8x] [%#08x] [%#016x]\n"u8, (nint)(255), (nint)(255), (nint)(255), bits);
    fmt.Printf("[%08x] [%#08x] [%#08X] [%04x]\n"u8, (nint)(-255), (nint)(-255), (nint)(-255), (nint)(255));
    fmt.Printf("[%+x] [% x] [%+#x] [%+08x] [% 08x]\n"u8, (nint)(255), (nint)(255), (nint)(255), (nint)(255), (nint)(255));

    fmt.Printf("[%.4x] [%.4x] [%#.4x] [%8.4x] [%-8.4x]\n"u8, (nint)(255), (nint)(-255), (nint)(255), (nint)(255), (nint)(255));
    fmt.Printf("[%.0x] [%.0x] [%5.2x] [%05.2x]\n"u8, (nint)(0), (nint)(255), (nint)(255), (nint)(255));

    fmt.Printf("[%x] [%#x] [%x] [%x]\n"u8, (int8)(-128), (int8)(-128), (uint8)255, (nuint)255);
    fmt.Printf("[%x] [%#x]\n"u8, (uint64)18446744073709551615UL, (int64)(-1));

    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, "abc", "abc", "abc", "abc");
    fmt.Printf("[% x] [% X] [% #x]\n"u8, "abc", "abc", "abc");
    fmt.Printf("[%x] [%#x] [% x]\n"u8, slice<byte>("abc"u8), slice<byte>("abc"u8), new byte[]{1, 2}.slice());
    fmt.Printf("[%x] [%.2x] [%.2x] [%#.2x]\n"u8, "äb", "äb", "abcd", "abcd");

    fmt.Printf("[%6x] [%-6x] [%06x] [%#08x] [%-08x]\n"u8, "ab", "ab", "ab", "ab", "ab");
    fmt.Printf("[% 08x] [%12x] [%10.3x]\n"u8, "ab", "abc", "abcdef");
    fmt.Printf("[%x] [%#x] [%8x] [%08x] [%#x]\n"u8, "", "", "", "", new byte[]{}.slice());

    fmt.Printf("[%b] [%b] [%b] [%b] [%#b] [%#b] [%#b]\n"u8, (nint)(5), (nint)(255), (nint)(-255), (nint)(0), (nint)(5), (nint)(-5), (nint)(0));
    fmt.Printf("[%+b] [% b] [% #b] [%12b] [%-12b] [%012b] [%012b]\n"u8, (nint)(5), (nint)(5), (nint)(5), (nint)(255), (nint)(255), (nint)(255), (nint)(-255));
    fmt.Printf("[%#12b] [%#012b] [%+012b] [%#012b]\n"u8, (nint)(255), (nint)(255), (nint)(255), (nint)(-255));
    fmt.Printf("[%.12b] [%.12b] [%#.12b] [%16.12b] [%-16.12b] [%.0b] [%.0b]\n"u8, (nint)(255), (nint)(-255), (nint)(255), (nint)(255), (nint)(255), (nint)(0), (nint)(255));
    fmt.Printf("[%b] [%b] [%b] [%b]\n"u8, (int8)(-128), (uint8)255, (uint64)18446744073709551615UL, (int64)(-9223372036854775808L));

    fmt.Printf("[%o] [%o] [%o] [%o] [%#o] [%#o] [%#o] [%#o]\n"u8, (nint)(8), (nint)(255), (nint)(-255), (nint)(0), (nint)(8), (nint)(-8), (nint)(0), (nint)(1));
    fmt.Printf("[%+o] [% o] [%+#o] [%8o] [%-8o] [%08o] [%08o]\n"u8, (nint)(8), (nint)(8), (nint)(8), (nint)(255), (nint)(255), (nint)(255), (nint)(-255));
    fmt.Printf("[%#8o] [%#08o] [%#-8o] [%#010o] [%#o] [%#5o]\n"u8, (nint)(255), (nint)(255), (nint)(255), (nint)(-8), (nint)(511), (nint)(511));
    fmt.Printf("[%.6o] [%.6o] [%#.6o] [%8.6o] [%.0o] [%.0o] [%#.0o] [%#.2o] [%#.2o]\n"u8, (nint)(255), (nint)(-255), (nint)(255), (nint)(255), (nint)(0), (nint)(255), (nint)(0), (nint)(8), (nint)(1));
    fmt.Printf("[%o] [%o] [%o]\n"u8, (int8)(-128), (uint8)255, (uint64)18446744073709551615UL);

    fmt.Printf("[%O] [%O] [%O] [%O] [%#O] [%#O] [%#O] [% #O]\n"u8, (nint)(8), (nint)(255), (nint)(-255), (nint)(0), (nint)(8), (nint)(-8), (nint)(0), (nint)(8));
    fmt.Printf("[%12O] [%-12O] [%012O] [%012O] [%+O] [% O] [%+012O]\n"u8, (nint)(255), (nint)(255), (nint)(255), (nint)(-255), (nint)(8), (nint)(8), (nint)(255));
    fmt.Printf("[%.6O] [%12.6O] [%#.6O] [%#12O] [%#012O]\n"u8, (nint)(255), (nint)(255), (nint)(255), (nint)(255), (nint)(255));
    fmt.Printf("[%O] [%O]\n"u8, (uint64)18446744073709551615UL, (int64)(-9223372036854775808L));

    fmt.Printf("[%#.0x] [%+.0x] [%5.0x] [%#05.0x] [%#.0b] [%.0O] [%-3.0x]\n"u8, (nint)(0), (nint)(0), (nint)(0), (nint)(0), (nint)(0), (nint)(0), (nint)(0));

    var negZero = -zero;
    fmt.Printf("[%b] [%b] [%b] [%b] [%b]\n"u8, 1.0D, 8.0D, 0.5D, 3.14159D, -3.14159D);
    fmt.Printf("[%b] [%b] [%b] [%b]\n"u8, 0.0D, negZero, 1e300D, 5e-324D);
    fmt.Printf("[%b] [%b] [%b] [%b]\n"u8, (float32)1.0F, (float32)0.1F, (float32)(-2.5F), (float32)16384.0F);
    fmt.Printf("[%b] [%b] [%b] [%.3b] [%#b]\n"u8, posInf, negInf, nan, 3.14159D, 1.5D);
    fmt.Printf("[%25b] [%-25b] [%025b] [%025b] [%+b] [% b]\n"u8, 1.5D, 1.5D, 1.5D, -1.5D, 1.5D, 1.5D);
    fmt.Printf("[%b] [%b]\n"u8, 9007199254740992.0D, 1.7976931348623157e308D);

    fmt.Printf("[%x] [%X] [%x] [%x] [%x] [%x]\n"u8, 3.14D, 3.14D, -3.14D, 1.0D, 2.0D, 0.5D);
    fmt.Printf("[%x] [%x] [%x] [%x]\n"u8, 0.0D, negZero, 1e300D, 5e-324D);
    fmt.Printf("[%x] [%X] [%x] [%x]\n"u8, (float32)0.1F, (float32)0.1F, (float32)(-2.5F), (float32)3.4028235e38F);
    fmt.Printf("[%x] [%x] [%x] [%.2x] [%.2X]\n"u8, posInf, negInf, nan, posInf, nan);
    fmt.Printf("[%x] [%x]\n"u8, 255.0D, 4503599627370495.5D);

    fmt.Printf("[%.2x] [%.0x] [%.0x] [%.0x] [%.5x] [%.13x] [%.15x]\n"u8, 3.14D, 1.9D, 1.5D, 2.5D, 3.14D, 3.14D, 3.14D);
    fmt.Printf("[%.2x] [%.2x] [%.10x] [%.1x] [%.1x] [%.4x]\n"u8, 0.0D, 1.0D, 3.14D, 1.999999D, 0.09999D, 0.1D);
    fmt.Printf("[%.3x] [%.1x] [%.0x]\n"u8, 1.9999999D, 1.99D, 255.9999D);

    fmt.Printf("[%20x] [%-20x] [%020x] [%020x] [%+x] [% x] [%+020x]\n"u8, 1.5D, 1.5D, 1.5D, -1.5D, 1.5D, 1.5D, 1.5D);
    fmt.Printf("[%030x] [%08x]\n"u8, negInf, nan);

    fmt.Printf("[%#x] [%#x] [%#x] [%#x] [%#x] [%#x]\n"u8, 1.0D, 1.5D, 3.14D, 0.0D, negZero, 255.0D);
    fmt.Printf("[%#X] [%#X] [%#.2X] [%#.2x] [%#.0x] [%#020x] [%#16x] [%#x]\n"u8, 1.0D, 1.5D, 3.14D, 3.14D, 3.14D, 1.5D, 1.5D, -1.5D);
}

} // end main_package
