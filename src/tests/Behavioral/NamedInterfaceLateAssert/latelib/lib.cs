namespace go.NamedInterfaceLateAssert;

partial class latelib_package {

partial interface Base {
    @string Name();
}

partial interface Mid :
    Base
{
    @string Kind();
}

partial interface Top :
    Mid
{
    @string Detail(@string prefix);
}

partial interface quiet {
    @string Whisper();
}

public static (@string, bool) TryTop(any v) {
    {
        var (t, ok) = v._<Top>(ᐧ); if (ok) {
            return (t.Name() + "/" + t.Kind() + "/" + t.Detail("d"u8), true);
        }
    }
    return ("", false);
}

public static (@string, bool) TryMid(any v) {
    {
        var (m, ok) = v._<Mid>(ᐧ); if (ok) {
            return (m.Name() + "/" + m.Kind(), true);
        }
    }
    return ("", false);
}

public static (@string, bool) TryBase(any v) {
    {
        var (b, ok) = v._<Base>(ᐧ); if (ok) {
            return (b.Name(), true);
        }
    }
    return ("", false);
}

public static (@string, bool) TryQuiet(any v) {
    {
        var (q, ok) = v._<quiet>(ᐧ); if (ok) {
            return (q.Whisper(), true);
        }
    }
    return ("", false);
}

} // end latelib_package
