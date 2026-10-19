namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using lib = RootedUsingCapture.lib_package;
using RootedUsingCapture;
using Δsync = sync_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object embedˢ = (@string)"embed:"u8;
private static readonly object typeˢ = (@string)"type:"u8;
private static readonly object pkgpathˢ = (@string)"pkgpath:"u8;
private static readonly object pointerMethodsˢ = (@string)"pointer methods:"u8;
private static readonly @string tryLockˢ = "TryLock"u8;
private static readonly object promotedTryLockˢ = (@string)"promoted TryLock:"u8;
private static readonly object addˢ = (@string)"add:"u8;

internal static void Main() {
    var t = reflect.TypeOf(new lib.Guarded(nil));
    var f = t.Field(0);
    fmt.Println(embedˢ, f.Name, f.Anonymous);
    fmt.Println(typeˢ, f.Type.String());
    fmt.Println(pkgpathˢ, f.Type.PkgPath());
    fmt.Println(pointerMethodsˢ, reflect.PointerTo(f.Type).NumMethod());
    var (_, tryLock) = reflect.PointerTo(t).MethodByName(tryLockˢ);
    fmt.Println(promotedTryLockˢ, tryLock);

    var g = lib.New(lib.Base());
    fmt.Println(addˢ, g.Add(4));
    fmt.Printf("%T\n"u8, g.of(lib.Guarded.ᏑMutex));
}

} // end main_package
