namespace go;

using fmt = fmt_package;

partial class main_package {

internal static ж<Func<@string, @string>> Ꮡlookup = new StandardBox<Func<@string, @string>>((@string key) => "prod:"u8 + key);
internal static ref Func<@string, @string> lookup => ref Ꮡlookup.ValueSlot;

internal static ж<nint> Ꮡcounter = new StandardBox<nint>(default(nint));
internal static ref nint counter => ref Ꮡcounter.Value;

internal static nint untouched = 5;

partial struct entry {
    internal @string name;
}

internal static ж<entry> Ꮡhead = new StandardBox<entry>(default(entry));
internal static ref entry head => ref Ꮡhead.Value;

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
