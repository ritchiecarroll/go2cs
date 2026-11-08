// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database.sql;

using reflect = reflect_package;
using testing = testing_package;
using time = time_package;
using static go.database.sql.driver_package;

partial class driver_internal_test_package {

internal partial struct valueConverterTest {
    internal global::go.database.sql.driver_package.ValueConverter c;
    internal any @in;
    internal any @out;
    internal @string err;
}

internal static readonly ж<time.Time> Ꮡnow = new StaticBox<time.Time>(time.Now());
internal static ref time.Time now => ref ((StaticBox<time.Time>)Ꮡnow).ValueSlot;
internal static readonly ж<int64> Ꮡanswer = new StaticBox<int64>(42);
internal static ref int64 answer => ref ((StaticBox<int64>)Ꮡanswer).ValueSlot;

internal partial struct i /*num:int64*/;

internal partial struct f /*num:float64*/;

internal partial struct b /*bool*/;

internal partial struct bs /*[]byte*/;

internal partial struct s /*@string*/;

internal partial struct t /*global::go.time_package.Time*/;

internal partial struct @is /*[]nint*/;

internal static slice<valueConverterTest> valueConverterTests;
internal static void initᴛvalueConverterTests() { valueConverterTests = [
    new(Bool, (@string)"true", true, ""),
    new(Bool, (@string)"True", true, ""),
    new(Bool, slice<byte>("t"u8), true, ""),
    new(Bool, true, true, ""),
    new(Bool, (@string)"1", true, ""),
    new(Bool, (nint)1, true, ""),
    new(Bool, (int64)1, true, ""),
    new(Bool, (uint16)1, true, ""),
    new(Bool, (@string)"false", false, ""),
    new(Bool, false, false, ""),
    new(Bool, (@string)"0", false, ""),
    new(Bool, (nint)0, false, ""),
    new(Bool, (int64)0, false, ""),
    new(Bool, (uint16)0, false, ""),
    new(c: Bool, @in: (@string)"foo", err: "sql/driver: couldn't convert \"foo\" into type bool"),
    new(c: Bool, @in: (nint)2, err: "sql/driver: couldn't convert 2 into type bool"),
    new(DefaultParameterConverter, now, now, ""),
    new(DefaultParameterConverter, (ж<int64>)nil, default, ""),
    new(DefaultParameterConverter, Ꮡanswer, answer, ""),
    new(DefaultParameterConverter, Ꮡnow, now, ""),
    new(DefaultParameterConverter, (i)9, (int64)9, ""),
    new(DefaultParameterConverter, (f)0.1D, (float64)0.1D, ""),
    new(DefaultParameterConverter, (b)true, true, ""),
    new(DefaultParameterConverter, new bs(new byte[]{1}.slice()), slice<byte>([1]), ""),
    new(DefaultParameterConverter, (s)(@string)"a", (@string)"a", ""),
    new(DefaultParameterConverter, (t)now, default, "unsupported type driver.t, a struct"),
    new(DefaultParameterConverter, new @is(new nint[]{1}.slice()), default, "unsupported type driver.is, a slice of int"),
    new(DefaultParameterConverter, new dec(exponent: -6), new dec(exponent: -6), "")
]; }

public static void TestValueConverters(ж<testing.T> t) {
    foreach (var (i, tt) in range(valueConverterTests)) {
        var (@out, err) = tt.c.ConvertValue(tt.@in);
        @string goterr = ""u8;
        if (err != nil) {
            goterr = err.Error();
        }
        if (goterr != tt.err) {
            t.Errorf("test %d: %T(%T(%v)) error = %q; want error = %q",
                i, tt.c, tt.@in, tt.@in, goterr, tt.err);
        }
        if (tt.err != ""u8) {
            continue;
        }
        if (!reflect.DeepEqual(@out, tt.@out)) {
            t.Errorf("test %d: %T(%T(%v)) = %v (%T); want %v (%T)",
                i, tt.c, tt.@in, tt.@in, @out, @out, tt.@out, tt.@out);
        }
    }
}

internal partial struct dec {
    internal byte form;
    internal bool neg;
    internal array<byte> coefficient = new(16);
    internal int32 exponent;
}

internal static (byte form, bool negative, slice<byte> coefficient, int32 exponent) Decompose(this dec d, slice<byte> buf) {
    d = d.ΔClone();

    var coef = new slice<byte>(16);
    copy(coef, d.coefficient[..]);
    return (d.form, d.neg, coef, d.exponent);
}

} // end driver_internal_test_package
