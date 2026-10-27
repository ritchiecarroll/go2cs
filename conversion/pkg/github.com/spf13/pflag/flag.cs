// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using ParseErrorsWhitelist = go.github.com.spf13.pflag_package.ParseErrorsAllowlist;

namespace go.github.com.spf13;

using bytes = bytes_package;
using errors = errors_package;
using goflag = flag_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using sort = sort_package;
using strings = strings_package;
using flag = flag_package;

partial class pflag_package {

public static error ErrHelp = errors.New("pflag: help requested");

partial struct ErrorHandling /*num:nint*/;

public static ErrorHandling ContinueOnError => /* iota */ 0;
public static ErrorHandling ExitOnError => 1;
public static ErrorHandling PanicOnError => 2;

partial struct ParseErrorsAllowlist {
    public bool UnknownFlags;
}

partial struct NormalizedName /*@string*/;

partial struct FlagSet {
    public Action Usage;
    public bool SortFlags;
    public ParseErrorsAllowlist ParseErrorsAllowlist;
    public ParseErrorsAllowlist ParseErrorsWhitelist;
    internal @string name;
    internal bool parsed;
    internal map<NormalizedName, ж<Flag>> actual;
    internal slice<ж<Flag>> orderedActual;
    internal slice<ж<Flag>> sortedActual;
    internal map<NormalizedName, ж<Flag>> formal;
    internal slice<ж<Flag>> orderedFormal;
    internal slice<ж<Flag>> sortedFormal;
    internal map<byte, ж<Flag>> shorthands;
    internal slice<@string> args;
    internal nint argsLenAtDash;
    internal ErrorHandling errorHandling;
    internal io.Writer output;
    internal bool interspersed;
    internal Func<ж<FlagSet>, @string, NormalizedName> normalizeNameFunc;
    internal slice<ж<goflag.FlagSet>> addedGoFlagSets;
}

partial struct Flag {
    public @string Name;
    public @string Shorthand;
    public @string Usage;
    public Value Value;
    public @string DefValue;
    public bool Changed;
    public @string NoOptDefVal;
    public @string Deprecated;
    public bool Hidden;
    public @string ShorthandDeprecated;
    public map<@string, slice<@string>> Annotations;
}

partial interface Value :
    goflag.Value
{
    @string Type();
}

partial interface SliceValue {
    error Append(@string _);
    error Replace(slice<@string> _);
    slice<@string> GetSlice();
}

internal static slice<ж<Flag>> sortFlags(map<NormalizedName, ж<Flag>> flags) {
    var list = new sort.StringSlice(len(flags));
    nint i = 0;
    foreach (var (k, _) in flags) {
        list[i] = (@string)k;
        i++;
    }
    list.Sort();
    var result = new slice<ж<Flag>>(len(list));
    foreach (var (iΔ1, name) in list) {
        result[iΔ1] = flags[((NormalizedName)name)];
    }
    return result;
}

public static void SetNormalizeFunc(this ж<FlagSet> Ꮡf, Func<ж<FlagSet>, @string, NormalizedName> n) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.normalizeNameFunc = n;
    f.sortedFormal = f.sortedFormal[..0];
    foreach (var (fname, flag) in f.formal) {
        NormalizedName nname = Ꮡf.normalizeFlagName((~flag).Name);
        if (fname == nname) {
            continue;
        }
        flag.Value.Name = (@string)nname;
        delete(f.formal, fname);
        f.formal[nname] = flag;
        {
            var (_, set) = f.actual[fname, ꟷ]; if (set) {
                delete(f.actual, fname);
                f.actual[nname] = flag;
            }
        }
    }
}

public static Func<ж<FlagSet>, @string, NormalizedName> GetNormalizeFunc(this ref FlagSet f) {
    if (f.normalizeNameFunc != default!) {
        return f.normalizeNameFunc;
    }
    return (ж<FlagSet> fΔ1, @string name) => (NormalizedName)name;
}

internal static NormalizedName normalizeFlagName(this ж<FlagSet> Ꮡf, @string name) {
    ref var f = ref Ꮡf.DerefOrNull();

    var n = f.GetNormalizeFunc();
    return n(Ꮡf, name);
}

public static io.Writer Output(this ref FlagSet f) {
    if (f.output == default!) {
        return new os.FileжWriter(os.Stderr);
    }
    return f.output;
}

public static @string Name(this ref FlagSet f) {
    return f.name;
}

public static void SetOutput(this ref FlagSet f, io.Writer output) {
    f.output = output;
}

public static void VisitAll(this ref FlagSet f, Action<ж<Flag>> fn) {
    if (len(f.formal) == 0) {
        return;
    }

    slice<ж<Flag>> flags = default!;
    if (f.SortFlags){
        if (len(f.formal) != len(f.sortedFormal)) {
            f.sortedFormal = sortFlags(f.formal);
        }
        flags = f.sortedFormal;
    } else {
        flags = f.orderedFormal;
    }

    foreach (var (_, flag) in flags) {
        fn(flag);
    }
}

public static bool HasFlags(this ref FlagSet f) {
    return len(f.formal) > 0;
}

public static bool HasAvailableFlags(this ref FlagSet f) {
    foreach (var (_, flag) in f.formal) {
        if (!(~flag).Hidden) {
            return true;
        }
    }
    return false;
}

public static void VisitAll(Action<ж<Flag>> fn) {
    CommandLine.VisitAll(fn);
}

public static void Visit(this ref FlagSet f, Action<ж<Flag>> fn) {
    if (len(f.actual) == 0) {
        return;
    }

    slice<ж<Flag>> flags = default!;
    if (f.SortFlags){
        if (len(f.actual) != len(f.sortedActual)) {
            f.sortedActual = sortFlags(f.actual);
        }
        flags = f.sortedActual;
    } else {
        flags = f.orderedActual;
    }

    foreach (var (_, flag) in flags) {
        fn(flag);
    }
}

public static void Visit(Action<ж<Flag>> fn) {
    CommandLine.Visit(fn);
}

public static ж<Flag> Lookup(this ж<FlagSet> Ꮡf, @string name) {
    ref var f = ref Ꮡf.DerefOrNull();

    return f.lookup(Ꮡf.normalizeFlagName(name));
}

public static ж<Flag> ShorthandLookup(this ref FlagSet f, @string name) {
    if (name == ""u8) {
        return default!;
    }
    if (len(name) > 1) {
        @string msg = fmt.Sprintf("can not look up shorthand which is more than one ASCII character: %q"u8, name);
        fmt.Fprintf(f.Output(), msg);
        throw panic(msg);
    }
    var c = name[0];
    return f.shorthands[c];
}

internal static ж<Flag> lookup(this ref FlagSet f, NormalizedName name) {
    return f.formal[name];
}

internal static (any, error) getFlagType(this ж<FlagSet> Ꮡf, @string name, @string ftype, Func<@string, (any, error)> convFunc) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        var errΔ1 = Ꮡ(new NotExistError(name: name, messageType: flagNotDefinedMessage));
        return (default!, new NotExistErrorжerror(errΔ1));
    }

    if ((~flag).Value.Type() != ftype) {
        var errΔ2 = fmt.Errorf("trying to get %s value of flag of type %s"u8, ftype, (~flag).Value.Type());
        return (default!, errΔ2);
    }

    @string sval = (~flag).Value.String();
    var (result, err) = convFunc(sval);
    if (err != default!) {
        return (default!, err);
    }
    return (result, default!);
}

public static nint ArgsLenAtDash(this ref FlagSet f) {
    return f.argsLenAtDash;
}

public static error MarkDeprecated(this ж<FlagSet> Ꮡf, @string name, @string usageMessage) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        return new NotExistErrorжerror(Ꮡ(new NotExistError(name: name, messageType: flagNotExistMessage)));
    }
    if (usageMessage == ""u8) {
        return fmt.Errorf("deprecated message for flag %q must be set"u8, name);
    }
    flag.Value.Deprecated = usageMessage;
    flag.Value.Hidden = true;
    return default!;
}

public static error MarkShorthandDeprecated(this ж<FlagSet> Ꮡf, @string name, @string usageMessage) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        return new NotExistErrorжerror(Ꮡ(new NotExistError(name: name, messageType: flagNotExistMessage)));
    }
    if (usageMessage == ""u8) {
        return fmt.Errorf("deprecated message for flag %q must be set"u8, name);
    }
    flag.Value.ShorthandDeprecated = usageMessage;
    return default!;
}

public static error MarkHidden(this ж<FlagSet> Ꮡf, @string name) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        return new NotExistErrorжerror(Ꮡ(new NotExistError(name: name, messageType: flagNotExistMessage)));
    }
    flag.Value.Hidden = true;
    return default!;
}

public static ж<Flag> Lookup(@string name) {
    return CommandLine.Lookup(name);
}

public static ж<Flag> ShorthandLookup(@string name) {
    return CommandLine.ShorthandLookup(name);
}

public static error Set(this ж<FlagSet> Ꮡf, @string name, @string value) {
    ref var f = ref Ꮡf.DerefOrNull();

    NormalizedName normalName = Ꮡf.normalizeFlagName(name);
    var (flag, ok) = f.formal[normalName, ꟷ];
    if (!ok) {
        return new NotExistErrorжerror(Ꮡ(new NotExistError(name: name, messageType: flagNoSuchFlagMessage)));
    }

    var err = (~flag).Value.Set(value);
    if (err != default!) {
        return new InvalidValueErrorжerror(Ꮡ(new InvalidValueError(
            flag: flag,
            value: value,
            cause: err
        )));
    }

    if (!(~flag).Changed) {
        if (f.actual == default!) {
            f.actual = new map<NormalizedName, ж<Flag>>();
        }
        f.actual[normalName] = flag;
        f.orderedActual = append(f.orderedActual, flag);

        flag.Value.Changed = true;
    }

    if ((~flag).Deprecated != ""u8) {
        fmt.Fprintf(f.Output(), "Flag --%s has been deprecated, %s\n"u8, (~flag).Name, (~flag).Deprecated);
    }
    return default!;
}

public static error SetAnnotation(this ж<FlagSet> Ꮡf, @string name, @string key, slice<@string> values) {
    ref var f = ref Ꮡf.DerefOrNull();

    NormalizedName normalName = Ꮡf.normalizeFlagName(name);
    var (flag, ok) = f.formal[normalName, ꟷ];
    if (!ok) {
        return new NotExistErrorжerror(Ꮡ(new NotExistError(name: name, messageType: flagNoSuchFlagMessage)));
    }
    if ((~flag).Annotations == default!) {
        flag.Value.Annotations = new map<@string, slice<@string>>{};
    }
    flag.Value.Annotations[key] = values;
    return default!;
}

public static bool Changed(this ж<FlagSet> Ꮡf, @string name) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        return false;
    }
    return (~flag).Changed;
}

public static error Set(@string name, @string value) {
    return CommandLine.Set(name, value);
}

public static void PrintDefaults(this ref FlagSet f) {
    @string usages = f.FlagUsages();
    fmt.Fprint(f.Output(), usages);
}

internal static bool defaultIsZeroValue(this ref Flag f) {
    switch (f.Value.type()) {
    case {} ᴛ0 when ᴛ0._<boolFlag>(out var _): {
        return f.DefValue == "false"u8 || f.DefValue == ""u8;
    }
    case ж<durationValue>: {
        return f.DefValue == "0"u8 || f.DefValue == "0s"u8;
    }
    case ж<intValue> _:
    case ж<int8Value> _:
    case ж<int32Value> _:
    case ж<int64Value> _:
    case ж<uintValue> _:
    case ж<uint8Value> _:
    case ж<uint16Value> _:
    case ж<uint32Value> _:
    case ж<uint64Value> _:
    case ж<countValue> _:
    case ж<float32Value> _:
    case ж<float64Value> _: {
        return f.DefValue == "0"u8;
    }
    case ж<stringValue>: {
        return f.DefValue == ""u8;
    }
    case ж<ipValue> _:
    case ж<ipMaskValue> _:
    case ж<ipNetValue> _: {
        return f.DefValue == "<nil>"u8;
    }
    case ж<intSliceValue> _:
    case ж<stringSliceValue> _:
    case ж<stringArrayValue> _: {
        return f.DefValue == "[]"u8;
    }
    default: {
        var exprᴛ1 = f.DefValue;
        if (exprᴛ1 == "false"u8) {
            return true;
        }
        if (exprᴛ1 == "<nil>"u8) {
            return true;
        }
        if (exprᴛ1 == ""u8) {
            return true;
        }
        if (exprᴛ1 == "0"u8) {
            return true;
        }

        return false;
    }}

}

public static (@string name, @string usage) UnquoteUsage(ж<Flag> Ꮡflag) {
    @string name = default!;
    @string usage = default!;
    ref var flag = ref Ꮡflag.DerefOrNull();

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
            break;
        }
    }

    name = flag.Value.Type();
    var exprᴛ1 = name;
    if (exprᴛ1 == "bool"u8 || exprᴛ1 == "boolfunc"u8) {
        name = "";
    }
    else if (exprᴛ1 == "func"u8) {
        name = "value";
    }
    else if (exprᴛ1 == "float64"u8) {
        name = "float";
    }
    else if (exprᴛ1 == "int64"u8) {
        name = "int";
    }
    else if (exprᴛ1 == "uint64"u8) {
        name = "uint";
    }
    else if (exprᴛ1 == "stringSlice"u8) {
        name = "strings";
    }
    else if (exprᴛ1 == "intSlice"u8) {
        name = "ints";
    }
    else if (exprᴛ1 == "uintSlice"u8) {
        name = "uints";
    }
    else if (exprᴛ1 == "boolSlice"u8) {
        name = "bools";
    }

    return (name, usage);
}

internal static (@string, @string) wrapN(nint i, nint slop, @string s) {
    if (i + slop > len(s)) {
        return (s, "");
    }

    nint w = strings.LastIndexAny(s.slice(0, i), " \t\n");
    if (w <= 0) {
        return (s, "");
    }
    nint nlPos = strings.LastIndex(s.slice(0, i), "\n");
    if (nlPos > 0 && nlPos < w) {
        return (s.slice(0, nlPos), s.slice(nlPos + 1));
    }
    return (s.slice(0, w), s.slice(w + 1));
}

internal static @string wrap(nint i, nint w, @string s) {
    if (w == 0) {
        return strings.Replace(s, "\n", "\n"u8 + strings.Repeat(" ", i), -1);
    }

    nint wrap = w - i;

    @string r = default!;
    @string l = default!;

    if (wrap < 24) {
        i = 16;
        wrap = w - i;
        r += "\n"u8 + strings.Repeat(" ", i);
    }
    if (wrap < 24) {
        return strings.Replace(s, "\n", r, -1);
    }

    nint slop = 5;
    wrap = wrap - slop;

    (l, s) = wrapN(wrap, slop, s);
    r = r + strings.Replace(l, "\n", "\n"u8 + strings.Repeat(" ", i), -1);

    while (s != ""u8) {
        @string t = default!;

        (t, s) = wrapN(wrap, slop, s);
        r = r + "\n"u8 + strings.Repeat(" ", i) + strings.Replace(t, "\n", "\n"u8 + strings.Repeat(" ", i), -1);
    }

    return r;
}

public static @string FlagUsagesWrapped(this ref FlagSet f, nint cols) {
    var buf = @new<bytes.Buffer>();

    ref var lines = ref heap<slice<@string>>(out var Ꮡlines);
    lines = new slice<@string>(0, len(f.formal));

    nint maxlen = 0;
    f.VisitAll((ж<Flag> flag) => {
        if ((~flag).Hidden) {
            return;
        }

        @string line = "";
        if ((~flag).Shorthand != ""u8 && (~flag).ShorthandDeprecated == ""u8){
            line = fmt.Sprintf("  -%s, --%s"u8, (~flag).Shorthand, (~flag).Name);
        } else {
            line = fmt.Sprintf("      --%s"u8, (~flag).Name);
        }

        var (varname, usage) = UnquoteUsage(flag);
        if (varname != ""u8) {
            line += " "u8 + varname;
        }
        if ((~flag).NoOptDefVal != ""u8) {
            var exprᴛ1 = (~flag).Value.Type();
            if (exprᴛ1 == "string"u8) {
                line += fmt.Sprintf("[=\"%s\"]"u8, (~flag).NoOptDefVal);
            }
            else if (exprᴛ1 == "bool"u8 || exprᴛ1 == "boolfunc"u8) {
                if ((~flag).NoOptDefVal != "true"u8) {
                    line += fmt.Sprintf("[=%s]"u8, (~flag).NoOptDefVal);
                }
            }
            else if (exprᴛ1 == "count"u8) {
                if ((~flag).NoOptDefVal != "+1"u8) {
                    line += fmt.Sprintf("[=%s]"u8, (~flag).NoOptDefVal);
                }
            }
            else { /* default: */
                line += fmt.Sprintf("[=%s]"u8, (~flag).NoOptDefVal);
            }

        }

        line += "\x00"u8;
        if (len(line) > maxlen) {
            maxlen = len(line);
        }

        line += usage;
        if (!flag.defaultIsZeroValue()) {
            if ((~flag).Value.Type() == "string"u8){
                line += fmt.Sprintf(" (default %q)"u8, (~flag).DefValue);
            } else {
                line += fmt.Sprintf(" (default %s)"u8, (~flag).DefValue);
            }
        }
        if (len((~flag).Deprecated) != 0) {
            line += fmt.Sprintf(" (DEPRECATED: %s)"u8, (~flag).Deprecated);
        }

        Ꮡlines.ValueSlot = append(Ꮡlines.ValueSlot, line);
    });

    foreach (var (_, line) in lines) {
        nint sidx = strings.Index(line, "\x00");
        @string spacing = strings.Repeat(" ", maxlen - sidx);
        fmt.Fprintln(new bytes_BufferжWriter(buf), line.slice(0, sidx), spacing, wrap(maxlen + 2, cols, line.slice(sidx + 1)));
    }

    return buf.String();
}

public static @string FlagUsages(this ref FlagSet f) {
    return f.FlagUsagesWrapped(0);
}

public static void PrintDefaults() {
    CommandLine.PrintDefaults();
}

internal static void defaultUsage(ж<FlagSet> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    fmt.Fprintf(f.Output(), "Usage of %s:\n"u8, f.name);
    f.PrintDefaults();
}

[GoLazy] partial class lazyᴛUsage { public static Action Usage = () => {
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "Usage of %s:\n"u8, os.Args[0]);
    PrintDefaults();
}; }

public static nint NFlag(this ref FlagSet f) {
    return len(f.actual);
}

public static nint NFlag() {
    return len((~CommandLine).actual);
}

public static @string Arg(this ref FlagSet f, nint i) {
    if (i < 0 || i >= len(f.args)) {
        return "";
    }
    return f.args[i];
}

public static @string Arg(nint i) {
    return CommandLine.Arg(i);
}

public static nint NArg(this ref FlagSet f) {
    return len(f.args);
}

public static nint NArg() {
    return len((~CommandLine).args);
}

public static slice<@string> Args(this ref FlagSet f) {
    return f.args;
}

public static slice<@string> Args() {
    return (~CommandLine).args;
}

public static void Var(this ж<FlagSet> Ꮡf, Value value, @string name, @string usage) {
    Ꮡf.VarP(value, name, "", usage);
}

public static ж<Flag> VarPF(this ж<FlagSet> Ꮡf, Value value, @string name, @string shorthand, @string usage) {
    var flag = Ꮡ(new Flag(
        Name: name,
        Shorthand: shorthand,
        Usage: usage,
        Value: value,
        DefValue: value.String()
    ));
    Ꮡf.AddFlag(flag);
    return flag;
}

public static void VarP(this ж<FlagSet> Ꮡf, Value value, @string name, @string shorthand, @string usage) {
    Ꮡf.VarPF(value, name, shorthand, usage);
}

public static void AddFlag(this ж<FlagSet> Ꮡf, ж<Flag> Ꮡflag) {
    ref var f = ref Ꮡf.DerefOrNull();
    ref var flag = ref Ꮡflag.DerefOrNull();

    NormalizedName normalizedFlagName = Ꮡf.normalizeFlagName(flag.Name);

    var (_, alreadyThere) = f.formal[normalizedFlagName, ꟷ];
    if (alreadyThere) {
        @string msg = fmt.Sprintf("%s flag redefined: %s"u8, f.name, flag.Name);
        fmt.Fprintln(f.Output(), msg);
        throw panic(msg);
    }
    if (f.formal == default!) {
        f.formal = new map<NormalizedName, ж<Flag>>();
    }

    flag.Name = (@string)normalizedFlagName;
    f.formal[normalizedFlagName] = Ꮡflag;
    f.orderedFormal = append(f.orderedFormal, Ꮡflag);

    if (flag.Shorthand == ""u8) {
        return;
    }
    if (len(flag.Shorthand) > 1) {
        @string msg = fmt.Sprintf("%q shorthand is more than one ASCII character"u8, flag.Shorthand);
        fmt.Fprintf(f.Output(), msg);
        throw panic(msg);
    }
    if (f.shorthands == default!) {
        f.shorthands = new map<byte, ж<Flag>>();
    }
    var c = flag.Shorthand[0];
    (var used, alreadyThere) = f.shorthands[c, ꟷ];
    if (alreadyThere) {
        @string msg = fmt.Sprintf("unable to redefine %q shorthand in %q flagset: it's already used for %q flag"u8, c, f.name, (~used).Name);
        fmt.Fprintf(f.Output(), msg);
        throw panic(msg);
    }
    f.shorthands[c] = Ꮡflag;
}

public static void AddFlagSet(this ж<FlagSet> Ꮡf, ж<FlagSet> ᏑnewSet) {
    ref var newSet = ref ᏑnewSet.DerefOrNull();

    if (ᏑnewSet == nil) {
        return;
    }
    newSet.VisitAll((ж<Flag> flag) => {
        if (Ꮡf.Lookup((~flag).Name) == nil) {
            Ꮡf.AddFlag(flag);
        }
    });
}

public static void Var(Value value, @string name, @string usage) {
    CommandLine.VarP(value, name, "", usage);
}

public static void VarP(Value value, @string name, @string shorthand, @string usage) {
    CommandLine.VarP(value, name, shorthand, usage);
}

internal static error fail(this ж<FlagSet> Ꮡf, error err) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.errorHandling != ContinueOnError) {
        Ꮡf.usage();
    }
    return err;
}

internal static void usage(this ж<FlagSet> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (Ꮡf == CommandLine){
        Usage();
    } else 
    if (f.Usage == default!){
        defaultUsage(Ꮡf);
    } else {
        f.Usage();
    }
}

internal static slice<@string> stripUnknownFlagValue(slice<@string> args) {
    if (len(args) == 0) {
        return args;
    }

    @string first = args[0];
    if (len(first) > 0 && first[0] == (rune)'-') {
        return args;
    }

    if (len(args) > 1) {
        return args[1..];
    }
    return default!;
}

internal static (slice<@string> a, error err) parseLongArg(this ж<FlagSet> Ꮡf, @string s, slice<@string> args, Func<ж<Flag>, @string, error> fn) {
    slice<@string> a = default!;
    error err = default!;
    ref var f = ref Ꮡf.DerefOrNull();

    a = args;
    ref var name = ref heap<@string>(out var Ꮡname);
    name = s[2..];
    if (len(name) == 0 || name[0] == (rune)'-' || name[0] == (rune)'=') {
        err = Ꮡf.fail(new InvalidSyntaxErrorжerror(Ꮡ(new InvalidSyntaxError(specifiedFlag: s))));
        return (a, err);
    }

    var split = strings.SplitN(name, "=", 2);
    name = split[0];
    var (flag, exists) = f.formal[Ꮡf.normalizeFlagName(name), ꟷ];

    if (!exists) {
        var matchᴛ1 = false;
        if (name == "help"u8) { matchᴛ1 = true;
            Ꮡf.usage();
            return (a, ErrHelp);
        }
        if (f.ParseErrorsWhitelist.UnknownFlags) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (f.ParseErrorsAllowlist.UnknownFlags)) {
            if (len(split) >= 2) {
                return (a, default!);
            }

            return (stripUnknownFlagValue(a), default!);
        }
        if (!matchᴛ1) { /* default: */
            err = Ꮡf.fail(new NotExistErrorжerror(Ꮡ(new NotExistError(name: name, messageType: flagUnknownFlagMessage))));
            return (a, err);
        }
        return default!;

    }

    @string value = default!;
    if (len(split) == 2){
        value = split[1];
    } else 
    if ((~flag).NoOptDefVal != ""u8){
        value = flag.Value.NoOptDefVal;
    } else 
    if (len(a) > 0){
        value = a[0];
        a = a[1..];
    } else {
        err = Ꮡf.fail(new ValueRequiredErrorжerror(Ꮡ(new ValueRequiredError(
            flag: flag,
            specifiedName: name
        ))));
        return (a, err);
    }

    err = fn(flag, value);
    if (err != default!) {
        Ꮡf.fail(err);
    }
    return (a, err);
}

internal static (@string outShorts, slice<@string> outArgs, error err) parseSingleShortArg(this ж<FlagSet> Ꮡf, @string shorthands, slice<@string> args, Func<ж<Flag>, @string, error> fn) {
    @string outShorts = default!;
    slice<@string> outArgs = default!;
    error err = default!;
    ref var f = ref Ꮡf.DerefOrNull();

    outArgs = args;

    if (isGotestShorthandFlag(shorthands)) {
        return (outShorts, outArgs, err);
    }

    outShorts = shorthands[1..];
    var c = shorthands[0];

    var (flag, exists) = f.shorthands[c, ꟷ];
    if (!exists) {
        var matchᴛ1 = false;
        if (c is (rune)'h') { matchᴛ1 = true;
            Ꮡf.usage();
            err = ErrHelp;
            return (outShorts, outArgs, err);
        }
        if (f.ParseErrorsWhitelist.UnknownFlags) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (f.ParseErrorsAllowlist.UnknownFlags)) {
            if (len(shorthands) > 2 && shorthands[1] == (rune)'=') {
                outShorts = "";
                return (outShorts, outArgs, err);
            }

            outArgs = stripUnknownFlagValue(outArgs);
            return (outShorts, outArgs, err);
        }
        if (!matchᴛ1) { /* default: */
            err = Ꮡf.fail(new NotExistErrorжerror(Ꮡ(new NotExistError(
                name: ((@string)c),
                specifiedShorthands: shorthands,
                messageType: flagUnknownShorthandFlagMessage
            ))));
            return (outShorts, outArgs, err);
        }
        return default!;

    }

    @string value = default!;
    if (len(shorthands) > 2 && shorthands[1] == (rune)'='){
        value = shorthands[2..];
        outShorts = "";
    } else 
    if ((~flag).NoOptDefVal != ""u8){
        value = flag.Value.NoOptDefVal;
    } else 
    if (len(shorthands) > 1){
        value = shorthands[1..];
        outShorts = "";
    } else 
    if (len(args) > 0){
        value = args[0];
        outArgs = args[1..];
    } else {
        err = Ꮡf.fail(new ValueRequiredErrorжerror(Ꮡ(new ValueRequiredError(
            flag: flag,
            specifiedName: ((@string)c),
            specifiedShorthands: shorthands
        ))));
        return (outShorts, outArgs, err);
    }

    if ((~flag).ShorthandDeprecated != ""u8) {
        fmt.Fprintf(f.Output(), "Flag shorthand -%s has been deprecated, %s\n"u8, (~flag).Shorthand, (~flag).ShorthandDeprecated);
    }

    err = fn(flag, value);
    if (err != default!) {
        Ꮡf.fail(err);
    }
    return (outShorts, outArgs, err);
}

internal static (slice<@string> a, error err) parseShortArg(this ж<FlagSet> Ꮡf, @string s, slice<@string> args, Func<ж<Flag>, @string, error> fn) {
    slice<@string> a = default!;
    error err = default!;

    a = args;
    @string shorthands = s[1..];

    while (len(shorthands) > 0) {
        (shorthands, a, err) = Ꮡf.parseSingleShortArg(shorthands, args, fn);
        if (err != default!) {
            return (a, err);
        }
    }

    return (a, err);
}

internal static error /*err*/ parseArgs(this ж<FlagSet> Ꮡf, slice<@string> args, Func<ж<Flag>, @string, error> fn) {
    error err = default!;
    ref var f = ref Ꮡf.DerefOrNull();

    while (len(args) > 0) {
        @string s = args[0];
        args = args[1..];
        if (len(s) == 0 || s[0] != (rune)'-' || len(s) == 1) {
            if (!f.interspersed) {
                f.args = append(f.args, s);
                f.args = appendꓸꓸꓸ(f.args, args);
                return default!;
            }
            f.args = append(f.args, s);
            continue;
        }

        if (s[1] == (rune)'-'){
            if (len(s) == 2) {
                f.argsLenAtDash = len(f.args);
                f.args = appendꓸꓸꓸ(f.args, args);
                break;
            }
            (args, err) = Ꮡf.parseLongArg(s, args, fn);
        } else {
            (args, err) = Ꮡf.parseShortArg(s, args, fn);
        }
        if (err != default!) {
            return err;
        }
    }
    return err;
}

public static error Parse(this ж<FlagSet> Ꮡf, slice<@string> arguments) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.addedGoFlagSets != default!) {
        foreach (var (_, goFlagSet) in f.addedGoFlagSets) {
            goFlagSet.Parse(default!);
        }
    }
    f.parsed = true;

    f.args = new slice<@string>(0, len(arguments));

    if (len(arguments) == 0) {
        return default!;
    }

    var set = (ж<Flag> flag, @string value) => Ꮡf.Set((~flag).Name, value);

    var err = Ꮡf.parseArgs(arguments, new Func<ж<Flag>, @string, error>(set));
    if (err != default!) {
        var exprᴛ1 = f.errorHandling;
        if (exprᴛ1 == ContinueOnError) {
            return err;
        }
        if (exprᴛ1 == ExitOnError) {
            if (errors.Is(err, ErrHelp)) {
                os.Exit(0);
            }
            fmt.Fprintln(f.Output(), err);
            os.Exit(2);
        }
        else if (exprᴛ1 == PanicOnError) {
            throw panic(err);
        }

    }
    return default!;
}

// type parseFunc is a methodless func type — rendered inline as its base delegate

public static error ParseAll(this ж<FlagSet> Ꮡf, slice<@string> arguments, Func<ж<Flag>, @string, error> fn) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.parsed = true;
    f.args = new slice<@string>(0, len(arguments));

    var err = Ꮡf.parseArgs(arguments, new Func<ж<Flag>, @string, error>(fn));
    if (err != default!) {
        var exprᴛ1 = f.errorHandling;
        if (exprᴛ1 == ContinueOnError) {
            return err;
        }
        if (exprᴛ1 == ExitOnError) {
            if (errors.Is(err, ErrHelp)) {
                os.Exit(0);
            }
            fmt.Fprintln(f.Output(), err);
            os.Exit(2);
        }
        else if (exprᴛ1 == PanicOnError) {
            throw panic(err);
        }

    }
    return default!;
}

public static bool Parsed(this ref FlagSet f) {
    return f.parsed;
}

public static void Parse() {
    CommandLine.Parse(os.Args[1..]);
}

public static void ParseAll(Func<ж<Flag>, @string, error> fn) {
    CommandLine.ParseAll(os.Args[1..], fn);
}

public static void SetInterspersed(bool interspersed) {
    CommandLine.SetInterspersed(interspersed);
}

public static bool Parsed() {
    return CommandLine.Parsed();
}

public static ж<FlagSet> CommandLine = NewFlagSet(os.Args[0], ExitOnError);

public static ж<FlagSet> NewFlagSet(@string name, ErrorHandling errorHandling) {
    var f = Ꮡ(new FlagSet(
        name: name,
        errorHandling: errorHandling,
        argsLenAtDash: -1,
        interspersed: true,
        SortFlags: true
    ));
    return f;
}

public static void SetInterspersed(this ref FlagSet f, bool interspersed) {
    f.interspersed = interspersed;
}

public static void Init(this ref FlagSet f, @string name, ErrorHandling errorHandling) {
    f.name = name;
    f.errorHandling = errorHandling;
    f.argsLenAtDash = -1;
}

} // end pflag_package
