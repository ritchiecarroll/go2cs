namespace go.github.com.itchyny.gojq;

using os = os_package;

partial class cli_package {

public static nint Run() {
    return (Ꮡ(new cli(
        inStream: new os_FileжReader(os.Stdin),
        outStream: new os.FileжWriter(os.Stdout),
        errStream: new os.FileжWriter(os.Stderr)
    ))).run(os.Args[1..]);
}

} // end cli_package
