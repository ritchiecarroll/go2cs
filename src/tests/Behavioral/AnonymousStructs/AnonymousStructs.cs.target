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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object anonymousStructTypeˢ = (@string)"Anonymous struct type assertion:"u8;
private static readonly object namedStructWithIdenticalˢ = (@string)"Named struct with identical fields:"u8;
private static readonly object functionParameterTestsˢ = (@string)"\n=== Function Parameter Tests ==="u8;
private static readonly object packageGlobalAnonymousˢ = (@string)"\n=== Package-Global Anonymous Struct ==="u8;
private static readonly object inFunctionVarSliceOfˢ = (@string)"\n=== In-Function var Slice of Anonymous Struct ==="u8;
private static readonly object anonymousStructWithEmptyˢ = (@string)"\n=== Anonymous Struct With Empty Interface Field ==="u8;

internal partial struct main_type /*dyn*/ {
    internal @string name;
    internal uint32 size;
}

internal static void Main() {
    var namedPerson = new Person(Name: "Alice"u8, Age: 30);
    var anonPerson = new processAnonymousStruct_data(Name: "Bob"u8, Age: 25);
    any someInterface = anonPerson;
    var (_, ok) = someInterface._<processAnonymousStruct_data>(ᐧ);
    fmt.Println(anonymousStructTypeˢ, ok);
    someInterface = namedPerson;
    (_, ok) = someInterface._<processAnonymousStruct_data>(ᐧ);
    fmt.Println(namedStructWithIdenticalˢ, ok);
    fmt.Println(functionParameterTestsˢ);
    processAnonymousStruct(new processAnonymousStruct_data(Name: "Charlie"u8, Age: 40));
    processAnonymousStruct(anonPerson);
    processAnonymousStruct(new processAnonymousStruct_data(namedPerson.Name, namedPerson.Age));
    fmt.Println(packageGlobalAnonymousˢ);
    fmt.Printf("settings: Verbose=%t Retries=%d\n"u8, settings.Verbose, settings.Retries);
    var pRetries = Ꮡsettings.of(settingsᴛ1.ᏑRetries);
    pRetries.Value = 5;
    fmt.Printf("after &settings.Retries=5: *p=%d global=%d\n"u8, pRetries.Value, settings.Retries);
    fmt.Println(inFunctionVarSliceOfˢ);
    slice<main_type> sects = new main_type[]{
        new("text"u8, 100),
        new("data"u8, 200),
        new("syms"u8, 300)
    }.slice();
    var total = (uint32)0;
    foreach (var (_, sect) in sects) {
        total += sect.size;
    }
    fmt.Printf("sections=%d total=%d first=%s\n"u8, len(sects), total, sects[0].name);
    fmt.Println(anonymousStructWithEmptyˢ);
    cycleMemo();
}

} // end main_package
