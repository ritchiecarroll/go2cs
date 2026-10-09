// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using fmt = fmt_package;
using io = io_package;
using maps = maps_package;
using strings = strings_package;
using testing = testing_package;
using slices = slices_package;
using iter = iter_package;
using static global::go.net.http_package;
using ꓸꓸꓸstring = Span<@string>;

partial class http_internal_test_package {

internal partial struct TestRoutingFirstSegment_type /*dyn*/ {
    internal @string @in;
    internal slice<@string> want;
}

public static void TestRoutingFirstSegment(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestRoutingFirstSegment_type[]{
        new("/a/b/c"u8, new @string[]{"a"u8, "b"u8, "c"u8}.slice()),
        new("/a/b/"u8, new @string[]{"a"u8, "b"u8, "/"u8}.slice()),
        new("/"u8, new @string[]{"/"u8}.slice()),
        new("/a/%62/c"u8, new @string[]{"a"u8, "b"u8, "c"u8}.slice()),
        new("/a%2Fb%2fc"u8, new @string[]{"a/b/c"u8}.slice())
    }.slice()) {
        slice<@string> got = default!;
        @string rest = test.@in;
        while (builtin.len(rest) > 0) {
            @string seg = default!;
            (seg, rest) = firstSegment(rest);
            got = append(got, seg);
        }
        if (!slices.Equal<slice<@string>, @string>(got, test.want)) {
            Ꮡt.Errorf("%q: got %v, want %v"u8, test.@in, got, test.want);
        }
    }
}

// TODO: test host and method
internal static ж<global::go.net.http_package.routingNode> testTree;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gHIˢ = "/g/h/i"u8;
internal static readonly @string gXJˢ = "/g/{x}/j"u8;
internal static readonly @string aBXˢ = "/a/b/{x...}"u8;
internal static readonly @string aBYˢ = "/a/b/{y}"u8;

internal static ж<global::go.net.http_package.routingNode> getTestTree() {
    if (testTree == nil) {
        testTree = buildTree("/a"u8, "/a/b", "/a/{x}",
            gHIˢ, gXJˢ,
            aBXˢ, aBYˢ, "/a/b/{$}");
    }
    return testTree;
}

internal static ж<global::go.net.http_package.routingNode> buildTree(params ꓸꓸꓸstring patsʗp) {
    var pats = patsʗp.sslice();

    var root = Ꮡ(new routingNode(nil));
    foreach (var (_, p) in pats) {
        var (pat, err) = parsePattern(p);
        if (err != default!) {
            throw panic(err);
        }
        root.addPattern(pat, default!);
    }
    return root;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aAAXBABABYABMultiABXGJGXˢ = """
"":
    "":
        "a":
            "/a"
            "":
                "/a/{x}"
            "b":
                "/a/b"
                "":
                    "/a/b/{y}"
                "/":
                    "/a/b/{$}"
                MULTI:
                    "/a/b/{x...}"
        "g":
            "":
                "j":
                    "/g/{x}/j"
            "h":
                "i":
                    "/g/h/i"

"""u8;

public static void TestRoutingAddPattern(ж<testing.T> Ꮡt) {
    @string want = aAAXBABABYABMultiABXGJGXˢ;
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    getTestTree().print(new http_test_package.strings_BuilderжWriter(Ꮡb), 0);
    @string got = b.String();
    if (got != want) {
        Ꮡt.Errorf("got\n%s\nwant\n%s"u8, got, want);
    }
}

internal partial struct testCase {
    internal @string method, host, path;
    internal @string wantPat; // "" for nil (no match)
    internal slice<@string> wantMatches;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string itemˢ = "/item/"u8;
internal static readonly @string postItemUserˢ = "POST /item/{user}"u8;
internal static readonly @string getItemUserˢ = "GET /item/{user}"u8;
internal static readonly @string itemUserˢ = "/item/{user}"u8;
internal static readonly @string itemUserIdˢ = "/item/{user}/{id}"u8;
internal static readonly @string itemUserNewˢ = "/item/{user}/new"u8;
internal static readonly @string itemˢ2 = "/item/{$}"u8;
internal static readonly @string postAltComItemUserˢ = "POST alt.com/item/{user}"u8;
internal static readonly @string getHeadwinsˢ = "GET /headwins"u8;
internal static readonly @string headHeadwinsˢ = "HEAD /headwins"u8;
internal static readonly @string pathPˢ = "/path/{p...}"u8;
internal static readonly @string aBWˢ = "/a/b/{w}"u8;
internal static readonly @string aBWˢ2 = "/a/b/{w...}"u8;

public static void TestRoutingNodeMatch(ж<testing.T> Ꮡt) {
    void test(ж<global::go.net.http_package.routingNode> treeΔ1, slice<testCase> tests) {
        Ꮡt.Helper();
        foreach (var (_, testΔ1) in tests) {
            var (gotNode, gotMatches) = treeΔ1.match(testΔ1.host, testΔ1.method, testΔ1.path);
            @string got = ""u8;
            if (gotNode != nil) {
                got = (~gotNode).pattern.String();
            }
            if (got != testΔ1.wantPat) {
                Ꮡt.Errorf("%s, %s, %s: got %q, want %q"u8, testΔ1.host, testΔ1.method, testΔ1.path, got, testΔ1.wantPat);
            }
            if (!slices.Equal<slice<@string>, @string>(gotMatches, testΔ1.wantMatches)) {
                Ꮡt.Errorf("%s, %s, %s: got matches %v, want %v"u8, testΔ1.host, testΔ1.method, testΔ1.path, gotMatches, testΔ1.wantMatches);
            }
        }
    }
    test(getTestTree(), new testCase[]{
        new("GET"u8, ""u8, "/a"u8, "/a"u8, default!),
        new("Get"u8, ""u8, "/b"u8, ""u8, default!),
        new("Get"u8, ""u8, "/a/b"u8, "/a/b"u8, default!),
        new("Get"u8, ""u8, "/a/c"u8, "/a/{x}"u8, new @string[]{"c"u8}.slice()),
        new("Get"u8, ""u8, "/a/b/"u8, "/a/b/{$}"u8, default!),
        new("Get"u8, ""u8, "/a/b/c"u8, "/a/b/{y}"u8, new @string[]{"c"u8}.slice()),
        new("Get"u8, ""u8, "/a/b/c/d"u8, "/a/b/{x...}"u8, new @string[]{"c/d"u8}.slice()),
        new("Get"u8, ""u8, "/g/h/i"u8, "/g/h/i"u8, default!),
        new("Get"u8, ""u8, "/g/h/j"u8, "/g/{x}/j"u8, new @string[]{"h"u8}.slice())
    }.slice());
    var tree = buildTree(
        itemˢ,
        postItemUserˢ,
        getItemUserˢ,
        itemUserˢ,
        itemUserIdˢ,
        itemUserNewˢ,
        itemˢ2,
        postAltComItemUserˢ,
        getHeadwinsˢ,
        headHeadwinsˢ,
        pathPˢ);
    test(tree, new testCase[]{
        new("GET"u8, ""u8, "/item/jba"u8,
            "GET /item/{user}"u8, new @string[]{"jba"u8}.slice()),
        new("POST"u8, ""u8, "/item/jba"u8,
            "POST /item/{user}"u8, new @string[]{"jba"u8}.slice()),
        new("HEAD"u8, ""u8, "/item/jba"u8,
            "GET /item/{user}"u8, new @string[]{"jba"u8}.slice()),
        new("get"u8, ""u8, "/item/jba"u8,
            "/item/{user}"u8, new @string[]{"jba"u8}.slice()), // method matches are case-sensitive

        new("POST"u8, ""u8, "/item/jba/17"u8,
            "/item/{user}/{id}"u8, new @string[]{"jba"u8, "17"u8}.slice()),
        new("GET"u8, ""u8, "/item/jba/new"u8,
            "/item/{user}/new"u8, new @string[]{"jba"u8}.slice()),
        new("GET"u8, ""u8, "/item/"u8,
            "/item/{$}"u8, new @string[]{}.slice()),
        new("GET"u8, ""u8, "/item/jba/17/line2"u8,
            "/item/"u8, default!),
        new("POST"u8, "alt.com"u8, "/item/jba"u8,
            "POST alt.com/item/{user}"u8, new @string[]{"jba"u8}.slice()),
        new("GET"u8, "alt.com"u8, "/item/jba"u8,
            "GET /item/{user}"u8, new @string[]{"jba"u8}.slice()),
        new("GET"u8, ""u8, "/item"u8,
            ""u8, default!), // does not match

        new("GET"u8, ""u8, "/headwins"u8,
            "GET /headwins"u8, default!),
        new("HEAD"u8, ""u8, "/headwins"u8, // HEAD is more specific than GET

            "HEAD /headwins"u8, default!),
        new("GET"u8, ""u8, "/path/to/file"u8,
            "/path/{p...}"u8, new @string[]{"to/file"u8}.slice()),
        new("GET"u8, ""u8, "/path/*"u8,
            "/path/{p...}"u8, new @string[]{"*"u8}.slice())
    }.slice());
    // A pattern ending in {$} should only match URLS with a trailing slash.
    @string pat1 = "/a/b/{$}"u8;
    test(buildTree(pat1), new testCase[]{
        new("GET"u8, ""u8, "/a/b"u8, ""u8, default!),
        new("GET"u8, ""u8, "/a/b/"u8, pat1, default!),
        new("GET"u8, ""u8, "/a/b/c"u8, ""u8, default!),
        new("GET"u8, ""u8, "/a/b/c/d"u8, ""u8, default!)
    }.slice());
    // A pattern ending in a single wildcard should not match a trailing slash URL.
    @string pat2 = aBWˢ;
    test(buildTree(pat2), new testCase[]{
        new("GET"u8, ""u8, "/a/b"u8, ""u8, default!),
        new("GET"u8, ""u8, "/a/b/"u8, ""u8, default!),
        new("GET"u8, ""u8, "/a/b/c"u8, pat2, new @string[]{"c"u8}.slice()),
        new("GET"u8, ""u8, "/a/b/c/d"u8, ""u8, default!)
    }.slice());
    // A pattern ending in a multi wildcard should match both URLs.
    @string pat3 = aBWˢ2;
    test(buildTree(pat3), new testCase[]{
        new("GET"u8, ""u8, "/a/b"u8, ""u8, default!),
        new("GET"u8, ""u8, "/a/b/"u8, pat3, new @string[]{""u8}.slice()),
        new("GET"u8, ""u8, "/a/b/c"u8, pat3, new @string[]{"c"u8}.slice()),
        new("GET"u8, ""u8, "/a/b/c/d"u8, pat3, new @string[]{"c/d"u8}.slice())
    }.slice());
    // All three of the above should work together.
    test(buildTree(pat1, pat2, pat3), new testCase[]{
        new("GET"u8, ""u8, "/a/b"u8, ""u8, default!),
        new("GET"u8, ""u8, "/a/b/"u8, pat1, default!),
        new("GET"u8, ""u8, "/a/b/c"u8, pat2, new @string[]{"c"u8}.slice()),
        new("GET"u8, ""u8, "/a/b/c/d"u8, pat3, new @string[]{"c/d"u8}.slice())
    }.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getAComˢ = "GET a.com/"u8;
internal static readonly @string putBComˢ = "PUT b.com/"u8;
internal static readonly @string postFooXˢ = "POST /foo/{x}"u8;
internal static readonly @string postˢ2 = "POST /"u8;
internal static readonly @string getˢ3 = "GET /"u8;

internal partial struct TestMatchingMethods_type /*dyn*/ {
    internal @string name;
    internal ж<global::go.net.http_package.routingNode> tree;
    internal @string host, path;
    internal @string want;
}

public static void TestMatchingMethods(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var hostTree = buildTree(getAComˢ, putBComˢ, postFooXˢ);
    foreach (var (_, vᴛ1) in new TestMatchingMethods_type[]{
        new(
            "post"u8,
            buildTree(postˢ2), ""u8, "/foo"u8,
            "POST"u8
        ),
        new(
            "get"u8,
            buildTree(getˢ3), ""u8, "/foo"u8,
            "GET,HEAD"u8
        ),
        new(
            "host"u8,
            hostTree, ""u8, "/foo"u8,
            ""u8
        ),
        new(
            "host"u8,
            hostTree, ""u8, "/foo/bar"u8,
            "POST"u8
        ),
        new(
            "host2"u8,
            hostTree, "a.com"u8, "/foo/bar"u8,
            "GET,HEAD,POST"u8
        ),
        new(
            "host3"u8,
            hostTree, "b.com"u8, "/bar"u8,
            "PUT"u8
        ),
        new(
            "empty"u8, // This case shouldn't come up because we only call matchingMethods
 // when there was no match, but we include it for completeness.

            buildTree("/"u8), ""u8, "/"u8,
            ""u8
        )
    }.slice()) {
        ref var test = ref heap(new TestMatchingMethods_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var ms = new map<@string, bool>{};
            testʗ1.tree.matchingMethods(testʗ1.host, testʗ1.path, ms);
            @string got = strings.Join(slices.Sorted(maps.Keys<map<@string, bool>, @string, bool>(ms)), ","u8);
            if (got != testʗ1.want) {
                tΔ1.Errorf("got %s, want %s"u8, got, testʗ1.want);
            }
        });
    }
}

internal static void print(this ж<global::go.net.http_package.routingNode> Ꮡn, io.Writer w, nint level) {
    ref var n = ref Ꮡn.DerefOrNull();

    @string indent = strings.Repeat("    "u8, level);
    if (n.pattern != nil) {
        fmt.Fprintf(w, "%s%q\n"u8, indent, n.pattern.OrTypedNil());
    }
    if (n.emptyChild != nil) {
        fmt.Fprintf(w, "%s%q:\n"u8, indent, (@string)""u8);
        n.emptyChild.print(w, level + 1);
    }
    ref var keys = ref heap<slice<@string>>(out var Ꮡkeys);
    Ꮡn.of(global::go.net.http_package.routingNode.Ꮡchildren).eachPair((@string k, ж<global::go.net.http_package.routingNode> _Δp1) => {
        Ꮡkeys.ValueSlot = append(Ꮡkeys.ValueSlot, k);
        return true;
    });
    slices.Sort<slice<@string>, @string>(keys);
    foreach (var (_, k) in keys) {
        fmt.Fprintf(w, "%s%q:\n"u8, indent, k);
        var (nΔ1, _) = Ꮡn.of(global::go.net.http_package.routingNode.Ꮡchildren).find(k);
        nΔ1.print(w, level + 1);
    }
    if (n.multiChild != nil) {
        fmt.Fprintf(w, "%sMULTI:\n"u8, indent);
        n.multiChild.print(w, level + 1);
    }
}

} // end http_internal_test_package
