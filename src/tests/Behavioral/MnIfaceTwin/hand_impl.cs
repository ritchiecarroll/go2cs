namespace go;

using time = time_package;

// M:N scheduler, experimental and opt-in (GoTwins=true to build, GO2CS_MN=1 to run); inert otherwise.
// What a C# consumer writes against a Go interface, by hand and with no knowledge of twins: a class that
// implements the interface, and a class that only has the method (duck-typed at the assertion). This file must
// compile unchanged with twins off, under the hidden shape and under the default-bodied shape.
partial class main_package {

public sealed class HandParker : Parker {
    public nint Park(time.Duration d) {
        time.Sleep(d);
        return 5;
    }
}

public sealed class DuckParker {
    public nint Park(time.Duration d) {
        return 7;
    }
}

[System.Runtime.CompilerServices.ModuleInitializer]
internal static void HandInit() {
    if (System.Environment.GetEnvironmentVariable("MN_IFACE_HAND") != "1")
        return;

    Hand = new HandParker();
    System.Console.Error.WriteLine("clr-methods " + typeof(Parker).GetMethods().Length);

    try {
        any z = new DuckParker();
        var (q, ok) = z._<Parker>(ᐧ);
        System.Console.Error.WriteLine("duck " + ok + " " + (ok ? q.Park(0) : -1));
    }
    catch (System.Exception ex) {
        System.Console.Error.WriteLine("duck threw " + ex.GetType().Name);
    }
}

}
