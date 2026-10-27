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

global using Completion = go.@string;
global using CompletionFunc = System.Func<go.ж<go.github.com.spf13.cobra_package.Command>, go.slice<go.@string>, go.@string, (go.slice<go.@string>, go.github.com.spf13.cobra_package.ShellCompDirective)>;

namespace go.github.com.spf13;

using fmt = fmt_package;
using os = os_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using pflag = go.github.com.spf13.pflag_package;
using fs = go.io.fs_package;
using go.github.com.spf13;
using io = io_package;
using ꓸꓸꓸstring = Span<@string>;

partial class cobra_package {

public static readonly @string ShellCompRequestCmd = "__complete"u8;
public static readonly @string ShellCompNoDescRequestCmd = "__completeNoDesc"u8;

internal static map<ж<pflag.Flag>, CompletionFunc> flagCompletionFunctions = new map<ж<pflag.Flag>, CompletionFunc>{};

internal static ж<sync.RWMutex> flagCompletionMutex = Ꮡ(new sync.RWMutex(nil));

partial struct ShellCompDirective /*num:nint*/;

partial struct flagCompError {
    internal @string subCommand;
    internal @string flagName;
}

internal static @string Error(this ref flagCompError e) {
    return "Subcommand '"u8 + e.subCommand + "' does not support flag '"u8 + e.flagName + "'"u8;
}

public static ShellCompDirective ShellCompDirectiveError => /* 1 << iota */ 1;
public static ShellCompDirective ShellCompDirectiveNoSpace => 2;
public static ShellCompDirective ShellCompDirectiveNoFileComp => 4;
public static ShellCompDirective ShellCompDirectiveFilterFileExt => 8;
public static ShellCompDirective ShellCompDirectiveFilterDirs => 16;
public static ShellCompDirective ShellCompDirectiveKeepOrder => 32;
internal static ShellCompDirective shellCompDirectiveMaxValue => 64;
public static ShellCompDirective ShellCompDirectiveDefault => 0;

internal static readonly @string compCmdName = "completion"u8;
internal static readonly @string compCmdNoDescFlagName = "no-descriptions"u8;
internal static readonly @string compCmdNoDescFlagDesc = "disable completion descriptions"u8;
internal const bool compCmdNoDescFlagDefault = false;

partial struct CompletionOptions {
    public bool DisableDefaultCmd;
    public bool DisableNoDescFlag;
    public bool DisableDescriptions;
    public bool HiddenDefaultCmd;
    public ж<ShellCompDirective> DefaultShellCompDirective;
}

public static void SetDefaultShellCompDirective(this ref CompletionOptions receiver, ShellCompDirective directiveʗp) {
    ref var directive = ref heap(directiveʗp, out var Ꮡdirective);

    receiver.DefaultShellCompDirective = Ꮡdirective;
}

public static Completion CompletionWithDesc(@string choice, @string description) {
    return choice + "\t"u8 + description;
}

public static (slice<Completion>, ShellCompDirective) NoFileCompletions(ж<Command> Ꮡcmd, slice<@string> args, @string toComplete) {
    return (default!, ShellCompDirectiveNoFileComp);
}

public static CompletionFunc FixedCompletions(slice<Completion> choices, ShellCompDirective directive) {
    var choicesʗ1 = choices;
    return (ж<Command> cmd, slice<@string> args, @string toComplete) => (choicesʗ1, directive);
}

public static error RegisterFlagCompletionFunc(this ж<Command> Ꮡc, @string flagName, CompletionFunc f) {
    GoFrame ᒐ = default;
    try {
        var flag = Ꮡc.Flag(flagName);
        if (flag == nil) {
            return fmt.Errorf("RegisterFlagCompletionFunc: flag '%s' does not exist"u8, flagName);
        }
        flagCompletionMutex.Lock();
        defer(flagCompletionMutex.Unlock, ref ᒐ);

        {
            var (_, exists) = flagCompletionFunctions[flag, ꟷ]; if (exists) {
                return fmt.Errorf("RegisterFlagCompletionFunc: flag '%s' already registered"u8, flagName);
            }
        }
        flagCompletionFunctions[flag] = f;
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static (CompletionFunc, bool) GetFlagCompletionFunc(this ж<Command> Ꮡc, @string flagName) {
    GoFrame ᒐ = default;
    try {
        var flag = Ꮡc.Flag(flagName);
        if (flag == nil) {
            return (default!, false);
        }

        flagCompletionMutex.RLock();
        defer(flagCompletionMutex.RUnlock, ref ᒐ);

        var (completionFunc, exists) = flagCompletionFunctions[flag, ꟷ];
        return (completionFunc, exists);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static @string @string(this ShellCompDirective d) {
    var directivesᴷ = new stackAppend<@string>();

    slice<@string> directives = default!;
    if ((ShellCompDirective)(d & ShellCompDirectiveError) != 0) {
        directives = append(directivesᴷ, directives, "ShellCompDirectiveError"u8);
    }
    if ((ShellCompDirective)(d & ShellCompDirectiveNoSpace) != 0) {
        directives = append(directives, "ShellCompDirectiveNoSpace"u8);
    }
    if ((ShellCompDirective)(d & ShellCompDirectiveNoFileComp) != 0) {
        directives = append(directives, "ShellCompDirectiveNoFileComp"u8);
    }
    if ((ShellCompDirective)(d & ShellCompDirectiveFilterFileExt) != 0) {
        directives = append(directives, "ShellCompDirectiveFilterFileExt"u8);
    }
    if ((ShellCompDirective)(d & ShellCompDirectiveFilterDirs) != 0) {
        directives = append(directives, "ShellCompDirectiveFilterDirs"u8);
    }
    if ((ShellCompDirective)(d & ShellCompDirectiveKeepOrder) != 0) {
        directives = append(directives, "ShellCompDirectiveKeepOrder"u8);
    }
    if (len(directives) == 0) {
        directives = append(directives, "ShellCompDirectiveDefault"u8);
    }

    if (d >= shellCompDirectiveMaxValue) {
        return fmt.Sprintf("ERROR: unexpected ShellCompDirective value: %d"u8, d);
    }
    return strings.Join(directives, ", ");
}

internal static void initCompleteCmd(this ж<Command> Ꮡc, slice<@string> args) {
    ref var c = ref Ꮡc.DerefOrNull();

    var completeCmd = Ꮡ(new Command(
        Use: fmt.Sprintf("%s [command-line]"u8, ShellCompRequestCmd),
        Aliases: new @string[]{ShellCompNoDescRequestCmd}.slice(),
        DisableFlagsInUseLine: true,
        Hidden: true,
        DisableFlagParsing: true,
        Args: MinimumNArgs(1),
        Short: "Request shell completion choices for the specified command-line",
        Long: fmt.Sprintf("%[2]s is a special command that is used by the shell completion logic\n%[1]s"u8,
            "to request completion choices for the specified command-line.", ShellCompRequestCmd),
        Run: (ж<Command> cmd, slice<@string> argsΔ1) => {
            var (finalCmd, completions, directive, errΔ1) = cmd.getCompletions(argsΔ1);
            if (errΔ1 != default!) {
                CompErrorln(errΔ1.Error());
            }

            var noDescriptions = cmd.CalledAs() == ShellCompNoDescRequestCmd;
            if (!noDescriptions) {
                {
                    var (doDescriptions, errΔ2) = strconv.ParseBool(getEnvConfig(cmd, configEnvVarSuffixDescriptions)); if (errΔ2 == default!) {
                        noDescriptions = !doDescriptions;
                    }
                }
            }
            var noActiveHelp = GetActiveHelpConfig(finalCmd) == activeHelpGlobalDisable;
            var @out = finalCmd.OutOrStdout();
            foreach (var (_, vᴛ1) in completions) {
                var comp = vᴛ1;

                if (noActiveHelp && strings.HasPrefix(comp, activeHelpMarker)) {
                    continue;
                }
                if (noDescriptions) {
                    comp = strings.SplitN(comp, "\t", 2)[0];
                }

                comp = strings.SplitN(comp, "\n", 2)[0];

                comp = strings.TrimSpace(comp);

                fmt.Fprintln(@out, comp);
            }

            fmt.Fprintf(@out, ":%d\n"u8, directive);

            fmt.Fprintf(finalCmd.ErrOrStderr(), "Completion ended with directive: %s\n"u8, directive.@string());
        }
    ));
    Ꮡc.AddCommand(completeCmd);
    var (subCmd, _, err) = Ꮡc.Find(args);
    if (err != default! || subCmd.Name() != ShellCompRequestCmd) {
        c.RemoveCommand(completeCmd);
    }
}

partial interface SliceValue {
    slice<@string> GetSlice();
}

internal static (ж<Command>, slice<Completion>, ShellCompDirective, error) getCompletions(this ж<Command> Ꮡc, slice<@string> args) {
    ref var c = ref Ꮡc.DerefOrNull();
    var finalArgsᴷ = new stackAppend<@string>();

    @string toComplete = args[len(args) - 1];
    var trimmedArgs = args.slice(0, len(args) - 1);

    ж<Command> finalCmd = default!;
    slice<@string> finalArgs = default!;
    error err = default!;
    if ((~Ꮡc.Root()).TraverseChildren){
        (finalCmd, finalArgs, err) = Ꮡc.Root().Traverse(trimmedArgs);
    } else {
        var rootCmd = Ꮡc.Root();
        if (len(rootCmd.Commands()) == 1) {
            rootCmd.RemoveCommand(Ꮡc);
        }

        (finalCmd, finalArgs, err) = rootCmd.Find(trimmedArgs);
    }
    if (err != default!) {
        return (Ꮡc, new Completion[]{}.slice(), ShellCompDirectiveDefault, fmt.Errorf("unable to find a command for arguments: %v"u8, trimmedArgs));
    }
    finalCmd.Value.ctx = c.ctx;

    if (!(~finalCmd).DisableFlagParsing) {
        finalCmd.InitDefaultHelpFlag();
        finalCmd.InitDefaultVersionFlag();
    }

    (var flag, finalArgs, toComplete, var flagErr) = checkIfFlagCompletion(finalCmd, finalArgs, toComplete);

    var flagCompletion = true;
    _ = finalCmd.ParseFlags(append(finalArgsᴷ, finalArgs, "--"u8));
    nint newArgCount = finalCmd.Flags().NArg();

    {
        err = finalCmd.ParseFlags(finalArgs); if (err != default!) {
            return (finalCmd, new Completion[]{}.slice(), ShellCompDirectiveDefault, fmt.Errorf("Error while parsing flags from args %v: %s"u8, finalArgs, err.Error()));
        }
    }

    nint realArgCount = finalCmd.Flags().NArg();
    if (newArgCount > realArgCount) {
        flagCompletion = false;
    }
    if (flagErr != default!) {
        {
            var (_, ok) = flagErr._<ж<flagCompError>>(ᐧ); if (!ok || flagCompletion) {
                return (finalCmd, new Completion[]{}.slice(), ShellCompDirectiveDefault, flagErr);
            }
        }
    }

    if (helpOrVersionFlagPresent(finalCmd)) {
        return (finalCmd, new Completion[]{}.slice(), ShellCompDirectiveNoFileComp, default!);
    }

    if (!(~finalCmd).DisableFlagParsing) {
        finalArgs = finalCmd.Flags().Args();
    }

    if (flag != nil && flagCompletion) {
        {
            var (validExts, present) = (~flag).Annotations[BashCompFilenameExt, ꟷ]; if (present) {
                if (len(validExts) != 0) {
                    return (finalCmd, validExts, ShellCompDirectiveFilterFileExt, default!);
                }
            }
        }

        {
            var (subDir, present) = (~flag).Annotations[BashCompSubdirsInDir, ꟷ]; if (present) {
                if (len(subDir) == 1) {
                    return (finalCmd, subDir, ShellCompDirectiveFilterDirs, default!);
                }
                return (finalCmd, new Completion[]{}.slice(), ShellCompDirectiveFilterDirs, default!);
            }
        }
    }

    ref var completions = ref heap<slice<Completion>>(out var Ꮡcompletions);
    ShellCompDirective directive = default!;

    finalCmd.enforceFlagGroupsForCompletion();

    if (flag == nil && len(toComplete) > 0 && toComplete[0] == (rune)'-' && !strings.Contains(toComplete, "=") && flagCompletion){
        completions = completeRequireFlags(finalCmd, toComplete);

        if (len(completions) == 0) {
            void doCompleteFlags(ж<pflag.Flag> flagΔ1) {
                var (_, acceptsMultiple) = (~flagΔ1).Value._<SliceValue>(ᐧ);
                acceptsMultiple = acceptsMultiple || strings.Contains((~flagΔ1).Value.Type(), "Slice") || strings.Contains((~flagΔ1).Value.Type(), "Array") || strings.HasPrefix((~flagΔ1).Value.Type(), "stringTo");

                if (!(~flagΔ1).Changed || acceptsMultiple) {
                    Ꮡcompletions.ValueSlot = appendꓸꓸꓸ(Ꮡcompletions.ValueSlot, getFlagNameCompletions(ref (flagΔ1).DerefOrNull(), toComplete));
                }
            }

            var doCompleteFlagsʗ1 = doCompleteFlags;
            finalCmd.InheritedFlags().VisitAll((ж<pflag.Flag> flagΔ2) => {
                doCompleteFlagsʗ1(flagΔ2);
            });
            var doCompleteFlagsʗ2 = doCompleteFlags;
            finalCmd.NonInheritedFlags().VisitAll((ж<pflag.Flag> flagΔ3) => {
                doCompleteFlagsʗ2(flagΔ3);
            });
        }

        directive = ShellCompDirectiveNoFileComp;
        if (len(completions) == 1 && strings.HasSuffix(completions[0], "=")) {
            directive = ShellCompDirectiveNoSpace;
        }

        if (!(~finalCmd).DisableFlagParsing) {
            return (finalCmd, completions, directive, default!);
        }
    } else {
        directive = ShellCompDirectiveDefault;
        for (var cmd = finalCmd; cmd != nil; cmd = cmd.Value.parent) {
            if ((~cmd).CompletionOptions.DefaultShellCompDirective != nil) {
                directive = (~cmd).CompletionOptions.DefaultShellCompDirective.Value;
                break;
            }
        }

        if (flag == nil) {
            var foundLocalNonPersistentFlag = false;
            if (!(~finalCmd.Root()).TraverseChildren) {
                var localNonPersistentFlags = finalCmd.LocalNonPersistentFlags();
                var localNonPersistentFlagsʗ1 = localNonPersistentFlags;
                finalCmd.NonInheritedFlags().VisitAll((ж<pflag.Flag> flagΔ4) => {
                    if (localNonPersistentFlagsʗ1.Lookup((~flagΔ4).Name) != nil && (~flagΔ4).Changed) {
                        foundLocalNonPersistentFlag = true;
                    }
                });
            }

            if (len(finalArgs) == 0 && !foundLocalNonPersistentFlag) {
                foreach (var (_, subCmd) in finalCmd.Commands()) {
                    if (subCmd.IsAvailableCommand() || subCmd == (~finalCmd).helpCommand) {
                        if (strings.HasPrefix(subCmd.Name(), toComplete)) {
                            completions = append(completions, CompletionWithDesc(subCmd.Name(), (~subCmd).Short));
                        }
                        directive = ShellCompDirectiveNoFileComp;
                    }
                }
            }

            completions = appendꓸꓸꓸ(completions, completeRequireFlags(finalCmd, toComplete));

            if (len((~finalCmd).ValidArgs) > 0) {
                if (len(finalArgs) == 0) {
                    foreach (var (_, validArg) in (~finalCmd).ValidArgs) {
                        if (strings.HasPrefix(validArg, toComplete)) {
                            completions = append(completions, validArg);
                        }
                    }
                    directive = ShellCompDirectiveNoFileComp;

                    if (len(completions) == 0) {
                        foreach (var (_, argAlias) in (~finalCmd).ArgAliases) {
                            if (strings.HasPrefix(argAlias, toComplete)) {
                                completions = append(completions, argAlias);
                            }
                        }
                    }
                }

                return (finalCmd, completions, directive, default!);
            }
        }
    }

    CompletionFunc completionFn = default!;
    if (flag != nil && flagCompletion){
        flagCompletionMutex.RLock();
        completionFn = flagCompletionFunctions[flag];
        flagCompletionMutex.RUnlock();
    } else {
        completionFn = finalCmd.Value.ValidArgsFunction;
    }
    if (completionFn != default!) {
        slice<Completion> comps = default!;
        (comps, directive) = completionFn(finalCmd, finalArgs, toComplete);
        completions = appendꓸꓸꓸ(completions, comps);
    }

    return (finalCmd, completions, directive, default!);
}

internal static bool helpOrVersionFlagPresent(ж<Command> Ꮡcmd) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    {
        var versionFlag = cmd.Flags().Lookup("version"); if (versionFlag != nil && len((~versionFlag).Annotations[FlagSetByCobraAnnotation]) > 0 && (~versionFlag).Changed) {
            return true;
        }
    }
    {
        var helpFlag = cmd.Flags().Lookup(helpFlagName); if (helpFlag != nil && len((~helpFlag).Annotations[FlagSetByCobraAnnotation]) > 0 && (~helpFlag).Changed) {
            return true;
        }
    }
    return false;
}

internal static slice<Completion> getFlagNameCompletions(ref pflag.Flag flag, @string toComplete) {
    if (nonCompletableFlag(ref flag)) {
        return new Completion[]{}.slice();
    }

    slice<Completion> completions = default!;
    @string flagName = "--"u8 + flag.Name;
    if (strings.HasPrefix(flagName, toComplete)) {
        completions = append(completions, CompletionWithDesc(flagName, flag.Usage));
    }

    flagName = "-"u8 + flag.Shorthand;
    if (len(flag.Shorthand) > 0 && strings.HasPrefix(flagName, toComplete)) {
        completions = append(completions, CompletionWithDesc(flagName, flag.Usage));
    }

    return completions;
}

internal static slice<Completion> completeRequireFlags(ж<Command> ᏑfinalCmd, @string toComplete) {
    ref var completions = ref heap<slice<Completion>>(out var Ꮡcompletions);

    void doCompleteRequiredFlags(ж<pflag.Flag> flag) {
        {
            var (_, present) = (~flag).Annotations[BashCompOneRequiredFlag, ꟷ]; if (present) {
                if (!(~flag).Changed) {
                    Ꮡcompletions.ValueSlot = appendꓸꓸꓸ(Ꮡcompletions.ValueSlot, getFlagNameCompletions(ref (flag).DerefOrNull(), toComplete));
                }
            }
        }
    }

    var doCompleteRequiredFlagsʗ1 = doCompleteRequiredFlags;
    ᏑfinalCmd.InheritedFlags().VisitAll((ж<pflag.Flag> flag) => {
        doCompleteRequiredFlagsʗ1(flag);
    });
    var doCompleteRequiredFlagsʗ2 = doCompleteRequiredFlags;
    ᏑfinalCmd.NonInheritedFlags().VisitAll((ж<pflag.Flag> flag) => {
        doCompleteRequiredFlagsʗ2(flag);
    });

    return completions;
}

internal static (ж<pflag.Flag>, slice<@string>, @string, error) checkIfFlagCompletion(ж<Command> ᏑfinalCmd, slice<@string> args, @string lastArg) {
    ref var finalCmd = ref ᏑfinalCmd.DerefOrNull();

    if (finalCmd.DisableFlagParsing) {
        return (default!, args, lastArg, default!);
    }

    ref var flagName = ref heap(new @string(), out var ᏑflagName);
    var trimmedArgs = args;
    var flagWithEqual = false;
    @string orgLastArg = lastArg;

    if (len(lastArg) > 0 && lastArg[0] == (rune)'-') {
        {
            nint index = strings.Index(lastArg, "="); if (index >= 0){
                if (strings.HasPrefix(lastArg.slice(0, index), "--")){
                    flagName = lastArg.slice(2, index);
                } else {
                    flagName = lastArg.slice(index - 1, index);
                }
                lastArg = lastArg.slice(index + 1);
                flagWithEqual = true;
            } else {
                return (default!, args, lastArg, default!);
            }
        }
    }

    if (len(flagName) == 0) {
        if (len(args) > 0) {
            @string prevArg = args[len(args) - 1];
            if (isFlagArg(prevArg)) {
                {
                    nint index = strings.Index(prevArg, "="); if (index < 0) {
                        if (strings.HasPrefix(prevArg, "--")){
                            flagName = prevArg[2..];
                        } else {
                            flagName = prevArg.slice(len(prevArg) - 1);
                        }
                        trimmedArgs = args.slice(0, len(args) - 1);
                    }
                }
            }
        }
    }

    if (len(flagName) == 0) {
        return (default!, trimmedArgs, lastArg, default!);
    }

    var flag = findFlag(ᏑfinalCmd, flagName);
    if (flag == nil) {
        return (default!, args, orgLastArg, new flagCompErrorжerror(Ꮡ(new flagCompError(subCommand: finalCmd.Name(), flagName: flagName))));
    }

    if (!flagWithEqual) {
        if (len((~flag).NoOptDefVal) != 0) {
            trimmedArgs = args;
            flag = default!;
        }
    }

    return (flag, trimmedArgs, lastArg, default!);
}

public static void InitDefaultCompletionCmd(this ж<Command> Ꮡc, params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.slice();
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.CompletionOptions.DisableDefaultCmd) {
        return;
    }

    foreach (var (_, cmd) in c.commands) {
        if (cmd.Name() == compCmdName || cmd.HasAlias(compCmdName)) {
            return;
        }
    }

    var haveNoDescFlag = !c.CompletionOptions.DisableNoDescFlag && !c.CompletionOptions.DisableDescriptions;

    var hasSubCommands = false;
    foreach (var (_, cmd) in c.commands) {
        if (cmd.Name() != ShellCompRequestCmd && cmd.Name() != helpCommandName) {
            hasSubCommands = true;
            break;
        }
    }

    var completionCmd = Ꮡ(new Command(
        Use: compCmdName,
        Short: "Generate the autocompletion script for the specified shell",
        Long: fmt.Sprintf("""
Generate the autocompletion script for %[1]s for the specified shell.
See each sub-command's help for details on how to use the generated script.

"""u8.Raw(), Ꮡc.Root().Name()),
        Args: new Func<ж<Command>, slice<@string>, error>(NoArgs),
        ValidArgsFunction: NoFileCompletions,
        Hidden: c.CompletionOptions.HiddenDefaultCmd,
        GroupID: c.completionCommandGroupID
    ));
    Ꮡc.AddCommand(completionCmd);

    if (!hasSubCommands) {
        var (subCmd, cmdArgs, err) = Ꮡc.Find(args);
        if (err != default! || subCmd.Name() != compCmdName && (subCmd.Name() != ShellCompRequestCmd || len(cmdArgs) <= 1 || cmdArgs[0] != compCmdName)) {
            c.RemoveCommand(completionCmd);
            return;
        }
    }

    var @out = c.OutOrStdout();
    ref var noDesc = ref heap<bool>(out var ᏑnoDesc);
    noDesc = c.CompletionOptions.DisableDescriptions;
    @string shortDesc = "Generate the autocompletion script for %s";
        var outʗ1 = @out;
    var bash = Ꮡ(new Command(
        Use: "bash",
        Short: fmt.Sprintf(shortDesc, "bash"),
        Long: fmt.Sprintf("""
Generate the autocompletion script for the bash shell.

This script depends on the 'bash-completion' package.
If it is not installed already, you can install it via your OS's package manager.

To load completions in your current shell session:

	source <(%[1]s completion bash)

To load completions for every new session, execute once:

#### Linux:

	%[1]s completion bash > /etc/bash_completion.d/%[1]s

#### macOS:

	%[1]s completion bash > $(brew --prefix)/etc/bash_completion.d/%[1]s

You will need to start a new shell for this setup to take effect.

"""u8.Raw(), Ꮡc.Root().Name()),
        Args: new Func<ж<Command>, slice<@string>, error>(NoArgs),
        DisableFlagsInUseLine: true,
        ValidArgsFunction: NoFileCompletions,
        RunE: (ж<Command> cmd, slice<@string> argsΔ1) => cmd.Root().GenBashCompletionV2(outʗ1, !ᏑnoDesc.Value)
    ));
    if (haveNoDescFlag) {
        bash.Flags().BoolVar(ᏑnoDesc, compCmdNoDescFlagName, compCmdNoDescFlagDefault, compCmdNoDescFlagDesc);
    }

        var outʗ2 = @out;
    var zsh = Ꮡ(new Command(
        Use: "zsh",
        Short: fmt.Sprintf(shortDesc, "zsh"),
        Long: fmt.Sprintf("""
Generate the autocompletion script for the zsh shell.

If shell completion is not already enabled in your environment you will need
to enable it.  You can execute the following once:

	echo "autoload -U compinit; compinit" >> ~/.zshrc

To load completions in your current shell session:

	source <(%[1]s completion zsh)

To load completions for every new session, execute once:

#### Linux:

	%[1]s completion zsh > "${fpath[1]}/_%[1]s"

#### macOS:

	%[1]s completion zsh > $(brew --prefix)/share/zsh/site-functions/_%[1]s

You will need to start a new shell for this setup to take effect.

"""u8.Raw(), Ꮡc.Root().Name()),
        Args: new Func<ж<Command>, slice<@string>, error>(NoArgs),
        ValidArgsFunction: NoFileCompletions,
        RunE: (ж<Command> cmd, slice<@string> argsΔ2) => {
            if (ᏑnoDesc.Value) {
                return cmd.Root().GenZshCompletionNoDesc(outʗ2);
            }
            return cmd.Root().GenZshCompletion(outʗ2);
        }
    ));
    if (haveNoDescFlag) {
        zsh.Flags().BoolVar(ᏑnoDesc, compCmdNoDescFlagName, compCmdNoDescFlagDefault, compCmdNoDescFlagDesc);
    }

        var outʗ3 = @out;
    var fish = Ꮡ(new Command(
        Use: "fish",
        Short: fmt.Sprintf(shortDesc, "fish"),
        Long: fmt.Sprintf("""
Generate the autocompletion script for the fish shell.

To load completions in your current shell session:

	%[1]s completion fish | source

To load completions for every new session, execute once:

	%[1]s completion fish > ~/.config/fish/completions/%[1]s.fish

You will need to start a new shell for this setup to take effect.

"""u8.Raw(), Ꮡc.Root().Name()),
        Args: new Func<ж<Command>, slice<@string>, error>(NoArgs),
        ValidArgsFunction: NoFileCompletions,
        RunE: (ж<Command> cmd, slice<@string> argsΔ3) => cmd.Root().GenFishCompletion(outʗ3, !ᏑnoDesc.Value)
    ));
    if (haveNoDescFlag) {
        fish.Flags().BoolVar(ᏑnoDesc, compCmdNoDescFlagName, compCmdNoDescFlagDefault, compCmdNoDescFlagDesc);
    }

        var outʗ4 = @out;
    var powershell = Ꮡ(new Command(
        Use: "powershell",
        Short: fmt.Sprintf(shortDesc, "powershell"),
        Long: fmt.Sprintf("""
Generate the autocompletion script for powershell.

To load completions in your current shell session:

	%[1]s completion powershell | Out-String | Invoke-Expression

To load completions for every new session, add the output of the above command
to your powershell profile.

"""u8.Raw(), Ꮡc.Root().Name()),
        Args: new Func<ж<Command>, slice<@string>, error>(NoArgs),
        ValidArgsFunction: NoFileCompletions,
        RunE: (ж<Command> cmd, slice<@string> argsΔ4) => {
            if (ᏑnoDesc.Value) {
                return cmd.Root().GenPowerShellCompletion(outʗ4);
            }
            return cmd.Root().GenPowerShellCompletionWithDesc(outʗ4);
        }
    ));
    if (haveNoDescFlag) {
        powershell.Flags().BoolVar(ᏑnoDesc, compCmdNoDescFlagName, compCmdNoDescFlagDefault, compCmdNoDescFlagDesc);
    }

    completionCmd.AddCommand(bash, zsh, fish, powershell);
}

internal static ж<pflag.Flag> findFlag(ж<Command> Ꮡcmd, @string name) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    var flagSet = cmd.Flags();
    if (len(name) == 1) {
        {
            var @short = flagSet.ShorthandLookup(name); if (@short != nil){
                name = @short.Value.Name;
            } else {
                var set = Ꮡcmd.InheritedFlags();
                {
                    @short = set.ShorthandLookup(name); if (@short != nil){
                        name = @short.Value.Name;
                    } else {
                        return default!;
                    }
                }
            }
        }
    }
    return Ꮡcmd.Flag(name);
}

public static void CompDebug(@string msg, bool printToStdErr) {
    GoFrame ᒐ = default;
    try {
        msg = fmt.Sprintf("[Debug] %s"u8, msg);

        {
            @string path = os.Getenv("BASH_COMP_DEBUG_FILE"); if (path != ""u8) {
                var (f, err) = os.OpenFile(path,
                    (nint)((nint)(nint)(os.O_APPEND | os.O_CREATE) | os.O_WRONLY), 420);
                if (err == default!) {
                    var fʗ1 = f;
                    defer(() => fʗ1.Close(), ref ᒐ);
                    WriteStringAndCheck(new os_FileжStringWriter(f), msg);
                }
            }
        }

        if (printToStdErr) {
            fmt.Fprint(new os.FileжWriter(os.Stderr), msg);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void CompDebugln(@string msg, bool printToStdErr) {
    CompDebug(fmt.Sprintf("%s\n"u8, msg), printToStdErr);
}

public static void CompError(@string msg) {
    msg = fmt.Sprintf("[Error] %s"u8, msg);
    CompDebug(msg, true);
}

public static void CompErrorln(@string msg) {
    CompError(fmt.Sprintf("%s\n"u8, msg));
}

internal static readonly @string configEnvVarGlobalPrefix = "COBRA"u8;
internal static readonly @string configEnvVarSuffixDescriptions = "COMPLETION_DESCRIPTIONS"u8;

internal static ж<regexp.Regexp> configEnvVarPrefixSubstRegexp = regexp.MustCompile(@"[^A-Z0-9_]");

internal static @string configEnvVar(@string name, @string suffix) {
    @string v = strings.ToUpper(fmt.Sprintf("%s_%s"u8, name, suffix));
    v = configEnvVarPrefixSubstRegexp.ReplaceAllString(v, "_");
    return v;
}

internal static @string getEnvConfig(ж<Command> Ꮡcmd, @string suffix) {
    @string v = os.Getenv(configEnvVar(Ꮡcmd.Root().Name(), suffix));
    if (v == ""u8) {
        v = os.Getenv(configEnvVar(configEnvVarGlobalPrefix, suffix));
    }
    return v;
}

} // end cobra_package
