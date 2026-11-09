namespace go.go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class anonifacedbg_package {

partial struct Kind /*num:nint*/;

public partial interface Fail_n /*dyn*/ {
    @string KindString();
}

public static @string Fail(Fail_n n, params ꓸꓸꓸany messageʗp) {
    var message = messageʗp.sslice();

    return "fail:"u8 + n.KindString() + fmt.Sprint(message.ꓸꓸꓸ);
}

public partial interface Describe_d /*dyn*/ {
    @string Name();
    Kind Kind();
}

public static @string Describe(Describe_d d) {
    return fmt.Sprintf("%s/%d"u8, d.Name(), d.Kind());
}

} // end anonifacedbg_package
