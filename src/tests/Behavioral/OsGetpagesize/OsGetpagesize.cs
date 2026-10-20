namespace go;

using fmt = fmt_package;
using os = os_package;
using syscall = syscall_package;

partial class main_package {

internal static void Main() {
    nint size = os.Getpagesize();
    fmt.Println("page size:", size);
    fmt.Println("matches syscall.Getpagesize:", size == syscall.Getpagesize());
    fmt.Println("positive power of two:", size > 0 && (size & (size - 1)) == 0);
}

} // end main_package
