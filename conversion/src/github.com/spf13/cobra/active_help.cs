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
using os = os_package;

partial class cobra_package {

internal static readonly @string activeHelpMarker = "_activeHelp_ "u8;
internal static readonly @string activeHelpEnvVarSuffix = "ACTIVE_HELP"u8;
internal static readonly @string activeHelpGlobalEnvVar = "COBRA_ACTIVE_HELP";
internal static readonly @string activeHelpGlobalDisable = "0"u8;

public static slice<Completion> AppendActiveHelp(slice<Completion> compArray, @string activeHelpStr) {
    return append(compArray, fmt.Sprintf("%s%s"u8, activeHelpMarker, activeHelpStr));
}

public static @string GetActiveHelpConfig(ж<Command> Ꮡcmd) {
    @string activeHelpCfg = os.Getenv(activeHelpGlobalEnvVar);
    if (activeHelpCfg != activeHelpGlobalDisable) {
        activeHelpCfg = os.Getenv(activeHelpEnvVar(Ꮡcmd.Root().Name()));
    }
    return activeHelpCfg;
}

internal static @string activeHelpEnvVar(@string name) {
    return configEnvVar(name, activeHelpEnvVarSuffix);
}

} // end cobra_package
