// Copyright 2013-2023 The Cobra Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace go.github.com.spf13;

using bytes = bytes_package;
using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strings = strings_package;
using flag = go.github.com.spf13.pflag_package;
using go.github.com.spf13;
using path;
using pflag = go.github.com.spf13.pflag_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸжCommand = Span<ж<cobra_package.Command>>;
using ꓸꓸꓸжGroup = Span<ж<cobra_package.Group>>;

partial class cobra_package {

public static readonly @string FlagSetByCobraAnnotation = "cobra_annotation_flag_set_by_cobra"u8;
public static readonly @string CommandDisplayNameAnnotation = "cobra_annotation_command_display_name"u8;
internal static readonly @string helpFlagName = "help"u8;
internal static readonly @string helpCommandName = "help"u8;

partial struct FParseErrWhitelist /*global::go.github.com.spf13.pflag_package.ParseErrorsAllowlist*/;

partial struct Group {
    public @string ID;
    public @string Title;
}

partial struct Command_commandCalledAs /*dyn*/ {
    internal @string name;
    internal bool called;
}

partial struct Command {
    public @string Use;
    public slice<@string> Aliases;
    public slice<@string> SuggestFor;
    public @string Short;
    public @string GroupID;
    public @string Long;
    public @string Example;
    public slice<Completion> ValidArgs;
    public CompletionFunc ValidArgsFunction;
    public Func<ж<Command>, slice<@string>, error> Args;
    public slice<@string> ArgAliases;
    public @string BashCompletionFunction;
    public @string Deprecated;
    public map<@string, @string> Annotations;
    public @string Version;
    public Action<ж<Command>, slice<@string>> PersistentPreRun;
    public Func<ж<Command>, slice<@string>, error> PersistentPreRunE;
    public Action<ж<Command>, slice<@string>> PreRun;
    public Func<ж<Command>, slice<@string>, error> PreRunE;
    public Action<ж<Command>, slice<@string>> Run;
    public Func<ж<Command>, slice<@string>, error> RunE;
    public Action<ж<Command>, slice<@string>> PostRun;
    public Func<ж<Command>, slice<@string>, error> PostRunE;
    public Action<ж<Command>, slice<@string>> PersistentPostRun;
    public Func<ж<Command>, slice<@string>, error> PersistentPostRunE;
    internal slice<ж<Group>> commandgroups;
    internal slice<@string> args;
    internal ж<bytes.Buffer> flagErrorBuf;
    internal ж<flag.FlagSet> flags;
    internal ж<flag.FlagSet> pflags;
    internal ж<flag.FlagSet> lflags;
    internal ж<flag.FlagSet> iflags;
    internal ж<flag.FlagSet> parentsPflags;
    internal Func<ж<flag.FlagSet>, @string, flag.NormalizedName> globNormFunc;
    internal Func<ж<Command>, error> usageFunc;
    internal ж<tmplFunc> usageTemplate;
    internal Func<ж<Command>, error, error> flagErrorFunc;
    internal ж<tmplFunc> helpTemplate;
    internal Action<ж<Command>, slice<@string>> helpFunc;
    internal ж<Command> helpCommand;
    internal @string helpCommandGroupID;
    internal @string completionCommandGroupID;
    internal ж<tmplFunc> versionTemplate;
    internal @string errPrefix;
    internal io.Reader inReader;
    internal io.Writer outWriter;
    internal io.Writer errWriter;
    public FParseErrWhitelist FParseErrWhitelist;
    public CompletionOptions CompletionOptions;
    internal bool commandsAreSorted;
    internal Command_commandCalledAs commandCalledAs;
    internal context.Context ctx;
    internal slice<ж<Command>> commands;
    internal ж<Command> parent;
    internal nint commandsMaxUseLen;
    internal nint commandsMaxCommandPathLen;
    internal nint commandsMaxNameLen;
    public bool TraverseChildren;
    public bool Hidden;
    public bool SilenceErrors;
    public bool SilenceUsage;
    public bool DisableFlagParsing;
    public bool DisableAutoGenTag;
    public bool DisableFlagsInUseLine;
    public bool DisableSuggestions;
    public nint SuggestionsMinimumDistance;
}

public static context.Context Context(this ref Command c) {
    return c.ctx;
}

public static void SetContext(this ref Command c, context.Context ctx) {
    c.ctx = ctx;
}

public static void SetArgs(this ref Command c, slice<@string> a) {
    c.args = a;
}

public static void SetOutput(this ref Command c, io.Writer output) {
    c.outWriter = output;
    c.errWriter = output;
}

public static void SetOut(this ref Command c, io.Writer newOut) {
    c.outWriter = newOut;
}

public static void SetErr(this ref Command c, io.Writer newErr) {
    c.errWriter = newErr;
}

public static void SetIn(this ref Command c, io.Reader newIn) {
    c.inReader = newIn;
}

public static void SetUsageFunc(this ref Command c, Func<ж<Command>, error> f) {
    c.usageFunc = f;
}

public static void SetUsageTemplate(this ref Command c, @string s) {
    if (s == ""u8) {
        c.usageTemplate = default!;
        return;
    }
    c.usageTemplate = tmpl(s);
}

public static void SetFlagErrorFunc(this ref Command c, Func<ж<Command>, error, error> f) {
    c.flagErrorFunc = f;
}

public static void SetHelpFunc(this ref Command c, Action<ж<Command>, slice<@string>> f) {
    c.helpFunc = f;
}

public static void SetHelpCommand(this ref Command c, ж<Command> Ꮡcmd) {
    c.helpCommand = Ꮡcmd;
}

public static void SetHelpCommandGroupID(this ref Command c, @string groupID) {
    if (c.helpCommand != nil) {
        c.helpCommand.Value.GroupID = groupID;
    }
    c.helpCommandGroupID = groupID;
}

public static void SetCompletionCommandGroupID(this ж<Command> Ꮡc, @string groupID) {
    Ꮡc.Root().Value.completionCommandGroupID = groupID;
}

public static void SetHelpTemplate(this ref Command c, @string s) {
    if (s == ""u8) {
        c.helpTemplate = default!;
        return;
    }
    c.helpTemplate = tmpl(s);
}

public static void SetVersionTemplate(this ref Command c, @string s) {
    if (s == ""u8) {
        c.versionTemplate = default!;
        return;
    }
    c.versionTemplate = tmpl(s);
}

public static void SetErrPrefix(this ref Command c, @string s) {
    c.errPrefix = s;
}

public static void SetGlobalNormalizationFunc(this ref Command c, Func<ж<flag.FlagSet>, @string, flag.NormalizedName> n) {
    c.Flags().SetNormalizeFunc(n);
    c.PersistentFlags().SetNormalizeFunc(n);
    c.globNormFunc = n;

    foreach (var (_, command) in c.commands) {
        command.SetGlobalNormalizationFunc(n);
    }
}

public static io.Writer OutOrStdout(this ref Command c) {
    return c.getOut(new os.FileжWriter(os.Stdout));
}

public static io.Writer OutOrStderr(this ref Command c) {
    return c.getOut(new os.FileжWriter(os.Stderr));
}

public static io.Writer ErrOrStderr(this ref Command c) {
    return c.getErr(new os.FileжWriter(os.Stderr));
}

public static io.Reader InOrStdin(this ref Command c) {
    return c.getIn(new os_FileжReader(os.Stdin));
}

internal static io.Writer getOut(this ref Command c, io.Writer def) {
    if (c.outWriter != default!) {
        return c.outWriter;
    }
    if (c.HasParent()) {
        return c.parent.getOut(def);
    }
    return def;
}

internal static io.Writer getErr(this ref Command c, io.Writer def) {
    if (c.errWriter != default!) {
        return c.errWriter;
    }
    if (c.HasParent()) {
        return c.parent.getErr(def);
    }
    return def;
}

internal static io.Reader getIn(this ref Command c, io.Reader def) {
    if (c.inReader != default!) {
        return c.inReader;
    }
    if (c.HasParent()) {
        return c.parent.getIn(def);
    }
    return def;
}

public static Func<ж<Command>, error> /*f*/ UsageFunc(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.usageFunc != default!) {
        return c.usageFunc;
    }
    if (c.HasParent()) {
        return c.Parent().UsageFunc();
    }
    return (ж<Command> cΔ1) => {
        cΔ1.mergePersistentFlags();
        var fn = cΔ1.getUsageTemplateFunc();
        var err = fn(cΔ1.OutOrStderr(), cΔ1.OrTypedNil());
        if (err != default!) {
            cΔ1.PrintErrln(err);
        }
        return err;
    };
}

internal static Func<io.Writer, any, error> getUsageTemplateFunc(this ref Command c) {
    if (c.usageTemplate != nil) {
        return (~c.usageTemplate).fn;
    }

    if (c.HasParent()) {
        return c.parent.getUsageTemplateFunc();
    }
    return defaultUsageFunc;
}

public static error Usage(this ж<Command> Ꮡc) {
    return Ꮡc.UsageFunc()(Ꮡc);
}

public static Action<ж<Command>, slice<@string>> HelpFunc(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.helpFunc != default!) {
        return c.helpFunc;
    }
    if (c.HasParent()) {
        return c.Parent().HelpFunc();
    }
    return (ж<Command> cΔ1, slice<@string> a) => {
        cΔ1.mergePersistentFlags();
        var fn = cΔ1.getHelpTemplateFunc();
        var err = fn(cΔ1.OutOrStdout(), cΔ1.OrTypedNil());
        if (err != default!) {
            cΔ1.PrintErrln(err);
        }
    };
}

internal static Func<io.Writer, any, error> getHelpTemplateFunc(this ref Command c) {
    if (c.helpTemplate != nil) {
        return (~c.helpTemplate).fn;
    }

    if (c.HasParent()) {
        return c.parent.getHelpTemplateFunc();
    }

    return defaultHelpFunc;
}

public static error Help(this ж<Command> Ꮡc) {
    Ꮡc.HelpFunc()(Ꮡc, new @string[]{}.slice());
    return default!;
}

public static @string UsageString(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var tmpOutput = c.outWriter;
    var tmpErr = c.errWriter;

    var bb = @new<bytes.Buffer>();
    c.outWriter = new bytes_BufferжWriter(bb);
    c.errWriter = new bytes_BufferжWriter(bb);

    CheckErr(Ꮡc.Usage());

    c.outWriter = tmpOutput;
    c.errWriter = tmpErr;

    return bb.String();
}

public static Func<ж<Command>, error, error> /*f*/ FlagErrorFunc(this ref Command c) {
    if (c.flagErrorFunc != default!) {
        return c.flagErrorFunc;
    }

    if (c.HasParent()) {
        return c.parent.FlagErrorFunc();
    }
    return (ж<Command> cΔ1, error err) => err;
}

internal static UntypedInt minUsagePadding => 25;

public static nint UsagePadding(this ref Command c) {
    if (c.parent == nil || minUsagePadding > (~c.parent).commandsMaxUseLen) {
        return minUsagePadding;
    }
    return (~c.parent).commandsMaxUseLen;
}

internal static UntypedInt minCommandPathPadding => 11;

public static nint CommandPathPadding(this ref Command c) {
    if (c.parent == nil || minCommandPathPadding > (~c.parent).commandsMaxCommandPathLen) {
        return minCommandPathPadding;
    }
    return (~c.parent).commandsMaxCommandPathLen;
}

internal static UntypedInt minNamePadding => 11;

public static nint NamePadding(this ref Command c) {
    if (c.parent == nil || minNamePadding > (~c.parent).commandsMaxNameLen) {
        return minNamePadding;
    }
    return (~c.parent).commandsMaxNameLen;
}

public static @string UsageTemplate(this ref Command c) {
    if (c.usageTemplate != nil) {
        return (~c.usageTemplate).tmpl;
    }

    if (c.HasParent()) {
        return c.parent.UsageTemplate();
    }
    return defaultUsageTemplate;
}

public static @string HelpTemplate(this ref Command c) {
    if (c.helpTemplate != nil) {
        return (~c.helpTemplate).tmpl;
    }

    if (c.HasParent()) {
        return c.parent.HelpTemplate();
    }
    return defaultHelpTemplate;
}

public static @string VersionTemplate(this ref Command c) {
    if (c.versionTemplate != nil) {
        return (~c.versionTemplate).tmpl;
    }

    if (c.HasParent()) {
        return c.parent.VersionTemplate();
    }
    return defaultVersionTemplate;
}

internal static Func<io.Writer, any, error> getVersionTemplateFunc(this ref Command c) {
    if (c.versionTemplate != nil) {
        return (~c.versionTemplate).fn;
    }

    if (c.HasParent()) {
        return c.parent.getVersionTemplateFunc();
    }
    return defaultVersionFunc;
}

public static @string ErrPrefix(this ref Command c) {
    if (c.errPrefix != ""u8) {
        return c.errPrefix;
    }

    if (c.HasParent()) {
        return c.parent.ErrPrefix();
    }
    return "Error:";
}

internal static bool hasNoOptDefVal(@string name, ж<flag.FlagSet> Ꮡfs) {
    var flagΔ1 = Ꮡfs.Lookup(name);
    if (flagΔ1 == nil) {
        return false;
    }
    return (~flagΔ1).NoOptDefVal != ""u8;
}

internal static bool shortHasNoOptDefVal(@string name, ж<flag.FlagSet> Ꮡfs) {
    ref var fs = ref Ꮡfs.DerefOrNull();

    if (len(name) == 0) {
        return false;
    }

    var flagΔ1 = fs.ShorthandLookup(name[..1]);
    if (flagΔ1 == nil) {
        return false;
    }
    return (~flagΔ1).NoOptDefVal != ""u8;
}

internal static slice<@string> stripFlags(slice<@string> args, ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();
    var commandsᴷ = new stackAppend<@string>(moves: true);

    if (len(args) == 0) {
        return args;
    }
    Ꮡc.mergePersistentFlags();

    var commands = new @string[]{}.slice();
    var flags = c.Flags();

Loop:
    while (len(args) > 0) {
        @string s = args[0];
        args = args[1..];
        var matchᴛ1 = false;
        if (s == "--"u8) { matchᴛ1 = true;
            goto break_Loop;
        }
        else if (strings.HasPrefix(s, "--") && !strings.Contains(s, "=") && !hasNoOptDefVal(s[2..], flags)) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (strings.HasPrefix(s, "-") && !strings.Contains(s, "=") && len(s) == 2 && !shortHasNoOptDefVal(s[1..], flags))) {
            if (len(args) <= 1){
                goto break_Loop;
            } else {
                args = args[1..];
                continue;
            }
        }
        else if (s != ""u8 && !strings.HasPrefix(s, "-")) { matchᴛ1 = true;
            commands = append(commandsᴷ, commands, s);
        }

continue_Loop:;
    }
break_Loop:;

    commands = move2heap(commandsᴷ, commands);
    return commands;
}

internal static slice<@string> argsMinusFirstX(this ж<Command> Ꮡc, slice<@string> args, @string x) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(args) == 0) {
        return args;
    }
    Ꮡc.mergePersistentFlags();
    var flags = c.Flags();

Loop:
    for (nint pos = 0; pos < len(args); pos++) {
        @string s = args[pos];
        var matchᴛ1 = false;
        if (s == "--"u8) { matchᴛ1 = true;
            goto break_Loop;
        }
        else if (strings.HasPrefix(s, "--") && !strings.Contains(s, "=") && !hasNoOptDefVal(s[2..], flags)) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && (strings.HasPrefix(s, "-") && !strings.Contains(s, "=") && len(s) == 2 && !shortHasNoOptDefVal(s[1..], flags))) {
            pos++;
            continue;
        }
        else if (!strings.HasPrefix(s, "-")) { matchᴛ1 = true;
            if (s == x) {
                var ret = new slice<@string>(0, len(args) - 1);
                ret = appendꓸꓸꓸ(ret, args.slice(0, pos));
                ret = appendꓸꓸꓸ(ret, args.slice(pos + 1));
                return ret;
            }
        }

continue_Loop:;
    }
break_Loop:;
    return args;
}

internal static bool isFlagArg(@string arg) {
    return ((len(arg) >= 3 && arg[0..2] == "--") || (len(arg) >= 2 && arg[0] == (rune)'-' && arg[1] != (rune)'-'));
}

public static (ж<Command>, slice<@string>, error) Find(this ж<Command> Ꮡc, slice<@string> args) {
    ref var innerfind = ref heap<Func<ж<Command>, slice<@string>, (ж<Command>, slice<@string>)>>(out var Ꮡinnerfind);

    innerfind = (ж<Command> cΔ1, slice<@string> innerArgs) => {
        var argsWOflags = stripFlags(innerArgs, cΔ1);
        if (len(argsWOflags) == 0) {
            return (cΔ1, innerArgs);
        }
        @string nextSubCmd = argsWOflags[0];

        var cmd = cΔ1.findNext(nextSubCmd);
        if (cmd != nil) {
            return Ꮡinnerfind.ValueSlot(cmd, cΔ1.argsMinusFirstX(innerArgs, nextSubCmd));
        }
        return (cΔ1, innerArgs);
    };

    var (commandFound, a) = innerfind(Ꮡc, args);
    if ((~commandFound).Args == default!) {
        return (commandFound, a, legacyArgs(commandFound, stripFlags(a, commandFound)));
    }
    return (commandFound, a, default!);
}

internal static @string findSuggestions(this ref Command c, @string arg) {
    if (c.DisableSuggestions) {
        return "";
    }
    if (c.SuggestionsMinimumDistance <= 0) {
        c.SuggestionsMinimumDistance = 2;
    }
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    {
        var suggestions = c.SuggestionsFor(arg); if (len(suggestions) > 0) {
            Ꮡsb.WriteString("\n\nDid you mean this?\n");
            foreach (var (_, s) in suggestions) {
                (_, _) = fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "\t%v\n"u8, s);
            }
        }
    }
    return sb.String();
}

internal static ж<Command> findNext(this ref Command c, @string next) {
    var matchesᴷ = new stackAppend<ж<Command>>();

    var matches = new slice<ж<Command>>(0);
    foreach (var (_, cmd) in c.commands) {
        if (commandNameMatches(cmd.Name(), next) || cmd.HasAlias(next)) {
            cmd.Value.commandCalledAs.name = next;
            return cmd;
        }
        if (EnablePrefixMatching && cmd.hasNameOrAliasPrefix(next)) {
            matches = append(matchesᴷ, matches, cmd);
        }
    }

    if (len(matches) == 1) {
        return matches[0];
    }

    return default!;
}

public static (ж<Command>, slice<@string>, error) Traverse(this ж<Command> Ꮡc, slice<@string> args) {
    ref var c = ref Ꮡc.DerefOrNull();
    var flagsᴷ = new stackAppend<@string>();

    var flags = new @string[]{}.slice();
    var inFlag = false;

    foreach (var (i, arg) in args) {
        switch (ᐧ) {
        case {} when strings.HasPrefix(arg, "--") && !strings.Contains(arg, "="): {
            inFlag = !hasNoOptDefVal(arg[2..], c.Flags());
            flags = append(flagsᴷ, flags, arg);
            continue;
            break;
        }
        case {} when strings.HasPrefix(arg, "-") && !strings.Contains(arg, "=") && len(arg) == 2 && !shortHasNoOptDefVal(arg[1..], c.Flags()): {
            inFlag = true;
            flags = append(flags, arg);
            continue;
            break;
        }
        case {} when inFlag: {
            inFlag = false;
            flags = append(flags, arg);
            continue;
            break;
        }
        case {} when isFlagArg(arg): {
            flags = append(flags, arg);
            continue;
            break;
        }}

        var cmd = c.findNext(arg);
        if (cmd == nil) {
            return (Ꮡc, args, default!);
        }

        {
            var err = Ꮡc.ParseFlags(flags); if (err != default!) {
                return (default!, args, err);
            }
        }
        return cmd.Traverse(args.slice(i + 1));
    }
    return (Ꮡc, args, default!);
}

public static slice<@string> SuggestionsFor(this ref Command c, @string typedName) {
    var suggestions = new @string[]{}.slice();
    foreach (var (_, cmd) in c.commands) {
        if (cmd.IsAvailableCommand()) {
            nint levenshteinDistance = ld(typedName, cmd.Name(), true);
            var suggestByLevenshtein = levenshteinDistance <= c.SuggestionsMinimumDistance;
            var suggestByPrefix = strings.HasPrefix(strings.ToLower(cmd.Name()), strings.ToLower(typedName));
            if (suggestByLevenshtein || suggestByPrefix) {
                suggestions = append(suggestions, cmd.Name());
            }
            foreach (var (_, explicitSuggestion) in (~cmd).SuggestFor) {
                if (strings.EqualFold(typedName, explicitSuggestion)) {
                    suggestions = append(suggestions, cmd.Name());
                }
            }
        }
    }
    return suggestions;
}

public static void VisitParents(this ref Command c, Action<ж<Command>> fn) {
    if (c.HasParent()) {
        fn(c.Parent());
        c.Parent().VisitParents(fn);
    }
}

public static ж<Command> Root(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.HasParent()) {
        return c.Parent().Root();
    }
    return Ꮡc;
}

public static nint ArgsLenAtDash(this ref Command c) {
    return c.Flags().ArgsLenAtDash();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helpFlagDeclaredAsNonˢ = (@string)"\"help\" flag declared as non-bool. Please correct your code"u8;
internal static readonly object versionFlagDeclaredAsNonˢ = (@string)"\"version\" flag declared as non-bool. Please correct your code"u8;

internal static error /*err*/ execute(this ж<Command> Ꮡc, slice<@string> a) {
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();
        var parentsᴷ = new stackAppend<ж<Command>>();

        if (Ꮡc == nil) {
            err = fmt.Errorf("called Execute() on a nil Command"u8); goto ᒐdone;
        }

        if (len(c.Deprecated) > 0) {
            c.Printf("Command %q is deprecated, %s\n"u8, c.Name(), c.Deprecated);
        }

        Ꮡc.InitDefaultHelpFlag();
        Ꮡc.InitDefaultVersionFlag();

        err = Ꮡc.ParseFlags(a);
        if (err != default!) {
            err = c.FlagErrorFunc()(Ꮡc, err); goto ᒐdone;
        }

        (var helpVal, err) = c.Flags().GetBool(helpFlagName);
        if (err != default!) {
            c.Println(helpFlagDeclaredAsNonˢ);
            goto ᒐdone;
        }

        if (helpVal) {
            err = flag.ErrHelp; goto ᒐdone;
        }

        if (c.Version != ""u8) {
            var (versionVal, errΔ1) = c.Flags().GetBool("version");
            if (errΔ1 != default!) {
                c.Println(versionFlagDeclaredAsNonˢ);
                err = errΔ1; goto ᒐdone;
            }
            if (versionVal) {
                var fn = c.getVersionTemplateFunc();
                var errΔ2 = fn(c.OutOrStdout(), Ꮡc.OrTypedNil());
                if (errΔ2 != default!) {
                    c.Println(errΔ2);
                }
                err = errΔ2; goto ᒐdone;
            }
        }

        if (!c.Runnable()) {
            err = flag.ErrHelp; goto ᒐdone;
        }

        c.preRun();

        defer(Ꮡc.postRun, ref ᒐ);

        var argWoFlags = c.Flags().Args();
        if (c.DisableFlagParsing) {
            argWoFlags = a;
        }

        {
            var errΔ3 = Ꮡc.ValidateArgs(argWoFlags); if (errΔ3 != default!) {
                err = errΔ3; goto ᒐdone;
            }
        }

        var parents = new slice<ж<Command>>(0, 5);
        for (var p = Ꮡc; p != nil; p = p.Parent()) {
            if (EnableTraverseRunHooks){
                parents = appendꓸꓸꓸ(new ж<Command>[]{p}.slice(), parents);
            } else {
                parents = append(parentsᴷ, parents, p);
            }
        }
        foreach (var (_, p) in parents) {
            if ((~p).PersistentPreRunE != default!){
                {
                    var errΔ4 = (~p).PersistentPreRunE(Ꮡc, argWoFlags); if (errΔ4 != default!) {
                        err = errΔ4; goto ᒐdone;
                    }
                }
                if (!EnableTraverseRunHooks) {
                    break;
                }
            } else 
            if ((~p).PersistentPreRun != default!) {
                (~p).PersistentPreRun(Ꮡc, argWoFlags);
                if (!EnableTraverseRunHooks) {
                    break;
                }
            }
        }
        if (c.PreRunE != default!){
            {
                var errΔ5 = c.PreRunE(Ꮡc, argWoFlags); if (errΔ5 != default!) {
                    err = errΔ5; goto ᒐdone;
                }
            }
        } else 
        if (c.PreRun != default!) {
            c.PreRun(Ꮡc, argWoFlags);
        }

        {
            var errΔ6 = c.ValidateRequiredFlags(); if (errΔ6 != default!) {
                err = errΔ6; goto ᒐdone;
            }
        }
        {
            var errΔ7 = c.ValidateFlagGroups(); if (errΔ7 != default!) {
                err = errΔ7; goto ᒐdone;
            }
        }

        if (c.RunE != default!){
            {
                var errΔ8 = c.RunE(Ꮡc, argWoFlags); if (errΔ8 != default!) {
                    err = errΔ8; goto ᒐdone;
                }
            }
        } else {
            c.Run(Ꮡc, argWoFlags);
        }
        if (c.PostRunE != default!){
            {
                var errΔ9 = c.PostRunE(Ꮡc, argWoFlags); if (errΔ9 != default!) {
                    err = errΔ9; goto ᒐdone;
                }
            }
        } else 
        if (c.PostRun != default!) {
            c.PostRun(Ꮡc, argWoFlags);
        }
        for (var p = Ꮡc; p != nil; p = p.Parent()) {
            if ((~p).PersistentPostRunE != default!){
                {
                    var errΔ10 = (~p).PersistentPostRunE(Ꮡc, argWoFlags); if (errΔ10 != default!) {
                        err = errΔ10; goto ᒐdone;
                    }
                }
                if (!EnableTraverseRunHooks) {
                    break;
                }
            } else 
            if ((~p).PersistentPostRun != default!) {
                (~p).PersistentPostRun(Ꮡc, argWoFlags);
                if (!EnableTraverseRunHooks) {
                    break;
                }
            }
        }

        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return err;
}

internal static void preRun(this ref Command c) {
    foreach (var (_, x) in initializers) {
        x();
    }
}

internal static void postRun(this ref Command c) {
    foreach (var (_, x) in finalizers) {
        x();
    }
}

public static error ExecuteContext(this ж<Command> Ꮡc, context.Context ctx) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.ctx = ctx;
    return Ꮡc.Execute();
}

public static error Execute(this ж<Command> Ꮡc) {
    var (_, err) = Ꮡc.ExecuteC();
    return err;
}

public static (ж<Command>, error) ExecuteContextC(this ж<Command> Ꮡc, context.Context ctx) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.ctx = ctx;
    return Ꮡc.ExecuteC();
}

public static (ж<Command> cmd, error err) ExecuteC(this ж<Command> Ꮡc) {
    ж<Command> cmd = default!;
    error err = default!;
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.ctx == default!) {
        c.ctx = context.Background();
    }

    if (c.HasParent()) {
        return Ꮡc.Root().ExecuteC();
    }

    if (preExecHookFn != default!) {
        preExecHookFn(Ꮡc);
    }

    Ꮡc.InitDefaultHelpCmd();

    var args = c.args;

    if (c.args == default! && filepath.Base(os.Args[0]) != "cobra.test"u8) {
        args = os.Args[1..];
    }

    Ꮡc.initCompleteCmd(args);

    Ꮡc.InitDefaultCompletionCmd(args.ꓸꓸꓸ);

    c.checkCommandGroups();

    slice<@string> flags = default!;
    if (c.TraverseChildren){
        (cmd, flags, err) = Ꮡc.Traverse(args);
    } else {
        (cmd, flags, err) = Ꮡc.Find(args);
    }
    if (err != default!) {
        if (cmd != nil) {
            Ꮡc = cmd; c = ref Ꮡc.DerefOrNull();
        }
        if (!c.SilenceErrors) {
            c.PrintErrln(c.ErrPrefix(), err.Error());
            c.PrintErrf("Run '%v --help' for usage.\n"u8, c.CommandPath());
        }
        return (Ꮡc, err);
    }

    cmd.Value.commandCalledAs.called = true;
    if ((~cmd).commandCalledAs.name == ""u8) {
        cmd.Value.commandCalledAs.name = cmd.Name();
    }

    if ((~cmd).ctx == default!) {
        cmd.Value.ctx = c.ctx;
    }

    err = cmd.execute(flags);
    if (err != default!) {
        if (errors.Is(err, flag.ErrHelp)) {
            cmd.HelpFunc()(cmd, args);
            return (cmd, default!);
        }

        if (!(~cmd).SilenceErrors && !c.SilenceErrors) {
            c.PrintErrln(cmd.ErrPrefix(), err.Error());
        }

        if (!(~cmd).SilenceUsage && !c.SilenceUsage) {
            c.Println(cmd.UsageString());
        }
    }
    return (cmd, err);
}

public static error ValidateArgs(this ж<Command> Ꮡc, slice<@string> args) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.Args == default!) {
        return ArbitraryArgs(Ꮡc, args);
    }
    return c.Args(Ꮡc, args);
}

public static error ValidateRequiredFlags(this ref Command c) {
    if (c.DisableFlagParsing) {
        return default!;
    }

    var flags = c.Flags();
    ref var missingFlagNames = ref heap<slice<@string>>(out var ᏑmissingFlagNames);
    missingFlagNames = new @string[]{}.slice();
    flags.VisitAll((ж<flag.Flag> pflag) => {
        var (requiredAnnotation, found) = (~pflag).Annotations[BashCompOneRequiredFlag, ꟷ];
        if (!found) {
            return;
        }
        if ((requiredAnnotation[0] == "true") && !(~pflag).Changed) {
            ᏑmissingFlagNames.ValueSlot = append(ᏑmissingFlagNames.ValueSlot, (~pflag).Name);
        }
    });

    if (len(missingFlagNames) > 0) {
        return fmt.Errorf(@"required flag(s) ""%s"" not set"u8, strings.Join(missingFlagNames, @""", """));
    }
    return default!;
}

internal static void checkCommandGroups(this ref Command c) {
    foreach (var (_, sub) in c.commands) {
        if ((~sub).GroupID != ""u8 && !c.ContainsGroup((~sub).GroupID)) {
            throw panic(fmt.Sprintf("group id '%s' is not defined for subcommand '%s'"u8, (~sub).GroupID, sub.CommandPath()));
        }

        sub.checkCommandGroups();
    }
}

public static void InitDefaultHelpFlag(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.mergePersistentFlags();
    if (c.Flags().Lookup(helpFlagName) == nil) {
        @string usage = "help for ";
        @string name = c.DisplayName();
        if (name == ""u8){
            usage += "this command"u8;
        } else {
            usage += name;
        }
        c.Flags().BoolP(helpFlagName, "h", false, usage);
        _ = c.Flags().SetAnnotation(helpFlagName, FlagSetByCobraAnnotation, new @string[]{"true"}.slice());
    }
}

public static void InitDefaultVersionFlag(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.Version == ""u8) {
        return;
    }

    Ꮡc.mergePersistentFlags();
    if (c.Flags().Lookup("version") == nil) {
        @string usage = "version for ";
        if (c.Name() == ""u8){
            usage += "this command"u8;
        } else {
            usage += c.DisplayName();
        }
        if (c.Flags().ShorthandLookup("v") == nil){
            c.Flags().BoolP("version", "v", false, usage);
        } else {
            c.Flags().Bool("version", false, usage);
        }
        _ = c.Flags().SetAnnotation("version", FlagSetByCobraAnnotation, new @string[]{"true"}.slice());
    }
}

public static void InitDefaultHelpCmd(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (!c.HasSubCommands()) {
        return;
    }

    if (c.helpCommand == nil) {
        c.helpCommand = Ꮡ(new Command(
            Use: "help [command]",
            Short: "Help about any command",
            Long: """
Help provides help for any command in the application.
Simply type 
"""u8.Raw() + c.DisplayName() + @" help [path to command] for full details."u8,
            ValidArgsFunction: (ж<Command> cΔ1, slice<@string> args, @string toComplete) => {
                var completionsᴷ = new stackAppend<Completion>(moves: true);
                slice<Completion> completions = default!;
                var (cmd, _, e) = cΔ1.Root().Find(args);
                if (e != default!) {
                    return (default!, ShellCompDirectiveNoFileComp);
                }
                if (cmd == nil) {
                    cmd = cΔ1.Root();
                }
                foreach (var (_, subCmd) in cmd.Commands()) {
                    if (subCmd.IsAvailableCommand() || subCmd == (~cmd).helpCommand) {
                        if (strings.HasPrefix(subCmd.Name(), toComplete)) {
                            completions = append(completionsᴷ, completions, CompletionWithDesc(subCmd.Name(), (~subCmd).Short));
                        }
                    }
                }
                completions = move2heap(completionsᴷ, completions);
                return (completions, ShellCompDirectiveNoFileComp);
            },
            Run: (ж<Command> cΔ2, slice<@string> args) => {
                var (cmd, _, e) = cΔ2.Root().Find(args);
                if (cmd == nil || e != default!){
                    cΔ2.Printf("Unknown help topic %#q\n"u8, args);
                    CheckErr(cΔ2.Root().Usage());
                } else {
                    if ((~cmd).ctx == default!) {
                        cmd.Value.ctx = cΔ2.Value.ctx;
                    }

                    cmd.InitDefaultHelpFlag();
                    cmd.InitDefaultVersionFlag();
                    CheckErr(cmd.Help());
                }
            },
            GroupID: c.helpCommandGroupID
        ));
    }
    c.RemoveCommand(c.helpCommand);
    Ꮡc.AddCommand(c.helpCommand);
}

public static void ResetCommands(this ref Command c) {
    c.parent = default!;
    c.commands = default!;
    c.helpCommand = default!;
    c.parentsPflags = default!;
}

partial struct commandSorterByName /*[]ж<Command>*/;

internal static nint Len(this commandSorterByName c) {
    return len(c);
}

internal static void Swap(this commandSorterByName c, nint i, nint j) {
    (c[i], c[j]) = (c[j], c[i]);
}

internal static bool Less(this commandSorterByName c, nint i, nint j) {
    return c[i].Name() < c[j].Name();
}

public static slice<ж<Command>> Commands(this ref Command c) {
    if (EnableCommandSorting && !c.commandsAreSorted) {
        sort.Sort((commandSorterByName)c.commands);
        c.commandsAreSorted = true;
    }
    return c.commands;
}

public static void AddCommand(this ж<Command> Ꮡc, params ꓸꓸꓸжCommand cmdsʗp) {
    var cmds = cmdsʗp.sslice();
    ref var c = ref Ꮡc.DerefOrNull();

    foreach (var (i, x) in cmds) {
        if (cmds[i] == Ꮡc) {
            throw panic("Command can't be a child of itself");
        }
        cmds[i].Value.parent = Ꮡc;
        nint usageLen = len((~x).Use);
        if (usageLen > c.commandsMaxUseLen) {
            c.commandsMaxUseLen = usageLen;
        }
        nint commandPathLen = len(x.CommandPath());
        if (commandPathLen > c.commandsMaxCommandPathLen) {
            c.commandsMaxCommandPathLen = commandPathLen;
        }
        nint nameLen = len(x.Name());
        if (nameLen > c.commandsMaxNameLen) {
            c.commandsMaxNameLen = nameLen;
        }
        if (c.globNormFunc != default!) {
            x.SetGlobalNormalizationFunc(c.globNormFunc);
        }
        c.commands = append(c.commands, x);
        c.commandsAreSorted = false;
    }
}

public static slice<ж<Group>> Groups(this ref Command c) {
    return c.commandgroups;
}

public static bool AllChildCommandsHaveGroup(this ref Command c) {
    foreach (var (_, sub) in c.commands) {
        if ((sub.IsAvailableCommand() || sub == c.helpCommand) && (~sub).GroupID == ""u8) {
            return false;
        }
    }
    return true;
}

public static bool ContainsGroup(this ref Command c, @string groupID) {
    foreach (var (_, x) in c.commandgroups) {
        if ((~x).ID == groupID) {
            return true;
        }
    }
    return false;
}

public static void AddGroup(this ref Command c, params ꓸꓸꓸжGroup groupsʗp) {
    var groups = groupsʗp.sslice();

    c.commandgroups = appendꓸꓸꓸ(c.commandgroups, groups);
}

public static void RemoveCommand(this ref Command c, params ꓸꓸꓸжCommand cmdsʗp) {
    var cmds = cmdsʗp.sslice();
    var commandsᴷ = new stackAppend<ж<Command>>(moves: true);

    var commands = new ж<Command>[]{}.slice();
main:
    foreach (var (_, command) in c.commands) {
        foreach (var (_, cmd) in cmds) {
            if (command == cmd) {
                command.Value.parent = default!;
                goto continue_main;
            }
        }
        commands = append(commandsᴷ, commands, command);
continue_main:;
    }
break_main:;
    commands = move2heap(commandsᴷ, commands);
    c.commands = commands;
    c.commandsMaxUseLen = 0;
    c.commandsMaxCommandPathLen = 0;
    c.commandsMaxNameLen = 0;
    foreach (var (_, command) in c.commands) {
        nint usageLen = len((~command).Use);
        if (usageLen > c.commandsMaxUseLen) {
            c.commandsMaxUseLen = usageLen;
        }
        nint commandPathLen = len(command.CommandPath());
        if (commandPathLen > c.commandsMaxCommandPathLen) {
            c.commandsMaxCommandPathLen = commandPathLen;
        }
        nint nameLen = len(command.Name());
        if (nameLen > c.commandsMaxNameLen) {
            c.commandsMaxNameLen = nameLen;
        }
    }
}

public static void Print(this ref Command c, params ꓸꓸꓸany iʗp) {
    var i = iʗp.sslice();

    fmt.Fprint(c.OutOrStderr(), i.ꓸꓸꓸ);
}

public static void Println(this ref Command c, params ꓸꓸꓸany iʗp) {
    var i = iʗp.sslice();

    c.Print(fmt.Sprintln(i.ꓸꓸꓸ));
}

public static void Printf(this ref Command c, @string format, params ꓸꓸꓸany iʗp) {
    var i = iʗp.sslice();

    c.Print(fmt.Sprintf(format, i.ꓸꓸꓸ));
}

public static void PrintErr(this ref Command c, params ꓸꓸꓸany iʗp) {
    var i = iʗp.sslice();

    fmt.Fprint(c.ErrOrStderr(), i.ꓸꓸꓸ);
}

public static void PrintErrln(this ref Command c, params ꓸꓸꓸany iʗp) {
    var i = iʗp.sslice();

    c.PrintErr(fmt.Sprintln(i.ꓸꓸꓸ));
}

public static void PrintErrf(this ref Command c, @string format, params ꓸꓸꓸany iʗp) {
    var i = iʗp.sslice();

    c.PrintErr(fmt.Sprintf(format, i.ꓸꓸꓸ));
}

public static @string CommandPath(this ref Command c) {
    if (c.HasParent()) {
        return c.Parent().CommandPath() + " "u8 + c.Name();
    }
    return c.DisplayName();
}

public static @string DisplayName(this ref Command c) {
    {
        var (displayName, ok) = c.Annotations[CommandDisplayNameAnnotation, ꟷ]; if (ok) {
            return displayName;
        }
    }
    return c.Name();
}

public static @string UseLine(this ref Command c) {
    @string useline = default!;
    @string use = strings.Replace(c.Use, c.Name(), c.DisplayName(), 1);
    if (c.HasParent()){
        useline = c.parent.CommandPath() + " "u8 + use;
    } else {
        useline = use;
    }
    if (c.DisableFlagsInUseLine) {
        return useline;
    }
    if (c.HasAvailableFlags() && !strings.Contains(useline, "[flags]")) {
        useline += " [flags]"u8;
    }
    return useline;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object debugFlagsCalledOnˢ = (@string)"DebugFlags called on"u8;

public static void DebugFlags(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    c.Println(debugFlagsCalledOnˢ, c.Name());
    ref var debugflags = ref heap<Action<ж<Command>>>(out var Ꮡdebugflags);

    debugflags = (ж<Command> x) => {
        if (x.HasFlags() || x.HasPersistentFlags()) {
            Ꮡc.Value.Println(x.Name());
        }
        if (x.HasFlags()) {
            (~x).flags.VisitAll((ж<flag.Flag> f) => {
                if (x.HasPersistentFlags() && x.persistentFlag((~f).Name) != nil){
                    Ꮡc.Value.Println("  -" + (~f).Shorthand + ",", "--" + (~f).Name, "[" + (~f).DefValue + "]", (@string)""u8, (~f).Value, (@string)"  [LP]"u8);
                } else {
                    Ꮡc.Value.Println("  -" + (~f).Shorthand + ",", "--" + (~f).Name, "[" + (~f).DefValue + "]", (@string)""u8, (~f).Value, (@string)"  [L]"u8);
                }
            });
        }
        if (x.HasPersistentFlags()) {
            (~x).pflags.VisitAll((ж<flag.Flag> f) => {
                if (x.HasFlags()){
                    if ((~x).flags.Lookup((~f).Name) == nil) {
                        Ꮡc.Value.Println("  -" + (~f).Shorthand + ",", "--" + (~f).Name, "[" + (~f).DefValue + "]", (@string)""u8, (~f).Value, (@string)"  [P]"u8);
                    }
                } else {
                    Ꮡc.Value.Println("  -" + (~f).Shorthand + ",", "--" + (~f).Name, "[" + (~f).DefValue + "]", (@string)""u8, (~f).Value, (@string)"  [P]"u8);
                }
            });
        }
        Ꮡc.Value.Println((~x).flagErrorBuf.OrTypedNil());
        if (x.HasSubCommands()) {
            foreach (var (_, y) in (~x).commands) {
                Ꮡdebugflags.ValueSlot(y);
            }
        }
    };

    debugflags(Ꮡc);
}

public static @string Name(this ref Command c) {
    @string name = c.Use;
    nint i = strings.Index(name, " ");
    if (i >= 0) {
        name = name.slice(0, i);
    }
    return name;
}

public static bool HasAlias(this ref Command c, @string s) {
    foreach (var (_, a) in c.Aliases) {
        if (commandNameMatches(a, s)) {
            return true;
        }
    }
    return false;
}

public static @string CalledAs(this ref Command c) {
    if (c.commandCalledAs.called) {
        return c.commandCalledAs.name;
    }
    return "";
}

internal static bool hasNameOrAliasPrefix(this ref Command c, @string prefix) {
    if (strings.HasPrefix(c.Name(), prefix)) {
        c.commandCalledAs.name = c.Name();
        return true;
    }
    foreach (var (_, alias) in c.Aliases) {
        if (strings.HasPrefix(alias, prefix)) {
            c.commandCalledAs.name = alias;
            return true;
        }
    }
    return false;
}

public static @string NameAndAliases(this ref Command c) {
    return strings.Join(appendꓸꓸꓸ(new @string[]{c.Name()}.slice(), c.Aliases), ", ");
}

public static bool HasExample(this ref Command c) {
    return len(c.Example) > 0;
}

public static bool Runnable(this ref Command c) {
    return c.Run != default! || c.RunE != default!;
}

public static bool HasSubCommands(this ref Command c) {
    return len(c.commands) > 0;
}

public static bool IsAvailableCommand(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(c.Deprecated) != 0 || c.Hidden) {
        return false;
    }

    if (c.HasParent() && (~c.Parent()).helpCommand == Ꮡc) {
        return false;
    }

    if (c.Runnable() || c.HasAvailableSubCommands()) {
        return true;
    }

    return false;
}

public static bool IsAdditionalHelpTopicCommand(this ref Command c) {
    if (c.Runnable() || len(c.Deprecated) != 0 || c.Hidden) {
        return false;
    }

    foreach (var (_, sub) in c.commands) {
        if (!sub.IsAdditionalHelpTopicCommand()) {
            return false;
        }
    }

    return true;
}

public static bool HasHelpSubCommands(this ref Command c) {
    foreach (var (_, sub) in c.commands) {
        if (sub.IsAdditionalHelpTopicCommand()) {
            return true;
        }
    }

    return false;
}

public static bool HasAvailableSubCommands(this ref Command c) {
    foreach (var (_, sub) in c.commands) {
        if (sub.IsAvailableCommand()) {
            return true;
        }
    }

    return false;
}

public static bool HasParent(this ref Command c) {
    return c.parent != nil;
}

public static Func<ж<flag.FlagSet>, @string, flag.NormalizedName> GlobalNormalizationFunc(this ref Command c) {
    return c.globNormFunc;
}

public static ж<flag.FlagSet> Flags(this ref Command c) {
    if (c.flags == nil) {
        c.flags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
        if (c.flagErrorBuf == nil) {
            c.flagErrorBuf = @new<bytes.Buffer>();
        }
        c.flags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));
    }

    return c.flags;
}

public static ж<flag.FlagSet> LocalNonPersistentFlags(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var persistentFlags = c.PersistentFlags();

    var @out = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
    var outʗ1 = @out;
    var persistentFlagsʗ1 = persistentFlags;
    Ꮡc.LocalFlags().VisitAll((ж<flag.Flag> f) => {
        if (persistentFlagsʗ1.Lookup((~f).Name) == nil) {
            outʗ1.AddFlag(f);
        }
    });
    return @out;
}

public static ж<flag.FlagSet> LocalFlags(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.mergePersistentFlags();

    if (c.lflags == nil) {
        c.lflags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
        if (c.flagErrorBuf == nil) {
            c.flagErrorBuf = @new<bytes.Buffer>();
        }
        c.lflags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));
    }
    c.lflags.Value.SortFlags = c.Flags().Value.SortFlags;
    if (c.globNormFunc != default!) {
        c.lflags.SetNormalizeFunc(c.globNormFunc);
    }

    var addToLocal = (ж<flag.Flag> f) => {
        if (Ꮡc.Value.lflags.Lookup((~f).Name) == nil && f != Ꮡc.Value.parentsPflags.Lookup((~f).Name)) {
            Ꮡc.Value.lflags.AddFlag(f);
        }
    };
    c.Flags().VisitAll(addToLocal);
    c.PersistentFlags().VisitAll(addToLocal);
    return c.lflags;
}

public static ж<flag.FlagSet> InheritedFlags(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.mergePersistentFlags();

    if (c.iflags == nil) {
        c.iflags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
        if (c.flagErrorBuf == nil) {
            c.flagErrorBuf = @new<bytes.Buffer>();
        }
        c.iflags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));
    }

    var local = Ꮡc.LocalFlags();
    if (c.globNormFunc != default!) {
        c.iflags.SetNormalizeFunc(c.globNormFunc);
    }

    var localʗ1 = local;
    c.parentsPflags.VisitAll((ж<flag.Flag> f) => {
        if (Ꮡc.Value.iflags.Lookup((~f).Name) == nil && localʗ1.Lookup((~f).Name) == nil) {
            Ꮡc.Value.iflags.AddFlag(f);
        }
    });
    return c.iflags;
}

public static ж<flag.FlagSet> NonInheritedFlags(this ж<Command> Ꮡc) {
    return Ꮡc.LocalFlags();
}

public static ж<flag.FlagSet> PersistentFlags(this ref Command c) {
    if (c.pflags == nil) {
        c.pflags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
        if (c.flagErrorBuf == nil) {
            c.flagErrorBuf = @new<bytes.Buffer>();
        }
        c.pflags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));
    }
    return c.pflags;
}

public static void ResetFlags(this ref Command c) {
    c.flagErrorBuf = @new<bytes.Buffer>();
    c.flagErrorBuf.Reset();
    c.flags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
    c.flags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));
    c.pflags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
    c.pflags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));

    c.lflags = default!;
    c.iflags = default!;
    c.parentsPflags = default!;
}

public static bool HasFlags(this ref Command c) {
    return c.Flags().HasFlags();
}

public static bool HasPersistentFlags(this ref Command c) {
    return c.PersistentFlags().HasFlags();
}

public static bool HasLocalFlags(this ж<Command> Ꮡc) {
    return Ꮡc.LocalFlags().HasFlags();
}

public static bool HasInheritedFlags(this ж<Command> Ꮡc) {
    return Ꮡc.InheritedFlags().HasFlags();
}

public static bool HasAvailableFlags(this ref Command c) {
    return c.Flags().HasAvailableFlags();
}

public static bool HasAvailablePersistentFlags(this ref Command c) {
    return c.PersistentFlags().HasAvailableFlags();
}

public static bool HasAvailableLocalFlags(this ж<Command> Ꮡc) {
    return Ꮡc.LocalFlags().HasAvailableFlags();
}

public static bool HasAvailableInheritedFlags(this ж<Command> Ꮡc) {
    return Ꮡc.InheritedFlags().HasAvailableFlags();
}

public static ж<flag.Flag> /*flag*/ Flag(this ж<Command> Ꮡc, @string name) {
    ж<flag.Flag> flagΔ1 = default!;
    ref var c = ref Ꮡc.DerefOrNull();

    flagΔ1 = c.Flags().Lookup(name);

    if (flagΔ1 == nil) {
        flagΔ1 = Ꮡc.persistentFlag(name);
    }

    return flagΔ1;
}

internal static ж<flag.Flag> /*flag*/ persistentFlag(this ж<Command> Ꮡc, @string name) {
    ж<flag.Flag> flagΔ1 = default!;
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.HasPersistentFlags()) {
        flagΔ1 = c.PersistentFlags().Lookup(name);
    }

    if (flagΔ1 == nil) {
        Ꮡc.updateParentsPflags();
        flagΔ1 = c.parentsPflags.Lookup(name);
    }
    return flagΔ1;
}

public static error ParseFlags(this ж<Command> Ꮡc, slice<@string> args) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.DisableFlagParsing) {
        return default!;
    }

    if (c.flagErrorBuf == nil) {
        c.flagErrorBuf = @new<bytes.Buffer>();
    }
    nint beforeErrorBufLen = c.flagErrorBuf.Len();
    Ꮡc.mergePersistentFlags();

    c.Flags().Value.ParseErrorsAllowlist = (flag.ParseErrorsAllowlist)c.FParseErrWhitelist;

    var err = c.Flags().Parse(args);
    if (c.flagErrorBuf.Len() - beforeErrorBufLen > 0 && err == default!) {
        c.Print(c.flagErrorBuf.String());
    }

    return err;
}

public static ж<Command> Parent(this ref Command c) {
    return c.parent;
}

internal static void mergePersistentFlags(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.updateParentsPflags();
    c.Flags().AddFlagSet(c.PersistentFlags());
    c.Flags().AddFlagSet(c.parentsPflags);
}

internal static void updateParentsPflags(this ж<Command> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.parentsPflags == nil) {
        c.parentsPflags = flag.NewFlagSet(c.DisplayName(), flag.ContinueOnError);
        c.parentsPflags.SetOutput(new bytes_BufferжWriter(c.flagErrorBuf));
        c.parentsPflags.Value.SortFlags = false;
    }

    if (c.globNormFunc != default!) {
        c.parentsPflags.SetNormalizeFunc(c.globNormFunc);
    }

    Ꮡc.Root().PersistentFlags().AddFlagSet(flag.CommandLine);

    c.VisitParents((ж<Command> parent) => {
        Ꮡc.Value.parentsPflags.AddFlagSet(parent.PersistentFlags());
    });
}

internal static bool commandNameMatches(@string s, @string t) {
    if (EnableCaseInsensitive) {
        return strings.EqualFold(s, t);
    }

    return s == t;
}

partial struct tmplFunc {
    internal @string tmpl;
    internal Func<io.Writer, any, error> fn;
}

internal static readonly @string defaultUsageTemplate = """
Usage:{{if .Runnable}}
  {{.UseLine}}{{end}}{{if .HasAvailableSubCommands}}
  {{.CommandPath}} [command]{{end}}{{if gt (len .Aliases) 0}}

Aliases:
  {{.NameAndAliases}}{{end}}{{if .HasExample}}

Examples:
{{.Example}}{{end}}{{if .HasAvailableSubCommands}}{{$cmds := .Commands}}{{if eq (len .Groups) 0}}

Available Commands:{{range $cmds}}{{if (or .IsAvailableCommand (eq .Name "help"))}}
  {{rpad .Name .NamePadding }} {{.Short}}{{end}}{{end}}{{else}}{{range $group := .Groups}}

{{.Title}}{{range $cmds}}{{if (and (eq .GroupID $group.ID) (or .IsAvailableCommand (eq .Name "help")))}}
  {{rpad .Name .NamePadding }} {{.Short}}{{end}}{{end}}{{end}}{{if not .AllChildCommandsHaveGroup}}

Additional Commands:{{range $cmds}}{{if (and (eq .GroupID "") (or .IsAvailableCommand (eq .Name "help")))}}
  {{rpad .Name .NamePadding }} {{.Short}}{{end}}{{end}}{{end}}{{end}}{{end}}{{if .HasAvailableLocalFlags}}

Flags:
{{.LocalFlags.FlagUsages | trimTrailingWhitespaces}}{{end}}{{if .HasAvailableInheritedFlags}}

Global Flags:
{{.InheritedFlags.FlagUsages | trimTrailingWhitespaces}}{{end}}{{if .HasHelpSubCommands}}

Additional help topics:{{range .Commands}}{{if .IsAdditionalHelpTopicCommand}}
  {{rpad .CommandPath .CommandPathPadding}} {{.Short}}{{end}}{{end}}{{end}}{{if .HasAvailableSubCommands}}

Use "{{.CommandPath}} [command] --help" for more information about a command.{{end}}

"""u8.Raw();

internal static error defaultUsageFunc(io.Writer w, any @in) {
    var c = @in._<ж<Command>>();
    fmt.Fprint(w, "Usage:");
    if (c.Runnable()) {
        fmt.Fprintf(w, "\n  %s"u8, c.UseLine());
    }
    if (c.HasAvailableSubCommands()) {
        fmt.Fprintf(w, "\n  %s [command]"u8, c.CommandPath());
    }
    if (len((~c).Aliases) > 0) {
        fmt.Fprintf(w, "\n\nAliases:\n"u8);
        fmt.Fprintf(w, "  %s"u8, c.NameAndAliases());
    }
    if (c.HasExample()) {
        fmt.Fprintf(w, "\n\nExamples:\n"u8);
        fmt.Fprintf(w, "%s"u8, (~c).Example);
    }
    if (c.HasAvailableSubCommands()) {
        var cmds = c.Commands();
        if (len(c.Groups()) == 0){
            fmt.Fprintf(w, "\n\nAvailable Commands:"u8);
            foreach (var (_, subcmd) in cmds) {
                if (subcmd.IsAvailableCommand() || subcmd.Name() == helpCommandName) {
                    fmt.Fprintf(w, "\n  %s %s"u8, rpad(subcmd.Name(), subcmd.NamePadding()), (~subcmd).Short);
                }
            }
        } else {
            foreach (var (_, group) in c.Groups()) {
                fmt.Fprintf(w, "\n\n%s"u8, (~group).Title);
                foreach (var (_, subcmd) in cmds) {
                    if ((~subcmd).GroupID == (~group).ID && (subcmd.IsAvailableCommand() || subcmd.Name() == helpCommandName)) {
                        fmt.Fprintf(w, "\n  %s %s"u8, rpad(subcmd.Name(), subcmd.NamePadding()), (~subcmd).Short);
                    }
                }
            }
            if (!c.AllChildCommandsHaveGroup()) {
                fmt.Fprintf(w, "\n\nAdditional Commands:"u8);
                foreach (var (_, subcmd) in cmds) {
                    if ((~subcmd).GroupID == ""u8 && (subcmd.IsAvailableCommand() || subcmd.Name() == helpCommandName)) {
                        fmt.Fprintf(w, "\n  %s %s"u8, rpad(subcmd.Name(), subcmd.NamePadding()), (~subcmd).Short);
                    }
                }
            }
        }
    }
    if (c.HasAvailableLocalFlags()) {
        fmt.Fprintf(w, "\n\nFlags:\n"u8);
        fmt.Fprint(w, trimRightSpace(c.LocalFlags().FlagUsages()));
    }
    if (c.HasAvailableInheritedFlags()) {
        fmt.Fprintf(w, "\n\nGlobal Flags:\n"u8);
        fmt.Fprint(w, trimRightSpace(c.InheritedFlags().FlagUsages()));
    }
    if (c.HasHelpSubCommands()) {
        fmt.Fprintf(w, "\n\nAdditional help topics:"u8);
        foreach (var (_, subcmd) in c.Commands()) {
            if (subcmd.IsAdditionalHelpTopicCommand()) {
                fmt.Fprintf(w, "\n  %s %s"u8, rpad(subcmd.CommandPath(), subcmd.CommandPathPadding()), (~subcmd).Short);
            }
        }
    }
    if (c.HasAvailableSubCommands()) {
        fmt.Fprintf(w, "\n\nUse \"%s [command] --help\" for more information about a command."u8, c.CommandPath());
    }
    fmt.Fprintln(w);
    return default!;
}

internal static readonly @string defaultHelpTemplate = """
{{with (or .Long .Short)}}{{. | trimTrailingWhitespaces}}

{{end}}{{if or .Runnable .HasSubCommands}}{{.UsageString}}{{end}}
"""u8.Raw();

internal static error defaultHelpFunc(io.Writer w, any @in) {
    var c = @in._<ж<Command>>();
    @string usage = c.Value.Long;
    if (usage == ""u8) {
        usage = c.Value.Short;
    }
    usage = trimRightSpace(usage);
    if (usage != ""u8) {
        fmt.Fprintln(w, usage);
        fmt.Fprintln(w);
    }
    if (c.Runnable() || c.HasSubCommands()) {
        fmt.Fprint(w, c.UsageString());
    }
    return default!;
}

internal static readonly @string defaultVersionTemplate = """
{{with .DisplayName}}{{printf "%s " .}}{{end}}{{printf "version %s" .Version}}

"""u8.Raw();

internal static error defaultVersionFunc(io.Writer w, any @in) {
    var c = @in._<ж<Command>>();
    var (_, err) = fmt.Fprintf(w, "%s version %s\n"u8, c.DisplayName(), (~c).Version);
    return err;
}

} // end cobra_package
