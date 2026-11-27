namespace go.ForeignEmbedSealedMarker;

partial class pb_package {

partial interface GreeterServer {
    @string SayHello(@string name);
    void mustEmbedUnimplementedGreeterServer();
}

partial struct UnimplementedGreeterServer {
}

internal static @string SayHello⁔(/*this*/ UnimplementedGreeterServer _, @string name) {
    return "unimplemented SayHello("u8 + name + ")"u8;
}

internal static void mustEmbedUnimplementedGreeterServer⁔(/*this*/ UnimplementedGreeterServer _) { }

internal static slice<GreeterServer> servers;

public static void RegisterGreeterServer(GreeterServer s) {
    s.mustEmbedUnimplementedGreeterServer();
    servers = append(servers, s);
}

public static slice<@string> Serve(@string name) {
    slice<@string> replies = default;

    foreach (var (_, s) in range(servers)) {
        replies = append(replies, s.SayHello(name));
    }

    return replies;
}

} // end pb_package
