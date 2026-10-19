namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Employee {
    public @string Name;
    public nint Age;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object toAnonymousWriteThroughˢ = (@string)"to anonymous, write through converted:"u8;

internal partial struct toAnonymous_q /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void toAnonymous() {
    var e = Ꮡ(new Employee(Name: "ada"u8, Age: 13));
    var q = e.Reinterpret<Employee, toAnonymous_q>();

    q.Value.Age = 14;
    fmt.Println(toAnonymousWriteThroughˢ, (~e).Age);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tagsOnlyWriteThroughˢ = (@string)"tags only, write through converted:"u8;

internal partial struct tagsOnly_t /*dyn*/ {
    public @string Name; /*`json:"name"`*/
    public nint Age; /*`json:"age"`*/
}

internal static void tagsOnly() {
    var t = Ꮡ(new tagsOnly_t("bea"u8, 15));
    var u = t.Reinterpret<tagsOnly_t, toAnonymous_q>();

    u.Value.Age = 16;
    fmt.Println(tagsOnlyWriteThroughˢ, (~t).Age);
}

internal static void Main() {
    toAnonymous();
    tagsOnly();
}

} // end main_package
