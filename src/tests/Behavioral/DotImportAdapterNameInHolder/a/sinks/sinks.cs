namespace go.DotImportAdapterNameInHolder.a;

using bytes = bytes_package;
using io = io_package;

partial class sinks_package {

partial struct sink {
    internal nint n;
}

internal static (nint, error) Write⁔(/*this*/ ref sink s, slice<byte> p) {
    s.n += len(p);
    return (len(p), default);
}

public static (io.Writer, ж<bytes.Buffer>) NewWriter() {
    var buf = Ꮡ(new bytes.Buffer());
    return (new bytes_BufferжWriter(buf), buf);
}

public static (io.Writer, Func<nint>) Counted() {
    var s = Ꮡ(new sink());
    var sʗ1 = s;
    return (new sinkжWriter(s), [GoFunc("Counted.func1")] () => sʗ1.n);
}

} // end sinks_package
