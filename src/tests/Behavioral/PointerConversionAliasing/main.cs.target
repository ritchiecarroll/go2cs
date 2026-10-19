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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string bobˢ = "bob"u8;
private static readonly object namedWriteThroughˢ = (@string)"named, write through converted:"u8;
private static readonly object namedWriteThroughˢ2 = (@string)"named, write through original:"u8;
private static readonly object namedSameAddressˢ = (@string)"named, same address:"u8;
private static readonly object namedRoundTripIdentityˢ = (@string)"named, round trip identity:"u8;

internal static void named() {
    var e = Ꮡ(new Employee(Name: "ann"u8, Age: 30));
    var p = e.Reinterpret<Employee, Person>();

    p.Value.Name = bobˢ;
    fmt.Println(namedWriteThroughˢ, (~e).Name);

    e.Value.Age = 31;
    fmt.Println(namedWriteThroughˢ2, (~p).Age);

    fmt.Println(namedSameAddressˢ, @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(e));
    fmt.Println(namedRoundTripIdentityˢ, p.Reinterpret<Person, Employee>() == e);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nilStaysNilˢ = (@string)"nil, stays nil:"u8;

internal static void nilPointer() {
    ж<Employee> e = default!;
    var p = e.Reinterpret<Employee, Person>();
    fmt.Println(nilStaysNilˢ, p == nil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object unsafeWriteThroughˢ = (@string)"unsafe, write through converted:"u8;
private static readonly object unsafeRoundTripIdentityˢ = (@string)"unsafe, round trip identity:"u8;

internal static void viaUnsafe() {
    var e = Ꮡ(new Employee(Name: "cat"u8, Age: 5));
    var p = e.Reinterpret<Employee, Person>();

    p.Age = 6;
    fmt.Println(unsafeWriteThroughˢ, (~e).Age);
    fmt.Println(unsafeRoundTripIdentityˢ, p.Reinterpret<Person, Employee>() == e);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object anonymousWriteThroughˢ = (@string)"anonymous, write through converted:"u8;
private static readonly @string dovˢ = "dov"u8;
private static readonly object anonymousWriteThroughˢ2 = (@string)"anonymous, write through original:"u8;
private static readonly object anonymousSameAddressˢ = (@string)"anonymous, same address:"u8;

internal partial struct anonymous_a /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void anonymous() {
    var a = Ꮡ(new anonymous_a("dot"u8, 7));
    var p = a.Reinterpret<anonymous_a, Person>();

    p.Value.Age = 8;
    fmt.Println(anonymousWriteThroughˢ, (~a).Age);

    a.Value.Name = dovˢ;
    fmt.Println(anonymousWriteThroughˢ2, (~p).Name);
    fmt.Println(anonymousSameAddressˢ, @unsafe.Pointer.FromPinnedBox(p) == @unsafe.Pointer.FromPinnedBox(a));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object anonymousNilStaysNilˢ = (@string)"anonymous nil, stays nil:"u8;

internal partial struct anonymousNil_a /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void anonymousNil() {
    ж<anonymousNil_a> a = default!;
    var p = a.Reinterpret<anonymousNil_a, Person>();
    fmt.Println(anonymousNilStaysNilˢ, p == nil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object tagsOnlyNamedWriteˢ = (@string)"tags only named, write through converted:"u8;
private static readonly object tagsOnlyNamedRoundTripˢ = (@string)"tags only named, round trip identity:"u8;

internal static void tagsOnlyNamed() {
    var t = Ꮡ(new Tagged(Name: "kim"u8, Age: 18));
    var p = t.Reinterpret<Tagged, Person>();

    p.Value.Age = 19;
    fmt.Println(tagsOnlyNamedWriteˢ, (~t).Age);
    fmt.Println(tagsOnlyNamedRoundTripˢ, p.Reinterpret<Person, Tagged>() == t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object equalityTwoConversionsˢ = (@string)"equality, two conversions:"u8;
private static readonly object equalityConversionAndˢ = (@string)"equality, conversion and variable:"u8;

internal static void equality() {
    var e = Ꮡ(new Employee(Name: "cy"u8, Age: 17));
    var p = e.Reinterpret<Employee, Person>();
    fmt.Println(equalityTwoConversionsˢ, e.Reinterpret<Employee, Person>() == e.Reinterpret<Employee, Person>());
    fmt.Println(equalityConversionAndˢ, p == e.Reinterpret<Employee, Person>());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string fayˢ = "fay"u8;
private static readonly object definedWriteThroughˢ = (@string)"defined, write through converted:"u8;

internal static void definedType() {
    var e = Ꮡ(new Employee(Name: "eve"u8, Age: 9));
    var s = e.Reinterpret<Employee, Staff>();

    s.Value.Name = fayˢ;
    fmt.Println(definedWriteThroughˢ, (~e).Name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string halˢ = "hal"u8;
private static readonly object argumentWriteThroughˢ = (@string)"argument, write through converted:"u8;

internal static void callArgument() {
    var e = Ꮡ(new Employee(Name: "gus"u8, Age: 10));
    rename(e.Reinterpret<Employee, Person>(), halˢ);
    fmt.Println(argumentWriteThroughˢ, (~e).Name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sliceElementWriteThroughˢ = (@string)"slice element, write through converted:"u8;

internal static void sliceElement() {
    var es = new ж<Employee>[]{Ꮡ(new Employee(Name: "ivy"u8, Age: 11))}.slice();
    var p = es[0].Reinterpret<Employee, Person>();

    p.Value.Age = 12;
    fmt.Println(sliceElementWriteThroughˢ, (~es[0]).Age);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string janˢ = "jan"u8;
private static readonly object fieldAddressWriteThroughˢ = (@string)"field address, write through converted:"u8;

internal partial struct fieldAddress_pair /*dyn*/ {
    public Employee First;
    public Employee Second;
}

internal static void fieldAddress() {
    ref var pr = ref heap(new fieldAddress_pair(), out var Ꮡpr);
    var p = Ꮡpr.of(fieldAddress_pair.ᏑSecond).Reinterpret<Employee, Person>();

    p.Value.Name = janˢ;
    fmt.Println(fieldAddressWriteThroughˢ, pr.Second.Name);
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
