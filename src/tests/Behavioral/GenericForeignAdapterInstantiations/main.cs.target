namespace go;

using fmt = fmt_package;
using fieldlib = GenericForeignAdapterInstantiations.fieldlib_package;
using GenericForeignAdapterInstantiations;

partial class main_package {

internal static void show(@string label, fieldlib.Field f) {
    fmt.Println(label, f.IsPresent(), f.Describe());
}

internal static void Main() {
    ref var name = ref heap<fieldlib.Expected<@string>>(new fieldlib.Expected<@string>(Value: "pkg", Valid: true), out var Ꮡname);
    ref var deps = ref heap<fieldlib.Expected<map<@string, @string>>>(new fieldlib.Expected<map<@string, @string>>(Value: new map<@string, @string>{["a"] = "1"}, Valid: false), out var Ꮡdeps);
    ref var size = ref heap<fieldlib.Expected<nint>>(new fieldlib.Expected<nint>(Value: 42, Valid: true), out var Ꮡsize);

    show("string", new fieldlib_ExpectedжField<@string>(Ꮡname));
    show("map", new fieldlib_ExpectedжField<map<@string, @string>>(Ꮡdeps));
    show("int", new fieldlib_ExpectedжField<nint>(Ꮡsize));

    var all = slice<fieldlib.Field>([new fieldlib_ExpectedжField<@string>(Ꮡname), new fieldlib_ExpectedжField<map<@string, @string>>(Ꮡdeps), new fieldlib_ExpectedжField<nint>(Ꮡsize), new fieldlib_ExpectedжField<@string>(Ꮡname)]);
    nint present = 0;
    foreach (var (_, f) in range(all)) {
        if (f.IsPresent()) {
            present++;
        }
    }
    fmt.Println("present", present, "of", len(all));

    ref var box = ref heap<fieldlib.Box<@string>>(new fieldlib.Box<@string>(Item: "boxed"), out var Ꮡbox);
    fieldlib.Getter g = new fieldlib_BoxжGetter(Ꮡbox);
    fmt.Println("getter", g.Get());
}

} // end main_package
