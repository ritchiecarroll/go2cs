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

namespace go.github.com.spf13.cobra;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using md2man = global::go.github.com.cpuguy83.go_md2man.v2.md2man_package;
using cobra = global::go.github.com.spf13.cobra_package;
using pflag = global::go.github.com.spf13.pflag_package;
using global::go.github.com.cpuguy83.go_md2man.v2;
using global::go.github.com.spf13;
using path;

partial class doc_package {

public static error GenManTree(ж<cobra.Command> Ꮡcmd, ж<GenManHeader> Ꮡheader, @string dir) {
    return GenManTreeFromOpts(Ꮡcmd, new GenManTreeOptions(
        Header: Ꮡheader,
        Path: dir,
        CommandSeparator: "-"
    ));
}

public static error GenManTreeFromOpts(ж<cobra.Command> Ꮡcmd, GenManTreeOptions opts) {
    GoFrame ᒐ = default;
    try {
        ref var cmd = ref Ꮡcmd.DerefOrNull();

        var header = opts.Header;
        if (header == nil) {
            header = Ꮡ(new GenManHeader(nil));
        }
        foreach (var (_, c) in cmd.Commands()) {
            if (!c.IsAvailableCommand() || c.IsAdditionalHelpTopicCommand()) {
                continue;
            }
            {
                var errΔ1 = GenManTreeFromOpts(c, opts); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }
        @string section = "1";
        if ((~header).Section != ""u8) {
            section = header.Value.Section;
        }

        @string separator = "_";
        if (opts.CommandSeparator != ""u8) {
            separator = opts.CommandSeparator;
        }
        @string basename = strings.ReplaceAll(cmd.CommandPath(), " ", separator);
        @string filename = filepath.Join(opts.Path, basename + "." + section);
        var (f, err) = os.Create(filename);
        if (err != default!) {
            return err;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        ref var headerCopy = ref heap<GenManHeader>(out var ᏑheaderCopy);
        headerCopy = header.Value;
        return GenMan(Ꮡcmd, ᏑheaderCopy, new os.FileжWriter(f));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

partial struct GenManTreeOptions {
    public ж<GenManHeader> Header;
    public @string Path;
    public @string CommandSeparator;
}

partial struct GenManHeader {
    public @string Title;
    public @string Section;
    public ж<time.Time> Date;
    internal @string date;
    public @string Source;
    public @string Manual;
}

public static error GenMan(ж<cobra.Command> Ꮡcmd, ж<GenManHeader> Ꮡheader, io.Writer w) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();
    ref var header = ref Ꮡheader.DerefOrNull();

    if (Ꮡheader == nil) {
        Ꮡheader = Ꮡ(new GenManHeader(nil)); header = ref Ꮡheader.DerefOrNull();
    }
    {
        var errΔ1 = fillHeader(ref (Ꮡheader).DerefOrNull(), cmd.CommandPath(), cmd.DisableAutoGenTag); if (errΔ1 != default!) {
            return errΔ1;
        }
    }

    var b = genMan(Ꮡcmd, ref (Ꮡheader).DerefOrNull());
    var (_, err) = w.Write(md2man.Render(b));
    return err;
}

internal static error fillHeader(ref GenManHeader header, @string name, bool disableAutoGen) {
    if (header.Title == ""u8) {
        header.Title = strings.ToUpper(strings.ReplaceAll(name, " ", "\\-"));
    }
    if (header.Section == ""u8) {
        header.Section = "1";
    }
    if (header.Date == nil) {
        ref var now = ref heap<time.Time>(out var Ꮡnow);
        now = time.Now();
        {
            @string epoch = os.Getenv("SOURCE_DATE_EPOCH"); if (epoch != ""u8) {
                var (unixEpoch, err) = strconv.ParseInt(epoch, 10, 64);
                if (err != default!) {
                    return fmt.Errorf("invalid SOURCE_DATE_EPOCH: %v"u8, err);
                }
                now = time.Unix(unixEpoch, 0);
            }
        }
        header.Date = Ꮡnow;
    }
    header.date = (~header.Date).Format("Jan 2006");
    if (header.Source == ""u8 && !disableAutoGen) {
        header.Source = "Auto generated by spf13/cobra";
    }
    return default!;
}

internal static void manPreamble(io.StringWriter buf, ref GenManHeader header, ж<cobra.Command> Ꮡcmd, @string dashedName) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    @string description = cmd.Long;
    if (len(description) == 0) {
        description = cmd.Short;
    }

    cobra.WriteStringAndCheck(buf, fmt.Sprintf("""
%% "%s" "%s" "%s" "%s" "%s"
# NAME

"""u8.Raw(), header.Title, header.Section, header.date, header.Source, header.Manual));
    cobra.WriteStringAndCheck(buf, fmt.Sprintf("%s \\- %s\n\n"u8, dashedName, cmd.Short));
    cobra.WriteStringAndCheck(buf, "# SYNOPSIS\n");
    cobra.WriteStringAndCheck(buf, fmt.Sprintf("**%s**\n\n"u8, cmd.UseLine()));
    cobra.WriteStringAndCheck(buf, "# DESCRIPTION\n");
    cobra.WriteStringAndCheck(buf, description + "\n\n"u8);
}

internal static void manPrintFlags(io.StringWriter buf, ж<pflag.FlagSet> Ꮡflags) {
    ref var flags = ref Ꮡflags.DerefOrNull();

    flags.VisitAll((ж<pflag.Flag> flag) => {
        if (len((~flag).Deprecated) > 0 || (~flag).Hidden) {
            return;
        }
        @string format = "";
        if (len((~flag).Shorthand) > 0 && len((~flag).ShorthandDeprecated) == 0){
            format = fmt.Sprintf("**-%s**, **--%s**"u8, (~flag).Shorthand, (~flag).Name);
        } else {
            format = fmt.Sprintf("**--%s**"u8, (~flag).Name);
        }
        if (len((~flag).NoOptDefVal) > 0) {
            format += "["u8;
        }
        if ((~flag).Value.Type() == "string"u8){
            format += "=%q"u8;
        } else {
            format += "=%s"u8;
        }
        if (len((~flag).NoOptDefVal) > 0) {
            format += "]"u8;
        }
        format += "\n\t%s\n\n"u8;
        cobra.WriteStringAndCheck(buf, fmt.Sprintf(format, (~flag).DefValue, (~flag).Usage));
    });
}

internal static void manPrintOptions(io.StringWriter buf, ж<cobra.Command> Ꮡcommand) {
    var flags = Ꮡcommand.NonInheritedFlags();
    if (flags.HasAvailableFlags()) {
        cobra.WriteStringAndCheck(buf, "# OPTIONS\n");
        manPrintFlags(buf, flags);
        cobra.WriteStringAndCheck(buf, "\n");
    }
    flags = Ꮡcommand.InheritedFlags();
    if (flags.HasAvailableFlags()) {
        cobra.WriteStringAndCheck(buf, "# OPTIONS INHERITED FROM PARENT COMMANDS\n");
        manPrintFlags(buf, flags);
        cobra.WriteStringAndCheck(buf, "\n");
    }
}

internal static slice<byte> genMan(ж<cobra.Command> Ꮡcmd, ref GenManHeader header) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();
    var seealsosᴷ = new stackAppend<@string>();

    Ꮡcmd.InitDefaultHelpCmd();
    Ꮡcmd.InitDefaultHelpFlag();

    @string dashCommandName = strings.ReplaceAll(cmd.CommandPath(), " ", "-");

    var buf = @new<bytes.Buffer>();

    manPreamble(new bytes_BufferжStringWriter(buf), ref header, Ꮡcmd, dashCommandName);
    manPrintOptions(new bytes_BufferжStringWriter(buf), Ꮡcmd);
    if (len(cmd.Example) > 0) {
        buf.WriteString("# EXAMPLE\n");
        fmt.Fprintf(new bytes_BufferжWriter(buf), "```\n%s\n```\n"u8, cmd.Example);
    }
    if (hasSeeAlso(Ꮡcmd)) {
        buf.WriteString("# SEE ALSO\n");
        var seealsos = new slice<@string>(0);
        if (cmd.HasParent()) {
            @string parentPath = cmd.Parent().CommandPath();
            @string dashParentPath = strings.ReplaceAll(parentPath, " ", "-");
            @string seealso = fmt.Sprintf("**%s(%s)**"u8, dashParentPath, header.Section);
            seealsos = append(seealsosᴷ, seealsos, seealso);
            cmd.VisitParents((ж<cobra.Command> c) => {
                if ((~c).DisableAutoGenTag) {
                    Ꮡcmd.Value.DisableAutoGenTag = c.Value.DisableAutoGenTag;
                }
            });
        }
        var children = cmd.Commands();
        sort.Sort((byName)children);
        foreach (var (_, c) in children) {
            if (!c.IsAvailableCommand() || c.IsAdditionalHelpTopicCommand()) {
                continue;
            }
            @string seealso = fmt.Sprintf("**%s-%s(%s)**"u8, dashCommandName, c.Name(), header.Section);
            seealsos = append(seealsos, seealso);
        }
        buf.WriteString(strings.Join(seealsos, ", ") + "\n"u8);
    }
    if (!cmd.DisableAutoGenTag) {
        fmt.Fprintf(new bytes_BufferжWriter(buf), "# HISTORY\n%s Auto generated by spf13/cobra\n"u8, (~header.Date).Format("2-Jan-2006"));
    }
    return buf.Bytes();
}

} // end doc_package
