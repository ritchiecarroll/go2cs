namespace go;

using fmt = fmt_package;
using strings = strings_package;
using time = time_package;

partial class main_package {

internal static slice<@string> lines = new @string[]{
    "//go:build linux && amd64",
    "// regular comment line",
    "package main",
    "import \"fmt\"",
    "func main() {",
    "\tx := true",
    "}",
    "//go:build windows",
    "var y = false",
    "// another comment"
}.slice();

internal static slice<@string> words = new @string[]{"true", "false", "linux", "windows", "amd64", "arm64", "ignore", "main"}.slice();

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
        if (strings.HasPrefix(line, "// ")) {
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

internal static void Main() {
    var start = time.Now().UnixNano();

    nint total = run(20000000);

    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", total);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
