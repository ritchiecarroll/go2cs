namespace go.EmbeddedTypeNameCollision;

using fmt = fmt_package;

partial class inner_package {

partial struct Buffer {
    public @string Data;
    public nint N;
}

public static Buffer NewBuffer(@string data, nint n) {
    return new Buffer(Data: data, N: n);
}

public static @string Describe(this Buffer b) {
    return fmt.Sprintf("inner.Buffer(%s,%d)"u8, b.Data, b.N);
}

public static void Bump(this ref Buffer b) {
    b.N++;
}

public static void Append(this ref Buffer b, @string s) {
    b.Data += s;
    b.N += len(s);
}

} // end inner_package
