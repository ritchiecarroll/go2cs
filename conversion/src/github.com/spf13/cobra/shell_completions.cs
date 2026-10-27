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

using pflag = go.github.com.spf13.pflag_package;
using go.github.com.spf13;
using ꓸꓸꓸstring = Span<@string>;

partial class cobra_package {

public static error MarkFlagRequired(this ref Command c, @string name) {
    return MarkFlagRequired(c.Flags(), name);
}

public static error MarkPersistentFlagRequired(this ref Command c, @string name) {
    return MarkFlagRequired(c.PersistentFlags(), name);
}

public static error MarkFlagRequired(ж<pflag.FlagSet> Ꮡflags, @string name) {
    return Ꮡflags.SetAnnotation(name, BashCompOneRequiredFlag, new @string[]{"true"}.slice());
}

public static error MarkFlagFilename(this ref Command c, @string name, params ꓸꓸꓸstring extensionsʗp) {
    var extensions = extensionsʗp.sslice();

    return MarkFlagFilename(c.Flags(), name, extensions.ꓸꓸꓸ);
}

public static error MarkFlagCustom(this ref Command c, @string name, @string f) {
    return MarkFlagCustom(c.Flags(), name, f);
}

public static error MarkPersistentFlagFilename(this ref Command c, @string name, params ꓸꓸꓸstring extensionsʗp) {
    var extensions = extensionsʗp.sslice();

    return MarkFlagFilename(c.PersistentFlags(), name, extensions.ꓸꓸꓸ);
}

public static error MarkFlagFilename(ж<pflag.FlagSet> Ꮡflags, @string name, params ꓸꓸꓸstring extensionsʗp) {
    var extensions = extensionsʗp.slice();

    return Ꮡflags.SetAnnotation(name, BashCompFilenameExt, extensions);
}

public static error MarkFlagCustom(ж<pflag.FlagSet> Ꮡflags, @string name, @string f) {
    return Ꮡflags.SetAnnotation(name, BashCompCustom, new @string[]{f}.slice());
}

public static error MarkFlagDirname(this ref Command c, @string name) {
    return MarkFlagDirname(c.Flags(), name);
}

public static error MarkPersistentFlagDirname(this ref Command c, @string name) {
    return MarkFlagDirname(c.PersistentFlags(), name);
}

public static error MarkFlagDirname(ж<pflag.FlagSet> Ꮡflags, @string name) {
    return Ꮡflags.SetAnnotation(name, BashCompSubdirsInDir, new @string[]{}.slice());
}

} // end cobra_package
