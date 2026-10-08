// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package flag implements command-line flag parsing.

# Usage

Define flags using [flag.String], [Bool], [Int], etc.

This declares an integer flag, -n, stored in the pointer nFlag, with type *int:

	import "flag"
	var nFlag = flag.Int("n", 1234, "help message for flag n")

If you like, you can bind the flag to a variable using the Var() functions.

	var flagvar int
	func init() {
		flag.IntVar(&flagvar, "flagname", 1234, "help message for flagname")
	}

Or you can create custom flags that satisfy the Value interface (with
pointer receivers) and couple them to flag parsing by

	flag.Var(&flagVal, "name", "help message for flagname")

For such flags, the default value is just the initial value of the variable.

After all flags are defined, call

	flag.Parse()

to parse the command line into the defined flags.

Flags may then be used directly. If you're using the flags themselves,
they are all pointers; if you bind to variables, they're values.

	fmt.Println("ip has value ", *ip)
	fmt.Println("flagvar has value ", flagvar)

After parsing, the arguments following the flags are available as the
slice [flag.Args] or individually as [flag.Arg](i).
The arguments are indexed from 0 through [flag.NArg]-1.

# Command line flag syntax

The following forms are permitted:

	-flag
	--flag   // double dashes are also permitted
	-flag=x
	-flag x  // non-boolean flags only

One or two dashes may be used; they are equivalent.
The last form is not permitted for boolean flags because the
meaning of the command

	cmd -x *

where * is a Unix shell wildcard, will change if there is a file
called 0, false, etc. You must use the -flag=false form to turn
off a boolean flag.

Flag parsing stops just before the first non-flag argument
("-" is a non-flag argument) or after the terminator "--".

Integer flags accept 1234, 0664, 0x1234 and may be negative.
Boolean flags may be:

	1, 0, t, f, T, F, true, false, TRUE, FALSE, True, False

Duration flags accept any input valid for time.ParseDuration.

The default set of command-line flags is controlled by
top-level functions.  The [FlagSet] type allows one to define
independent sets of flags, such as to implement subcommands
in a command-line interface. The methods of [FlagSet] are
analogous to the top-level functions for the command-line
flag set.
*/
namespace go;

using encoding = encoding_package;
using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using reflect = reflect_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸany = Span<any>;

partial class flag_package {

// ErrHelp is the error returned if the -help or -h flag is invoked
// but no such flag is defined.
public static error ErrHelp = errors.New("flag: help requested"u8);

// errParse is returned by Set if a flag's value fails to parse, such as with an invalid integer for Int.
// It then gets wrapped through failf to provide more information.
internal static error errParse = errors.New("parse error"u8);

// errRange is returned by Set if a flag's value is out of range.
// It then gets wrapped through failf to provide more information.
internal static error errRange = errors.New("value out of range"u8);

internal static error numError(error err) {
    var (ne, ok) = err._<ж<strconv.NumError>>(ᐧ);
    if (!ok) {
        return err;
    }
    if (AreEqual((~ne).Err, strconv.ErrSyntax)) {
        return errParse;
    }
    if (AreEqual((~ne).Err, strconv.ErrRange)) {
        return errRange;
    }
    return err;
}

partial struct boolValue /*bool*/;

internal static ж<boolValue> newBoolValue(bool val, ж<bool> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<bool, boolValue>();
}

internal static error Set(this ref boolValue b, @string s) {
    var (v, err) = strconv.ParseBool(s);
    if (err != default!) {
        err = errParse;
    }
    b = ((boolValue)v);
    return err;
}

internal static any Get(this ref boolValue b) {
    return (bool)(b);
}

internal static @string String(this ref boolValue b) {
    return strconv.FormatBool((bool)(b));
}

internal static bool IsBoolFlag(this ref boolValue b) {
    return true;
}

// optional interface to indicate boolean flags that can be
// supplied without "=value" text
partial interface boolFlag :
    Value
{
    bool IsBoolFlag();
}

partial struct intValue /*num:nint*/;

internal static ж<intValue> newIntValue(nint val, ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<nint, intValue>();
}

internal static error Set(this ref intValue i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, strconv.IntSize);
    if (err != default!) {
        err = numError(err);
    }
    i = ((intValue)(nint)v);
    return err;
}

internal static any Get(this ref intValue i) {
    return (nint)(i);
}

internal static @string String(this ref intValue i) {
    return strconv.Itoa((nint)(i));
}

partial struct int64Value /*num:int64*/;

internal static ж<int64Value> newInt64Value(int64 val, ж<int64> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<int64, int64Value>();
}

internal static error Set(this ref int64Value i, @string s) {
    var (v, err) = strconv.ParseInt(s, 0, 64);
    if (err != default!) {
        err = numError(err);
    }
    i = ((int64Value)v);
    return err;
}

internal static any Get(this ref int64Value i) {
    return (int64)(i);
}

internal static @string String(this ref int64Value i) {
    return strconv.FormatInt((int64)(i), 10);
}

partial struct uintValue /*num:nuint*/;

internal static ж<uintValue> newUintValue(nuint val, ж<nuint> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<nuint, uintValue>();
}

internal static error Set(this ref uintValue i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, strconv.IntSize);
    if (err != default!) {
        err = numError(err);
    }
    i = ((uintValue)(nuint)v);
    return err;
}

internal static any Get(this ref uintValue i) {
    return (nuint)(i);
}

internal static @string String(this ref uintValue i) {
    return strconv.FormatUint((uint64)(nuint)(i), 10);
}

partial struct uint64Value /*num:uint64*/;

internal static ж<uint64Value> newUint64Value(uint64 val, ж<uint64> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<uint64, uint64Value>();
}

internal static error Set(this ref uint64Value i, @string s) {
    var (v, err) = strconv.ParseUint(s, 0, 64);
    if (err != default!) {
        err = numError(err);
    }
    i = ((uint64Value)v);
    return err;
}

internal static any Get(this ref uint64Value i) {
    return (uint64)(i);
}

internal static @string String(this ref uint64Value i) {
    return strconv.FormatUint((uint64)(i), 10);
}

partial struct stringValue /*@string*/;

internal static ж<stringValue> newStringValue(@string val, ж<@string> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<@string, stringValue>();
}

internal static error Set(this ref stringValue s, @string val) {
    s = ((stringValue)val);
    return default!;
}

internal static any Get(this ref stringValue s) {
    return ((@string)(s));
}

internal static @string String(this ref stringValue s) {
    return ((@string)(s));
}

partial struct float64Value /*num:float64*/;

internal static ж<float64Value> newFloat64Value(float64 val, ж<float64> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<float64, float64Value>();
}

internal static error Set(this ref float64Value f, @string s) {
    var (v, err) = strconv.ParseFloat(s, 64);
    if (err != default!) {
        err = numError(err);
    }
    f = ((float64Value)v);
    return err;
}

internal static any Get(this ref float64Value f) {
    return (float64)(f);
}

internal static @string String(this ref float64Value f) {
    return strconv.FormatFloat((float64)(f), (rune)'g', -1, 64);
}

partial struct durationValue /*global::go.time_package.Duration*/;

internal static ж<durationValue> newDurationValue(time.Duration val, ж<time.Duration> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<time.Duration, durationValue>();
}

internal static error Set(this ref durationValue d, @string s) {
    var (v, err) = time.ParseDuration(s);
    if (err != default!) {
        err = errParse;
    }
    d = ((durationValue)v);
    return err;
}

internal static any Get(this ref durationValue d) {
    return ((time.Duration)d);
}

internal static @string String(this ж<durationValue> Ꮡd) {
    return (Ꮡd.Reinterpret<durationValue, time.Duration>()).Value.String();
}

// -- encoding.TextUnmarshaler Value
partial struct textValue {
    internal encoding.TextUnmarshaler p;
}

internal static textValue newTextValue(encoding.TextMarshaler val, encoding.TextUnmarshaler p) {
    var ptrVal = reflect.ValueOf(p);
    if (ptrVal.Kind() != reflect.Ptr) {
        throw panic("variable value type must be a pointer");
    }
    var defVal = reflect.ValueOf(val);
    if (defVal.Kind() == reflect.Ptr) {
        defVal = defVal.Elem();
    }
    if (!AreEqual(defVal.Type(), ptrVal.Type().Elem())) {
        throw panic(fmt.Sprintf("default type does not match variable type: %v != %v"u8, defVal.Type(), ptrVal.Type().Elem()));
    }
    ptrVal.Elem().Set(defVal);
    return new textValue(p);
}

internal static error Set(this textValue v, @string s) {
    return v.p.UnmarshalText(slice<byte>(s));
}

internal static any Get(this textValue v) {
    return v.p;
}

internal static @string String(this textValue v) {
    {
        var (m, ok) = v.p._<encoding.TextMarshaler>(ᐧ); if (ok) {
            {
                var (b, err) = m.MarshalText(); if (err == default!) {
                    return ((@string)b);
                }
            }
        }
    }
    return ""u8;
}

internal delegate error funcValue(@string _);

internal static partial error Set(this funcValue f, @string s) {
    return f(s);
}

internal static @string String(this funcValue f) {
    return ""u8;
}

internal delegate error boolFuncValue(@string _);

internal static partial error Set(this boolFuncValue f, @string s) {
    return f(s);
}

internal static @string String(this boolFuncValue f) {
    return ""u8;
}

internal static bool IsBoolFlag(this boolFuncValue f) {
    return true;
}

// Value is the interface to the dynamic value stored in a flag.
// (The default value is represented as a string.)
//
// If a Value has an IsBoolFlag() bool method returning true,
// the command-line parser makes -name equivalent to -name=true
// rather than using the next command-line argument.
//
// Set is called once, in command line order, for each flag present.
// The flag package may call the [String] method with a zero-valued receiver,
// such as a nil pointer.
partial interface Value :
    fmt.Stringer
{
    error Set(@string _);
}

// Getter is an interface that allows the contents of a [Value] to be retrieved.
// It wraps the [Value] interface, rather than being part of it, because it
// appeared after Go 1 and its compatibility rules. All [Value] types provided
// by this package satisfy the [Getter] interface, except the type used by [Func].
partial interface Getter :
    Value
{
    any Get();
}

partial struct ΔErrorHandling /*num:nint*/;

// These constants cause [FlagSet.Parse] to behave as described if the parse fails.
public static ΔErrorHandling ContinueOnError => /* iota */ 0; // Return a descriptive error.

public static ΔErrorHandling ExitOnError => 1;    // Call os.Exit(2) or for -h/-help Exit(0).

public static ΔErrorHandling PanicOnError => 2;   // Call panic with a descriptive error.

// A FlagSet represents a set of defined flags. The zero value of a FlagSet
// has no name and has [ContinueOnError] error handling.
//
// [Flag] names must be unique within a FlagSet. An attempt to define a flag whose
// name is already in use will cause a panic.
partial struct FlagSet {
    // Usage is the function called when an error occurs while parsing flags.
    // The field is a function (not a method) that may be changed to point to
    // a custom error handler. What happens after Usage is called depends
    // on the ErrorHandling setting; for the command line, this defaults
    // to ExitOnError, which exits the program after calling Usage.
    public Action Usage;
    internal @string name;
    internal bool parsed;
    internal map<@string, ж<Flag>> actual;
    internal map<@string, ж<Flag>> formal;
    internal slice<@string> args; // arguments after flags
    internal ΔErrorHandling errorHandling;
    internal Δio.Writer output;         // nil means stderr; use Output() accessor
    internal map<@string, @string> undef; // flags which didn't exist at the time of Set
}

// A Flag represents the state of a flag.
partial struct Flag {
    public @string Name; // name as it appears on command line
    public @string Usage; // help message
    public Value Value;  // value as set
    public @string DefValue; // default value (as text); for usage message
}

// sortFlags returns the flags as a slice in lexicographical sorted order.
internal static slice<ж<Flag>> sortFlags(map<@string, ж<Flag>> flags) {
    var result = new slice<ж<Flag>>(len(flags));
    nint i = 0;
    foreach (var (_, f) in flags) {
        result[i] = f;
        i++;
    }
    slices.SortFunc(result, (ж<Flag> a, ж<Flag> b) => strings.Compare((~a).Name, (~b).Name));
    return result;
}

// Output returns the destination for usage and error messages. [os.Stderr] is returned if
// output was not set or was set to nil.
public static Δio.Writer Output(this ref FlagSet f) {
    if (f.output == default!) {
        return new os.FileжWriter(os.Stderr);
    }
    return f.output;
}

// Name returns the name of the flag set.
public static @string Name(this ref FlagSet f) {
    return f.name;
}

// ErrorHandling returns the error handling behavior of the flag set.
public static ΔErrorHandling ErrorHandling(this ref FlagSet f) {
    return f.errorHandling;
}

// SetOutput sets the destination for usage and error messages.
// If output is nil, [os.Stderr] is used.
public static void SetOutput(this ref FlagSet f, Δio.Writer output) {
    f.output = output;
}

// VisitAll visits the flags in lexicographical order, calling fn for each.
// It visits all flags, even those not set.
public static void VisitAll(this ref FlagSet f, Action<ж<Flag>> fn) {
    foreach (var (_, flag) in sortFlags(f.formal)) {
        fn(flag);
    }
}

// VisitAll visits the command-line flags in lexicographical order, calling
// fn for each. It visits all flags, even those not set.
public static void VisitAll(Action<ж<Flag>> fn) {
    CommandLine.VisitAll(fn);
}

// Visit visits the flags in lexicographical order, calling fn for each.
// It visits only those flags that have been set.
public static void Visit(this ref FlagSet f, Action<ж<Flag>> fn) {
    foreach (var (_, flag) in sortFlags(f.actual)) {
        fn(flag);
    }
}

// Visit visits the command-line flags in lexicographical order, calling fn
// for each. It visits only those flags that have been set.
public static void Visit(Action<ж<Flag>> fn) {
    CommandLine.Visit(fn);
}

// Lookup returns the [Flag] structure of the named flag, returning nil if none exists.
public static ж<Flag> Lookup(this ref FlagSet f, @string name) {
    return f.formal[name];
}

// Lookup returns the [Flag] structure of the named command-line flag,
// returning nil if none exists.
public static ж<Flag> Lookup(@string name) {
    return (~CommandLine).formal[name];
}

// Set sets the value of the named flag.
public static partial error Set(this ref FlagSet f, @string name, @string value) {
    return f.set(name, value);
}

internal static partial error set(this ref FlagSet f, @string name, @string value) {
    var (flag, ok) = f.formal[name, ꟷ];
    if (!ok) {
        // Remember that a flag that isn't defined is being set.
        // We return an error in this case, but in addition if
        // subsequently that flag is defined, we want to panic
        // at the definition point.
        // This is a problem which occurs if both the definition
        // and the Set call are in init code and for whatever
        // reason the init code changes evaluation order.
        // See issue 57411.
        var (_, @file, line, okΔ1) = runtime.Caller(2);
        if (!okΔ1) {
            @file = "?"u8;
            line = 0;
        }
        if (f.undef == default!) {
            f.undef = new map<@string, @string>{};
        }
        f.undef[name] = fmt.Sprintf("%s:%d"u8, @file, line);
        return fmt.Errorf("no such flag -%v"u8, name);
    }
    var err = (~flag).Value.Set(value);
    if (err != default!) {
        return err;
    }
    if (f.actual == default!) {
        f.actual = new map<@string, ж<Flag>>();
    }
    f.actual[name] = flag;
    return default!;
}

// Set sets the value of the named command-line flag.
public static partial error Set(@string name, @string value) {
    return CommandLine.set(name, value);
}

// isZeroValue determines whether the string represents the zero
// value for a flag.
internal static (bool ok, error err) isZeroValue(ж<Flag> Ꮡflag, @string value) {
    bool ok = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var flag = ref Ꮡflag.DerefOrNull();

        // Build a zero value of the flag's Value type, and see if the
        // result of calling its String method equals the value passed in.
        // This works unless the Value type is itself an interface type.
        ref var typ = ref heap<reflectꓸType>(out var Ꮡtyp);
        typ = reflect.TypeOf(flag.Value);
        reflectꓸValue z = new(nil);
        if (typ.Kind() == reflect.ΔPointer){
            z = reflect.New(typ.Elem());
        } else {
            z = reflect.Zero(typ);
        }
        // Catch panics calling the String method, which shouldn't prevent the
        // usage message from being printed, but that we should report to the
        // user so that they know to fix their code.
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    if (Ꮡtyp.ValueSlot.Kind() == reflect.ΔPointer) {
                        Ꮡtyp.ValueSlot = Ꮡtyp.ValueSlot.Elem();
                    }
                    err = fmt.Errorf("panic calling String method on zero %v for flag %s: %v"u8, Ꮡtyp.ValueSlot, Ꮡflag.Value.Name, e);
                }
            }
        }, ref ᒐ);
        (ok, err) = (value == z.Interface()._<Value>().String(), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (ok, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string valueˢ = "value"u8;
internal static readonly @string durationˢ = "duration"u8;
internal static readonly @string floatˢ = "float"u8;
internal static readonly @string intˢ = "int"u8;
internal static readonly @string stringˢ = "string"u8;
internal static readonly @string uintˢ = "uint"u8;

// UnquoteUsage extracts a back-quoted name from the usage
// string for a flag and returns it and the un-quoted usage.
// Given "a `name` to show" it returns ("name", "a name to show").
// If there are no back quotes, the name is an educated guess of the
// type of the flag's value, or the empty string if the flag is boolean.
public static (@string name, @string usage) UnquoteUsage(ж<Flag> Ꮡflag) {
    @string name = default!;
    @string usage = default!;

    ref var flag = ref Ꮡflag.DerefOrNull();
    // Look for a back-quoted name, but avoid the strings package.
    usage = flag.Usage;
    for (nint i = 0; i < len(usage); i++) {
        if (usage[i] == (rune)'`') {
            for (nint j = i + 1; j < len(usage); j++) {
                if (usage[j] == (rune)'`') {
                    name = usage.slice(i + 1, j);
                    usage = usage.slice(0, i) + name + usage.slice(j + 1);
                    return (name, usage);
                }
            }
            break; // Only one back quote; use type name.
        }
    }
    // No explicit name, so use type if we can find one.
    name = valueˢ;
    switch (flag.Value.type()) {
    case {} Δfv when Δfv._<boolFlag>(out var fv): {
        if (fv.IsBoolFlag()) {
            name = ""u8;
        }
        break;
    }
    case ж<durationValue> fv: {
        name = durationˢ;
        break;
    }
    case ж<float64Value> fv: {
        name = floatˢ;
        break;
    }
    case ж<intValue> _:
    case ж<int64Value> _: {
        var fv = flag.Value;
        name = intˢ;
        break;
    }
    case ж<stringValue> fv: {
        name = stringˢ;
        break;
    }
    case ж<uintValue> _:
    case ж<uint64Value> _: {
        var fv = flag.Value;
        name = uintˢ;
        break;
    }}
    return (name, usage);
}

// PrintDefaults prints, to standard error unless configured otherwise, the
// default values of all defined command-line flags in the set. See the
// documentation for the global function PrintDefaults for more information.
public static void PrintDefaults(this ж<FlagSet> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    ref var isZeroValueErrs = ref heap<slice<error>>(out var ᏑisZeroValueErrs);
    f.VisitAll((ж<Flag> flag) => {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "  -%s"u8, (~flag).Name); // Two spaces before -; see next two comments.
        var (name, usage) = UnquoteUsage(flag);
        if (len(name) > 0) {
            Ꮡb.WriteString(" "u8);
            Ꮡb.WriteString(name);
        }
        // Boolean flags of one ASCII letter are so common we
        // treat them specially, putting their usage on the same line.
        if (b.Len() <= 4){
            // space, space, '-', 'x'.
            Ꮡb.WriteString("\t"u8);
        } else {
            // Four spaces before the tab triggers good alignment
            // for both 4- and 8-space tab stops.
            Ꮡb.WriteString("\n    \t"u8);
        }
        Ꮡb.WriteString(strings.ReplaceAll(usage, "\n"u8, "\n    \t"u8));
        // Print the default value only if it differs to the zero value
        // for this flag type.
        {
            var (isZero, err) = isZeroValue(flag, (~flag).DefValue); if (err != default!){
                ᏑisZeroValueErrs.ValueSlot = append(ᏑisZeroValueErrs.ValueSlot, err);
            } else 
            if (!isZero) {
                {
                    var (_, ok) = (~flag).Value._<ж<stringValue>>(ᐧ); if (ok){
                        // put quotes on the value
                        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), " (default %q)"u8, (~flag).DefValue);
                    } else {
                        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), " (default %v)"u8, (~flag).DefValue);
                    }
                }
            }
        }
        fmt.Fprint(Ꮡf.Value.Output(), b.String(), (@string)"\n"u8);
    });
    // If calling String on any zero flag.Values triggered a panic, print
    // the messages after the full set of defaults so that the programmer
    // knows to fix the panic.
    {
        var errs = isZeroValueErrs; if (len(errs) > 0) {
            fmt.Fprintln(f.Output());
            foreach (var (_, err) in errs) {
                fmt.Fprintln(f.Output(), err);
            }
        }
    }
}

// PrintDefaults prints, to standard error unless configured otherwise,
// a usage message showing the default settings of all defined
// command-line flags.
// For an integer valued flag x, the default output has the form
//
//	-x int
//		usage-message-for-x (default 7)
//
// The usage message will appear on a separate line for anything but
// a bool flag with a one-byte name. For bool flags, the type is
// omitted and if the flag name is one byte the usage message appears
// on the same line. The parenthetical default is omitted if the
// default is the zero value for the type. The listed type, here int,
// can be changed by placing a back-quoted name in the flag's usage
// string; the first such item in the message is taken to be a parameter
// name to show in the message and the back quotes are stripped from
// the message when displayed. For instance, given
//
//	flag.String("I", "", "search `directory` for include files")
//
// the output will be
//
//	-I directory
//		search directory for include files.
//
// To change the destination for flag messages, call [CommandLine].SetOutput.
public static void PrintDefaults() {
    CommandLine.PrintDefaults();
}

// defaultUsage is the default function to print a usage message.
internal static void defaultUsage(this ж<FlagSet> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.name == ""u8){
        fmt.Fprintf(f.Output(), "Usage:\n"u8);
    } else {
        fmt.Fprintf(f.Output(), "Usage of %s:\n"u8, f.name);
    }
    Ꮡf.PrintDefaults();
}

// NOTE: Usage is not just defaultUsage(CommandLine)
// because it serves (via godoc flag Usage) as the example
// for how to write your own usage function.

// Usage prints a usage message documenting all defined command-line flags
// to [CommandLine]'s output, which by default is [os.Stderr].
// It is called when an error occurs while parsing flags.
// The function is a variable that may be changed to point to a custom function.
// By default it prints a simple header and calls [PrintDefaults]; for details about the
// format of the output and how to control it, see the documentation for [PrintDefaults].
// Custom usage functions may choose to exit the program; by default exiting
// happens anyway as the command line's error handling strategy is set to
// [ExitOnError].
public static Action Usage;
internal static void initᴛUsage() { Usage = () => {
    fmt.Fprintf(CommandLine.Output(), "Usage of %s:\n"u8, os.Args[0]);
    PrintDefaults();
}; }

// NFlag returns the number of flags that have been set.
public static nint NFlag(this ref FlagSet f) {
    return len(f.actual);
}

// NFlag returns the number of command-line flags that have been set.
public static nint NFlag() {
    return len((~CommandLine).actual);
}

// Arg returns the i'th argument. Arg(0) is the first remaining argument
// after flags have been processed. Arg returns an empty string if the
// requested element does not exist.
public static @string Arg(this ref FlagSet f, nint i) {
    if (i < 0 || i >= len(f.args)) {
        return ""u8;
    }
    return f.args[i];
}

// Arg returns the i'th command-line argument. Arg(0) is the first remaining argument
// after flags have been processed. Arg returns an empty string if the
// requested element does not exist.
public static @string Arg(nint i) {
    return CommandLine.Arg(i);
}

// NArg is the number of arguments remaining after flags have been processed.
public static nint NArg(this ref FlagSet f) {
    return len(f.args);
}

// NArg is the number of arguments remaining after flags have been processed.
public static nint NArg() {
    return len((~CommandLine).args);
}

// Args returns the non-flag arguments.
public static slice<@string> Args(this ref FlagSet f) {
    return f.args;
}

// Args returns the non-flag command-line arguments.
public static slice<@string> Args() {
    return (~CommandLine).args;
}

// BoolVar defines a bool flag with specified name, default value, and usage string.
// The argument p points to a bool variable in which to store the value of the flag.
public static void BoolVar(this ref FlagSet f, ж<bool> Ꮡp, @string name, bool value, @string usage) {
    f.Var(new boolValueжValue(newBoolValue(value, Ꮡp)), name, usage);
}

// BoolVar defines a bool flag with specified name, default value, and usage string.
// The argument p points to a bool variable in which to store the value of the flag.
public static void BoolVar(ж<bool> Ꮡp, @string name, bool value, @string usage) {
    CommandLine.Var(new boolValueжValue(newBoolValue(value, Ꮡp)), name, usage);
}

// Bool defines a bool flag with specified name, default value, and usage string.
// The return value is the address of a bool variable that stores the value of the flag.
public static ж<bool> Bool(this ref FlagSet f, @string name, bool value, @string usage) {
    var p = @new<bool>();
    f.BoolVar(p, name, value, usage);
    return p;
}

// Bool defines a bool flag with specified name, default value, and usage string.
// The return value is the address of a bool variable that stores the value of the flag.
public static ж<bool> Bool(@string name, bool value, @string usage) {
    return CommandLine.Bool(name, value, usage);
}

// IntVar defines an int flag with specified name, default value, and usage string.
// The argument p points to an int variable in which to store the value of the flag.
public static void IntVar(this ref FlagSet f, ж<nint> Ꮡp, @string name, nint value, @string usage) {
    f.Var(new intValueжValue(newIntValue(value, Ꮡp)), name, usage);
}

// IntVar defines an int flag with specified name, default value, and usage string.
// The argument p points to an int variable in which to store the value of the flag.
public static void IntVar(ж<nint> Ꮡp, @string name, nint value, @string usage) {
    CommandLine.Var(new intValueжValue(newIntValue(value, Ꮡp)), name, usage);
}

// Int defines an int flag with specified name, default value, and usage string.
// The return value is the address of an int variable that stores the value of the flag.
public static ж<nint> Int(this ref FlagSet f, @string name, nint value, @string usage) {
    var p = @new<nint>();
    f.IntVar(p, name, value, usage);
    return p;
}

// Int defines an int flag with specified name, default value, and usage string.
// The return value is the address of an int variable that stores the value of the flag.
public static ж<nint> Int(@string name, nint value, @string usage) {
    return CommandLine.Int(name, value, usage);
}

// Int64Var defines an int64 flag with specified name, default value, and usage string.
// The argument p points to an int64 variable in which to store the value of the flag.
public static void Int64Var(this ref FlagSet f, ж<int64> Ꮡp, @string name, int64 value, @string usage) {
    f.Var(new int64ValueжValue(newInt64Value(value, Ꮡp)), name, usage);
}

// Int64Var defines an int64 flag with specified name, default value, and usage string.
// The argument p points to an int64 variable in which to store the value of the flag.
public static void Int64Var(ж<int64> Ꮡp, @string name, int64 value, @string usage) {
    CommandLine.Var(new int64ValueжValue(newInt64Value(value, Ꮡp)), name, usage);
}

// Int64 defines an int64 flag with specified name, default value, and usage string.
// The return value is the address of an int64 variable that stores the value of the flag.
public static ж<int64> Int64(this ref FlagSet f, @string name, int64 value, @string usage) {
    var p = @new<int64>();
    f.Int64Var(p, name, value, usage);
    return p;
}

// Int64 defines an int64 flag with specified name, default value, and usage string.
// The return value is the address of an int64 variable that stores the value of the flag.
public static ж<int64> Int64(@string name, int64 value, @string usage) {
    return CommandLine.Int64(name, value, usage);
}

// UintVar defines a uint flag with specified name, default value, and usage string.
// The argument p points to a uint variable in which to store the value of the flag.
public static void UintVar(this ref FlagSet f, ж<nuint> Ꮡp, @string name, nuint value, @string usage) {
    f.Var(new uintValueжValue(newUintValue(value, Ꮡp)), name, usage);
}

// UintVar defines a uint flag with specified name, default value, and usage string.
// The argument p points to a uint variable in which to store the value of the flag.
public static void UintVar(ж<nuint> Ꮡp, @string name, nuint value, @string usage) {
    CommandLine.Var(new uintValueжValue(newUintValue(value, Ꮡp)), name, usage);
}

// Uint defines a uint flag with specified name, default value, and usage string.
// The return value is the address of a uint variable that stores the value of the flag.
public static ж<nuint> Uint(this ref FlagSet f, @string name, nuint value, @string usage) {
    var p = @new<nuint>();
    f.UintVar(p, name, value, usage);
    return p;
}

// Uint defines a uint flag with specified name, default value, and usage string.
// The return value is the address of a uint variable that stores the value of the flag.
public static ж<nuint> Uint(@string name, nuint value, @string usage) {
    return CommandLine.Uint(name, value, usage);
}

// Uint64Var defines a uint64 flag with specified name, default value, and usage string.
// The argument p points to a uint64 variable in which to store the value of the flag.
public static void Uint64Var(this ref FlagSet f, ж<uint64> Ꮡp, @string name, uint64 value, @string usage) {
    f.Var(new uint64ValueжValue(newUint64Value(value, Ꮡp)), name, usage);
}

// Uint64Var defines a uint64 flag with specified name, default value, and usage string.
// The argument p points to a uint64 variable in which to store the value of the flag.
public static void Uint64Var(ж<uint64> Ꮡp, @string name, uint64 value, @string usage) {
    CommandLine.Var(new uint64ValueжValue(newUint64Value(value, Ꮡp)), name, usage);
}

// Uint64 defines a uint64 flag with specified name, default value, and usage string.
// The return value is the address of a uint64 variable that stores the value of the flag.
public static ж<uint64> Uint64(this ref FlagSet f, @string name, uint64 value, @string usage) {
    var p = @new<uint64>();
    f.Uint64Var(p, name, value, usage);
    return p;
}

// Uint64 defines a uint64 flag with specified name, default value, and usage string.
// The return value is the address of a uint64 variable that stores the value of the flag.
public static ж<uint64> Uint64(@string name, uint64 value, @string usage) {
    return CommandLine.Uint64(name, value, usage);
}

// StringVar defines a string flag with specified name, default value, and usage string.
// The argument p points to a string variable in which to store the value of the flag.
public static void StringVar(this ref FlagSet f, ж<@string> Ꮡp, @string name, @string value, @string usage) {
    f.Var(new stringValueжValue(newStringValue(value, Ꮡp)), name, usage);
}

// StringVar defines a string flag with specified name, default value, and usage string.
// The argument p points to a string variable in which to store the value of the flag.
public static void StringVar(ж<@string> Ꮡp, @string name, @string value, @string usage) {
    CommandLine.Var(new stringValueжValue(newStringValue(value, Ꮡp)), name, usage);
}

// String defines a string flag with specified name, default value, and usage string.
// The return value is the address of a string variable that stores the value of the flag.
public static ж<@string> String(this ref FlagSet f, @string name, @string value, @string usage) {
    var p = @new<@string>();
    f.StringVar(p, name, value, usage);
    return p;
}

// String defines a string flag with specified name, default value, and usage string.
// The return value is the address of a string variable that stores the value of the flag.
public static ж<@string> String(@string name, @string value, @string usage) {
    return CommandLine.String(name, value, usage);
}

// Float64Var defines a float64 flag with specified name, default value, and usage string.
// The argument p points to a float64 variable in which to store the value of the flag.
public static void Float64Var(this ref FlagSet f, ж<float64> Ꮡp, @string name, float64 value, @string usage) {
    f.Var(new float64ValueжValue(newFloat64Value(value, Ꮡp)), name, usage);
}

// Float64Var defines a float64 flag with specified name, default value, and usage string.
// The argument p points to a float64 variable in which to store the value of the flag.
public static void Float64Var(ж<float64> Ꮡp, @string name, float64 value, @string usage) {
    CommandLine.Var(new float64ValueжValue(newFloat64Value(value, Ꮡp)), name, usage);
}

// Float64 defines a float64 flag with specified name, default value, and usage string.
// The return value is the address of a float64 variable that stores the value of the flag.
public static ж<float64> Float64(this ref FlagSet f, @string name, float64 value, @string usage) {
    var p = @new<float64>();
    f.Float64Var(p, name, value, usage);
    return p;
}

// Float64 defines a float64 flag with specified name, default value, and usage string.
// The return value is the address of a float64 variable that stores the value of the flag.
public static ж<float64> Float64(@string name, float64 value, @string usage) {
    return CommandLine.Float64(name, value, usage);
}

// DurationVar defines a time.Duration flag with specified name, default value, and usage string.
// The argument p points to a time.Duration variable in which to store the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static void DurationVar(this ref FlagSet f, ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) {
    f.Var(new durationValueжValue(newDurationValue(value, Ꮡp)), name, usage);
}

// DurationVar defines a time.Duration flag with specified name, default value, and usage string.
// The argument p points to a time.Duration variable in which to store the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static void DurationVar(ж<time.Duration> Ꮡp, @string name, time.Duration value, @string usage) {
    CommandLine.Var(new durationValueжValue(newDurationValue(value, Ꮡp)), name, usage);
}

// Duration defines a time.Duration flag with specified name, default value, and usage string.
// The return value is the address of a time.Duration variable that stores the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static ж<time.Duration> Duration(this ref FlagSet f, @string name, time.Duration value, @string usage) {
    var p = @new<time.Duration>();
    f.DurationVar(p, name, value, usage);
    return p;
}

// Duration defines a time.Duration flag with specified name, default value, and usage string.
// The return value is the address of a time.Duration variable that stores the value of the flag.
// The flag accepts a value acceptable to time.ParseDuration.
public static ж<time.Duration> Duration(@string name, time.Duration value, @string usage) {
    return CommandLine.Duration(name, value, usage);
}

// TextVar defines a flag with a specified name, default value, and usage string.
// The argument p must be a pointer to a variable that will hold the value
// of the flag, and p must implement encoding.TextUnmarshaler.
// If the flag is used, the flag value will be passed to p's UnmarshalText method.
// The type of the default value must be the same as the type of p.
public static void TextVar(this ref FlagSet f, encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) {
    f.Var(newTextValue(value, p), name, usage);
}

// TextVar defines a flag with a specified name, default value, and usage string.
// The argument p must be a pointer to a variable that will hold the value
// of the flag, and p must implement encoding.TextUnmarshaler.
// If the flag is used, the flag value will be passed to p's UnmarshalText method.
// The type of the default value must be the same as the type of p.
public static void TextVar(encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) {
    CommandLine.Var(newTextValue(value, p), name, usage);
}

// Func defines a flag with the specified name and usage string.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static void Func(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) {
    f.Var(new funcValueᴠValue(NilSafeDelegateConversion<funcValue, Func<@string, error>>(fn)), name, usage);
}

// Func defines a flag with the specified name and usage string.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static void Func(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.Func(name, usage, fn);
}

// BoolFunc defines a flag with the specified name and usage string without requiring values.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static void BoolFunc(this ref FlagSet f, @string name, @string usage, Func<@string, error> fn) {
    f.Var(new boolFuncValueᴠValue(NilSafeDelegateConversion<boolFuncValue, Func<@string, error>>(fn)), name, usage);
}

// BoolFunc defines a flag with the specified name and usage string without requiring values.
// Each time the flag is seen, fn is called with the value of the flag.
// If fn returns a non-nil error, it will be treated as a flag value parsing error.
public static void BoolFunc(@string name, @string usage, Func<@string, error> fn) {
    CommandLine.BoolFunc(name, usage, fn);
}

// Var defines a flag with the specified name and usage string. The type and
// value of the flag are represented by the first argument, of type [Value], which
// typically holds a user-defined implementation of [Value]. For instance, the
// caller could create a flag that turns a comma-separated string into a slice
// of strings by giving the slice the methods of [Value]; in particular, [Set] would
// decompose the comma-separated string into the slice.
public static void Var(this ref FlagSet f, Value value, @string name, @string usage) {
    // Flag must not begin "-" or contain "=".
    if (strings.HasPrefix(name, "-"u8)){
        throw panic(f.sprintf("flag %q begins with -"u8, name));
    } else 
    if (strings.Contains(name, "="u8)) {
        throw panic(f.sprintf("flag %q contains ="u8, name));
    }
    // Remember the default value as a string; it won't change.
    var flag = Ꮡ(new Flag(name, usage, value, value.String()));
    var (_, alreadythere) = f.formal[name, ꟷ];
    if (alreadythere) {
        @string msg = default!;
        if (f.name == ""u8){
            msg = f.sprintf("flag redefined: %s"u8, name);
        } else {
            msg = f.sprintf("%s flag redefined: %s"u8, f.name, name);
        }
        throw panic(msg); // Happens only if flags are declared with identical names
    }
    {
        @string pos = f.undef[name]; if (pos != ""u8) {
            throw panic(fmt.Sprintf("flag %s set at %s before being defined"u8, name, pos));
        }
    }
    if (f.formal == default!) {
        f.formal = new map<@string, ж<Flag>>();
    }
    f.formal[name] = flag;
}

// Var defines a flag with the specified name and usage string. The type and
// value of the flag are represented by the first argument, of type [Value], which
// typically holds a user-defined implementation of [Value]. For instance, the
// caller could create a flag that turns a comma-separated string into a slice
// of strings by giving the slice the methods of [Value]; in particular, [Set] would
// decompose the comma-separated string into the slice.
public static void Var(Value value, @string name, @string usage) {
    CommandLine.Var(value, name, usage);
}

// sprintf formats the message, prints it to output, and returns it.
internal static @string sprintf(this ref FlagSet f, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    @string msg = fmt.Sprintf(format, a.ꓸꓸꓸ);
    fmt.Fprintln(f.Output(), msg);
    return msg;
}

// failf prints to standard error a formatted error and usage message and
// returns the error.
internal static error failf(this ж<FlagSet> Ꮡf, @string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    ref var f = ref Ꮡf.DerefOrNull();
    @string msg = f.sprintf(format, a.ꓸꓸꓸ);
    Ꮡf.usage();
    return errors.New(msg);
}

// usage calls the Usage method for the flag set if one is specified,
// or the appropriate default usage function otherwise.
internal static void usage(this ж<FlagSet> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.Usage == default!){
        Ꮡf.defaultUsage();
    } else {
        f.Usage();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string trueˢ = "true"u8;

// parseOne parses one flag. It reports whether a flag was seen.
internal static (bool, error) parseOne(this ж<FlagSet> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (len(f.args) == 0) {
        return (false, default!);
    }
    @string s = f.args[0];
    if (len(s) < 2 || s[0] != (rune)'-') {
        return (false, default!);
    }
    nint numMinuses = 1;
    if (s[1] == (rune)'-') {
        numMinuses++;
        if (len(s) == 2) {
            // "--" terminates the flags
            f.args = f.args[1..];
            return (false, default!);
        }
    }
    @string name = s.slice(numMinuses);
    if (len(name) == 0 || name[0] == (rune)'-' || name[0] == (rune)'=') {
        return (false, Ꮡf.failf("bad flag syntax: %s"u8, s));
    }
    // it's a flag. does it have an argument?
    f.args = f.args[1..];
    var hasValue = false;
    @string value = ""u8;
    for (nint i = 1; i < len(name); i++) {
        // equals cannot be first
        if (name[i] == (rune)'=') {
            value = name.slice(i + 1);
            hasValue = true;
            name = name.slice(0, i);
            break;
        }
    }
    var (flag, ok) = f.formal[name, ꟷ];
    if (!ok) {
        if (name == "help"u8 || name == "h"u8) {
            // special case for nice help message.
            Ꮡf.usage();
            return (false, ErrHelp);
        }
        return (false, Ꮡf.failf("flag provided but not defined: -%s"u8, name));
    }
    {
        var (fv, okΔ1) = (~flag).Value._<boolFlag>(ᐧ); if (okΔ1 && fv.IsBoolFlag()){
            // special case: doesn't need an arg
            if (hasValue){
                {
                    var err = fv.Set(value); if (err != default!) {
                        return (false, Ꮡf.failf("invalid boolean value %q for -%s: %v"u8, value, name, err));
                    }
                }
            } else {
                {
                    var err = fv.Set(trueˢ); if (err != default!) {
                        return (false, Ꮡf.failf("invalid boolean flag %s: %v"u8, name, err));
                    }
                }
            }
        } else {
            // It must have a value, which might be the next argument.
            if (!hasValue && len(f.args) > 0) {
                // value is the next arg
                hasValue = true;
                (value, f.args) = (f.args[0], f.args[1..]);
            }
            if (!hasValue) {
                return (false, Ꮡf.failf("flag needs an argument: -%s"u8, name));
            }
            {
                var err = (~flag).Value.Set(value); if (err != default!) {
                    return (false, Ꮡf.failf("invalid value %q for flag -%s: %v"u8, value, name, err));
                }
            }
        }
    }
    if (f.actual == default!) {
        f.actual = new map<@string, ж<Flag>>();
    }
    f.actual[name] = flag;
    return (true, default!);
}

// Parse parses flag definitions from the argument list, which should not
// include the command name. Must be called after all flags in the [FlagSet]
// are defined and before flags are accessed by the program.
// The return value will be [ErrHelp] if -help or -h were set but not defined.
public static error Parse(this ж<FlagSet> Ꮡf, slice<@string> arguments) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.parsed = true;
    f.args = arguments;
    while (ᐧ) {
        var (seen, err) = Ꮡf.parseOne();
        if (seen) {
            continue;
        }
        if (err == default!) {
            break;
        }
        var exprᴛ1 = f.errorHandling;
        if (exprᴛ1 == ContinueOnError) {
            return err;
        }
        if (exprᴛ1 == ExitOnError) {
            if (AreEqual(err, ErrHelp)) {
                os.Exit(0);
            }
            os.Exit(2);
        }
        else if (exprᴛ1 == PanicOnError) {
            throw panic(err);
        }

    }
    return default!;
}

// Parsed reports whether f.Parse has been called.
public static bool Parsed(this ref FlagSet f) {
    return f.parsed;
}

// Parse parses the command-line flags from [os.Args][1:]. Must be called
// after all flags are defined and before flags are accessed by the program.
public static void Parse() {
    // Ignore errors; CommandLine is set for ExitOnError.
    CommandLine.Parse(os.Args[1..]);
}

// Parsed reports whether the command-line flags have been parsed.
public static bool Parsed() {
    return CommandLine.Parsed();
}

// CommandLine is the default set of command-line flags, parsed from [os.Args].
// The top-level functions such as [BoolVar], [Arg], and so on are wrappers for the
// methods of CommandLine.
public static ж<FlagSet> CommandLine;

[GoInit] internal static void init() {
    // It's possible for execl to hand us an empty os.Args.
    if (len(os.Args) == 0){
        CommandLine = NewFlagSet(""u8, ExitOnError);
    } else {
        CommandLine = NewFlagSet(os.Args[0], ExitOnError);
    }
    // Override generic FlagSet default Usage with call to global Usage.
    // Note: This is not CommandLine.Usage = Usage,
    // because we want any eventual call to use any updated value of Usage,
    // not the value it has when this line is run.
    CommandLine.Value.Usage = commandLineUsage;
}

internal static partial void commandLineUsage() {
    Usage();
}

// NewFlagSet returns a new, empty flag set with the specified name and
// error handling property. If the name is not empty, it will be printed
// in the default usage message and in error messages.
public static ж<FlagSet> NewFlagSet(@string name, ΔErrorHandling errorHandling) {
    var f = Ꮡ(new FlagSet(
        name: name,
        errorHandling: errorHandling
    ));
    var fʗ1 = f;
        f.Value.Usage = () => fʗ1.defaultUsage();
    return f;
}

// Init sets the name and error handling property for a flag set.
// By default, the zero [FlagSet] uses an empty name and the
// [ContinueOnError] error handling policy.
public static void Init(this ref FlagSet f, @string name, ΔErrorHandling errorHandling) {
    f.name = name;
    f.errorHandling = errorHandling;
}

} // end flag_package
