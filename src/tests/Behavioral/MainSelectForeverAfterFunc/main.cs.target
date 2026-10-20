namespace go;

using fmt = fmt_package;
using os = os_package;
using time = time_package;

partial class main_package {

internal static void Main() {
    time.AfterFunc(500 * time.Millisecond, () => {
        fmt.Println("the timer fired while main sat in select {}");
        os.Exit(0);
    });

    select();
}

} // end main_package
