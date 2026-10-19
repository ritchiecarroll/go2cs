namespace go;

using fmt = fmt_package;
using consumer = GoHostModuleShadow.consumer_package;
using GoHostModuleShadow;

partial class main_package {

internal static void Main() {
    fmt.Println(consumer.Describe("GoLang"));
}

} // end main_package
