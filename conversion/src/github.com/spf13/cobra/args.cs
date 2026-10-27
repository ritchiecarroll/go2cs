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

using fmt = fmt_package;
using strings = strings_package;

partial class cobra_package {

// type PositionalArgs is a methodless func type — rendered inline as its base delegate

internal static error legacyArgs(ж<Command> Ꮡcmd, slice<@string> args) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    if (!cmd.HasSubCommands()) {
        return default!;
    }

    if (!cmd.HasParent() && len(args) > 0) {
        return fmt.Errorf("unknown command %q for %q%s"u8, args[0], cmd.CommandPath(), cmd.findSuggestions(args[0]));
    }
    return default!;
}

public static error NoArgs(ж<Command> Ꮡcmd, slice<@string> args) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    if (len(args) > 0) {
        return fmt.Errorf("unknown command %q for %q"u8, args[0], cmd.CommandPath());
    }
    return default!;
}

public static error OnlyValidArgs(ж<Command> Ꮡcmd, slice<@string> args) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();
    var validArgsᴷ = new stackAppend<@string>();

    if (len(cmd.ValidArgs) > 0) {
        var validArgs = new slice<@string>(0, len(cmd.ValidArgs));
        foreach (var (_, v) in cmd.ValidArgs) {
            validArgs = append(validArgsᴷ, validArgs, strings.SplitN(v, "\t", 2)[0]);
        }
        foreach (var (_, v) in args) {
            if (!stringInSlice(v, validArgs)) {
                return fmt.Errorf("invalid argument %q for %q%s"u8, v, cmd.CommandPath(), cmd.findSuggestions(args[0]));
            }
        }
    }
    return default!;
}

public static error ArbitraryArgs(ж<Command> Ꮡcmd, slice<@string> args) {
    return default!;
}

public static Func<ж<Command>, slice<@string>, error> MinimumNArgs(nint n) {
    return error (ж<Command> cmd, slice<@string> args) => {
        if (len(args) < n) {
            return fmt.Errorf("requires at least %d arg(s), only received %d"u8, n, len(args));
        }
        return default!;
    };
}

public static Func<ж<Command>, slice<@string>, error> MaximumNArgs(nint n) {
    return error (ж<Command> cmd, slice<@string> args) => {
        if (len(args) > n) {
            return fmt.Errorf("accepts at most %d arg(s), received %d"u8, n, len(args));
        }
        return default!;
    };
}

public static Func<ж<Command>, slice<@string>, error> ExactArgs(nint n) {
    return error (ж<Command> cmd, slice<@string> args) => {
        if (len(args) != n) {
            return fmt.Errorf("accepts %d arg(s), received %d"u8, n, len(args));
        }
        return default!;
    };
}

public static Func<ж<Command>, slice<@string>, error> RangeArgs(nint min, nint max) {
    return error (ж<Command> cmd, slice<@string> args) => {
        if (len(args) < min || len(args) > max) {
            return fmt.Errorf("accepts between %d and %d arg(s), received %d"u8, min, max, len(args));
        }
        return default!;
    };
}

public static Func<ж<Command>, slice<@string>, error> MatchAll(params Span<Func<ж<Command>, slice<@string>, error>> pargsʗp) {
    var pargs = pargsʗp.slice();

    var pargsʗ1 = pargs;
    return error (ж<Command> cmd, slice<@string> args) => {
        foreach (var (_, parg) in pargsʗ1) {
            {
                var err = parg(cmd, args); if (err != default!) {
                    return err;
                }
            }
        }
        return default!;
    };
}

public static Func<ж<Command>, slice<@string>, error> ExactValidArgs(nint n) {
    return MatchAll(ExactArgs(n), OnlyValidArgs);
}

} // end cobra_package
