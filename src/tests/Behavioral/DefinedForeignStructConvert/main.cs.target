namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using time = time_package;
using rec = DefinedForeignStructConvert.rec_package;
using DefinedForeignStructConvert;

partial class main_package {

[GoType("global::go.time_package.Time")] partial struct customTime;

[GoType("global::go.DefinedForeignStructConvert.rec_package.Hidden")] partial struct customHidden;

[GoType("global::go.DefinedForeignStructConvert.rec_package.Open")] partial struct customOpen;

[GoType] partial struct local {
    internal nint n;
    internal @string s;
}

[GoType("local")] partial struct customLocal;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object canConvertˢ = (@string)"CanConvert:"u8;
private static readonly object convertibleToˢ = (@string)"ConvertibleTo:"u8;
private static readonly object backˢ = (@string)"back:"u8;

internal static void check(@string name, any x, reflectꓸType to) {
    var v = reflect.ValueOf(x);
    fmt.Println(name, canConvertˢ, v.CanConvert(to), convertibleToˢ, v.Type().ConvertibleTo(to), backˢ, to.ConvertibleTo(v.Type()));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string customTimeTimeTimeˢ = "customTime -> time.Time"u8;
private static readonly @string customHiddenRecHiddenˢ = "customHidden -> rec.Hidden"u8;
private static readonly @string customOpenRecOpenˢ = "customOpen -> rec.Open"u8;
private static readonly @string customLocalLocalˢ = "customLocal -> local"u8;
private static readonly @string customLocalRecHiddenˢ = "customLocal -> rec.Hidden"u8;
private static readonly object convertedTimeˢ = (@string)"converted time:"u8;
private static readonly object convertedHiddenˢ = (@string)"converted hidden:"u8;

internal static void Main() {
    var when = time.Date(2020, 1, 2, 3, 4, 5, 0, time.ΔUTC);
    var timeType = reflect.TypeOf(new time.Time(nil));
    check(customTimeTimeTimeˢ, ((customTime)when), timeType);
    check(customHiddenRecHiddenˢ, ((customHidden)rec.NewHidden(1, "a"u8)), reflect.TypeOf(new rec.Hidden(nil)));
    check(customOpenRecOpenˢ, ((customOpen)new rec.Open(N: 2, S: "b"u8)), reflect.TypeOf(new rec.Open(nil)));
    check(customLocalLocalˢ, ((customLocal)new local(3, "c"u8)), reflect.TypeOf(new local(nil)));
    check(customLocalRecHiddenˢ, ((customLocal)new local(4, "d"u8)), reflect.TypeOf(new rec.Hidden(nil)));
    var v = reflect.ValueOf(((customTime)when));
    if (v.CanConvert(timeType)) {
        var back = v.Convert(timeType).Interface()._<time.Time>();
        fmt.Println(convertedTimeˢ, back.Equal(when), back.UnixNano() == when.UnixNano());
    }
    var h = reflect.ValueOf(((customHidden)rec.NewHidden(5, "e"u8))).Convert(reflect.TypeOf(new rec.Hidden(nil))).Interface()._<rec.Hidden>();
    fmt.Println(convertedHiddenˢ, h.N, h.S());
}

} // end main_package
