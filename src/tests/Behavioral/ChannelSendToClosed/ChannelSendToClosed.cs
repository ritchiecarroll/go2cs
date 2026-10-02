namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static void Main() {
    channel<nint> c = new channel<nint>(100);
    for (nint i = 0; i < 10; i++) {
        var cʗ1 = c;
        goǃ(() => {
            for (nint j = 0; j < 10; j++) {
                cʗ1.ᐸꟷ(j);
            }
            close(cʗ1);
        });
    }
    foreach (var i in c) {
        fmt.Println(i);
    }
}

} // end main_package
