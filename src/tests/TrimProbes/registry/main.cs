namespace go;

using json = encoding.json_package;
using expvar = expvar_package;
using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using encoding;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string memstatsˢ = "memstats"u8;
private static readonly object memstatsNotPublishedˢ = (@string)"memstats: not published"u8;
private static readonly object memstatsUnmarshalˢ = (@string)"memstats: unmarshal:"u8;
private static readonly @string bySizeˢ = "BySize"u8;
private static readonly object bySizeIsAnArrayˢ = (@string)"BySize is an array:"u8;
private static readonly object bySizeEntriesˢ = (@string)"BySize entries:"u8;
private static readonly @string sizeˢ = "Size"u8;
private static readonly @string mallocsˢ = "Mallocs"u8;
private static readonly @string freesˢ = "Frees"u8;
private static readonly object entryFieldsˢ = (@string)"entry fields:"u8;
private static readonly @string numGCˢ = "NumGC"u8;
private static readonly object numGCPresentˢ = (@string)"NumGC present:"u8;
private static readonly object bySizeFieldFoundˢ = (@string)"BySize field found:"u8;
private static readonly object reflectNewˢ = (@string)"reflect.New:"u8;
private static readonly object addrˢ = (@string)"Addr:"u8;

internal static void Main() {
    var v = expvar.Get(memstatsˢ);
    if (v == default!) {
        fmt.Println(memstatsNotPublishedˢ);
        return;
    }
    ref var m = ref heap<map<@string, any>>(out var Ꮡm);
    {
        var err = json.Unmarshal(slice<byte>(v.String()), Ꮡm); if (err != default!) {
            fmt.Println(memstatsUnmarshalˢ, err);
            return;
        }
    }
    var (bySize, ok) = m[bySizeˢ]._<slice<any>>(ᐧ);
    fmt.Println(bySizeIsAnArrayˢ, ok);
    fmt.Println(bySizeEntriesˢ, len(bySize));
    if (len(bySize) > 0) {
        var (entry, _) = bySize[0]._<map<@string, any>>(ᐧ);
        var (_, size) = entry[sizeˢ, ꟷ];
        var (_, mallocs) = entry[mallocsˢ, ꟷ];
        var (_, frees) = entry[freesˢ, ꟷ];
        fmt.Println(entryFieldsˢ, len(entry), size, mallocs, frees);
    }
    var (_, numGC) = m[numGCˢ, ꟷ];
    fmt.Println(numGCPresentˢ, numGC);
    ref var ms = ref heap(new runtime.MemStats(), out var Ꮡms);
    var (field, found) = reflect.TypeOf(ms).FieldByName(bySizeˢ);
    fmt.Println(bySizeFieldFoundˢ, found, field.Type.Kind(), field.Type.Len());
    var created = reflect.New(field.Type);
    fmt.Println(reflectNewˢ, created.Kind(), created.Elem().Kind(), created.Elem().Len(), created.Elem().Index(0).Kind());
    var addr = reflect.ValueOf(Ꮡms).Elem().FieldByName(bySizeˢ).Addr();
    fmt.Println(addrˢ, addr.Kind(), addr.Elem().Len());
}

} // end main_package
