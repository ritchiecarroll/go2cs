// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database.sql;

using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using time = time_package;

partial class driver_package {

// ValueConverter is the interface providing the ConvertValue method.
//
// Various implementations of ValueConverter are provided by the
// driver package to provide consistent implementations of conversions
// between drivers. The ValueConverters have several uses:
//
//   - converting from the [Value] types as provided by the sql package
//     into a database table's specific column type and making sure it
//     fits, such as making sure a particular int64 fits in a
//     table's uint16 column.
//
//   - converting a value as given from the database into one of the
//     driver [Value] types.
//
//   - by the [database/sql] package, for converting from a driver's [Value] type
//     to a user's type in a scan.
partial interface ValueConverter {
    // ConvertValue converts a value to a driver Value.
    (Value, error) ConvertValue(any v);
}

// Valuer is the interface providing the Value method.
//
// Errors returned by the [Value] method are wrapped by the database/sql package.
// This allows callers to use [errors.Is] for precise error handling after operations
// like [database/sql.Query], [database/sql.Exec], or [database/sql.QueryRow].
//
// Types implementing Valuer interface are able to convert
// themselves to a driver [Value].
partial interface Valuer {
    // Value returns a driver Value.
    // Value must not panic.
    (Value, error) Value();
}

// Bool is a [ValueConverter] that converts input values to bool.
//
// The conversion rules are:
//   - booleans are returned unchanged
//   - for integer types,
//     1 is true
//     0 is false,
//     other integers are an error
//   - for strings and []byte, same rules as [strconv.ParseBool]
//   - all other types are an error
public static boolType Bool;

public partial struct boolType {
}

internal static ValueConverter _ᴛ3ʗ = new boolType(nil);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string boolˢ = "Bool"u8;

public static @string String(this boolType _) {
    return boolˢ;
}

public static (Value, error) ConvertValue(this boolType _, any src) {
    switch (src.type()) {
    case bool s: {
        return (s, default!);
    }
    case @string s: {
        var (b, err) = strconv.ParseBool(s);
        if (err != default!) {
            return (default!, fmt.Errorf("sql/driver: couldn't convert %q into type bool"u8, s));
        }
        return (b, default!);
    }
    case slice<byte> s: {
        var (b, err) = strconv.ParseBool(((@string)s));
        if (err != default!) {
            return (default!, fmt.Errorf("sql/driver: couldn't convert %q into type bool"u8, s));
        }
        return (b, default!);
    }}
    var sv = reflect.ValueOf(src);
    var exprᴛ1 = sv.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        var iv = sv.Int();
        if (iv == 1 || iv == 0) {
            return (iv == 1, default!);
        }
        return (default!, fmt.Errorf("sql/driver: couldn't convert %d into type bool"u8, iv));
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64) {
        var uv = sv.Uint();
        if (uv == 1 || uv == 0) {
            return (uv == 1, default!);
        }
        return (default!, fmt.Errorf("sql/driver: couldn't convert %d into type bool"u8, uv));
    }

    return (default!, fmt.Errorf("sql/driver: couldn't convert %v (%T) into type bool"u8, src, src));
}

// Int32 is a [ValueConverter] that converts input values to int64,
// respecting the limits of an int32 value.
public static int32Type Int32;

public partial struct int32Type {
}

internal static ValueConverter _ᴛ4ʗ = new int32Type(nil);

public static (Value, error) ConvertValue(this int32Type _, any v) {
    var rv = reflect.ValueOf(v);
    var exprᴛ1 = rv.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        var i64 = rv.Int();
        if (i64 > (2147483648L) - 1 || i64 < -(2147483648L)) {
            return (default!, fmt.Errorf("sql/driver: value %d overflows int32"u8, v));
        }
        return (i64, default!);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64) {
        var u64 = rv.Uint();
        if (u64 > (uint64)((2147483648L) - 1)) {
            return (default!, fmt.Errorf("sql/driver: value %d overflows int32"u8, v));
        }
        return ((int64)u64, default!);
    }
    if (exprᴛ1 == reflect.ΔString) {
        var (i, err) = strconv.Atoi(rv.String());
        if (err != default!) {
            return (default!, fmt.Errorf("sql/driver: value %q can't be converted to int32"u8, v));
        }
        return ((int64)i, default!);
    }

    return (default!, fmt.Errorf("sql/driver: unsupported value %v (type %T) converting to int32"u8, v, v));
}

// String is a [ValueConverter] that converts its input to a string.
// If the value is already a string or []byte, it's unchanged.
// If the value is of another type, conversion to string is done
// with fmt.Sprintf("%v", v).
public static stringType ΔString;

public partial struct stringType {
}

public static (Value, error) ConvertValue(this stringType _, any v) {
    switch (v.type()) {
    case @string _:
    case slice<byte> _: {
        return (v, default!);
    }}

    return (fmt.Sprintf("%v"u8, v), default!);
}

// Null is a type that implements [ValueConverter] by allowing nil
// values but otherwise delegating to another [ValueConverter].
partial struct Null {
    public ValueConverter Converter;
}

public static (Value, error) ConvertValue(this Null n, any v) {
    if (v == default!) {
        return (default!, default!);
    }
    return n.Converter.ConvertValue(v);
}

// NotNull is a type that implements [ValueConverter] by disallowing nil
// values but otherwise delegating to another [ValueConverter].
partial struct NotNull {
    public ValueConverter Converter;
}

public static (Value, error) ConvertValue(this NotNull n, any v) {
    if (v == default!) {
        return (default!, fmt.Errorf("nil value not allowed"u8));
    }
    return n.Converter.ConvertValue(v);
}

// IsValue reports whether v is a valid [Value] parameter type.
public static bool IsValue(any v) {
    if (v == default!) {
        return true;
    }
    switch (v.type()) {
    case slice<byte> _:
    case bool _:
    case float64 _:
    case int64 _:
    case @string _:
    case time.Time _: {
        return true;
    }
    case {} ᴛ0 when ᴛ0._<decimalDecompose>(out var _): {
        return true;
    }}

    return false;
}

// IsScanValue is equivalent to [IsValue].
// It exists for compatibility.
public static bool IsScanValue(any v) {
    return IsValue(v);
}

// DefaultParameterConverter is the default implementation of
// [ValueConverter] that's used when a [Stmt] doesn't implement
// [ColumnConverter].
//
// DefaultParameterConverter returns its argument directly if
// IsValue(arg). Otherwise, if the argument implements [Valuer], its
// Value method is used to return a [Value]. As a fallback, the provided
// argument's underlying type is used to convert it to a [Value]:
// underlying integer types are converted to int64, floats to float64,
// bool, string, and []byte to themselves. If the argument is a nil
// pointer, defaultConverter.ConvertValue returns a nil [Value].
// If the argument is a non-nil pointer, it is dereferenced and
// defaultConverter.ConvertValue is called recursively. Other types
// are an error.
public static defaultConverter DefaultParameterConverter;

public partial struct defaultConverter {
}

internal static ValueConverter _ᴛ5ʗ = new defaultConverter(nil);

internal static reflectꓸType valuerReflectType = reflect.TypeFor<Valuer>();

// callValuerValue returns vr.Value(), with one exception:
// If vr.Value is an auto-generated method on a pointer type and the
// pointer is nil, it would panic at runtime in the panicwrap
// method. Treat it like nil instead.
// Issue 8415.
//
// This is so people can implement driver.Value on value types and
// still use nil pointers to those types to mean nil/NULL, just like
// string/*string.
//
// This function is mirrored in the database/sql package.
internal static (Value v, error err) callValuerValue(Valuer vr) {
    {
        var rv = reflect.ValueOf(vr); if (rv.Kind() == reflect.ΔPointer && rv.IsNil() && rv.Type().Elem().Implements(valuerReflectType)) {
            return (default!, default!);
        }
    }
    return vr.Value();
}

public static (Value, error) ConvertValue(this defaultConverter _, any v) {
    if (IsValue(v)) {
        return (v, default!);
    }
    switch (v.type()) {
    case {} Δvr when Δvr._<Valuer>(out var vr): {
        var (sv, err) = callValuerValue(vr);
        if (err != default!) {
            return (default!, err);
        }
        if (!IsValue(sv)) {
            return (default!, fmt.Errorf("non-Value type %T returned from Value"u8, sv));
        }
        return (sv, default!);
    }
    case {} Δvr when Δvr._<decimalDecompose>(out var vr): {
        return (vr, default!);
    }}
    // For now, continue to prefer the Valuer interface over the decimal decompose interface.
    var rv = reflect.ValueOf(v);
    var exprᴛ1 = rv.Kind();
    if (exprᴛ1 == reflect.ΔPointer) {
        if (rv.IsNil()){
            // indirect pointers
            return (default!, default!);
        } else {
            return new defaultConverter(nil).ConvertValue(rv.Elem().Interface());
        }
    }
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return (rv.Int(), default!);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32) {
        return ((int64)rv.Uint(), default!);
    }
    if (exprᴛ1 == reflect.Uint64) {
        var u64 = rv.Uint();
        if (u64 >= ((uint64)1 << (int)(63))) {
            return (default!, fmt.Errorf("uint64 values with high bit set are not supported"u8));
        }
        return ((int64)u64, default!);
    }
    if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        return (rv.Float(), default!);
    }
    if (exprᴛ1 == reflect.ΔBool) {
        return (rv.Bool(), default!);
    }
    if (exprᴛ1 == reflect.ΔSlice) {
        reflectꓸKind ek = rv.Type().Elem().Kind();
        if (ek == reflect.Uint8) {
            return (rv.Bytes(), default!);
        }
        return (default!, fmt.Errorf("unsupported type %T, a slice of %s"u8, v, ek));
    }
    if (exprᴛ1 == reflect.ΔString) {
        return (rv.String(), default!);
    }

    return (default!, fmt.Errorf("unsupported type %T, a %s"u8, v, rv.Kind()));
}

partial interface decimalDecompose {
    // Decompose returns the internal decimal state into parts.
    // If the provided buf has sufficient capacity, buf may be returned as the coefficient with
    // the value set and length set as appropriate.
    (byte form, bool negative, slice<byte> coefficient, int32 exponent) Decompose(slice<byte> buf);
}

} // end driver_package
