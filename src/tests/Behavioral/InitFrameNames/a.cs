namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoInit] internal static partial void init() {
    fmt.Println((@string)"a.go init:"u8, here());
}

} // end main_package
