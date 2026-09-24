namespace go;

// The shape a converted file gives a pointer-receiver method: a [GoType] struct and [GoRecv]
// `this ref T` methods inside the package class. RecvGenerator then emits the ж<T> overload.
partial class recvprobe_package {

[GoType] partial struct Recv {
    internal nint n;
}

// The probe: a pointer-receiver method whose parameter is sstring.
[GoRecv] public static nint PutS(this ref Recv r, sstring s) {
    r.n += s.Length;
    return r.n;
}

// The control: the same method taking @string.
[GoRecv] public static nint Put(this ref Recv r, @string s) {
    r.n += len(s);
    return r.n;
}

public static void Main() {
    ж<Recv> p = @new<Recv>();
    sstring v = "abc";
    Console.WriteLine($"PutS via ж: {p.PutS(v)}");
    Console.WriteLine($"Put via ж: {p.Put("de")}");
}

} // end recvprobe_package
