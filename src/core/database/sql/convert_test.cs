// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using driver = go.database.sql.driver_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using reflect = reflect_package;
using runtime = runtime_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using go.database.sql;
using static go.database.sql_package;
using ꓸꓸꓸany = Span<any>;

partial class sql_internal_test_package {

internal static time.Time someTime = time.Unix(123, 0);

internal static ж<int64> Ꮡanswer = new StandardBox<int64>(42);
internal static ref int64 answer => ref Ꮡanswer.Value;

[GoType("num:float64")] internal partial struct userDefined;

[GoType("[]nint")] internal partial struct userDefinedSlice;

[GoType("@string")] internal partial struct userDefinedString;

[GoType] internal partial struct conversionTest {
    internal any s, d; // source and destination
    // following are used if they're non-zero
    internal int64 wantint;
    internal uint64 wantuint;
    internal @string wantstr;
    internal slice<byte> wantbytes;
    internal global::go.database.sql_package.RawBytes wantraw;
    internal float32 wantf32;
    internal float64 wantf64;
    internal time.Time wanttime;
    internal bool wantbool; // used if d is of type *bool
    internal @string wanterr;
    internal any wantiface;
    internal ж<int64> wantptr; // if non-nil, *d's pointed value must be equal to *wantptr
    internal bool wantnil;   // if true, *d must be *int64(nil)
    internal userDefined wantusrdef;
    internal userDefinedString wantusrstr;
}

// Target variables for scanning into.
internal static ж<@string> Ꮡscanstr = new StandardBox<@string>(default(@string));
internal static ref @string scanstr => ref Ꮡscanstr.Value;

internal static ж<slice<byte>> Ꮡscanbytes = new StandardBox<slice<byte>>(default(slice<byte>));
internal static ref slice<byte> scanbytes => ref Ꮡscanbytes.ValueSlot;

internal static ж<global::go.database.sql_package.RawBytes> Ꮡscanraw = new StandardBox<global::go.database.sql_package.RawBytes>(default(global::go.database.sql_package.RawBytes));
internal static ref global::go.database.sql_package.RawBytes scanraw => ref Ꮡscanraw.ValueSlot;

internal static ж<nint> Ꮡscanint = new StandardBox<nint>(default(nint));
internal static ref nint scanint => ref Ꮡscanint.Value;

internal static ж<uint8> Ꮡscanuint8 = new StandardBox<uint8>(default(uint8));
internal static ref uint8 scanuint8 => ref Ꮡscanuint8.Value;

internal static ж<uint16> Ꮡscanuint16 = new StandardBox<uint16>(default(uint16));
internal static ref uint16 scanuint16 => ref Ꮡscanuint16.Value;

internal static ж<bool> Ꮡscanbool = new StandardBox<bool>(default(bool));
internal static ref bool scanbool => ref Ꮡscanbool.Value;

internal static ж<float32> Ꮡscanf32 = new StandardBox<float32>(default(float32));
internal static ref float32 scanf32 => ref Ꮡscanf32.Value;

internal static ж<float64> Ꮡscanf64 = new StandardBox<float64>(default(float64));
internal static ref float64 scanf64 => ref Ꮡscanf64.Value;

internal static ж<time.Time> Ꮡscantime = new StandardBox<time.Time>(default(time.Time));
internal static ref time.Time scantime => ref Ꮡscantime.Value;

internal static ж<ж<int64>> Ꮡscanptr = new StandardBox<ж<int64>>(default(ж<int64>));
internal static ref ж<int64> scanptr => ref Ꮡscanptr.ValueSlot;

internal static ж<any> Ꮡscaniface = new StandardBox<any>(default(any));
internal static ref any scaniface => ref Ꮡscaniface.ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hereˢ = "here"u8;

internal static slice<conversionTest> conversionTests() {
    // Return a fresh instance to test so "go test -count 2" works correctly.
    return new conversionTest[]{ // Exact conversions (destination pointer type matches source type)

        new(s: (@string)"foo"u8, d: Ꮡscanstr, wantstr: "foo"u8),
        new(s: (nint)(123), d: Ꮡscanint, wantint: 123),
        new(s: someTime, d: Ꮡscantime, wanttime: someTime), // To strings

        new(s: (@string)"string"u8, d: Ꮡscanstr, wantstr: "string"u8),
        new(s: slice<byte>("byteslice"u8), d: Ꮡscanstr, wantstr: "byteslice"u8),
        new(s: (nint)(123), d: Ꮡscanstr, wantstr: "123"u8),
        new(s: (int8)123, d: Ꮡscanstr, wantstr: "123"u8),
        new(s: (int64)123, d: Ꮡscanstr, wantstr: "123"u8),
        new(s: (uint8)123, d: Ꮡscanstr, wantstr: "123"u8),
        new(s: (uint16)123, d: Ꮡscanstr, wantstr: "123"u8),
        new(s: (uint32)123, d: Ꮡscanstr, wantstr: "123"u8),
        new(s: (uint64)123, d: Ꮡscanstr, wantstr: "123"u8),
        new(s: 1.5D, d: Ꮡscanstr, wantstr: "1.5"u8), // From time.Time:

        new(s: time.Unix(1, 0).UTC(), d: Ꮡscanstr, wantstr: "1970-01-01T00:00:01Z"u8),
        new(s: time.Unix(1453874597, 0).In(time.FixedZone(hereˢ, -3600 * 8)), d: Ꮡscanstr, wantstr: "2016-01-26T22:03:17-08:00"u8),
        new(s: time.Unix(1, 2).UTC(), d: Ꮡscanstr, wantstr: "1970-01-01T00:00:01.000000002Z"u8),
        new(s: new time.Time(nil), d: Ꮡscanstr, wantstr: "0001-01-01T00:00:00Z"u8),
        new(s: time.Unix(1, 2).UTC(), d: Ꮡscanbytes, wantbytes: slice<byte>("1970-01-01T00:00:01.000000002Z"u8)),
        new(s: time.Unix(1, 2).UTC(), d: Ꮡscaniface, wantiface: time.Unix(1, 2).UTC()), // To []byte

        new(s: default!, d: Ꮡscanbytes, wantbytes: default!),
        new(s: (@string)"string"u8, d: Ꮡscanbytes, wantbytes: slice<byte>("string"u8)),
        new(s: slice<byte>("byteslice"u8), d: Ꮡscanbytes, wantbytes: slice<byte>("byteslice"u8)),
        new(s: (nint)(123), d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: (int8)123, d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: (int64)123, d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: (uint8)123, d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: (uint16)123, d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: (uint32)123, d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: (uint64)123, d: Ꮡscanbytes, wantbytes: slice<byte>("123"u8)),
        new(s: 1.5D, d: Ꮡscanbytes, wantbytes: slice<byte>("1.5"u8)), // To RawBytes

        new(s: default!, d: Ꮡscanraw, wantraw: default!),
        new(s: slice<byte>("byteslice"u8), d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"byteslice"u8))),
        new(s: (@string)"string"u8, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"string"u8))),
        new(s: (nint)(123), d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: (int8)123, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: (int64)123, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: (uint8)123, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: (uint16)123, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: (uint32)123, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: (uint64)123, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"123"u8))),
        new(s: 1.5D, d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"1.5"u8))), // time.Time has been placed here to check that the RawBytes slice gets
 // correctly reset when calling time.Time.AppendFormat.

        new(s: time.Unix(2, 5).UTC(), d: Ꮡscanraw, wantraw: ((global::go.database.sql_package.RawBytes)slice<byte>((@string)"1970-01-01T00:00:02.000000005Z"u8))), // Strings to integers

        new(s: (@string)"255"u8, d: Ꮡscanuint8, wantuint: 255),
        new(s: (@string)"256"u8, d: Ꮡscanuint8, wanterr: "converting driver.Value type string (\"256\") to a uint8: value out of range"u8),
        new(s: (@string)"256"u8, d: Ꮡscanuint16, wantuint: 256),
        new(s: (@string)"-1"u8, d: Ꮡscanint, wantint: -1),
        new(s: (@string)"foo"u8, d: Ꮡscanint, wanterr: "converting driver.Value type string (\"foo\") to a int: invalid syntax"u8), // int64 to smaller integers

        new(s: (int64)5, d: Ꮡscanuint8, wantuint: 5),
        new(s: (int64)256, d: Ꮡscanuint8, wanterr: "converting driver.Value type int64 (\"256\") to a uint8: value out of range"u8),
        new(s: (int64)256, d: Ꮡscanuint16, wantuint: 256),
        new(s: (int64)65536, d: Ꮡscanuint16, wanterr: "converting driver.Value type int64 (\"65536\") to a uint16: value out of range"u8), // True bools

        new(s: true, d: Ꮡscanbool, wantbool: true),
        new(s: (@string)"True"u8, d: Ꮡscanbool, wantbool: true),
        new(s: (@string)"TRUE"u8, d: Ꮡscanbool, wantbool: true),
        new(s: (@string)"1"u8, d: Ꮡscanbool, wantbool: true),
        new(s: (nint)(1), d: Ꮡscanbool, wantbool: true),
        new(s: (int64)1, d: Ꮡscanbool, wantbool: true),
        new(s: (uint16)1, d: Ꮡscanbool, wantbool: true), // False bools

        new(s: false, d: Ꮡscanbool, wantbool: false),
        new(s: (@string)"false"u8, d: Ꮡscanbool, wantbool: false),
        new(s: (@string)"FALSE"u8, d: Ꮡscanbool, wantbool: false),
        new(s: (@string)"0"u8, d: Ꮡscanbool, wantbool: false),
        new(s: (nint)(0), d: Ꮡscanbool, wantbool: false),
        new(s: (int64)0, d: Ꮡscanbool, wantbool: false),
        new(s: (uint16)0, d: Ꮡscanbool, wantbool: false), // Not bools

        new(s: (@string)"yup"u8, d: Ꮡscanbool, wanterr: @"sql/driver: couldn't convert ""yup"" into type bool"u8),
        new(s: (nint)(2), d: Ꮡscanbool, wanterr: @"sql/driver: couldn't convert 2 into type bool"u8), // Floats

        new(s: (float64)1.5D, d: Ꮡscanf64, wantf64: (float64)1.5D),
        new(s: (int64)1, d: Ꮡscanf64, wantf64: (float64)1D),
        new(s: (float64)1.5D, d: Ꮡscanf32, wantf32: (float32)1.5F),
        new(s: (@string)"1.5"u8, d: Ꮡscanf32, wantf32: (float32)1.5F),
        new(s: (@string)"1.5"u8, d: Ꮡscanf64, wantf64: (float64)1.5D), // Pointers

        new(s: ((any)default!), d: Ꮡscanptr, wantnil: true),
        new(s: (int64)42, d: Ꮡscanptr, wantptr: Ꮡanswer), // To interface{}

        new(s: (float64)1.5D, d: Ꮡscaniface, wantiface: (float64)1.5D),
        new(s: (int64)1, d: Ꮡscaniface, wantiface: (int64)1),
        new(s: (@string)"str"u8, d: Ꮡscaniface, wantiface: (@string)"str"u8),
        new(s: slice<byte>("byteslice"u8), d: Ꮡscaniface, wantiface: slice<byte>("byteslice"u8)),
        new(s: true, d: Ꮡscaniface, wantiface: true),
        new(s: default!, d: Ꮡscaniface),
        new(s: slice<byte>(default!), d: Ꮡscaniface, wantiface: slice<byte>(default!)), // To a user-defined type

        new(s: 1.5D, d: @new<userDefined>(), wantusrdef: 1.5D),
        new(s: (int64)123, d: @new<userDefined>(), wantusrdef: 123D),
        new(s: (@string)"1.5"u8, d: @new<userDefined>(), wantusrdef: 1.5D),
        new(s: new byte[]{1, 2, 3}.slice(), d: @new<userDefinedSlice>(), wanterr: @"unsupported Scan, storing driver.Value type []uint8 into type *sql.userDefinedSlice"u8),
        new(s: (@string)"str"u8, d: @new<userDefinedString>(), wantusrstr: "str"u8), // Other errors

        new(s: complex(1D, 2D), d: Ꮡscanstr, wanterr: @"unsupported Scan, storing driver.Value type complex128 into type *string"u8)
    }.slice();
}

internal static any intPtrValue(any intptr) {
    return reflect.Indirect(reflect.Indirect(reflect.ValueOf(intptr))).Int();
}

internal static int64 intValue(any intptr) {
    return reflect.Indirect(reflect.ValueOf(intptr)).Int();
}

internal static uint64 uintValue(any intptr) {
    return reflect.Indirect(reflect.ValueOf(intptr)).Uint();
}

internal static float64 float64Value(any ptr) {
    return (ptr._<ж<float64>>()).Value;
}

internal static float32 float32Value(any ptr) {
    return (ptr._<ж<float32>>()).Value;
}

internal static time.Time timeValue(any ptr) {
    return (ptr._<ж<time.Time>>()).Value;
}

public static void TestConversions(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (n, vᴛ1) in conversionTests()) {
        ref var ct = ref heap(new conversionTest(), out var Ꮡct);
        ct = vᴛ1;

        var err = convertAssign(ct.d, ct.s);
        @string errstr = ""u8;
        if (err != default!) {
            errstr = err.Error();
        }
        var ctʗ1 = ct;
        void errf(@string format, params ꓸꓸꓸany argsʗp) {
            var args = argsʗp.sslice();
            @string @base = fmt.Sprintf("convertAssign #%d: for %v (%T) -> %T, "u8, n, ctʗ1.s, ctʗ1.s, ctʗ1.d);
            Ꮡt.Errorf(@base + format, args.ꓸꓸꓸ);
        }
        if (errstr != ct.wanterr) {
            errf("got error %q, want error %q"u8, errstr, ct.wanterr);
        }
        if (ct.wantstr != ""u8 && ct.wantstr != scanstr) {
            errf("want string %q, got %q"u8, ct.wantstr, scanstr);
        }
        if (ct.wantbytes != default! && ((sstring)ct.wantbytes) != ((sstring)scanbytes)) {
            errf("want byte %q, got %q"u8, ct.wantbytes, scanbytes);
        }
        if (ct.wantraw != default! && ((@string)(slice<byte>)ct.wantraw) != ((@string)(slice<byte>)scanraw)) {
            errf("want RawBytes %q, got %q"u8, ct.wantraw, scanraw);
        }
        if (ct.wantint != 0 && ct.wantint != intValue(ct.d)) {
            errf("want int %d, got %d"u8, ct.wantint, intValue(ct.d));
        }
        if (ct.wantuint != 0 && ct.wantuint != uintValue(ct.d)) {
            errf("want uint %d, got %d"u8, ct.wantuint, uintValue(ct.d));
        }
        if (ct.wantf32 != 0F && ct.wantf32 != float32Value(ct.d)) {
            errf("want float32 %v, got %v"u8, ct.wantf32, float32Value(ct.d));
        }
        if (ct.wantf64 != 0D && ct.wantf64 != float64Value(ct.d)) {
            errf("want float32 %v, got %v"u8, ct.wantf64, float64Value(ct.d));
        }
        {
            var (bp, boolTest) = ct.d._<ж<bool>>(ᐧ); if (boolTest && bp.Value != ct.wantbool && ct.wanterr == ""u8) {
                errf("want bool %v, got %v"u8, ct.wantbool, bp.Value);
            }
        }
        if (!ct.wanttime.IsZero() && !ct.wanttime.Equal(timeValue(ct.d))) {
            errf("want time %v, got %v"u8, ct.wanttime, timeValue(ct.d));
        }
        if (ct.wantnil && ct.d._<ж<ж<int64>>>().ValueSlot != nil) {
            errf("want nil, got %v"u8, intPtrValue(ct.d));
        }
        if (ct.wantptr != nil) {
            if (ct.d._<ж<ж<int64>>>().ValueSlot == nil){
                errf("want pointer to %v, got nil"u8, ct.wantptr.Value);
            } else 
            if (!AreEqual(ct.wantptr.Value, intPtrValue(ct.d))) {
                errf("want pointer to %v, got %v"u8, ct.wantptr.Value, intPtrValue(ct.d));
            }
        }
        {
            var (ifptr, ok) = ct.d._<ж<any>>(ᐧ); if (ok) {
                if (!reflect.DeepEqual(ct.wantiface, scaniface)) {
                    errf("want interface %#v, got %#v"u8, ct.wantiface, scaniface);
                    continue;
                }
                {
                    var (srcBytes, okΔ1) = ct.s._<slice<byte>>(ᐧ); if (okΔ1) {
                        var dstBytes = (ifptr.ValueSlot)._<slice<byte>>();
                        if (len(srcBytes) > 0 && Ꮡ(dstBytes, 0) == Ꮡ(srcBytes, 0)) {
                            errf("copy into interface{} didn't copy []byte data"u8);
                        }
                    }
                }
            }
        }
        if (ct.wantusrdef != 0D && ct.wantusrdef != ct.d._<ж<userDefined>>().Value) {
            errf("want userDefined %f, got %f"u8, ct.wantusrdef, ct.d._<ж<userDefined>>().Value);
        }
        if (len(ct.wantusrstr) != 0 && ct.wantusrstr != ct.d._<ж<userDefinedString>>().Value) {
            errf("want userDefined %q, got %q"u8, ct.wantusrstr, ct.d._<ж<userDefinedString>>().Value);
        }
    }
}

public static void TestNullString(ж<testing.T> Ꮡt) {
    ref var ns = ref heap(new global::go.database.sql_package.NullString(), out var Ꮡns);
    convertAssign(Ꮡns, slice<byte>("foo"u8));
    if (!ns.Valid) {
        Ꮡt.Errorf("expecting not null"u8);
    }
    if (ns.String != "foo"u8) {
        Ꮡt.Errorf("expecting foo; got %q"u8, ns.String);
    }
    convertAssign(Ꮡns, default!);
    if (ns.Valid) {
        Ꮡt.Errorf("expecting null on nil"u8);
    }
    if (ns.String != ""u8) {
        Ꮡt.Errorf("expecting blank on nil; got %q"u8, ns.String);
    }
}

[GoType] internal partial struct valueConverterTest {
    internal driver.ValueConverter c;
    internal any @in, @out;
    internal @string err;
}

internal static slice<valueConverterTest> valueConverterTests = new valueConverterTest[]{
    new(driver.DefaultParameterConverter, new NullString("hi"u8, true), (@string)"hi"u8, ""u8),
    new(driver.DefaultParameterConverter, new NullString(""u8, false), default!, ""u8)
}.slice();

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static readonly @string stringˢ = "string"u8;
internal static readonly @string fooˢ = "foo"u8;

[GoType("dyn")] internal partial struct TestRawBytesAllocs_type {
    internal @string name;
    internal any @in;
    internal @string want;
}

// Tests that assigning to RawBytes doesn't allocate (and also works).
public static void TestRawBytesAllocs(ж<testing.T> Ꮡt) {
    slice<TestRawBytesAllocs_type> tests = new TestRawBytesAllocs_type[]{
        new("uint64"u8, (uint64)12345678, "12345678"u8),
        new("uint32"u8, (uint32)1234, "1234"u8),
        new("uint16"u8, (uint16)12, "12"u8),
        new("uint8"u8, (uint8)1, "1"u8),
        new("uint"u8, (nuint)123, "123"u8),
        new("int"u8, (nint)123, "123"u8),
        new("int8"u8, (int8)1, "1"u8),
        new("int16"u8, (int16)12, "12"u8),
        new("int32"u8, (int32)1234, "1234"u8),
        new("int64"u8, (int64)12345678, "12345678"u8),
        new("float32"u8, (float32)1.5F, "1.5"u8),
        new("float64"u8, (float64)64D, "64"u8),
        new("bool"u8, false, "false"u8),
        new("time"u8, time.Unix(2, 5).UTC(), "1970-01-01T00:00:02.000000005Z"u8)
    }.slice();
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    ref var buf = ref heap<global::go.database.sql_package.RawBytes>(out var Ꮡbuf);
    var rows = Ꮡ(new Rows(nil));
    var rowsʗ1 = rows;
    void test(@string name, any @in, @string want) {
        {
            var err = convertAssignRows(Ꮡbuf, @in, rowsʗ1); if (err != default!) {
                Ꮡt.Fatalf("%s: convertAssign = %v"u8, name, err);
            }
        }
        var match = len(Ꮡbuf.ValueSlot) == len(want);
        if (match) {
            foreach (var (i, b) in Ꮡbuf.ValueSlot) {
                if (want[i] != b) {
                    match = false;
                    break;
                }
            }
        }
        if (!match) {
            Ꮡt.Fatalf("%s: got %q (len %d); want %q (len %d)"u8, name, Ꮡbuf.ValueSlot, len(Ꮡbuf.ValueSlot), want, len(want));
        }
    }
    var rowsʗ2 = rows;
    var testʗ1 = test;
    var testsʗ1 = tests;
    var n = testing.AllocsPerRun(100, () => {
        foreach (var (_, tt) in testsʗ1) {
            rowsʗ2.Value.raw = (~rowsʗ2).raw[..0];
            testʗ1(tt.name, tt.@in, tt.want);
        }
    });
    // The numbers below are only valid for 64-bit interface word sizes,
    // and gc. With 32-bit words there are more convT2E allocs, and
    // with gccgo, only pointers currently go in interface data.
    // So only care on amd64 gc for now.
    var measureAllocs = false;
    var exprᴛ1 = runtime.GOARCH;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8) {
        measureAllocs = runtime.Compiler == "gc";
    }

    if (n > 0.5D && measureAllocs) {
        Ꮡt.Fatalf("allocs = %v; want 0"u8, n);
    }
    // This one involves a convT2E allocation, string -> interface{}
    var testʗ2 = test;
    n = testing.AllocsPerRun(100, () => {
        testʗ2(stringˢ, fooˢ, fooˢ);
    });
    if (n > 1.5D && measureAllocs) {
        Ꮡt.Fatalf("allocs = %v; want max 1"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object userDefinedBytesGotˢ = (@string)"userDefinedBytes got potentially dirty driver memory"u8;

[GoLocalName("userDefinedBytes")] [GoType("[]byte")] internal partial struct TestUserDefinedBytes_userDefinedBytes;

// https://golang.org/issues/13905
public static void TestUserDefinedBytes(ж<testing.T> Ꮡt) {
    ref var u = ref heap<TestUserDefinedBytes_userDefinedBytes>(out var Ꮡu);
    var v = slice<byte>("foo"u8);
    convertAssign(Ꮡu, v);
    if (Ꮡ(u, 0) == Ꮡ(v, 0)) {
        Ꮡt.Fatal(userDefinedBytesGotˢ);
    }
}

[GoType("@string")] public partial struct Valuer_V;

public static (driverꓸValue, error) Value(this Valuer_V v) {
    return (strings.ToUpper(((@string)v)), default!);
}

[GoType("@string")] public partial struct Valuer_P;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nilToStrˢ = (@string)"nil-to-str"u8;

public static (driverꓸValue, error) Value(this ж<Valuer_P> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        return (nilToStrˢ, default!);
    }
    return (strings.ToUpper(((@string)(p))), default!);
}

[GoType("dyn")] internal partial struct TestDriverArgs_tests {
    internal slice<any> args;
    internal slice<driver.NamedValue> want;
}

public static void TestDriverArgs(ж<testing.T> Ꮡt) {
    ж<Valuer_V> nilValuerVPtr = default!;
    ж<Valuer_P> nilValuerPPtr = default!;
    ж<@string> nilStrPtr = default!;
    var tests = new slice<TestDriverArgs_tests>(5){
        [0] = new(
            args: new any[]{((Valuer_V)(@string)fooˢ)}.slice(),
            want: new driver.NamedValue[]{
                new(
                    Ordinal: 1,
                    Value: (@string)"FOO"u8
                )
            }.slice()
        ),
        [1] = new(
            args: new any[]{nilValuerVPtr.OrTypedNil()}.slice(),
            want: new driver.NamedValue[]{
                new(
                    Ordinal: 1,
                    Value: default!
                )
            }.slice()
        ),
        [2] = new(
            args: new any[]{nilValuerPPtr.OrTypedNil()}.slice(),
            want: new driver.NamedValue[]{
                new(
                    Ordinal: 1,
                    Value: (@string)"nil-to-str"u8
                )
            }.slice()
        ),
        [3] = new(
            args: new any[]{(@string)"plain-str"u8}.slice(),
            want: new driver.NamedValue[]{
                new(
                    Ordinal: 1,
                    Value: (@string)"plain-str"u8
                )
            }.slice()
        ),
        [4] = new(
            args: new any[]{nilStrPtr.OrTypedNil()}.slice(),
            want: new driver.NamedValue[]{
                new(
                    Ordinal: 1,
                    Value: default!
                )
            }.slice()
        )
    };
    foreach (var (i, tt) in tests) {
        var ds = Ꮡ(new driverStmt(Locker: new sync.MutexжLocker(Ꮡ(new sync.Mutex(nil))), si: new stubDriverStmt((error)(default!))));
        var (got, err) = driverArgsConnLocked(default!, ds, tt.args);
        if (err != default!) {
            Ꮡt.Errorf("test[%d]: %v"u8, i, err);
            continue;
        }
        if (!reflect.DeepEqual(got, tt.want)) {
            Ꮡt.Errorf("test[%d]: got %v, want %v"u8, i, got, tt.want);
        }
    }
}

[GoType] [GoValueClone("coefficient")] internal partial struct dec {
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

[GoRecv] internal static error Compose(this ref dec d, byte form, bool negative, slice<byte> coefficient, int32 exponent) {
    switch (form) {
    default: {
        return fmt.Errorf("unknown form %d"u8, form);
    }
    case 1 or 2: {
        d.form = form;
        d.neg = negative;
        return default!;
    }
    case 0: {
        break;
    }}

    d.form = form;
    d.neg = negative;
    d.exponent = exponent;
    // This isn't strictly correct, as the extra bytes could be all zero,
    // ignore this for this test.
    if (len(coefficient) > 16) {
        return fmt.Errorf("coefficient too large"u8);
    }
    copy(d.coefficient[..], coefficient);
    return default!;
}

[GoType] [GoValueClone("coefficient")] internal partial struct decFinite {
    internal bool neg;
    internal array<byte> coefficient = new(16);
    internal int32 exponent;
}

internal static (byte form, bool negative, slice<byte> coefficient, int32 exponent) Decompose(this decFinite d, slice<byte> buf) {
    d = d.ΔClone();

    var coef = new slice<byte>(16);
    copy(coef, d.coefficient[..]);
    return (0, d.neg, coef, d.exponent);
}

[GoRecv] internal static error Compose(this ref decFinite d, byte form, bool negative, slice<byte> coefficient, int32 exponent) {
    switch (form) {
    default: {
        return fmt.Errorf("unknown form %d"u8, form);
    }
    case 1 or 2: {
        return fmt.Errorf("unsupported form %d"u8, form);
    }
    case 0: {
        break;
    }}

    d.neg = negative;
    d.exponent = exponent;
    // This isn't strictly correct, as the extra bytes could be all zero,
    // ignore this for this test.
    if (len(coefficient) > 16) {
        return fmt.Errorf("coefficient too large"u8);
    }
    copy(d.coefficient[..], coefficient);
    return default!;
}

[GoType("dyn")] [GoValueClone("@out")] internal partial struct TestDecimal_list {
    internal @string name;
    internal global::go.database.sql_package.decimalDecompose @in;
    internal dec @out;
    internal bool err;
}

public static void TestDecimal(ж<testing.T> Ꮡt) {
    var list = new TestDecimal_list[]{
        new(name: "same"u8, @in: new dec(exponent: -6), @out: new dec(exponent: -6)), // Ensure reflection is not used to assign the value by using different types.

        new(name: "diff"u8, @in: new decFinite(exponent: -6), @out: new dec(exponent: -6)),
        new(name: "bad-form"u8, @in: new dec(form: 200), err: true)
    }.slice();
    foreach (var (_, vᴛ1) in list) {
        ref var item = ref heap(new TestDecimal_list(), out var Ꮡitem);
        item = vᴛ1.ΔClone();

        var itemʗ1 = item;
        Ꮡt.Run(item.name, (ж<testing.T> tΔ1) => {
            ref var @out = ref heap<dec>(out var Ꮡout);
            @out = new dec(nil);
            var err = convertAssign(Ꮡout, itemʗ1.@in);
            if (itemʗ1.err) {
                if (err == default!) {
                    tΔ1.Fatalf("unexpected nil error"u8);
                }
                return;
            }
            if (err != default!) {
                tΔ1.Fatalf("unexpected error: %v"u8, err);
            }
            if (!reflect.DeepEqual(@out, itemʗ1.@out)) {
                tΔ1.Fatalf("got %#v want %#v"u8, @out, itemʗ1.@out);
            }
        });
    }
}

} // end sql_internal_test_package
