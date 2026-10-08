namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct data {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nameˢ = (@string)"Name ="u8;

internal static void printName(this data d) {
    fmt.Println(nameˢ, d.name);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gretchenˢ = "Gretchen"u8;

internal static void Main() {
    var d = new data(name: "James"u8);
    
    var dʗ1 = d;
    var f1 = () => dʗ1.printName();
    f1();
    d.name = gretchenˢ;
    f1();
}

} // end main_package
