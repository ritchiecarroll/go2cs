namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct Row /*[3]nint*/;

partial struct Target {
    public /*[2]*/ map</*[2]*/ array<@string>, array<ж<float64>>> Marr;
    public /*[3]*/ ж<array<float64>> N;
    public /*[3]*/ ж<ж<ж<array<nint>>>> Deep;
    public /*[5]*/ map<@string, array<nint>> MapElem;
    public map</*[4]*/ array<byte>, nint> MapKey;
    public array<byte> Plain = new(4);
    public array<array<nint>> Nested = new(2, () => new(3));
    public ж<Row> Named;
    public /*[2]*/ slice<array<nint>> SlcArr;
}

internal static void Main() {
    var t = reflect.TypeOf(new Target());

    fmt.Println("-- field types --");

    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        fmt.Printf("%-8s %s\n"u8, f.Name, f.Type);
    }

    var marr = t.Field(0).Type;

    fmt.Println("-- map accessors --");
    fmt.Println("key ", marr.Key(), marr.Key().Len());
    fmt.Println("elem", marr.Elem(), marr.Elem().Len());

    fmt.Println("-- decodeMap allocation --");
    fmt.Println("new key len ", reflect.New(marr.Key()).Elem().Len());
    fmt.Println("new elem len", reflect.New(marr.Elem()).Elem().Len());

    var deep = t.Field(2).Type;

    fmt.Println("-- pointer chain --");
    fmt.Println("deep      ", deep);
    fmt.Println("deep elem3", deep.Elem().Elem().Elem(), deep.Elem().Elem().Elem().Len());
    fmt.Println("ptr elem  ", t.Field(1).Type.Elem(), t.Field(1).Type.Elem().Len());

    ref var target = ref heap(new Target(), out var Ꮡtarget);

    var walk = reflect.ValueOf(Ꮡtarget).Elem().Field(2);

    while (walk.Kind() == reflect.ΔPointer) {
        if (walk.IsNil()) {
            walk.Set(reflect.New(walk.Type().Elem()));
        }

        walk = walk.Elem();
    }

    fmt.Println("-- decIndirect walk --");
    fmt.Println("landed on", walk.Type(), walk.Len(), walk.CanAddr());

    fmt.Println("-- one accessor at a time --");
    fmt.Println("elem-only key ", t.Field(3).Type.Key());
    fmt.Println("elem-only elem", t.Field(3).Type.Elem(), t.Field(3).Type.Elem().Len());
    fmt.Println("key-only key  ", t.Field(4).Type.Key(), t.Field(4).Type.Key().Len());
    fmt.Println("key-only elem ", t.Field(4).Type.Elem());

    fmt.Println("-- initializer route (unchanged) --");
    fmt.Println("plain ", t.Field(5).Type, t.Field(5).Type.Len());
    fmt.Println("nested", t.Field(6).Type, t.Field(6).Type.Len(), t.Field(6).Type.Elem().Len());

    fmt.Println("-- boundaries --");
    fmt.Println("named ptr elem kind", t.Field(7).Type.Elem().Kind());
    fmt.Println("slice elem kind    ", t.Field(8).Type.Elem().Kind());
}

} // end main_package
