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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using iter = iter_package;
using static global::go.net.http_package;
using ꓸꓸꓸstring = Span<@string>;

partial class http_internal_test_package {

internal partial struct TestRoutingFirstSegment_type /*dyn*/ {
    internal @string @in;
    internal slice<@string> want;
}

public static void TestRoutingFirstSegment(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestRoutingFirstSegment_type>([
        new("/a/b/c", slice<@string>(["a", "b", "c"])),
        new("/a/b/", slice<@string>(["a", "b", "/"])),
        new("/", slice<@string>(["/"])),
        new("/a/%62/c", slice<@string>(["a", "b", "c"])),
        new("/a%2Fb%2fc", slice<@string>(["a/b/c"]))
    ])) {
        slice<@string> got = default;
        @string rest = test.@in;
        while (builtin.len(rest) > 0) {
            @string seg = default;
            (seg, rest) = firstSegment(rest);
            got = append(got, seg);
        }
        if (!slices.Equal<slice<@string>, @string>(got, test.want)) {
            t.Errorf("%q: got %v, want %v", test.@in, got, test.want);
        }
    }
}

// TODO: test host and method
internal static ж<global::go.net.http_package.routingNode> testTree;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gHIˢ => ref gHIˢᵇ;
private static readonly @string gHIˢᵇ = "/g/h/i"u8;
internal static ref readonly @string gXJˢ => ref gXJˢᵇ;
private static readonly @string gXJˢᵇ = "/g/{x}/j"u8;
internal static ref readonly @string aBXˢ => ref aBXˢᵇ;
private static readonly @string aBXˢᵇ = "/a/b/{x...}"u8;
internal static ref readonly @string aBYˢ => ref aBYˢᵇ;
private static readonly @string aBYˢᵇ = "/a/b/{y}"u8;

internal static ж<global::go.net.http_package.routingNode> getTestTree() {
    if (testTree == nil) {
        testTree = buildTree("/a", "/a/b", "/a/{x}",
            gHIˢ, gXJˢ,
            aBXˢ, aBYˢ, "/a/b/{$}");
    }
    return testTree;
}

internal static ж<global::go.net.http_package.routingNode> buildTree(params ꓸꓸꓸstring patsʗp) {
    var pats = patsʗp.sslice();

    var root = Ꮡ(new routingNode());
    foreach (var (_, p) in pats) {
        var (pat, err) = parsePattern(p);
        if (err != nil) {
            throw panic(err);
        }
        root.addPattern(pat, default);
    }
    return root;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aAAXBABABYABMultiABXGJGXˢ => ref aAAXBABABYABMultiABXGJGXˢᵇ;
private static readonly @string aAAXBABABYABMultiABXGJGXˢᵇ = """
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

"""u8.Raw();

public static void TestRoutingAddPattern(ж<testing.T> t) {
    @string want = aAAXBABABYABMultiABXGJGXˢ;

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    getTestTree().print(new http_test_package.strings_BuilderжWriter(Ꮡb), 0);
    @string got = b.String();
    if (got != want) {
        t.Errorf("got\n%s\nwant\n%s", got, want);
    }
}

internal partial struct testCase {
    internal @string method, host, path;
    internal @string wantPat; // "" for nil (no match)
    internal slice<@string> wantMatches;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string itemˢ => ref itemˢᵇ;
private static readonly @string itemˢᵇ = "/item/"u8;
internal static ref readonly @string postItemUserˢ => ref postItemUserˢᵇ;
private static readonly @string postItemUserˢᵇ = "POST /item/{user}"u8;
internal static ref readonly @string getItemUserˢ => ref getItemUserˢᵇ;
private static readonly @string getItemUserˢᵇ = "GET /item/{user}"u8;
internal static ref readonly @string itemUserˢ => ref itemUserˢᵇ;
private static readonly @string itemUserˢᵇ = "/item/{user}"u8;
internal static ref readonly @string itemUserIdˢ => ref itemUserIdˢᵇ;
private static readonly @string itemUserIdˢᵇ = "/item/{user}/{id}"u8;
internal static ref readonly @string itemUserNewˢ => ref itemUserNewˢᵇ;
private static readonly @string itemUserNewˢᵇ = "/item/{user}/new"u8;
internal static ref readonly @string itemˢ2 => ref itemˢ2ᵇ;
private static readonly @string itemˢ2ᵇ = "/item/{$}"u8;
internal static ref readonly @string postAltComItemUserˢ => ref postAltComItemUserˢᵇ;
private static readonly @string postAltComItemUserˢᵇ = "POST alt.com/item/{user}"u8;
internal static ref readonly @string getHeadwinsˢ => ref getHeadwinsˢᵇ;
private static readonly @string getHeadwinsˢᵇ = "GET /headwins"u8;
internal static ref readonly @string headHeadwinsˢ => ref headHeadwinsˢᵇ;
private static readonly @string headHeadwinsˢᵇ = "HEAD /headwins"u8;
internal static ref readonly @string pathPˢ => ref pathPˢᵇ;
private static readonly @string pathPˢᵇ = "/path/{p...}"u8;
internal static ref readonly @string aBWˢ => ref aBWˢᵇ;
private static readonly @string aBWˢᵇ = "/a/b/{w}"u8;
internal static ref readonly @string aBWˢ2 => ref aBWˢ2ᵇ;
private static readonly @string aBWˢ2ᵇ = "/a/b/{w...}"u8;

public static partial void TestRoutingNodeMatch(ж<testing.T> t) {
    void test(ж<global::go.net.http_package.routingNode> treeΔ1, slice<testCase> tests) {
        t.Helper();
        foreach (var (_, testΔ1) in tests) {
            var (gotNode, gotMatches) = treeΔ1.match(testΔ1.host, testΔ1.method, testΔ1.path);
            @string got = ""u8;
            if (gotNode != nil) {
                got = gotNode.pattern.String();
            }
            if (got != testΔ1.wantPat) {
                t.Errorf("%s, %s, %s: got %q, want %q", testΔ1.host, testΔ1.method, testΔ1.path, got, testΔ1.wantPat);
            }
            if (!slices.Equal<slice<@string>, @string>(gotMatches, testΔ1.wantMatches)) {
                t.Errorf("%s, %s, %s: got matches %v, want %v", testΔ1.host, testΔ1.method, testΔ1.path, gotMatches, testΔ1.wantMatches);
            }
        }
    }

    test(getTestTree(), slice<testCase>([
        new("GET", "", "/a", "/a", default),
        new("Get", "", "/b", "", default),
        new("Get", "", "/a/b", "/a/b", default),
        new("Get", "", "/a/c", "/a/{x}", slice<@string>(["c"])),
        new("Get", "", "/a/b/", "/a/b/{$}", default),
        new("Get", "", "/a/b/c", "/a/b/{y}", slice<@string>(["c"])),
        new("Get", "", "/a/b/c/d", "/a/b/{x...}", slice<@string>(["c/d"])),
        new("Get", "", "/g/h/i", "/g/h/i", default),
        new("Get", "", "/g/h/j", "/g/{x}/j", slice<@string>(["h"]))
    ]));

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

    test(tree, slice<testCase>([
        new("GET", "", "/item/jba",
            "GET /item/{user}", slice<@string>(["jba"])),
        new("POST", "", "/item/jba",
            "POST /item/{user}", slice<@string>(["jba"])),
        new("HEAD", "", "/item/jba",
            "GET /item/{user}", slice<@string>(["jba"])),
        new("get", "", "/item/jba",
            "/item/{user}", slice<@string>(["jba"])), // method matches are case-sensitive
        new("POST", "", "/item/jba/17",
            "/item/{user}/{id}", slice<@string>(["jba", "17"])),
        new("GET", "", "/item/jba/new",
            "/item/{user}/new", slice<@string>(["jba"])),
        new("GET", "", "/item/",
            "/item/{$}", slice<@string>([])),
        new("GET", "", "/item/jba/17/line2",
            "/item/", default),
        new("POST", "alt.com", "/item/jba",
            "POST alt.com/item/{user}", slice<@string>(["jba"])),
        new("GET", "alt.com", "/item/jba",
            "GET /item/{user}", slice<@string>(["jba"])),
        new("GET", "", "/item",
            "", default), // does not match
        new("GET", "", "/headwins",
            "GET /headwins", default),
        new("HEAD", "", "/headwins", // HEAD is more specific than GET
            "HEAD /headwins", default),
        new("GET", "", "/path/to/file",
            "/path/{p...}", slice<@string>(["to/file"])),
        new("GET", "", "/path/*",
            "/path/{p...}", slice<@string>(["*"]))
    ]));

    // A pattern ending in {$} should only match URLS with a trailing slash.
    @string pat1 = "/a/b/{$}";
    test(buildTree(pat1), slice<testCase>([
        new("GET", "", "/a/b", "", default),
        new("GET", "", "/a/b/", pat1, default),
        new("GET", "", "/a/b/c", "", default),
        new("GET", "", "/a/b/c/d", "", default)
    ]));

    // A pattern ending in a single wildcard should not match a trailing slash URL.
    @string pat2 = aBWˢ;
    test(buildTree(pat2), slice<testCase>([
        new("GET", "", "/a/b", "", default),
        new("GET", "", "/a/b/", "", default),
        new("GET", "", "/a/b/c", pat2, slice<@string>(["c"])),
        new("GET", "", "/a/b/c/d", "", default)
    ]));

    // A pattern ending in a multi wildcard should match both URLs.
    @string pat3 = aBWˢ2;
    test(buildTree(pat3), slice<testCase>([
        new("GET", "", "/a/b", "", default),
        new("GET", "", "/a/b/", pat3, slice<@string>([""])),
        new("GET", "", "/a/b/c", pat3, slice<@string>(["c"])),
        new("GET", "", "/a/b/c/d", pat3, slice<@string>(["c/d"]))
    ]));

    // All three of the above should work together.
    test(buildTree(pat1, pat2, pat3), slice<testCase>([
        new("GET", "", "/a/b", "", default),
        new("GET", "", "/a/b/", pat1, default),
        new("GET", "", "/a/b/c", pat2, slice<@string>(["c"])),
        new("GET", "", "/a/b/c/d", pat3, slice<@string>(["c/d"]))
    ]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getAComˢ => ref getAComˢᵇ;
private static readonly @string getAComˢᵇ = "GET a.com/"u8;
internal static ref readonly @string putBComˢ => ref putBComˢᵇ;
private static readonly @string putBComˢᵇ = "PUT b.com/"u8;
internal static ref readonly @string postFooXˢ => ref postFooXˢᵇ;
private static readonly @string postFooXˢᵇ = "POST /foo/{x}"u8;
internal static ref readonly @string postˢ2 => ref postˢ2ᵇ;
private static readonly @string postˢ2ᵇ = "POST /"u8;
internal static ref readonly @string getˢ3 => ref getˢ3ᵇ;
private static readonly @string getˢ3ᵇ = "GET /"u8;

internal partial struct TestMatchingMethods_type /*dyn*/ {
    internal @string name;
    internal ж<global::go.net.http_package.routingNode> tree;
    internal @string host, path;
    internal @string want;
}

public static void TestMatchingMethods(ж<testing.T> t) {
    var hostTree = buildTree(getAComˢ, putBComˢ, postFooXˢ);
    foreach (var (_, vᴛ1) in slice<TestMatchingMethods_type>([
        new(
            "post",
            buildTree(postˢ2), "", "/foo",
            "POST"
        ),
        new(
            "get",
            buildTree(getˢ3), "", "/foo",
            "GET,HEAD"
        ),
        new(
            "host",
            hostTree, "", "/foo",
            ""
        ),
        new(
            "host",
            hostTree, "", "/foo/bar",
            "POST"
        ),
        new(
            "host2",
            hostTree, "a.com", "/foo/bar",
            "GET,HEAD,POST"
        ),
        new(
            "host3",
            hostTree, "b.com", "/bar",
            "PUT"
        ),
        new(
            // This case shouldn't come up because we only call matchingMethods
            // when there was no match, but we include it for completeness.
            "empty",
            buildTree("/"), "", "/",
            ""
        )
    ])) {
        ref var test = ref heap(new TestMatchingMethods_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var ms = new map<@string, bool>{};
            testʗ1.tree.matchingMethods(testʗ1.host, testʗ1.path, ms);
            @string got = strings.Join(slices.Sorted(maps.Keys<map<@string, bool>, @string, bool>(ms)), ",");
            if (got != testʗ1.want) {
                tΔ1.Errorf("got %s, want %s", got, testʗ1.want);
            }
        });
    }
}

internal static void print(this ж<global::go.net.http_package.routingNode> Ꮡn, io.Writer w, nint level) {
    ref var n = ref Ꮡn.DerefOrNull();

    @string indent = strings.Repeat("    ", level);
    if (n.pattern != nil) {
        fmt.Fprintf(w, "%s%q\n"u8, indent, n.pattern.OrTypedNil());
    }
    if (n.emptyChild != nil) {
        fmt.Fprintf(w, "%s%q:\n"u8, indent, (@string)""u8);
        n.emptyChild.print(w, level + 1);
    }

    ref var keys = ref heap<slice<@string>>(out var Ꮡkeys);
    Ꮡn.Ꮡchildren.eachPair((@string k, ж<global::go.net.http_package.routingNode> _Δp1) => {
        Ꮡkeys.ValueSlot = append(Ꮡkeys.ValueSlot, k);
        return true;
    });
    slices.Sort<slice<@string>, @string>(keys);

    foreach (var (_, k) in keys) {
        fmt.Fprintf(w, "%s%q:\n"u8, indent, k);
        var (nΔ1, _) = Ꮡn.Ꮡchildren.find(k);
        nΔ1.print(w, level + 1);
    }

    if (n.multiChild != nil) {
        fmt.Fprintf(w, "%sMULTI:\n"u8, indent);
        n.multiChild.print(w, level + 1);
    }
}

} // end http_internal_test_package
