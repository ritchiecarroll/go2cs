namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Person {
    internal Action work;
    internal @string name;
    internal int32 age;
}

internal static void Main() {
    var person = new Person(work: default!, name: "Michał"u8, age: 29);
    fmt.Println(person);
}

} // end main_package
