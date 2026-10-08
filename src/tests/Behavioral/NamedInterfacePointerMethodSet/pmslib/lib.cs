namespace go.NamedInterfacePointerMethodSet;

partial class pmslib_package {

partial interface Speaker {
    @string Speak();
}

partial interface Getter {
    @string Get();
}

public static (@string, bool) TrySpeak(any v) {
    {
        var (s, ok) = v._<Speaker>(ᐧ); if (ok) {
            return (s.Speak(), true);
        }
    }
    return ("", false);
}

public static (@string, bool) TryGet(any v) {
    {
        var (g, ok) = v._<Getter>(ᐧ); if (ok) {
            return (g.Get(), true);
        }
    }
    return ("", false);
}

} // end pmslib_package
