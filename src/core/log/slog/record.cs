// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using runtime = runtime_package;
using slices = slices_package;
using time = time_package;
using ꓸꓸꓸAttr = Span<slog_package.Attr>;
using ꓸꓸꓸany = Span<any>;

partial class slog_package {

internal static UntypedInt nAttrsInline => 5;

// A Record holds information about a log event.
// Copies of a Record share state.
// Do not modify a Record after handing out a copy to it.
// Call [NewRecord] to create a new Record.
// Use [Record.Clone] to create a copy with no shared state.
[GoType] partial struct Record {
    // The time at which the output method (Log, Info, etc.) was called.
    public time.Time Time;
    // The log message.
    public @string Message;
    // The level of the event.
    public ΔLevel Level;
    // The program counter at the time the record was constructed, as determined
    // by runtime.Callers. If zero, no program counter is available.
    //
    // The only valid use for this value is as an argument to
    // [runtime.CallersFrames]. In particular, it must not be passed to
    // [runtime.FuncForPC].
    public uintptr PC;
    // Allocation optimization: an inline array sized to hold
    // the majority of log calls (based on examination of open-source
    // code). It holds the start of the list of Attrs.
    internal array<Attr> front = new(nAttrsInline);
    // The number of Attrs in front.
    internal nint nFront;
    // The list of Attrs except for those in front.
    // Invariants:
    //   - len(back) > 0 iff nFront == len(front)
    //   - Unused array elements are zero. Used to detect mistakes.
    internal slice<Attr> back;
}

// NewRecord creates a [Record] from the given arguments.
// Use [Record.AddAttrs] to add attributes to the Record.
//
// NewRecord is intended for logging APIs that want to support a [Handler] as
// a backend.
public static Record NewRecord(time.Time t, ΔLevel level, @string msg, uintptr pc) {
    return new Record(
        Time: t,
        Message: msg,
        Level: level,
        PC: pc
    );
}

// Clone returns a copy of the record with no shared state.
// The original record and the clone can both be modified
// without interfering with each other.
public static Record Clone(this Record r) {
    r = r.ΔClone();

    r.back = slices.Clip<slice<Attr>, Attr>(r.back); // prevent append from mutating shared array
    return r.ΔClone();
}

// NumAttrs returns the number of attributes in the [Record].
public static nint NumAttrs(this Record r) {
    r = r.ΔClone();

    return r.nFront + len(r.back);
}

// Attrs calls f on each Attr in the [Record].
// Iteration stops if f returns false.
public static void Attrs(this Record r, Func<Attr, bool> f) {
    r = r.ΔClone();

    for (nint i = 0; i < r.nFront; i++) {
        if (!f(r.front[i])) {
            return;
        }
    }
    foreach (var (_, a) in r.back) {
        if (!f(a)) {
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bugˢ = "!BUG"u8;
internal static readonly @string addAttrsUnsafelyCalledOnˢ = "AddAttrs unsafely called on copy of Record made without using Record.Clone"u8;

// AddAttrs appends the given Attrs to the [Record]'s list of Attrs.
// It omits empty groups.
[GoRecv] public static void AddAttrs(this ref Record r, params ꓸꓸꓸAttr attrsʗp) {
    var attrs = attrsʗp.slice();

    nint i = default!;
    for (i = 0; i < len(attrs) && r.nFront < len(r.front); i++) {
        var a = attrs[i];
        if (a.Value.isEmptyGroup()) {
            continue;
        }
        r.front[r.nFront] = a;
        r.nFront++;
    }
    // Check if a copy was modified by slicing past the end
    // and seeing if the Attr there is non-zero.
    if (cap(r.back) > len(r.back)) {
        var end = r.back.slice(0, len(r.back) + 1)[len(r.back)];
        if (!end.isEmpty()) {
            // Don't panic; copy and muddle through.
            r.back = slices.Clip<slice<Attr>, Attr>(r.back);
            r.back = builtin.append(r.back, String(bugˢ, addAttrsUnsafelyCalledOnˢ));
        }
    }
    nint ne = countEmptyGroups(attrs.slice(i));
    r.back = slices.Grow<slice<Attr>, Attr>(r.back, len(attrs.slice(i)) - ne);
    foreach (var (_, a) in attrs.slice(i)) {
        if (!a.Value.isEmptyGroup()) {
            r.back = builtin.append(r.back, a);
        }
    }
}

// Add converts the args to Attrs as described in [Logger.Log],
// then appends the Attrs to the [Record]'s list of Attrs.
// It omits empty groups.
[GoRecv] public static void Add(this ref Record r, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    Attr a = default!;
    while (len(args) > 0) {
        (a, args) = argsToAttr(args);
        if (a.Value.isEmptyGroup()) {
            continue;
        }
        if (r.nFront < len(r.front)){
            r.front[r.nFront] = a;
            r.nFront++;
        } else {
            if (r.back == default!) {
                r.back = new slice<Attr>(0, countAttrs(args) + 1);
            }
            r.back = builtin.append(r.back, a);
        }
    }
}

// countAttrs returns the number of Attrs that would be created from args.
internal static nint countAttrs(slice<any> args) {
    nint n = 0;
    for (nint i = 0; i < len(args); i++) {
        n++;
        {
            var (_, ok) = args[i]._<@string>(ᐧ); if (ok) {
                i++;
            }
        }
    }
    return n;
}

internal static readonly @string badKey = "!BADKEY"u8;

// argsToAttr turns a prefix of the nonempty args slice into an Attr
// and returns the unconsumed portion of the slice.
// If args[0] is an Attr, it returns it.
// If args[0] is a string, it treats the first two elements as
// a key-value pair.
// Otherwise, it treats args[0] as a value with a missing key.
internal static (Attr, slice<any>) argsToAttr(slice<any> args) {
    switch (args[0].type()) {
    case @string x: {
        if (len(args) == 1) {
            return (String(badKey, x), default!);
        }
        return (Any(x, args[1]), args[2..]);
    }
    case Attr x: {
        return (x, args[1..]);
    }
    default: {
        var x = args[0];
        return (Any(badKey, x), args[1..]);
    }}
}

// Source describes the location of a line of source code.
[GoType] partial struct Source {
    // Function is the package path-qualified function name containing the
    // source line. If non-empty, this string uniquely identifies a single
    // function in the program. This may be the empty string if not known.
    [GoTag(@"json:""function""")]
    public @string Function;
    // File and Line are the file name and line number (1-based) of the source
    // line. These may be the empty string and zero, respectively, if not known.
    [GoTag(@"json:""file""")]
    public @string File;
    [GoTag(@"json:""line""")]
    public nint Line;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string functionˢ = "function"u8;
internal static readonly @string fileˢ = "file"u8;
internal static readonly @string lineˢ = "line"u8;

// group returns the non-zero fields of s as a slice of attrs.
// It is similar to a LogValue method, but we don't want Source
// to implement LogValuer because it would be resolved before
// the ReplaceAttr function was called.
[GoRecv] internal static Value group(this ref Source s) {
    slice<Attr> @as = default!;
    if (s.Function != ""u8) {
        @as = builtin.append(@as, String(functionˢ, s.Function));
    }
    if (s.File != ""u8) {
        @as = builtin.append(@as, String(fileˢ, s.File));
    }
    if (s.Line != 0) {
        @as = builtin.append(@as, Int(lineˢ, s.Line));
    }
    return GroupValue(@as.ꓸꓸꓸ);
}

// source returns a Source for the log event.
// If the Record was created without the necessary information,
// or if the location is unavailable, it returns a non-nil *Source
// with zero fields.
internal static ж<Source> source(this Record r) {
    r = r.ΔClone();

    var fs = runtime.CallersFrames(new uintptr[]{r.PC}.slice());
    var (f, _) = fs.Next();
    return Ꮡ(new Source(
        Function: f.Function,
        File: f.File,
        Line: f.Line
    ));
}

} // end slog_package
