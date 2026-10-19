namespace go;

// blank import: embed_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

internal static @string testfp = go.embed_package.ΔEmbedString(typeof(main_package).Assembly, "go.embed/go2cs/EmbedDirectiveForms/", "testdata/testfp.txt");

internal static slice<byte> entropyCapabilitiesJson = go.embed_package.ΔEmbedBytes<byte>(typeof(main_package).Assembly, "go.embed/go2cs/EmbedDirectiveForms/", "capabilities.json");
internal static nint unrelated = 42;

internal static slice<byte> capabilitiesJson = go.embed_package.ΔEmbedBytes<byte>(typeof(main_package).Assembly, "go.embed/go2cs/EmbedDirectiveForms/", "capabilities.json");

internal static @string testfpAgain = go.embed_package.ΔEmbedString(typeof(main_package).Assembly, "go.embed/go2cs/EmbedDirectiveForms/", "testdata/testfp.txt");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object testfpTestfpAgainˢ = (@string)"testfp == testfpAgain:"u8;
private static readonly object unrelatedˢ = (@string)"unrelated:"u8;
private static readonly object byteSumˢ = (@string)"byte sum:"u8;

internal static void Main() {
    fmt.Printf("testfp: %d bytes, %d lines\n"u8, len(testfp), strings.Count(testfp, "\n"));
    foreach (var (i, line) in strings.Split(strings.TrimSuffix(testfp, "\n"), "\n")) {
        fmt.Printf("  %d: %q\n"u8, i, line);
    }
    fmt.Println(testfpTestfpAgainˢ, testfp == testfpAgain);

    fmt.Printf("entropyCapabilitiesJson: %d bytes, %q\n"u8, len(entropyCapabilitiesJson), entropyCapabilitiesJson);
    fmt.Printf("capabilitiesJson: %d bytes, %q\n"u8, len(capabilitiesJson), capabilitiesJson);
    fmt.Println(unrelatedˢ, unrelated);

    entropyCapabilitiesJson[0] = (rune)'[';
    fmt.Printf("after write: %c %c\n"u8, entropyCapabilitiesJson[0], capabilitiesJson[0]);

    nint sum = default!;
    foreach (var (_, b) in capabilitiesJson) {
        sum += (nint)b;
    }
    fmt.Println(byteSumˢ, sum);
}

} // end main_package
