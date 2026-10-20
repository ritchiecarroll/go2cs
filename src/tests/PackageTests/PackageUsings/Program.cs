// No `using go.encoding;`: the namespace that holds encoding/hex's Go methods comes from the package's
// build/<PackageId>.props. Built with GLOBAL_ALIASES defined, the file declares no alias either, and the
// `hex` alias must come from the same props, under GoGlobalPackageAliases=true.
#if !GLOBAL_ALIASES
using hex = go.encoding.hex_package;
#endif

public static class Program
{
    public static int Main()
    {
        hex.InvalidByteError bad = (hex.InvalidByteError)(byte)'z';
        @string text = bad.Error();         // a Go method: an extension method in namespace go.encoding
        @string encoded = hex.EncodeToString("go"u8.ToArray());
        bool ok = text == "encoding/hex: invalid byte: U+007A 'z'" && encoded == "676f";
        System.Console.WriteLine($"PACKAGE-USINGS: {(ok ? "ok" : "wrong")} ({text}; {encoded})");
        return ok ? 0 : 1;
    }
}
