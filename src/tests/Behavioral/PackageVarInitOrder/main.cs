namespace go;

using fmt = fmt_package;

partial class main_package {

internal static nint first;
internal static void initᴛfirst() { first = @base + 1; }

internal static nint @base = 41;

internal static @string describe(nint n) {
    return names[n];
}

internal static void Main() {
    fmt.Println(first);
    fmt.Println(entryName);
    fmt.Println(entryUpper);
    fmt.Println(stdinName);
    fmt.Println(computed);
    fmt.Println(registry.count);
    fmt.Println(len(sizedTable.codes), (nint)(tableSize));
    fmt.Println(len(chunkHolder.chunks), (nint)(numChunks));

    chunkHolder.chunks[numChunks - 1] = 7;
    fmt.Println(chunkHolder.chunks[numChunks - 1]);
    fmt.Println(len(kindNames), kindNames[kindFile], kindNames[kindPipe]);
    fmt.Println(pipeLabel);
}

} // end main_package
