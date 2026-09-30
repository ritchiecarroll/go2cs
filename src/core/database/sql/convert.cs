// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Type conversions for Scan.
namespace go.database;

using bytes = bytes_package;
using driver = go.database.sql.driver_package;
using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using time = time_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using go.database.sql;
using go.unicode;

partial class sql_package {

internal static error errNilPtr = errors.New("destination pointer is nil"u8); // embedded in descriptive error

internal static @string describeNamedValue(ref driver.NamedValue nv) {
    if (len(nv.Name) == 0) {
        return fmt.Sprintf("$%d"u8, nv.Ordinal);
    }
    return fmt.Sprintf("with name %q"u8, nv.Name);
}

internal static error validateNamedValueName(@string name) {
    if (len(name) == 0) {
        return default!;
    }
    var (r, _) = utf8.DecodeRuneInString(name);
    if (unicode.IsLetter(r)) {
        return default!;
    }
    return fmt.Errorf("name %q does not begin with a letter"u8, name);
}

// ccChecker wraps the driver.ColumnConverter and allows it to be used
// as if it were a NamedValueChecker. If the driver ColumnConverter
// is not present then the NamedValueChecker will return driver.ErrSkip.
[GoType] partial struct ccChecker {
    internal driver.ColumnConverter cci;
    internal nint want;
}

internal static error CheckNamedValue(this ccChecker c, ж<driver.NamedValue> Ꮡnv) {
    ref var nv = ref Ꮡnv.DerefOrNull();

    if (c.cci == default!) {
        return driver.ErrSkip;
    }
    // The column converter shouldn't be called on any index
    // it isn't expecting. The final error will be thrown
    // in the argument converter loop.
    nint index = nv.Ordinal - 1;
    if (c.want <= index) {
        return default!;
    }
    // First, see if the value itself knows how to convert
    // itself to a driver type. For example, a NullString
    // struct changing into a string or nil.
    {
        var (vr, ok) = nv.Value._<driver.Valuer>(ᐧ); if (ok) {
            var (sv, errΔ1) = callValuerValue(vr);
            if (errΔ1 != default!) {
                return errΔ1;
            }
            if (!driver.IsValue(sv)) {
                return fmt.Errorf("non-subset type %T returned from Value"u8, sv);
            }
            nv.Value = sv;
        }
    }
    // Second, ask the column to sanity check itself. For
    // example, drivers might use this to make sure that
    // an int64 values being inserted into a 16-bit
    // integer field is in range (before getting
    // truncated), or that a nil can't go into a NOT NULL
    // column before going across the network to get the
    // same error.
    error err = default!;
    var arg = nv.Value;
    (nv.Value, err) = c.cci.ColumnConverter(index).ConvertValue(arg);
    if (err != default!) {
        return err;
    }
    if (!driver.IsValue(nv.Value)) {
        return fmt.Errorf("driver ColumnConverter error converted %T to unsupported type %T"u8, arg, nv.Value);
    }
    return default!;
}

// defaultCheckNamedValue wraps the default ColumnConverter to have the same
// function signature as the CheckNamedValue in the driver.NamedValueChecker
// interface.
internal static error /*err*/ defaultCheckNamedValue(ж<driver.NamedValue> Ꮡnv) {
    error err = default!;

    ref var nv = ref Ꮡnv.DerefOrNull();
    (nv.Value, err) = driver.DefaultParameterConverter.ConvertValue(nv.Value);
    return err;
}

// driverArgsConnLocked converts arguments from callers of Stmt.Exec and
// Stmt.Query into driver Values.
//
// The statement ds may be nil, if no statement is available.
//
// ci must be locked.
internal static (slice<driver.NamedValue>, error) driverArgsConnLocked(driver.Conn ci, ж<driverStmt> Ꮡds, slice<any> args) {
    ref var ds = ref Ꮡds.DerefOrNull();

    var nvargs = new slice<driver.NamedValue>(len(args));
    // -1 means the driver doesn't know how to count the number of
    // placeholders, so we won't sanity check input here and instead let the
    // driver deal with errors.
    nint want = -1;
    driver.Stmt si = default!;
    ccChecker cc = default!;
    if (Ꮡds != nil) {
        si = ds.si;
        want = ds.si.NumInput();
        cc.want = want;
    }
    // Check all types of interfaces from the start.
    // Drivers may opt to use the NamedValueChecker for special
    // argument types, then return driver.ErrSkip to pass it along
    // to the column converter.
    var (nvc, ok) = si._<driver.NamedValueChecker>(ᐧ);
    if (!ok) {
        (nvc, _) = ci._<driver.NamedValueChecker>(ᐧ);
    }
    (var cci, ok) = si._<driver.ColumnConverter>(ᐧ);
    if (ok) {
        cc.cci = cci;
    }
    // Loop through all the arguments, checking each one.
    // If no error is returned simply increment the index
    // and continue. However, if driver.ErrRemoveArgument
    // is returned the argument is not included in the query
    // argument list.
    error err = default!;
    nint n = default!;
    foreach (var (_, vᴛ1) in args) {
        var arg = vᴛ1;

        var nv = Ꮡ(nvargs, n);
        {
            var (np, okΔ1) = arg._<NamedArg>(ᐧ); if (okΔ1) {
                {
                    err = validateNamedValueName(np.Name); if (err != default!) {
                        return (default!, err);
                    }
                }
                arg = np.Value;
                nv.Value.Name = np.Name;
            }
        }
        nv.Value.Ordinal = n + 1;
        nv.Value.Value = arg;
        // Checking sequence has four routes:
        // A: 1. Default
        // B: 1. NamedValueChecker 2. Column Converter 3. Default
        // C: 1. NamedValueChecker 3. Default
        // D: 1. Column Converter 2. Default
        //
        // The only time a Column Converter is called is first
        // or after NamedValueConverter. If first it is handled before
        // the nextCheck label. Thus for repeats tries only when the
        // NamedValueConverter is selected should the Column Converter
        // be used in the retry.
        var checker = defaultCheckNamedValue;
        var nextCC = false;
        switch (ᐧ) {
        case {} when nvc != default!: {
            nextCC = cci != default!;
            var nvcʗ1 = nvc;
                        checker = nvcʗ1.CheckNamedValue;
            break;
        }
        case {} when cci != default!: {
            var ccʗ1 = cc;
                        checker = (ж<driver.NamedValue> p1) => ccʗ1.CheckNamedValue(p1);
            break;
        }}

nextCheck:
        err = checker(nv);
        var exprᴛ1 = err;
        if (AreEqual(exprᴛ1, default!)) {
            n++;
            continue;
        }
        else if (AreEqual(exprᴛ1, driver.ErrRemoveArgument)) {
            nvargs = nvargs.slice(0, len(nvargs) - 1);
            continue;
        }
        else if (AreEqual(exprᴛ1, driver.ErrSkip)) {
            if (nextCC){
                nextCC = false;
                var ccʗ2 = cc;
                                checker = (ж<driver.NamedValue> p1) => ccʗ2.CheckNamedValue(p1);
            } else {
                checker = defaultCheckNamedValue;
            }
            goto nextCheck;
        }
        else { /* default: */
            return (default!, fmt.Errorf("sql: converting argument %s type: %w"u8, describeNamedValue(ref (nv).DerefOrNull()), err));
        }

    }
    // Check the length of arguments after conversion to allow for omitted
    // arguments.
    if (want != -1 && len(nvargs) != want) {
        return (default!, fmt.Errorf("sql: expected %d arguments, got %d"u8, want, len(nvargs)));
    }
    return (nvargs, default!);
}

// convertAssign is the same as convertAssignRows, but without the optional
// rows argument.
//
// convertAssign should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - ariga.io/entcache
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname convertAssign
internal static error convertAssign(any dest, any src) {
    return convertAssignRows(dest, src, nil);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidContextToConvertˢ = "invalid context to convert cursor rows, missing parent *Rows"u8;
internal static readonly @string destinationNotAPointerˢ = "destination not a pointer"u8;

// convertAssignRows copies to dest the value in src, converting it if possible.
// An error is returned if the copy would result in loss of information.
// dest should be a pointer type. If rows is passed in, the rows will
// be used as the parent for any cursor values converted from a
// driver.Rows to a *Rows.
internal static error convertAssignRows(any dest, any src, ж<Rows> Ꮡrows) {
    ref var rows = ref Ꮡrows.DerefOrNull();

    // Common cases, without reflect.
    switch (src.type()) {
    case @string s: {
        switch (dest.type()) {
        case ж<@string> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.Value = s;
            return default!;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = slice<byte>(s);
            return default!;
        }
        case ж<RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = Ꮡrows.setrawbuf(append(Ꮡrows.rawbuf(), s.ꓸꓸꓸ));
            return default!;
        }}
        break;
    }
    case slice<byte> s: {
        switch (dest.type()) {
        case ж<@string> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.Value = ((@string)s);
            return default!;
        }
        case ж<any> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = bytes.Clone(s);
            return default!;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = bytes.Clone(s);
            return default!;
        }
        case ж<RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = s;
            return default!;
        }}
        break;
    }
    case time.Time s: {
        switch (dest.type()) {
        case ж<time.Time> d: {
            d.Value = s;
            return default!;
        }
        case ж<@string> d: {
            d.Value = s.Format(time.RFC3339Nano);
            return default!;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = s.AppendFormat(new slice<byte>(0, len(time.RFC3339Nano)), time.RFC3339Nano);
            return default!;
        }
        case ж<RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = Ꮡrows.setrawbuf(s.AppendFormat(Ꮡrows.rawbuf(), time.RFC3339Nano));
            return default!;
        }}
        break;
    }
    case {} Δs when Δs._<decimalDecompose>(out var s): {
        switch (dest.type()) {
        case {} Δd when Δd._<decimalCompose>(out var d): {
            var (ᴛ1, ᴛ2, ᴛ3, ᴛ4) = s.Decompose(default!);
            return d.Compose(ᴛ1, ᴛ2, ᴛ3, ᴛ4);
        }}
        break;
    }
    case null: {
        switch (dest.type()) {
        case ж<any> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = default!;
            return default!;
        }
        case ж<slice<byte>> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = default!;
            return default!;
        }
        case ж<RawBytes> d: {
            if (d == nil) {
                return errNilPtr;
            }
            d.ValueSlot = default!;
            return default!;
        }}
        break;
    }
    case {} Δs when Δs._<driver.Rows>(out var s): {
        switch (dest.type()) {
        case ж<Rows> d: {
            if (d == nil) {
                // The driver is returning a cursor the client may iterate over.
                return errNilPtr;
            }
            if (Ꮡrows == nil) {
                return errors.New(invalidContextToConvertˢ);
            }
            d.Value = new Rows(
                dc: rows.dc,
                releaseConn: (error _) => {
                },
                rowsi: s
            );
            var parentCancel = rows.cancel;
            var parentCancelʗ1 = parentCancel;
            rows.cancel = () => {
                // Chain the cancel function.
                // When Rows.cancel is called, the closemu will be locked as well.
                // So we can access rs.lasterr.
                d.close(Ꮡrows.Value.lasterr);
                if (parentCancelʗ1 != default!) {
                    parentCancelʗ1();
                }
            };
            return default!;
        }}
        break;
    }}
    reflectꓸValue sv = new(nil);
    switch (dest.type()) {
    case ж<@string> d: {
        sv = reflect.ValueOf(src);
        var exprᴛ1 = sv.Kind();
        if (exprᴛ1 == reflect.ΔBool || exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
            d.Value = asString(src);
            return default!;
        }

        break;
    }
    case ж<slice<byte>> d: {
        sv = reflect.ValueOf(src);
        {
            var (b, ok) = asBytes(default!, sv); if (ok) {
                d.ValueSlot = b;
                return default!;
            }
        }
        break;
    }
    case ж<RawBytes> d: {
        sv = reflect.ValueOf(src);
        {
            var (b, ok) = asBytes(Ꮡrows.rawbuf(), sv); if (ok) {
                d.ValueSlot = Ꮡrows.setrawbuf(b);
                return default!;
            }
        }
        break;
    }
    case ж<bool> d: {
        var (bv, err) = driver.Bool.ConvertValue(src);
        if (err == default!) {
            d.Value = bv._<bool>();
        }
        return err;
    }
    case ж<any> d: {
        d.ValueSlot = src;
        return default!;
    }}
    {
        var (scanner, ok) = dest._<Scanner>(ᐧ); if (ok) {
            return scanner.Scan(src);
        }
    }
    var dpv = reflect.ValueOf(dest);
    if (dpv.Kind() != reflect.ΔPointer) {
        return errors.New(destinationNotAPointerˢ);
    }
    if (dpv.IsNil()) {
        return errNilPtr;
    }
    if (!sv.IsValid()) {
        sv = reflect.ValueOf(src);
    }
    var dv = reflect.Indirect(dpv);
    if (sv.IsValid() && sv.Type().AssignableTo(dv.Type())) {
        switch (src.type()) {
        case slice<byte> b: {
            dv.Set(reflect.ValueOf(bytes.Clone(b)));
            break;
        }
        default: {
            var b = src;
            dv.Set(sv);
            break;
        }}
        return default!;
    }
    if (dv.Kind() == sv.Kind() && sv.Type().ConvertibleTo(dv.Type())) {
        dv.Set(sv.Convert(dv.Type()));
        return default!;
    }
    // The following conversions use a string value as an intermediate representation
    // to convert between various numeric types.
    //
    // This also allows scanning into user defined types such as "type Int int64".
    // For symmetry, also check for string destination types.
    var exprᴛ2 = dv.Kind();
    if (exprᴛ2 == reflect.ΔPointer) {
        if (src == default!) {
            dv.SetZero();
            return default!;
        }
        dv.Set(reflect.New(dv.Type().Elem()));
        return convertAssignRows(dv.Interface(), src, Ꮡrows);
    }
    if (exprᴛ2 == reflect.ΔInt || exprᴛ2 == reflect.Int8 || exprᴛ2 == reflect.Int16 || exprᴛ2 == reflect.Int32 || exprᴛ2 == reflect.Int64) {
        if (src == default!) {
            return fmt.Errorf("converting NULL to %s is unsupported"u8, dv.Kind());
        }
        @string s = asString(src);
        var (i64, err) = strconv.ParseInt(s, 10, dv.Type().Bits());
        if (err != default!) {
            err = strconvErr(err);
            return fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v"u8, src, s, dv.Kind(), err);
        }
        dv.SetInt(i64);
        return default!;
    }
    if (exprᴛ2 == reflect.ΔUint || exprᴛ2 == reflect.Uint8 || exprᴛ2 == reflect.Uint16 || exprᴛ2 == reflect.Uint32 || exprᴛ2 == reflect.Uint64) {
        if (src == default!) {
            return fmt.Errorf("converting NULL to %s is unsupported"u8, dv.Kind());
        }
        @string s = asString(src);
        var (u64, err) = strconv.ParseUint(s, 10, dv.Type().Bits());
        if (err != default!) {
            err = strconvErr(err);
            return fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v"u8, src, s, dv.Kind(), err);
        }
        dv.SetUint(u64);
        return default!;
    }
    if (exprᴛ2 == reflect.Float32 || exprᴛ2 == reflect.Float64) {
        if (src == default!) {
            return fmt.Errorf("converting NULL to %s is unsupported"u8, dv.Kind());
        }
        @string s = asString(src);
        var (f64, err) = strconv.ParseFloat(s, dv.Type().Bits());
        if (err != default!) {
            err = strconvErr(err);
            return fmt.Errorf("converting driver.Value type %T (%q) to a %s: %v"u8, src, s, dv.Kind(), err);
        }
        dv.SetFloat(f64);
        return default!;
    }
    if (exprᴛ2 == reflect.ΔString) {
        if (src == default!) {
            return fmt.Errorf("converting NULL to %s is unsupported"u8, dv.Kind());
        }
        switch (src.type()) {
        case @string v: {
            dv.SetString(v);
            return default!;
        }
        case slice<byte> v: {
            dv.SetString(((@string)v));
            return default!;
        }}
    }

    return fmt.Errorf("unsupported Scan, storing driver.Value type %T into type %T"u8, src, dest);
}

internal static error strconvErr(error err) {
    {
        var (ne, ok) = err._<ж<strconv.NumError>>(ᐧ); if (ok) {
            return (~ne).Err;
        }
    }
    return err;
}

internal static @string asString(any src) {
    switch (src.type()) {
    case @string v: {
        return v;
    }
    case slice<byte> v: {
        return ((@string)v);
    }}
    var rv = reflect.ValueOf(src);
    var exprᴛ1 = rv.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return strconv.FormatInt(rv.Int(), 10);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64) {
        return strconv.FormatUint(rv.Uint(), 10);
    }
    if (exprᴛ1 == reflect.Float64) {
        return strconv.FormatFloat(rv.Float(), (rune)'g', -1, 64);
    }
    if (exprᴛ1 == reflect.Float32) {
        return strconv.FormatFloat(rv.Float(), (rune)'g', -1, 32);
    }
    if (exprᴛ1 == reflect.ΔBool) {
        return strconv.FormatBool(rv.Bool());
    }

    return fmt.Sprintf("%v"u8, src);
}

internal static (slice<byte> b, bool ok) asBytes(slice<byte> buf, reflectꓸValue rv) {
    slice<byte> b = default!;
    bool ok = default!;

    var exprᴛ1 = rv.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return (strconv.AppendInt(buf, rv.Int(), 10), true);
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64) {
        return (strconv.AppendUint(buf, rv.Uint(), 10), true);
    }
    if (exprᴛ1 == reflect.Float32) {
        return (strconv.AppendFloat(buf, rv.Float(), (rune)'g', -1, 32), true);
    }
    if (exprᴛ1 == reflect.Float64) {
        return (strconv.AppendFloat(buf, rv.Float(), (rune)'g', -1, 64), true);
    }
    if (exprᴛ1 == reflect.ΔBool) {
        return (strconv.AppendBool(buf, rv.Bool()), true);
    }
    if (exprᴛ1 == reflect.ΔString) {
        @string s = rv.String();
        return (append(buf, s.ꓸꓸꓸ), true);
    }

    return (b, ok);
}

internal static reflectꓸType valuerReflectType = reflect.TypeFor<driver.Valuer>();

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
// This function is mirrored in the database/sql/driver package.
internal static (driverꓸValue v, error err) callValuerValue(driver.Valuer vr) {
    {
        var rv = reflect.ValueOf(vr); if (rv.Kind() == reflect.ΔPointer && rv.IsNil() && rv.Type().Elem().Implements(valuerReflectType)) {
            return (default!, default!);
        }
    }
    return vr.Value();
}

// decimal composes or decomposes a decimal value to and from individual parts.
// There are four parts: a boolean negative flag, a form byte with three possible states
// (finite=0, infinite=1, NaN=2), a base-2 big-endian integer
// coefficient (also known as a significand) as a []byte, and an int32 exponent.
// These are composed into a final value as "decimal = (neg) (form=finite) coefficient * 10 ^ exponent".
// A zero length coefficient is a zero value.
// The big-endian integer coefficient stores the most significant byte first (at coefficient[0]).
// If the form is not finite the coefficient and exponent should be ignored.
// The negative parameter may be set to true for any form, although implementations are not required
// to respect the negative parameter in the non-finite form.
//
// Implementations may choose to set the negative parameter to true on a zero or NaN value,
// but implementations that do not differentiate between negative and positive
// zero or NaN values should ignore the negative parameter without error.
// If an implementation does not support Infinity it may be converted into a NaN without error.
// If a value is set that is larger than what is supported by an implementation,
// an error must be returned.
// Implementations must return an error if a NaN or Infinity is attempted to be set while neither
// are supported.
//
// NOTE(kardianos): This is an experimental interface. See https://golang.org/issue/30870
[GoType] partial interface @decimal :
    decimalDecompose,
    decimalCompose
{
}

[GoType] partial interface decimalDecompose {
    // Decompose returns the internal decimal state in parts.
    // If the provided buf has sufficient capacity, buf may be returned as the coefficient with
    // the value set and length set as appropriate.
    (byte form, bool negative, slice<byte> coefficient, int32 exponent) Decompose(slice<byte> buf);
}

[GoType] partial interface decimalCompose {
    // Compose sets the internal decimal value from parts. If the value cannot be
    // represented then an error should be returned.
    error Compose(byte form, bool negative, slice<byte> coefficient, int32 exponent);
}

} // end sql_package
