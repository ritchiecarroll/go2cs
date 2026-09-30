// The J0 consumer: the converted google/uuid v1.6.0, restored as a NuGet package from a LOCAL folder feed
// (j0-consume.ps1 writes the nuget.config), printing the same keys, in the same order and the same format, as
// the Go oracle (oracle/main.go). j0-consume.ps1 compares the two outputs line by line.
//
// Every line ends in "\n" explicitly, never Environment.NewLine, so the comparison measures the package and not
// the platform's line terminator.
using System;
using System.Text;
using go;
using go.github.com.google;
using static go.github.com.google.uuid_package;

internal static class Program
{
    private static readonly System.IO.TextWriter Out = Console.Out;

    private static void Line(string text) => Out.Write(text + "\n");

    private static string Err(error? err) => err is null ? "<nil>" : err.Error().ToString();

    private static string Text(slice<byte> bytes) => Encoding.UTF8.GetString(bytes.ToSpan());

    private static slice<byte> Bytes(string text) => new(Encoding.UTF8.GetBytes(text));

    private static void Show(string key, UUID u) =>
        Line($"{key}={u.String()} version={u.Version().String()} variant={u.Variant().String()}");

    private static void Structural(string key, UUID u, error? err)
    {
        if (err is not null)
        {
            Line($"{key}=error {Err(err)}");
            return;
        }

        string s = u.String().ToString();
        var (back, perr) = Parse((@string)s);
        Line($"{key}=version={u.Version().String()} variant={u.Variant().String()} len={s.Length} roundtrip={(perr is null && back == u ? "true" : "false")}");
    }

    private static int Main()
    {
        Show("namespace.dns", NameSpaceDNS);
        Show("namespace.url", NameSpaceURL);
        Show("namespace.oid", NameSpaceOID);
        Show("namespace.x500", NameSpaceX500);

        Show("v3.dns.example.com", NewMD5(NameSpaceDNS, Bytes("example.com")));
        Show("v3.url.go2cs", NewMD5(NameSpaceURL, Bytes("https://go2cs.net")));
        Show("v5.dns.example.com", NewSHA1(NameSpaceDNS, Bytes("example.com")));
        Show("v5.url.go2cs", NewSHA1(NameSpaceURL, Bytes("https://go2cs.net")));
        Show("v5.oid.empty", NewSHA1(NameSpaceOID, Bytes("")));
        Show("v5.x500.unicode", NewSHA1(NameSpaceX500, Bytes("CN=Grüße,O=go2cs")));

        foreach (string input in new[]
        {
            "f47ac10b-58cc-0372-8567-0e02b2c3d479",
            "urn:uuid:f47ac10b-58cc-4372-a567-0e02b2c3d479",
            "{f47ac10b-58cc-4372-a567-0e02b2c3d479}",
            "f47ac10b58cc4372a5670e02b2c3d479",
            "F47AC10B-58CC-4372-A567-0E02B2C3D479",
            "not-a-uuid",
            "f47ac10b-58cc-4372-a567-0e02b2c3d47",
            "f47ac10b-58cc-4372-a567+0e02b2c3d479",
        })
        {
            // Go's %q of these ASCII inputs is the input in double quotes.
            string key = $"parse \"{input}\"";
            var (u, err) = Parse((@string)input);

            if (err is not null)
            {
                Line($"{key}=error {Err(err)}");
                continue;
            }

            Show(key, u);
        }

        UUID fixedUuid = MustParse((@string)"f47ac10b-58cc-4372-a567-0e02b2c3d479");
        Line($"urn={fixedUuid.URN()}");
        var (text, _) = fixedUuid.MarshalText();
        Line($"marshaltext={Text(text)}");
        UUID back = default;
        error? unmarshalErr = back.UnmarshalText(text);
        Line($"unmarshaltext={back.String()} err={Err(unmarshalErr)} equal={(back == fixedUuid ? "true" : "false")}");
        var (bin, _) = fixedUuid.MarshalBinary();
        Line($"marshalbinary={Convert.ToHexString(bin.ToSpan()).ToLowerInvariant()}");
        Line($"validate.good={Err(Validate((@string)"f47ac10b-58cc-4372-a567-0e02b2c3d479"))}");
        Line($"validate.bad={Err(Validate((@string)"f47ac10b-58cc-4372-a567-0e02b2c3d47x"))}");

        var (v1, err1) = NewUUID();
        Structural("v1", v1, err1);
        Structural("v4", New(), null);
        var (v6, err6) = NewV6();
        Structural("v6", v6, err6);
        var (v7, err7) = NewV7();
        Structural("v7", v7, err7);

        Out.Flush();
        return 0;
    }
}
