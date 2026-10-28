namespace go;

using bytes = bytes_package;
using gob = encoding.gob_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using dotted = example.com.dotted.dotted_package;
using encoding;
using example.com.dotted;
using Δio = io_package;

partial class main_package {

internal static void Main() {
    fmt.Println(reflect.TypeOf(new dotted.T(nil)).PkgPath());
    fmt.Println(reflect.TypeOf(Ꮡ(new dotted.T(nil))).Elem().String());

    fmt.Println(dotted.Where());
    fmt.Println(runtime.FuncForPC(reflect.ValueOf(dotted.F).Pointer()).Name());

    gob.Register(new dotted.T(nil));

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    ref var @in = ref heap<any>(out var Ꮡin);

    @in = new dotted.T(N: dotted.F());

    {
        var err = gob.NewEncoder(new bytes_BufferжWriter(Ꮡbuf)).Encode(Ꮡin); if (err != nil) {
            fmt.Println("encode:", err);
            return;
        }
    }

    fmt.Println(bytes.Contains(buf.Bytes(), slice<byte>("example.com/dotted/v2.T"u8)));

    ref var @out = ref heap<any>(out var Ꮡout);
    {
        var err = gob.NewDecoder(new bytes_BufferжReader(Ꮡbuf)).Decode(Ꮡout); if (err != nil) {
            fmt.Println("decode:", err);
            return;
        }
    }

    fmt.Printf("%T %v\n"u8, @out, @out);
}

} // end main_package
