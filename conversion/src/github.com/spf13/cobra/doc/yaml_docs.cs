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

using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using sort = sort_package;
using strings = strings_package;
using cobra = global::go.github.com.spf13.cobra_package;
using pflag = global::go.github.com.spf13.pflag_package;
using yaml = global::go.go.yaml.@in.yaml.yaml_package;
using global::go.github.com.spf13;
using global::go.go.yaml.@in.yaml;
using path;

partial class doc_package {

public partial struct cmdOption {
    public @string Name;
    public @string Shorthand; /*`yaml:",omitempty"`*/
    public @string DefaultValue; /*`yaml:"default_value,omitempty"`*/
    public @string Usage; /*`yaml:",omitempty"`*/
}

partial struct cmdDoc {
    public @string Name;
    public @string Synopsis; /*`yaml:",omitempty"`*/
    public @string Description; /*`yaml:",omitempty"`*/
    public @string Usage; /*`yaml:",omitempty"`*/
    public slice<cmdOption> Options; /*`yaml:",omitempty"`*/
    public slice<cmdOption> InheritedOptions; /*`yaml:"inherited_options,omitempty"`*/
    public @string Example; /*`yaml:",omitempty"`*/
    public slice<@string> SeeAlso; /*`yaml:"see_also,omitempty"`*/
}

public static error GenYamlTree(ж<cobra.Command> Ꮡcmd, @string dir) {
    var identity = @string (@string s) => s;
    var emptyStr = @string (@string s) => "";
    return GenYamlTreeCustom(Ꮡcmd, dir, emptyStr, identity);
}

public static error GenYamlTreeCustom(ж<cobra.Command> Ꮡcmd, @string dir, Func<@string, @string> filePrepender, Func<@string, @string> linkHandler) {
    GoFrame ᒐ = default;
    try {
        ref var cmd = ref Ꮡcmd.DerefOrNull();

        foreach (var (_, c) in cmd.Commands()) {
            if (!c.IsAvailableCommand() || c.IsAdditionalHelpTopicCommand()) {
                continue;
            }
            {
                var errΔ1 = GenYamlTreeCustom(c, dir, filePrepender, linkHandler); if (errΔ1 != default!) {
                    return errΔ1;
                }
            }
        }

        @string basename = strings.ReplaceAll(cmd.CommandPath(), " ", "_") + ".yaml"u8;
        @string filename = filepath.Join(dir, basename);
        var (f, err) = os.Create(filename);
        if (err != default!) {
            return err;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        {
            var (_, errΔ2) = io.WriteString(new os.FileжWriter(f), filePrepender(filename)); if (errΔ2 != default!) {
                return errΔ2;
            }
        }
        {
            var errΔ3 = GenYamlCustom(Ꮡcmd, new os.FileжWriter(f), linkHandler); if (errΔ3 != default!) {
                return errΔ3;
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

public static error GenYaml(ж<cobra.Command> Ꮡcmd, io.Writer w) {
    return GenYamlCustom(Ꮡcmd, w, (@string s) => s);
}

public static error GenYamlCustom(ж<cobra.Command> Ꮡcmd, io.Writer w, Func<@string, @string> linkHandler) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    Ꮡcmd.InitDefaultHelpCmd();
    Ꮡcmd.InitDefaultHelpFlag();

    ref var yamlDoc = ref heap<cmdDoc>(out var ᏑyamlDoc);
    yamlDoc = new cmdDoc(nil);
    yamlDoc.Name = cmd.CommandPath();

    yamlDoc.Synopsis = forceMultiLine(cmd.Short);
    yamlDoc.Description = forceMultiLine(cmd.Long);

    if (cmd.Runnable()) {
        yamlDoc.Usage = cmd.UseLine();
    }

    if (len(cmd.Example) > 0) {
        yamlDoc.Example = cmd.Example;
    }

    var flags = Ꮡcmd.NonInheritedFlags();
    if (flags.HasFlags()) {
        yamlDoc.Options = genFlagResult(flags);
    }
    flags = Ꮡcmd.InheritedFlags();
    if (flags.HasFlags()) {
        yamlDoc.InheritedOptions = genFlagResult(flags);
    }

    if (hasSeeAlso(Ꮡcmd)) {
        var result = new @string[]{}.slice();
        if (cmd.HasParent()) {
            var parent = cmd.Parent();
            result = append(result, parent.CommandPath() + " - "u8 + (~parent).Short);
        }
        var children = cmd.Commands();
        sort.Sort((byName)children);
        foreach (var (_, child) in children) {
            if (!child.IsAvailableCommand() || child.IsAdditionalHelpTopicCommand()) {
                continue;
            }
            result = append(result, child.CommandPath() + " - "u8 + (~child).Short);
        }
        yamlDoc.SeeAlso = result;
    }

    var (final, err) = yaml.Marshal(ᏑyamlDoc);
    if (err != default!) {
        fmt.Println(err);
        os.Exit(1);
    }

    {
        var (_, errΔ1) = w.Write(final); if (errΔ1 != default!) {
            return errΔ1;
        }
    }
    return default!;
}

internal static slice<cmdOption> genFlagResult(ж<pflag.FlagSet> Ꮡflags) {
    ref var flags = ref Ꮡflags.DerefOrNull();

    ref var result = ref heap<slice<cmdOption>>(out var Ꮡresult);

    flags.VisitAll((ж<pflag.Flag> flag) => {
        if (len((~flag).ShorthandDeprecated) == 0 && len((~flag).Shorthand) > 0){
            var opt = new cmdOption(
                (~flag).Name,
                (~flag).Shorthand,
                (~flag).DefValue,
                forceMultiLine((~flag).Usage)
            );
            Ꮡresult.ValueSlot = append(Ꮡresult.ValueSlot, opt);
        } else {
            var opt = new cmdOption(
                Name: (~flag).Name,
                DefaultValue: forceMultiLine((~flag).DefValue),
                Usage: forceMultiLine((~flag).Usage)
            );
            Ꮡresult.ValueSlot = append(Ꮡresult.ValueSlot, opt);
        }
    });

    return result;
}

} // end doc_package
