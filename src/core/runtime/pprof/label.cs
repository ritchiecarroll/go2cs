// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using ꓸꓸꓸstring = Span<@string>;

partial class pprof_package {

partial struct label {
    internal @string key;
    internal @string value;
}

// LabelSet is a set of labels.
partial struct LabelSet {
    internal slice<label> list;
}

// labelContextKey is the type of contextKeys used for profiler labels.
partial struct labelContextKey {
}

internal static labelMap labelValue(context.Context ctx) {
    var (labels, _) = ctx.Value(new labelContextKey(nil))._<ж<labelMap>>(ᐧ);
    if (labels == nil) {
        return new labelMap(nil);
    }
    return labels.Value;
}

// labelMap is the representation of the label set held in the context type.
// This is an initial implementation, but it will be replaced with something
// that admits incremental immutable modification more efficiently.
partial struct labelMap {
    public partial ref LabelSet LabelSet { get; }
}

// String satisfies Stringer and returns key, value pairs in a consistent
// order.
internal static @string String(this ж<labelMap> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (Ꮡl == nil) {
        return ""u8;
    }
    var keyVals = new slice<@string>(0, len(l.list));
    foreach (var (_, lbl) in l.list) {
        keyVals = append(keyVals, fmt.Sprintf("%q:%q"u8, lbl.key, lbl.value));
    }
    slices.Sort<slice<@string>, @string>(keyVals);
    return "{"u8 + strings_package.Join(keyVals, ", "u8) + "}"u8;
}

// WithLabels returns a new [context.Context] with the given labels added.
// A label overwrites a prior label with the same key.
public static context.Context WithLabels(context.Context ctx, LabelSet labels) {
    var parentLabels = labelValue(ctx);
    return context.WithValue(ctx, new labelContextKey(nil), Ꮡ(new labelMap(mergeLabelSets(parentLabels.LabelSet, labels))));
}

internal static LabelSet mergeLabelSets(LabelSet left, LabelSet right) {
    if (len(left.list) == 0){
        return right;
    } else 
    if (len(right.list) == 0) {
        return left;
    }
    nint l = 0;
    nint r = 0;
    var result = new slice<label>(0, len(right.list));
    while (l < len(left.list) && r < len(right.list)) {
        var exprᴛ1 = strings_package.Compare(left.list[l].key, right.list[r].key);
        if (exprᴛ1 == -1) {
            result = append(result, // left key < right key
 left.list[l]);
            l++;
        }
        else if (exprᴛ1 is 1) {
            result = append(result, // right key < left key
 right.list[r]);
            r++;
        }
        else if (exprᴛ1 is 0) {
            result = append(result, // keys are equal, right value overwrites left value
 right.list[r]);
            l++;
            r++;
        }

    }
    // Append the remaining elements
    result = appendꓸꓸꓸ(result, left.list.slice(l));
    result = appendꓸꓸꓸ(result, right.list.slice(r));
    return new LabelSet(list: result);
}

// Labels takes an even number of strings representing key-value pairs
// and makes a [LabelSet] containing them.
// A label overwrites a prior label with the same key.
// Currently only the CPU and goroutine profiles utilize any labels
// information.
// See https://golang.org/issue/23458 for details.
public static LabelSet Labels(params ꓸꓸꓸstring argsʗp) {
    var args = argsʗp.sslice();

    if (len(args) % 2 != 0) {
        throw panic("uneven number of arguments to pprof.Labels");
    }
    var list = new slice<label>(0, len(args) / 2);
    var sortedNoDupes = true;
    for (nint i = 0; i + 1 < len(args); i += 2) {
        list = append(list, new label(key: args[i], value: args[i + 1]));
        sortedNoDupes = sortedNoDupes && (i < 2 || args[i] > args[i - 2]);
    }
    if (!sortedNoDupes) {
        // slow path: keys are unsorted, contain duplicates, or both
        slices.SortStableFunc(list, (label a, label b) => strings_package.Compare(a.key, b.key));
        var deduped = new slice<label>(0, len(list));
        foreach (var (i, lbl) in list) {
            if (i == 0 || lbl.key != list[i - 1].key){
                deduped = append(deduped, lbl);
            } else {
                deduped[len(deduped) - 1] = lbl;
            }
        }
        list = deduped;
    }
    return new LabelSet(list: list);
}

// Label returns the value of the label with the given key on ctx, and a boolean indicating
// whether that label exists.
public static (@string, bool) Label(context.Context ctx, @string key) {
    var ctxLabels = labelValue(ctx);
    foreach (var (_, lbl) in ctxLabels.list) {
        if (lbl.key == key) {
            return (lbl.value, true);
        }
    }
    return ("", false);
}

// ForLabels invokes f with each label set on the context.
// The function f should return true to continue iteration or false to stop iteration early.
public static void ForLabels(context.Context ctx, Func<@string, @string, bool> f) {
    var ctxLabels = labelValue(ctx);
    foreach (var (_, lbl) in ctxLabels.list) {
        if (!f(lbl.key, lbl.value)) {
            break;
        }
    }
}

} // end pprof_package
