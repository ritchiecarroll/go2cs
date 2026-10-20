namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct data {
    internal @string name;
}

internal static void printName(this data d) {
    fmt.Println("Name =", d.name);
}

internal static void Main() {
    var d = new data(name: "James");
    
    var dʗ1 = d;
    var f1 = () => dʗ1.printName();
    f1();
    d.name = "Gretchen";
    f1();
}

} // end main_package
