global using Entry = go.NestedAliasUser.inner_package.Entryᴛ1;

namespace go.NestedAliasUser;

partial class inner_package {

public partial struct Entryᴛ1 /*dyn*/ {
    public @string Name;
    public slice<byte> Data;
    public nint Count;
}

public static Entry NewEntry(@string name, nint count) {
    return new Entry(Name: name, Count: count);
}

public static nint Total(slice<Entry> entries) {
    nint sum = 0;
    foreach (var (_, e) in entries) {
        sum += e.Count;
    }
    return sum;
}

} // end inner_package
