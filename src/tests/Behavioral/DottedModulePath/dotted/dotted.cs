namespace go.example.com.dotted;

using runtime = runtime_package;

partial class dotted_package {

partial struct T {
    public nint N;
}

public static nint F() { return 7; }

public static partial @string Where() {
    var (pc, _, _, ok) = runtime.Caller(0);
    if (!ok) {
        return "no caller";
    }
    return runtime.FuncForPC(pc).Name();
}

} // end dotted_package
