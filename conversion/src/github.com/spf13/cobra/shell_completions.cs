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

// Converted from Go source: "shell_completions.go" in module github.com/spf13/cobra@v1.10.2.
// Modified from the original by conversion to C# with go2cs 1.26.11.1.

namespace go.github.com.spf13;

using pflag = go.github.com.spf13.pflag_package;
using go.github.com.spf13;
using ꓸꓸꓸstring = Span<@string>;
using cobra = cobra_package;
using static cobra_package;

partial class cobra_package {

} partial class cobraᴛCommand {

public static error MarkFlagRequired(this ref Command c, @string name) {
    return cobra.MarkFlagRequired(c.Flags(), name);
}

public static error MarkPersistentFlagRequired(this ref Command c, @string name) {
    return cobra.MarkFlagRequired(c.PersistentFlags(), name);
}

} partial class cobra_package {

public static error MarkFlagRequired(ж<pflag.FlagSet> flags, @string name) {
    return flags.SetAnnotation(name, BashCompOneRequiredFlag, ["true"]);
}

} partial class cobraᴛCommand {

public static error MarkFlagFilename(this ref Command c, @string name, params ꓸꓸꓸstring extensionsʗp) {
    var extensions = extensionsʗp.sslice();

    return cobra.MarkFlagFilename(c.Flags(), name, extensions.ꓸꓸꓸ);
}

public static error MarkFlagCustom(this ref Command c, @string name, @string f) {
    return cobra.MarkFlagCustom(c.Flags(), name, f);
}

public static error MarkPersistentFlagFilename(this ref Command c, @string name, params ꓸꓸꓸstring extensionsʗp) {
    var extensions = extensionsʗp.sslice();

    return cobra.MarkFlagFilename(c.PersistentFlags(), name, extensions.ꓸꓸꓸ);
}

} partial class cobra_package {

public static error MarkFlagFilename(ж<pflag.FlagSet> flags, @string name, params ꓸꓸꓸstring extensionsʗp) {
    var extensions = extensionsʗp.slice();

    return flags.SetAnnotation(name, BashCompFilenameExt, extensions);
}

public static error MarkFlagCustom(ж<pflag.FlagSet> flags, @string name, @string f) {
    return flags.SetAnnotation(name, BashCompCustom, [f]);
}

} partial class cobraᴛCommand {

public static error MarkFlagDirname(this ref Command c, @string name) {
    return cobra.MarkFlagDirname(c.Flags(), name);
}

public static error MarkPersistentFlagDirname(this ref Command c, @string name) {
    return cobra.MarkFlagDirname(c.PersistentFlags(), name);
}

} partial class cobra_package {

public static error MarkFlagDirname(ж<pflag.FlagSet> flags, @string name) {
    return flags.SetAnnotation(name, BashCompSubdirsInDir, []);
}

} // end cobra_package
