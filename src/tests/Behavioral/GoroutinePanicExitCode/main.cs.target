namespace go;

using fmt = fmt_package;

partial class main_package {

internal static partial void main() {
    fmt.Println("before goroutine panic");

    var done = new channel<EmptyStruct>(0);

    goǃ(() => {
        throw panic("goroutine boom");
    });

    ᐸꟷ(done);
}

} // end main_package
