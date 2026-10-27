namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly ж<Func<@string, @string>> Ꮡlookup = new StaticBox<Func<@string, @string>>((@string key) => "prod:"u8 + key);
internal static ref Func<@string, @string> lookup => ref ((StaticBox<Func<@string, @string>>)Ꮡlookup).ValueSlot;

internal static readonly ж<nint> Ꮡcounter = new StaticBox<nint>(default(nint));
internal static ref nint counter => ref ((StaticBox<nint>)Ꮡcounter).ValueSlot;

internal static nint untouched = 5;

partial struct entry {
    internal @string name;
}

internal static readonly ж<entry> Ꮡhead = new StaticBox<entry>(default(entry));
internal static ref entry head => ref ((StaticBox<entry>)Ꮡhead).ValueSlot;

internal static void Main() {
    fmt.Println(lookup("a"));

    lookup = @string (@string key) => "swapped:"u8 + key;
    fmt.Println(lookup("b"));

    counter += 3;
    counter++;
    fmt.Println(counter);

    head.name = "first";
    fmt.Println(head.name);

    fmt.Println(untouched);
}

} // end main_package
