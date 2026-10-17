namespace go;

using fmt = fmt_package;
using strings = strings_package;
using time = time_package;

partial class main_package {

internal static slice<@string> lines = new @string[]{
    "//go:build linux && amd64"u8,
    "// regular comment line"u8,
    "package main"u8,
    "import \"fmt\""u8,
    "func main() {"u8,
    "\tx := true"u8,
    "}"u8,
    "//go:build windows"u8,
    "var y = false"u8,
    "// another comment"u8
}.slice();

internal static slice<@string> words = new @string[]{"true"u8, "false"u8, "linux"u8, "windows"u8, "amd64"u8, "arm64"u8, "ignore"u8, "main"u8}.slice();

internal static nint classify(@string w) {
    var exprᴛ1 = w;
    if (exprᴛ1 == "true"u8) {
        return 1;
    }
    if (exprᴛ1 == "false"u8) {
        return 2;
    }
    if (exprᴛ1 == "linux"u8 || exprᴛ1 == "windows"u8 || exprᴛ1 == "darwin"u8) {
        return 3;
    }
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8) {
        return 4;
    }

    return 0;
}

internal static @string kindName(nint k) {
    switch (k) {
    case 1 or 2: {
        return "bool";
    }
    case 3: {
        return "goos";
    }
    case 4: {
        return "goarch";
    }}

    return "word";
}

internal static nint run(nint n) {
    nint total = 0;
    var counts = new map<@string, nint>{};
    for (nint i = 0; i < n; i++) {
        @string line = lines[i % len(lines)];
        if (strings.HasPrefix(line, "//go:build")){
            counts["build"]++;
            total += len(line);
        } else 
        if (strings.HasPrefix(line, "// "u8)) {
            counts["comment"]++;
        }
        @string w = words[i % len(words)];
        nint k = classify(w);
        total += k;
        total += len(kindName(k));
    }
    total += counts["build"] * 3 + counts["comment"];
    return total;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object checksumˢ = (@string)"checksum:"u8;
private static readonly object elapsedNsˢ = (@string)"elapsed_ns:"u8;

internal static void Main() {
    var start = time.Now().UnixNano();
    nint total = run(20000000);
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println(checksumˢ, total);
    fmt.Println(elapsedNsˢ, elapsed);
}

} // end main_package
