namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct arenaIdx /*num:nuint*/;

partial struct tag /*num:uint8*/;

partial struct big /*num:uint64*/;

internal static UntypedInt bits => 6;

internal static void Main() {
    arenaIdx a = (arenaIdx)((nuint)1 << (int)(bits));
    tag t = (tag)(uint8)(1 << (int)(3));
    big b = (big)((uint64)1 << (int)(40));
    fmt.Println((nuint)a, (uint8)t, (uint64)b);
}

} // end main_package
