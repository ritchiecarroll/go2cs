namespace go;

using fmt = fmt_package;
using build = global::go.go.build_package;
using runtime = runtime_package;
using global::go.go;

partial class main_package {

internal static void main() {
    fmt.Println("runtime.GOOS:", runtime.GOOS);
    fmt.Println("runtime.GOARCH:", runtime.GOARCH);
    fmt.Println("build.Default.GOOS:", build.Default.GOOS);
    fmt.Println("build.Default.GOARCH:", build.Default.GOARCH);
    fmt.Println("the two agree:", build.Default.GOARCH == runtime.GOARCH);
}

} // end main_package
