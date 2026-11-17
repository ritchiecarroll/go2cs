namespace go;

using fmt = fmt_package;
using strings = strings_package;
using static main_package;

partial class main_package {

partial struct Person {
    internal @string name;
    internal int32 age;
}

} partial class mainᴛPerson {

public static bool IsDr(this Person p) {
    return strings.HasPrefix(p.name, "Dr");
}

public static bool IsAdult(this Person p) {
    return p.age >= 18;
}

} partial class main_package {

partial struct Employee {
    internal @string position;
}

} partial class mainᴛEmployee {

public static bool IsManager(this Employee e) {
    return e.position == "manager"u8;
}

} partial class main_package {

partial struct Record {
    /*embed*/ public Person Person;
    /*embed*/ public Employee Employee;
}

} partial class mainᴛRecord {

public static bool IsDr(this Record p) {
    return strings.HasPrefix(p.name, "Dr") && p.age > 18;
}

} partial class main_package {

partial struct commonBase {
    internal @string tag;
}

} partial class mainᴛcommonBase {

internal static @string describe(this ref commonBase c) {
    return "base:"u8 + c.tag;
}

} partial class main_package {

partial struct ledger {
    /*embed*/ internal commonBase commonBase;
    internal nint seq;
}

} partial class mainᴛledger {

internal static @string describe(this ж<ledger> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    var p = Ꮡl.Ꮡseq;
    p.Value++;
    return "ledger:"u8 + l.tag;
}

} partial class main_package {

internal static void main() {
    var person = new Person(name: "Dr. Michał", age: 29);
    fmt.Println(person);
    fmt.Println(person.IsDr());

    var record = new Record();
    record.name = "Dr. Michał";
    record.age = 18;
    record.position = "software engineer";

    fmt.Println(record);
    fmt.Println(record.name);
    fmt.Println(record.age);
    fmt.Println(record.position);
    fmt.Println(record.IsAdult());
    fmt.Println(record.IsManager());
    fmt.Println(record.IsDr());

    var l = Ꮡ(new ledger());
    l.tag = "x";
    fmt.Println(l.describe(), l.describe(), l.seq);
}

} // end main_package
