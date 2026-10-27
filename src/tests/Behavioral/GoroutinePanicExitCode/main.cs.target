namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void Main() {
    fmt.Println("before goroutine panic");

    var done = new channel<EmptyStruct>(0);

    goǃ(() => {
        throw panic("goroutine boom");
    }, GoStack.Bounded);

    ᐸꟷ(done);
}

} // end main_package
