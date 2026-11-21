namespace go;

using fmt = fmt_package;
// import _ "unsafe" (nothing to initialize)
// import _ "LinknameVarPullLib" (nothing to initialize)

partial class main_package {

internal static @string secret { get => go.LinknameVarPullLib_package.secret; set => go.LinknameVarPullLib_package.secret = value; }

internal static void main() {
    fmt.Println(secret);
}

} // end main_package
