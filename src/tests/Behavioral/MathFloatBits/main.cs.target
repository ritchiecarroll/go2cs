namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

internal static void Main() {
    var z = 0.0D;
    var nz = -z;
    fmt.Println(Δmath.Float64bits(z));
    fmt.Println(Δmath.Float64bits(nz));

    var pinf = Δmath.Float64frombits(0x7FF0000000000000UL);
    var ninf = Δmath.Float64frombits(0xFFF0000000000000UL);
    fmt.Println(pinf > 1e308D);
    fmt.Println(ninf < -1e308D);
    fmt.Println(Δmath.Float64bits(pinf));
    fmt.Println(Δmath.Float64bits(ninf));
    fmt.Println(Δmath.Float64bits(1D / ninf));

    var nan = Δmath.Float64frombits(0x7FF8000000000001UL);
    fmt.Println(nan != nan);
    fmt.Println(Δmath.Float64bits(nan));

    var sub = Δmath.Float64frombits(1);
    fmt.Println(sub > 0D);
    fmt.Println(Δmath.Float64bits(sub));
    fmt.Println(Δmath.Float64bits(5e-324D));

    fmt.Println(Δmath.Float64bits(1.0D));
    fmt.Println(Δmath.Float64frombits(0x400921FB54442D18UL));
    fmt.Println(Δmath.Float64bits(Δmath.Float64frombits(0x400921FB54442D18UL)) == 0x400921FB54442D18UL);

    var x = 8.0D;
    var t = Δmath.Float64frombits(Δmath.Float64bits(x) / 3 + ((uint64)715094163 << 32));
    fmt.Println(Δmath.Float64bits(t));

    var fz = (float32)0.0F;
    var fnz = -fz;
    fmt.Println(Δmath.Float32bits(fz));
    fmt.Println(Δmath.Float32bits(fnz));

    var qnan = Δmath.Float32frombits(0x7FC00001);
    var snan = Δmath.Float32frombits(0x7F800001);
    fmt.Println(qnan != qnan);
    fmt.Println(snan != snan);
    fmt.Println(Δmath.Float32bits(qnan));
    fmt.Println(Δmath.Float32bits(snan));

    var fsub = Δmath.Float32frombits(1);
    fmt.Println(fsub > 0F);
    fmt.Println(Δmath.Float32bits(fsub));
    fmt.Println(Δmath.Float32bits(Δmath.Float32frombits(0x3F9D70A4)) == 0x3F9D70A4);
    fmt.Println(Δmath.Float32bits((float32)1.0F));
    fmt.Println(Δmath.Float32frombits(0x40490FDB));
}

} // end main_package
