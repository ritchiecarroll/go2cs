namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Employee {
    public @string Name;
    public nint Age;
}

internal partial struct toAnonymous_q /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void toAnonymous() {
    var e = Ꮡ(new Employee(Name: "ada", Age: 13));
    var q = e.Reinterpret<Employee, toAnonymous_q>();

    q.Value.Age = 14;
    fmt.Println("to anonymous, write through converted:", e.Age);
}

internal partial struct tagsOnly_t /*dyn*/ {
    public @string Name; /*`json:"name"`*/
    public nint Age; /*`json:"age"`*/
}

internal static void tagsOnly() {
    var t = Ꮡ(new tagsOnly_t("bea", 15));
    var u = t.Reinterpret<tagsOnly_t, toAnonymous_q>();

    u.Value.Age = 16;
    fmt.Println("tags only, write through converted:", (~t).Age);
}

internal static void Main() {
    toAnonymous();
    tagsOnly();
}

} // end main_package
