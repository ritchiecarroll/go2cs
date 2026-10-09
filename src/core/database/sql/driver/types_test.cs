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

internal static ж<time.Time> Ꮡnow = new StandardBox<time.Time>(time.Now());
internal static ref time.Time now => ref Ꮡnow.Value;

internal static ж<int64> Ꮡanswer = new StandardBox<int64>(42);
internal static ref int64 answer => ref Ꮡanswer.Value;

internal partial struct i /*num:int64*/;

internal partial struct f /*num:float64*/;

internal partial struct b /*bool*/;

internal partial struct bs /*[]byte*/;

internal partial struct s /*@string*/;

internal partial struct t /*global::go.time_package.Time*/;

internal partial struct @is /*[]nint*/;

internal static slice<valueConverterTest> valueConverterTests;
internal static void initᴛvalueConverterTests() { valueConverterTests = new valueConverterTest[]{
    new(Bool, (@string)"true"u8, true, ""u8),
    new(Bool, (@string)"True"u8, true, ""u8),
    new(Bool, slice<byte>("t"u8), true, ""u8),
    new(Bool, true, true, ""u8),
    new(Bool, (@string)"1"u8, true, ""u8),
    new(Bool, (nint)(1), true, ""u8),
    new(Bool, (int64)1, true, ""u8),
    new(Bool, (uint16)1, true, ""u8),
    new(Bool, (@string)"false"u8, false, ""u8),
    new(Bool, false, false, ""u8),
    new(Bool, (@string)"0"u8, false, ""u8),
    new(Bool, (nint)(0), false, ""u8),
    new(Bool, (int64)0, false, ""u8),
    new(Bool, (uint16)0, false, ""u8),
    new(c: Bool, @in: (@string)"foo"u8, err: "sql/driver: couldn't convert \"foo\" into type bool"u8),
    new(c: Bool, @in: (nint)(2), err: "sql/driver: couldn't convert 2 into type bool"u8),
    new(DefaultParameterConverter, now, now, ""u8),
    new(DefaultParameterConverter, ((ж<int64>)nil), default!, ""u8),
    new(DefaultParameterConverter, Ꮡanswer, answer, ""u8),
    new(DefaultParameterConverter, Ꮡnow, now, ""u8),
    new(DefaultParameterConverter, ((i)9), (int64)9, ""u8),
    new(DefaultParameterConverter, ((f)0.1D), (float64)0.1D, ""u8),
    new(DefaultParameterConverter, ((b)true), true, ""u8),
    new(DefaultParameterConverter, new bs(new byte[]{1}.slice()), new byte[]{1}.slice(), ""u8),
    new(DefaultParameterConverter, ((s)(@string)"a"u8), (@string)"a"u8, ""u8),
    new(DefaultParameterConverter, ((t)now), default!, "unsupported type driver.t, a struct"u8),
    new(DefaultParameterConverter, new @is(new nint[]{1}.slice()), default!, "unsupported type driver.is, a slice of int"u8),
    new(DefaultParameterConverter, new dec(exponent: -6), new dec(exponent: -6), ""u8)
}.slice(); }

public static void TestValueConverters(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in valueConverterTests) {
        var (@out, err) = tt.c.ConvertValue(tt.@in);
        @string goterr = ""u8;
        if (err != default!) {
            goterr = err.Error();
        }
        if (goterr != tt.err) {
            Ꮡt.Errorf("test %d: %T(%T(%v)) error = %q; want error = %q"u8,
                i, tt.c, tt.@in, tt.@in, goterr, tt.err);
        }
        if (tt.err != ""u8) {
            continue;
        }
        if (!reflect.DeepEqual(@out, tt.@out)) {
            Ꮡt.Errorf("test %d: %T(%T(%v)) = %v (%T); want %v (%T)"u8,
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
