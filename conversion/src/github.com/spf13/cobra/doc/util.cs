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

using strings = strings_package;
using cobra = global::go.github.com.spf13.cobra_package;
using global::go.github.com.spf13;

partial class doc_package {

internal static bool hasSeeAlso(ж<cobra.Command> Ꮡcmd) {
    ref var cmd = ref Ꮡcmd.DerefOrNull();

    if (cmd.HasParent()) {
        return true;
    }
    foreach (var (_, c) in cmd.Commands()) {
        if (!c.IsAvailableCommand() || c.IsAdditionalHelpTopicCommand()) {
            continue;
        }
        return true;
    }
    return false;
}

internal static @string forceMultiLine(@string s) {
    if (len(s) > 60 && !strings.Contains(s, "\n")) {
        s += "\n"u8;
    }
    return s;
}

partial struct byName /*[]ж<global::go.github.com.spf13.cobra_package.Command>*/;

internal static nint Len(this byName s) {
    return len(s);
}

internal static void Swap(this byName s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

internal static bool Less(this byName s, nint i, nint j) {
    return s[i].Name() < s[j].Name();
}

} // end doc_package
