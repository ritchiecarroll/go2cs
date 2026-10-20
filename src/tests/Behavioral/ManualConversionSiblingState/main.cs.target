namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void reportProcs() {
    fmt.Println("newprocs:", newprocs);
}

internal static void Main() {
    newprocs = 4;

    sched.disable.user = true;
    var np = Ꮡsched.of(schedlike.Ꮡdisable).of(schedlike_disable.Ꮡn);
    np.Value = 7;

    sched.label = "ok";

    reportProcs();
    fmt.Println("disable:", sched.disable.user, sched.disable.n);
    fmt.Println("label:", sched.label);
}

} // end main_package
