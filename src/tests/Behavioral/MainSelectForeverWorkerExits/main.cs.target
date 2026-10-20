namespace go;

using fmt = fmt_package;
using os = os_package;
using time = time_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    goǃ(() => {
        time.Sleep(500 * time.Millisecond);
        fmt.Println("the worker served while main sat in select {}");
        os.Exit(0);
    });

    select();
}

} // end main_package
