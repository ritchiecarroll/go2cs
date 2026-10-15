// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using label = @internal.runtime.pprof.label_package;
using slices = slices_package;
using strings = strings_package;
using @internal.runtime.pprof;
using ꓸꓸꓸstring = Span<@string>;

partial class pprof_package {

// LabelSet is a set of labels.
partial struct LabelSet {
    internal slice<label.Label> list;
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
    public partial ref @internal.runtime.pprof.label_package.Set Set { get; }
}

// String satisfies Stringer and returns key, value pairs in a consistent
// order.
internal static @string String(this ж<labelMap> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (Ꮡl == nil) {
        return ""u8;
    }
    var keyVals = new slice<@string>(0, len(l.Set.List));

    foreach (var (_, lbl) in l.Set.List) {
        keyVals = append(keyVals, fmt.Sprintf("%q:%q"u8, lbl.Key, lbl.Value));
    }

    slices.Sort<slice<@string>, @string>(keyVals);
    return "{"u8 + strings_package.Join(keyVals, ", "u8) + "}"u8;
}

// WithLabels returns a new [context.Context] with the given labels added.
// A label overwrites a prior label with the same key.
public static context.Context WithLabels(context.Context ctx, LabelSet labels) {
    var parentLabels = labelValue(ctx);
    return context.WithValue(ctx, new labelContextKey(nil), Ꮡ(new labelMap(mergeLabelSets(parentLabels.Set, labels))));
}

internal static label.Set mergeLabelSets(label.Set left, LabelSet right) {
    if (len(left.List) == 0){
        return label.NewSet(right.list);
    } else 
    if (len(right.list) == 0) {
        return left;
    }

    var (lList, rList) = (left.List, right.list);
    nint l = 0;
    nint r = 0;
    var result = new slice<label.Label>(0, len(rList));
    while (l < len(lList) && r < len(rList)) {
        var exprᴛ1 = strings_package.Compare(lList[l].Key, rList[r].Key);
        if (exprᴛ1 == -1) { // left key < right key
            result = append(result, lList[l]);
            l++;
        }
        else if (exprᴛ1 is 1) { // right key < left key
            result = append(result, rList[r]);
            r++;
        }
        else if (exprᴛ1 is 0) { // keys are equal, right value overwrites left value
            result = append(result, rList[r]);
            l++;
            r++;
        }
    }

    // Append the remaining elements
    result = appendꓸꓸꓸ(result, lList.slice(l));
    result = appendꓸꓸꓸ(result, rList.slice(r));

    return label.NewSet(result);
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
    var list = new slice<label.Label>(0, len(args) / 2);
    var sortedNoDupes = true;
    for (nint i = 0; i + 1 < len(args); i += 2) {
        list = append(list, new label.Label(Key: args[i], Value: args[i + 1]));
        sortedNoDupes = sortedNoDupes && (i < 2 || args[i] > args[i - 2]);
    }
    if (!sortedNoDupes) {
        // slow path: keys are unsorted, contain duplicates, or both
        slices.SortStableFunc(list, (label.Label a, label.Label b) => strings_package.Compare(a.Key, b.Key));
        var deduped = new slice<label.Label>(0, len(list));
        foreach (var (i, lbl) in list) {
            if (i == 0 || lbl.Key != list[i - 1].Key){
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
    foreach (var (_, lbl) in ctxLabels.Set.List) {
        if (lbl.Key == key) {
            return (lbl.Value, true);
        }
    }
    return ("", false);
}

// ForLabels invokes f with each label set on the context.
// The function f should return true to continue iteration or false to stop iteration early.
public static void ForLabels(context.Context ctx, Func<@string, @string, bool> f) {
    var ctxLabels = labelValue(ctx);
    foreach (var (_, lbl) in ctxLabels.Set.List) {
        if (!f(lbl.Key, lbl.Value)) {
            break;
        }
    }
}

} // end pprof_package
