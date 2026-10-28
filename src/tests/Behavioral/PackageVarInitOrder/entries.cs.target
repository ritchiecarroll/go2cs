namespace go;

partial class main_package {

internal static @string entryName;
internal static void initᴛentryName() { entryName = registry.add("alpha"); }

internal static @string entryUpper;
internal static void initᴛentryUpper() { entryUpper = entryName + "!"u8; }

internal static @string stdinName;
internal static void initᴛstdinName() { stdinName = describe(0); }

internal static nint computed;
internal static void initᴛcomputed() { computed = func(nint () => {
    return @base * 2;
})(); }

internal static ж<table> sizedTable = newTable(tableSize);

internal static holder chunkHolder = new();

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [kindNone] = "none",
    [kindFile] = "file",
    [kindPipe] = "pipe"
}.slice();

internal static @string pipeLabel;
internal static void initᴛpipeLabel() { pipeLabel = ((@string)labelPipe) + "!"u8; }

} // end main_package
