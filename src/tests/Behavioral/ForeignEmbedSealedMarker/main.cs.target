namespace go;

using fmt = fmt_package;
using pb = ForeignEmbedSealedMarker.pb_package;
using ForeignEmbedSealedMarker;

partial class main_package {

partial struct server {
    /*embed*/ public ForeignEmbedSealedMarker.pb_package.UnimplementedGreeterServer UnimplementedGreeterServer;
    internal @string greeting;
}

internal static @string SayHello(this ref server s, @string name) { return s.greeting + ", "u8 + name; }

partial struct quiet {
    /*embed*/ public ForeignEmbedSealedMarker.pb_package.UnimplementedGreeterServer UnimplementedGreeterServer;
}

internal static void main() {
    pb.RegisterGreeterServer(new serverжGreeterServer(Ꮡ(new server(greeting: "hello"))));
    pb.RegisterGreeterServer(new quiet());
    pb.RegisterGreeterServer(new quietжGreeterServer(Ꮡ(new quiet())));

    pb.GreeterServer s = new serverжGreeterServer(Ꮡ(new server(greeting: "hi")));
    pb.RegisterGreeterServer(s);

    foreach (var (_, reply) in range(pb.Serve("go"))) {
        fmt.Println(reply);
    }
}

} // end main_package
