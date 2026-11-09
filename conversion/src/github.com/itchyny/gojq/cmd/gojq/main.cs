namespace go;

using os = os_package;
using cli = github.com.itchyny.gojq.cli_package;
using github.com.itchyny.gojq;

partial class main_package {

internal static void Main() {
    os.Exit(cli.Run());
}

} // end main_package
