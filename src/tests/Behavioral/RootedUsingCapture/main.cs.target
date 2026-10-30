namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using lib = RootedUsingCapture.lib_package;
using RootedUsingCapture;

partial class main_package {

internal static void Main() {
    var t = reflect.TypeOf(new lib.Guarded());
    var f = t.Field(0);
    fmt.Println("embed:", f.Name, f.Anonymous);
    fmt.Println("type:", f.Type.String());
    fmt.Println("pkgpath:", f.Type.PkgPath());
    fmt.Println("pointer methods:", reflect.PointerTo(f.Type).NumMethod());
    var (_, tryLock) = reflect.PointerTo(t).MethodByName("TryLock");
    fmt.Println("promoted TryLock:", tryLock);

    var g = lib.New(lib.Base());
    fmt.Println("add:", g.Add(4));
    fmt.Printf("%T\n"u8, g.ᏑMutex);
}

} // end main_package
