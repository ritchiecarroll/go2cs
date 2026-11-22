namespace go.DotImportAdapterNameInHolder.b;

using bytes = bytes_package;
using io = io_package;
using static sinks_package;

partial class sinks_package {

partial struct sink {
    internal nint total;
}

} partial class sinksᴛsink {

internal static (nint, error) Write(this ref sink s, slice<byte> p) {
    s.total += 2 * len(p);
    return (len(p), default);
}

} partial class sinks_package {

public static io.Writer Wrap(ж<bytes.Buffer> buf) { return new bytes_BufferжWriter(buf); }

public static (io.Writer, Func<nint>) Doubled() {
    var s = Ꮡ(new sink());
    var sʗ1 = s;
    return (new sinkжWriter(s), () => sʗ1.total);
}

} // end sinks_package
