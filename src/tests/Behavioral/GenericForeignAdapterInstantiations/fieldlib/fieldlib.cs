namespace go.GenericForeignAdapterInstantiations;

using fmt = fmt_package;
using static fieldlib_package;

partial class fieldlib_package {

partial interface Field {
    bool IsPresent();
    @string Describe();
}

partial struct Expected<T> {
    public T Value;
    public bool Valid;
}

} partial class fieldlibᴛExpected {

public static bool IsPresent<T>(this ref Expected<T> e) { return e.Valid; }

public static @string Describe<T>(this ref Expected<T> e) { return fmt.Sprintf("%v"u8, (e.Value).OrTypedNilOf()); }

} partial class fieldlib_package {

partial interface Getter {
    @string Get();
}

partial struct Box<T> {
    public T Item;
}

} partial class fieldlibᴛBox {

public static T Get<T>(this ref Box<T> b) { return b.Item; }

} // end fieldlibᴛBox
