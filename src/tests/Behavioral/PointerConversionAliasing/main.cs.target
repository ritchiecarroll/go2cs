namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

[GoFieldAccess(typeof(Person))]
partial class main_package {

partial struct Employee {
    public @string Name;
    public nint Age;
}

partial struct Person {
    public @string Name;
    public nint Age;
}

partial struct Staff /*Employee*/;

partial struct Tagged {
    public @string Name; /*`json:"name"`*/
    public nint Age; /*`json:"age"`*/
}

internal static void rename(ж<Person> Ꮡp, @string name) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.Name = name;
}

internal static void named() {
    var e = Ꮡ(new Employee(Name: "ann", Age: 30));
    var p = e.Reinterpret<Employee, Person>();

    p.Value.Name = "bob";
    fmt.Println("named, write through converted:", (~e).Name);

    e.Value.Age = 31;
    fmt.Println("named, write through original:", (~p).Age);

    fmt.Println("named, same address:", @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(e));
    fmt.Println("named, round trip identity:", p.Reinterpret<Person, Employee>() == e);
}

internal static void nilPointer() {
    ж<Employee> e = default;
    var p = e.Reinterpret<Employee, Person>();
    fmt.Println("nil, stays nil:", p == nil);
}

internal static void viaUnsafe() {
    var e = Ꮡ(new Employee(Name: "cat", Age: 5));
    var p = e.Reinterpret<Employee, Person>();

    p.Age = 6;
    fmt.Println("unsafe, write through converted:", (~e).Age);
    fmt.Println("unsafe, round trip identity:", p.Reinterpret<Person, Employee>() == e);
}

internal partial struct anonymous_a /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void anonymous() {
    var a = Ꮡ(new anonymous_a("dot", 7));
    var p = a.Reinterpret<anonymous_a, Person>();

    p.Value.Age = 8;
    fmt.Println("anonymous, write through converted:", (~a).Age);

    a.Value.Name = "dov";
    fmt.Println("anonymous, write through original:", (~p).Name);
    fmt.Println("anonymous, same address:", @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(a));
}

internal partial struct anonymousNil_a /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void anonymousNil() {
    ж<anonymousNil_a> a = default;
    var p = a.Reinterpret<anonymousNil_a, Person>();
    fmt.Println("anonymous nil, stays nil:", p == nil);
}

internal static void tagsOnlyNamed() {
    var t = Ꮡ(new Tagged(Name: "kim", Age: 18));
    var p = t.Reinterpret<Tagged, Person>();

    p.Value.Age = 19;
    fmt.Println("tags only named, write through converted:", (~t).Age);
    fmt.Println("tags only named, round trip identity:", p.Reinterpret<Person, Tagged>() == t);
}

internal static void equality() {
    var e = Ꮡ(new Employee(Name: "cy", Age: 17));
    var p = e.Reinterpret<Employee, Person>();
    fmt.Println("equality, two conversions:", e.Reinterpret<Employee, Person>() == e.Reinterpret<Employee, Person>());
    fmt.Println("equality, conversion and variable:", p == e.Reinterpret<Employee, Person>());
}

internal static void definedType() {
    var e = Ꮡ(new Employee(Name: "eve", Age: 9));
    var s = e.Reinterpret<Employee, Staff>();

    s.Value.Name = "fay";
    fmt.Println("defined, write through converted:", (~e).Name);
}

internal static void callArgument() {
    var e = Ꮡ(new Employee(Name: "gus", Age: 10));
    rename(e.Reinterpret<Employee, Person>(), "hal");
    fmt.Println("argument, write through converted:", (~e).Name);
}

internal static void sliceElement() {
    var es = new ж<Employee>[]{Ꮡ(new Employee(Name: "ivy"u8, Age: 11))}.slice();
    var p = es[0].Reinterpret<Employee, Person>();

    p.Value.Age = 12;
    fmt.Println("slice element, write through converted:", (~es[0]).Age);
}

internal partial struct fieldAddress_pair /*dyn*/ {
    public Employee First;
    public Employee Second;
}

internal static void fieldAddress() {
    ref var pr = ref heap(new fieldAddress_pair(), out var Ꮡpr);
    var p = Ꮡpr.of(fieldAddress_pair.ᏑSecond).Reinterpret<Employee, Person>();

    p.Value.Name = "jan";
    fmt.Println("field address, write through converted:", pr.Second.Name);
}

internal static void Main() {
    named();
    nilPointer();
    viaUnsafe();
    anonymous();
    anonymousNil();
    tagsOnlyNamed();
    equality();
    definedType();
    callArgument();
    sliceElement();
    fieldAddress();
}

} // end main_package
