namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    fmt.Printf("%s:"u8, tableName);
    foreach (var (_, v) in range(lookupTable.ΔRangeSnapshot())) {
        fmt.Printf(" %d"u8, v);
    }
    fmt.Println();
}

} // end main_package
