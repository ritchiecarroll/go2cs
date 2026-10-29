// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using fmt = fmt_package;
using reflect = reflect_package;
using slices = slices_package;
using testing = testing_package;
using ꓸꓸꓸstring = Span<@string>;

partial class cryptotest_package {

// NoExtraMethods checks that the concrete type of *ms has no exported methods
// beyond the methods of the interface type of *ms, and any others specified in
// the allowed list.
//
// These methods are accessible through interface upgrades, so they end up part
// of the API even if undocumented per Hyrum's Law.
//
// ms must be a pointer to a non-nil interface.
public static void NoExtraMethods(ж<testing.T> t, any ms, params ꓸꓸꓸstring allowedʗp) {
    var allowed = allowedʗp.slice();

    t.Helper();
    var (extraMethodsΔ1, err) = extraMethods(ms);
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, m) in extraMethodsΔ1) {
        if (slices.Contains(allowed, m)) {
            continue;
        }
        t.Errorf("unexpected method %q"u8, m);
    }
}

internal static (slice<@string>, error) extraMethods(any ip) {
    var extraMethodsᴷ = new stackAppend<@string>(moves: true);

    var v = reflect.ValueOf(ip);
    if (v.Kind() != reflect.Ptr || v.Elem().Kind() != reflect.ΔInterface || v.Elem().IsNil()) {
        return (default, fmt.Errorf("argument must be a pointer to a non-nil interface"u8));
    }

    var interfaceType = v.Elem().Type();
    var concreteType = v.Elem().Elem().Type();

    var interfaceMethods = new map<@string, bool>();
    foreach (var i in range(interfaceType.NumMethod())) {
        interfaceMethods[interfaceType.Method(i).Name] = true;
    }

    slice<@string> extraMethods = default;
    foreach (var i in range(concreteType.NumMethod())) {
        var m = concreteType.Method(i);
        if (!m.IsExported()) {
            continue;
        }
        if (!interfaceMethods[m.Name]) {
            extraMethods = append(extraMethodsᴷ, extraMethods, m.Name);
        }
    }

    extraMethods = move2heap(extraMethodsᴷ, extraMethods);
    return (extraMethods, default);
}

} // end cryptotest_package
