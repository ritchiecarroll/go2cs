namespace go;

using fmt = fmt_package;
using @extension = global::go.go.kwlib.extension_package;
using @file = global::go.go.kwlib.file_package;
using @required = global::go.go.kwlib.required_package;
using @scoped = global::go.go.kwlib.scoped_package;
using global::go.go.kwlib;

partial class main_package {

internal static void main() {
    fmt.Println(extension.Name(), @file.Name(), @required.Name(), @scoped.Name());

    @extension.Kind e = 1;
    @file.Kind f = 2;
    @required.Kind r = 3;
    @scoped.Kind s = 4;
    fmt.Println(e.Double(), f.Double(), r.Double(), s.Double());
    fmt.Println(extension.Scaled(5), local());
}

internal static nint local() {
    nint extension = 7;
    return extension;
}

} // end main_package
