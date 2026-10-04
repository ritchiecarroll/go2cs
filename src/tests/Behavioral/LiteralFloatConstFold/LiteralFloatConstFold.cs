namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var q = new float64[]{0D, /* 1 - .999 */ 0.001D, /* 1 - .99 */ 0.01D, /* 1 - .95 */ 0.05D}.slice();
    var x = /* 0.1 + 0.2 */ 0.3D;
    var y = 1.5D * 2.0D;
    var z = 7 / 2 + .5D;
    float32 f = 0.1F + 0.2F;
    var w = /* (1 - .999) * 1000 */ 1D;
    fmt.Println(q, x, y, z, f, w);
    fmt.Println(q[1] == 0.001D, x == 0.3D, w == 1D, y == 3D, z == 3.5D);
}

} // end main_package
