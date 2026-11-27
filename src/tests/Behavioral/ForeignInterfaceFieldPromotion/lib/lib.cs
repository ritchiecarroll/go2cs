namespace go.ForeignInterfaceFieldPromotion;

partial class lib_package {

partial interface I {
    @string m();
    @string Name();
}

partial struct T {
    internal @string name;
}

internal static @string m⁔(/*this*/ T t) { return "lib.m of "u8 + t.name; }
internal static @string Name⁔(/*this*/ T t) { return t.name; }

public static I New(@string name) { return new T(name: name); }

public static @string CallM(I i) { return i.m(); }

partial interface Sentinel {
    void sentinel();
}

partial struct mark {
}

internal static void sentinel⁔(/*this*/ mark _) { }

public static Sentinel Mark = new mark();

public static bool IsMarked(any x) {
    var (_, ok) = x._<Sentinel>(ᐧ);
    return ok;
}

} // end lib_package
