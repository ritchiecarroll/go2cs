namespace go;

using fmt = fmt_package;

partial class main_package {

public partial struct DataProcessor_data /*dyn*/ {
public nint ID;
public @string Name;
public bool Valid;
}

partial interface DataProcessor {
    void Process(DataProcessor_data data);
}

partial struct Processor {
}

public static void Process(this Processor p, DataProcessor_data data) {
    fmt.Printf("Processing ID: %d, Name: %s, Valid: %t\n"u8, data.ID, data.Name, data.Valid);
}

internal static void Main() {
    DataProcessor p = default!;
    p = new Processor(nil);
    var data = new DataProcessor_data(ID: 1, Name: "Alice"u8, Valid: true);
    p.Process(data);
}

} // end main_package
