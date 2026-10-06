namespace go;

using fmt = fmt_package;
using static UsingStaticNamespaceAlias.aliaslib_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;

partial class main_package {

[GoType] partial struct noCopy {
}

[GoType] [StructLayout(LayoutKind.Explicit, Size = 4)] partial struct counter {
    [FieldOffset(0)] internal readonly noCopy _;
    [FieldOffset(0)] internal int32 v;
}

internal static partial void launch(channel<@string> done) {
    var doneʗ1 = done;
    goǃ(() => {
        doneʗ1.ᐸꟷ(Marshal(Unsafe));
    });
}

internal static void Main() {
    var done = new channel<@string>(0);
    launch(done);
    fmt.Println(ᐸꟷ(done));
    var c = new counter(v: 7);
    fmt.Println(Closure((nint)c.v));
    fmt.Println(Unsafe);
}

} // end main_package
