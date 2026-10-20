namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Person {
    public @string Name;
    public nint Age;
}


partial struct settingsᴛ1 /*dyn*/ {
    public bool Verbose;
    public nint Retries;
}
internal static ж<settingsᴛ1> Ꮡsettings = new StandardBox<settingsᴛ1>(new settingsᴛ1(Verbose: true, Retries: 3));
internal static ref settingsᴛ1 settings => ref Ꮡsettings.Value;

internal partial struct processAnonymousStruct_data /*dyn*/ {
    public @string Name;
    public nint Age;
}

internal static void processAnonymousStruct(processAnonymousStruct_data data) {
    fmt.Printf("Processing: %s, %d years old\n"u8, data.Name, data.Age);
}

internal partial struct cycleMemo_memo /*dyn*/ {
    internal any ptr;
    internal nint len;
}

internal static void cycleMemo() {
    var seen = new map<any, EmptyStruct>{};
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 10;

    var memo = new cycleMemo_memo(Ꮡa, 2);

    var (_, before) = seen[memo, ꟷ];
    seen[memo] = new EmptyStruct();
    var (_, after) = seen[memo, ꟷ];

    var got = memo.ptr._<ж<nint>>();
    fmt.Printf("cycleMemo: val=%d len=%d before=%t after=%t\n"u8, got.Value, memo.len, before, after);
}

internal partial struct main_type /*dyn*/ {
    internal @string name;
    internal uint32 size;
}

internal static void Main() {
    var namedPerson = new Person(Name: "Alice", Age: 30);
    var anonPerson = new processAnonymousStruct_data(Name: "Bob", Age: 25);

    any someInterface = anonPerson;

    var (_, ok) = someInterface._<processAnonymousStruct_data>(ᐧ);

    fmt.Println("Anonymous struct type assertion:", ok);

    someInterface = namedPerson;

    (_, ok) = someInterface._<processAnonymousStruct_data>(ᐧ);

    fmt.Println("Named struct with identical fields:", ok);

    fmt.Println("\n=== Function Parameter Tests ===");

    processAnonymousStruct(new processAnonymousStruct_data(Name: "Charlie", Age: 40));

    processAnonymousStruct(anonPerson);

    processAnonymousStruct(new processAnonymousStruct_data(namedPerson.Name, namedPerson.Age));

    fmt.Println("\n=== Package-Global Anonymous Struct ===");
    fmt.Printf("settings: Verbose=%t Retries=%d\n"u8, settings.Verbose, settings.Retries);

    var pRetries = Ꮡsettings.of(settingsᴛ1.ᏑRetries);
    pRetries.Value = 5;
    fmt.Printf("after &settings.Retries=5: *p=%d global=%d\n"u8, pRetries.Value, settings.Retries);

    fmt.Println("\n=== In-Function var Slice of Anonymous Struct ===");

    slice<main_type> sects = new main_type[]{
        new("text", 100),
        new("data", 200),
        new("syms", 300)
    }.slice();

    var total = (uint32)0;

    foreach (var (_, sect) in sects) {
        total += sect.size;
    }

    fmt.Printf("sections=%d total=%d first=%s\n"u8, len(sects), total, sects[0].name);

    fmt.Println("\n=== Anonymous Struct With Empty Interface Field ===");
    cycleMemo();
}

} // end main_package
