namespace go;

using fmt = fmt_package;
using Δos = os_package;
using signal = go.os.signal_package;
using go.os;

partial class main_package {

internal static void Main() {
    fmt.Println("initially ignored:", signal.Ignored(Δos.Interrupt));

    signal.Ignore(Δos.Interrupt);
    fmt.Println("after Ignore:", signal.Ignored(Δos.Interrupt));

    var c = new channel<osꓸSignal>(1);
    signal.Notify(c.AsSendOnly(), Δos.Interrupt);
    fmt.Println("after Notify:", signal.Ignored(Δos.Interrupt));

    signal.Stop(c.AsSendOnly());
    fmt.Println("after Stop:", signal.Ignored(Δos.Interrupt));

    signal.Ignore(Δos.Interrupt);
    fmt.Println("after Ignore again:", signal.Ignored(Δos.Interrupt));

    signal.Reset(Δos.Interrupt);
    fmt.Println("after Reset:", signal.Ignored(Δos.Interrupt));
}

} // end main_package
