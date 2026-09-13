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

[GoType] partial struct label {
    internal @string key;
    internal @string value;
}

// LabelSet is a set of labels.
[GoType] partial struct LabelSet {
    internal slice<label> list;
}

// labelContextKey is the type of contextKeys used for profiler labels.
[GoType] partial struct labelContextKey {
}

internal static labelMap labelValue(context.Context ctx) {
    var (labels, _) = ctx.Value(new labelContextKey(nil))._<ж<labelMap>>(ᐧ);
    if (labels == nil) {
        return ((labelMap)default!);
    }
    return labels.ValueSlot;
}

[GoType("map[@string, @string]")] partial struct labelMap;

// String satisfies Stringer and returns key, value pairs in a consistent
// order.
internal static @string String(this ж<labelMap> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    if (Ꮡl == nil) {
        return ""u8;
    }
    var keyVals = new slice<@string>(0, len(l));
    foreach (var (k, v) in l) {
        keyVals = append(keyVals, fmt.Sprintf("%q:%q"u8, k, v));
    }
    slices.Sort<slice<@string>, @string>(keyVals);
    return "{"u8 + strings_package.Join(keyVals, ", "u8) + "}"u8;
}

// WithLabels returns a new [context.Context] with the given labels added.
// A label overwrites a prior label with the same key.
public static context.Context WithLabels(context.Context ctx, LabelSet labels) {
    var parentLabels = labelValue(ctx);
    ref var childLabels = ref heap<labelMap>(out var ᏑchildLabels);
    childLabels = new labelMap(len(parentLabels));
    // TODO(matloob): replace the map implementation with something
    // more efficient so creating a child context WithLabels doesn't need
    // to clone the map.
    foreach (var (k, v) in parentLabels) {
        childLabels[k] = v;
    }
    foreach (var (_, label) in labels.list) {
        childLabels[label.key] = label.value;
    }
    return context.WithValue(ctx, new labelContextKey(nil), ᏑchildLabels);
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
    for (nint i = 0; i + 1 < len(args); i += 2) {
        list = append(list, new label(key: args[i], value: args[i + 1]));
    }
    return new LabelSet(list: list);
}

// Label returns the value of the label with the given key on ctx, and a boolean indicating
// whether that label exists.
public static (@string, bool) Label(context.Context ctx, @string key) {
    var ctxLabels = labelValue(ctx);
    var (v, ok) = ctxLabels[key, ꟷ];
    return (v, ok);
}

// ForLabels invokes f with each label set on the context.
// The function f should return true to continue iteration or false to stop iteration early.
public static void ForLabels(context.Context ctx, Func<@string, @string, bool> f) {
    var ctxLabels = labelValue(ctx);
    foreach (var (k, v) in ctxLabels) {
        if (!f(k, v)) {
            break;
        }
    }
}

} // end pprof_package
