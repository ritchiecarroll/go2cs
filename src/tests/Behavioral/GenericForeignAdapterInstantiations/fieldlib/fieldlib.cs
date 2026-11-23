namespace go.GenericForeignAdapterInstantiations;

using fmt = fmt_package;

partial class fieldlib_package {

partial interface Field {
    bool IsPresent();
    @string Describe();
}

partial struct Expected<T> {
    public T Value;
    public bool Valid;
}

internal static bool IsPresent⁔<T>(/*this*/ ref Expected<T> e) { return e.Valid; }

internal static @string Describe⁔<T>(/*this*/ ref Expected<T> e) { return fmt.Sprintf("%v"u8, (e.Value).OrTypedNilOf()); }

partial interface Getter {
    @string Get();
}

partial struct Box<T> {
    public T Item;
}

internal static T Get⁔<T>(/*this*/ ref Box<T> b) { return b.Item; }

} // end fieldlib_package
