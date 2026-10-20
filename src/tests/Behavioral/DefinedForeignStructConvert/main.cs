namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;
using rec = DefinedForeignStructConvert.rec_package;
using DefinedForeignStructConvert;

partial class main_package {

partial struct customTime /*global::go.time_package.Time*/;

partial struct customHidden /*global::go.DefinedForeignStructConvert.rec_package.Hidden*/;

partial struct customOpen /*global::go.DefinedForeignStructConvert.rec_package.Open*/;

partial struct local {
    internal nint n;
    internal @string s;
}

partial struct customLocal /*local*/;

internal static void check(@string name, any x, reflectꓸType to) {
    var v = reflect.ValueOf(x);
    fmt.Println(name, "CanConvert:", v.CanConvert(to), "ConvertibleTo:", v.Type().ConvertibleTo(to), "back:", to.ConvertibleTo(v.Type()));
}

internal static void Main() {
    var when = time.Date(2020, 1, 2, 3, 4, 5, 0, time.ΔUTC);
    var timeType = reflect.TypeOf(new time.Time(nil));

    check("customTime -> time.Time", (customTime)when, timeType);
    check("customHidden -> rec.Hidden", (customHidden)rec.NewHidden(1, "a"), reflect.TypeOf(new rec.Hidden(nil)));
    check("customOpen -> rec.Open", (customOpen)new rec.Open(N: 2, S: "b"), reflect.TypeOf(new rec.Open(nil)));
    check("customLocal -> local", (customLocal)new local(3, "c"), reflect.TypeOf(new local(nil)));

    check("customLocal -> rec.Hidden", (customLocal)new local(4, "d"), reflect.TypeOf(new rec.Hidden(nil)));

    var v = reflect.ValueOf((customTime)when);
    if (v.CanConvert(timeType)) {
        var back = v.Convert(timeType).Interface()._<time.Time>();
        fmt.Println("converted time:", back.Equal(when), back.UnixNano() == when.UnixNano());
    }

    var h = reflect.ValueOf((customHidden)rec.NewHidden(5, "e")).Convert(reflect.TypeOf(new rec.Hidden(nil))).Interface()._<rec.Hidden>();
    fmt.Println("converted hidden:", h.N, h.S());
}

} // end main_package
