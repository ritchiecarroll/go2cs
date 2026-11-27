namespace go;

using tls = crypto.tls_package;
using errors = errors_package;
using fmt = fmt_package;
using Δnet = net_package;
using crypto;
using time = time_package;

partial class main_package {

internal static error errHelloRead = errors.New("hello read");

internal static partial void main() {
    var (clientEnd, serverEnd) = Δnet.Pipe();
    var seen = new channel<slice<uint16>>(1);

        var seenʗ1 = seen;
    var server = tls.Server(serverEnd, Ꮡ(new tls.Config(
        GetConfigForClient: [GoFunc("main.func1")] (ж<tls.ClientHelloInfo> hello) => {
            seenʗ1.ᐸꟷ(hello.CipherSuites);
            return (default, errHelloRead);
        }
    )));

    var serverDone = new channel<error>(1);
    var serverʗ1 = server;
    var serverDoneʗ1 = serverDone;
    var serverEndʗ1 = serverEnd;
    goǃ([GoFunc("main.func2")] () => {
        var evalᴛ1 = serverʗ1.Handshake();
        serverDoneʗ1.ᐸꟷ(evalᴛ1);
        serverEndʗ1.Close();
    });

    var clientDone = new channel<error>(1);
    var clientDoneʗ1 = clientDone;
    var clientEndʗ1 = clientEnd;
    goǃ([GoFunc("main.func3")] () => {
        var evalᴛ2 = tls.Client(clientEndʗ1, Ꮡ(new tls.Config(ServerName: "example.test")));
        var evalᴛ3 = evalᴛ2.Handshake();
        clientDoneʗ1.ᐸꟷ(evalᴛ3);
        clientEndʗ1.Close();
    });

    var suites = ᐸꟷ(seen);

    nint firstAES = -1;
    nint firstChaCha = -1;
    foreach (var (i, id) in range(suites)) {
        @string name = tls.CipherSuiteName(id);
        fmt.Printf("%2d %s\n"u8, i, name);

        switch (id) {
        case tls.TLS_AES_128_GCM_SHA256 or tls.TLS_AES_256_GCM_SHA384 or tls.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 or tls.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 or tls.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 or tls.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384: {
            if (firstAES < 0) {
                firstAES = i;
            }
            break;
        }
        case tls.TLS_CHACHA20_POLY1305_SHA256 or tls.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256 or tls.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256: {
            if (firstChaCha < 0) {
                firstChaCha = i;
            }
            break;
        }}

    }

    fmt.Println("suites offered:", len(suites));
    fmt.Println("AES-GCM preferred (hardware AES-GCM detected):", firstAES >= 0 && firstAES < firstChaCha);

    var serverErr = ᐸꟷ(serverDone);
    fmt.Println("server stopped at the hello:", errors.Is(serverErr, errHelloRead));
    fmt.Println("client handshake failed:", ᐸꟷ(clientDone) != nil);
}

} // end main_package
