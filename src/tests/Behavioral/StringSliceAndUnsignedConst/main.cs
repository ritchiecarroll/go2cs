namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 takesUint32(uint32 v) {
    return v;
}

internal static uintptr big => unchecked((uintptr)33054211828000289);

internal const float32 hashLoad = /* float32(13) / float32(2) */ 6.5f;

internal static void Main() {
    fmt.Println(takesUint32(0x80000000U));

    fmt.Println(big);
    fmt.Println(hashLoad);

    @string env = "cpu.feature=on";

    fmt.Println(env[..4] == "cpu.");

    @string field = default;
    (field, env) = (env[..4], env[5..]);
    fmt.Println(field, env);

    (field, env) = (env, "");
    fmt.Printf("[%s][%s]\n"u8, field, env);
}

} // end main_package
