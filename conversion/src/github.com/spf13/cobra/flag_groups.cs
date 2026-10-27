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
using sort = sort_package;
using strings = strings_package;
using flag = go.github.com.spf13.pflag_package;
using go.github.com.spf13;
using pflag = go.github.com.spf13.pflag_package;
using ꓸꓸꓸstring = Span<@string>;

partial class cobra_package {

internal static readonly @string requiredAsGroupAnnotation = "cobra_annotation_required_if_others_set"u8;
internal static readonly @string oneRequiredAnnotation = "cobra_annotation_one_required"u8;
internal static readonly @string mutuallyExclusiveAnnotation = "cobra_annotation_mutually_exclusive"u8;

public static void MarkFlagsRequiredTogether(this ж<Command> Ꮡc, params ꓸꓸꓸstring flagNamesʗp) {
    var flagNames = flagNamesʗp.slice();
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.mergePersistentFlags();
    foreach (var (_, v) in flagNames) {
        var f = c.Flags().Lookup(v);
        if (f == nil) {
            throw panic(fmt.Sprintf("Failed to find flag %q and mark it as being required in a flag group"u8, v));
        }
        {
            var err = c.Flags().SetAnnotation(v, requiredAsGroupAnnotation, append((~f).Annotations[requiredAsGroupAnnotation], strings.Join(flagNames, " "))); if (err != default!) {
                throw panic(err);
            }
        }
    }
}

public static void MarkFlagsOneRequired(this ж<Command> Ꮡc, params ꓸꓸꓸstring flagNamesʗp) {
    var flagNames = flagNamesʗp.slice();
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.mergePersistentFlags();
    foreach (var (_, v) in flagNames) {
        var f = c.Flags().Lookup(v);
        if (f == nil) {
            throw panic(fmt.Sprintf("Failed to find flag %q and mark it as being in a one-required flag group"u8, v));
        }
        {
            var err = c.Flags().SetAnnotation(v, oneRequiredAnnotation, append((~f).Annotations[oneRequiredAnnotation], strings.Join(flagNames, " "))); if (err != default!) {
                throw panic(err);
            }
        }
    }
}

public static void MarkFlagsMutuallyExclusive(this ж<Command> Ꮡc, params ꓸꓸꓸstring flagNamesʗp) {
    var flagNames = flagNamesʗp.slice();
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.mergePersistentFlags();
    foreach (var (_, v) in flagNames) {
        var f = c.Flags().Lookup(v);
        if (f == nil) {
            throw panic(fmt.Sprintf("Failed to find flag %q and mark it as being in a mutually exclusive flag group"u8, v));
        }
        {
            var err = c.Flags().SetAnnotation(v, mutuallyExclusiveAnnotation, append((~f).Annotations[mutuallyExclusiveAnnotation], strings.Join(flagNames, " "))); if (err != default!) {
                throw panic(err);
            }
        }
    }
}

public static error ValidateFlagGroups(this ref Command c) {
    if (c.DisableFlagParsing) {
        return default!;
    }

    var flags = c.Flags();

    var groupStatus = new map<@string, map<@string, bool>>{};
    var oneRequiredGroupStatus = new map<@string, map<@string, bool>>{};
    var mutuallyExclusiveGroupStatus = new map<@string, map<@string, bool>>{};
    var flagsʗ1 = flags;
    var groupStatusʗ1 = groupStatus;
    var mutuallyExclusiveGroupStatusʗ1 = mutuallyExclusiveGroupStatus;
    var oneRequiredGroupStatusʗ1 = oneRequiredGroupStatus;
    flags.VisitAll((ж<flag.Flag> pflag) => {
        processFlagForGroupAnnotation(flagsʗ1, ref (pflag).DerefOrNull(), requiredAsGroupAnnotation, groupStatusʗ1);
        processFlagForGroupAnnotation(flagsʗ1, ref (pflag).DerefOrNull(), oneRequiredAnnotation, oneRequiredGroupStatusʗ1);
        processFlagForGroupAnnotation(flagsʗ1, ref (pflag).DerefOrNull(), mutuallyExclusiveAnnotation, mutuallyExclusiveGroupStatusʗ1);
    });

    {
        var err = validateRequiredFlagGroups(groupStatus); if (err != default!) {
            return err;
        }
    }
    {
        var err = validateOneRequiredFlagGroups(oneRequiredGroupStatus); if (err != default!) {
            return err;
        }
    }
    {
        var err = validateExclusiveFlagGroups(mutuallyExclusiveGroupStatus); if (err != default!) {
            return err;
        }
    }
    return default!;
}

internal static bool hasAllFlags(ж<flag.FlagSet> Ꮡfs, params ꓸꓸꓸstring flagnamesʗp) {
    var flagnames = flagnamesʗp.sslice();

    foreach (var (_, fname) in flagnames) {
        var f = Ꮡfs.Lookup(fname);
        if (f == nil) {
            return false;
        }
    }
    return true;
}

internal static void processFlagForGroupAnnotation(ж<flag.FlagSet> Ꮡflags, ref flag.Flag pflag, @string annotation, map<@string, map<@string, bool>> groupStatus) {
    var (groupInfo, found) = pflag.Annotations[annotation, ꟷ];
    if (found) {
        foreach (var (_, group) in groupInfo) {
            if (groupStatus[group] == default!) {
                var flagnames = strings.Split(group, " ");

                if (!hasAllFlags(Ꮡflags, flagnames.ꓸꓸꓸ)) {
                    continue;
                }

                groupStatus[group] = new map<@string, bool>(len(flagnames));
                foreach (var (_, name) in flagnames) {
                    groupStatus[group].Set(name, false);
                }
            }

            groupStatus[group].Set(pflag.Name, pflag.Changed);
        }
    }
}

internal static error validateRequiredFlagGroups(map<@string, map<@string, bool>> data) {
    var keys = sortedKeys(data);
    foreach (var (_, flagList) in keys) {
        var flagnameAndStatus = data[flagList];

        var unset = new @string[]{}.slice();
        foreach (var (flagname, isSet) in flagnameAndStatus) {
            if (!isSet) {
                unset = append(unset, flagname);
            }
        }
        if (len(unset) == len(flagnameAndStatus) || len(unset) == 0) {
            continue;
        }

        sort.Strings(unset);
        return fmt.Errorf("if any flags in the group [%v] are set they must all be set; missing %v"u8, flagList, unset);
    }

    return default!;
}

internal static error validateOneRequiredFlagGroups(map<@string, map<@string, bool>> data) {
    var setᴷ = new stackAppend<@string>();

    var keys = sortedKeys(data);
    foreach (var (_, flagList) in keys) {
        var flagnameAndStatus = data[flagList];
        slice<@string> set = default!;
        foreach (var (flagname, isSet) in flagnameAndStatus) {
            if (isSet) {
                set = append(setᴷ, set, flagname);
            }
        }
        if (len(set) >= 1) {
            continue;
        }

        sort.Strings(set);
        return fmt.Errorf("at least one of the flags in the group [%v] is required"u8, flagList);
    }
    return default!;
}

internal static error validateExclusiveFlagGroups(map<@string, map<@string, bool>> data) {
    var keys = sortedKeys(data);
    foreach (var (_, flagList) in keys) {
        var flagnameAndStatus = data[flagList];
        slice<@string> set = default!;
        foreach (var (flagname, isSet) in flagnameAndStatus) {
            if (isSet) {
                set = append(set, flagname);
            }
        }
        if (len(set) == 0 || len(set) == 1) {
            continue;
        }

        sort.Strings(set);
        return fmt.Errorf("if any flags in the group [%v] are set none of the others can be; %v were all set"u8, flagList, set);
    }
    return default!;
}

internal static slice<@string> sortedKeys(map<@string, map<@string, bool>> m) {
    var keys = new slice<@string>(len(m));
    nint i = 0;
    foreach (var (k, _) in m) {
        keys[i] = k;
        i++;
    }
    sort.Strings(keys);
    return keys;
}

internal static void enforceFlagGroupsForCompletion(this ref Command c) {
    if (c.DisableFlagParsing) {
        return;
    }

    var flags = c.Flags();
    var groupStatus = new map<@string, map<@string, bool>>{};
    var oneRequiredGroupStatus = new map<@string, map<@string, bool>>{};
    var mutuallyExclusiveGroupStatus = new map<@string, map<@string, bool>>{};
    var flagsʗ1 = flags;
    var groupStatusʗ1 = groupStatus;
    var mutuallyExclusiveGroupStatusʗ1 = mutuallyExclusiveGroupStatus;
    var oneRequiredGroupStatusʗ1 = oneRequiredGroupStatus;
    c.Flags().VisitAll((ж<flag.Flag> pflag) => {
        processFlagForGroupAnnotation(flagsʗ1, ref (pflag).DerefOrNull(), requiredAsGroupAnnotation, groupStatusʗ1);
        processFlagForGroupAnnotation(flagsʗ1, ref (pflag).DerefOrNull(), oneRequiredAnnotation, oneRequiredGroupStatusʗ1);
        processFlagForGroupAnnotation(flagsʗ1, ref (pflag).DerefOrNull(), mutuallyExclusiveAnnotation, mutuallyExclusiveGroupStatusʗ1);
    });

    foreach (var (flagList, flagnameAndStatus) in groupStatus) {
        foreach (var (_, isSet) in flagnameAndStatus) {
            if (isSet) {
                foreach (var (_, fName) in strings.Split(flagList, " ")) {
                    _ = c.MarkFlagRequired(fName);
                }
            }
        }
    }

    foreach (var (flagList, flagnameAndStatus) in oneRequiredGroupStatus) {
        var isSet = false;

        foreach (var (_, vᴛ1) in flagnameAndStatus) {
            isSet = vᴛ1;

            if (isSet) {
                break;
            }
        }

        if (!isSet) {
            foreach (var (_, fName) in strings.Split(flagList, " ")) {
                _ = c.MarkFlagRequired(fName);
            }
        }
    }

    foreach (var (flagList, flagnameAndStatus) in mutuallyExclusiveGroupStatus) {
        foreach (var (flagName, isSet) in flagnameAndStatus) {
            if (isSet) {
                foreach (var (_, fName) in strings.Split(flagList, " ")) {
                    if (fName != flagName) {
                        var flagΔ1 = c.Flags().Lookup(fName);
                        flagΔ1.Value.Hidden = true;
                    }
                }
            }
        }
    }
}

} // end cobra_package
