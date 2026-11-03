// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.net.http_package;

partial class http_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string somethingˢ => ref somethingˢᵇ;
private static readonly @string somethingˢᵇ = "something"u8;
internal static ref readonly @string litˢ => ref litˢᵇ;
private static readonly @string litˢᵇ = "lit"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string foo12ˢ => ref foo12ˢᵇ;
private static readonly @string foo12ˢᵇ = "foo12"u8;
internal static ref readonly @string restˢ => ref restˢᵇ;
private static readonly @string restˢᵇ = "rest"u8;
internal static ref readonly @string barˢ2 => ref barˢ2ᵇ;
private static readonly @string barˢ2ᵇ = "bar"u8;

internal partial struct TestParsePattern_type /*dyn*/ {
    internal @string @in;
    internal global::go.net.http_package.pattern want;
}

public static void TestParsePattern(ж<testing.T> t) {
    global::go.net.http_package.segment lit(@string name) => new segment(s: name);

    global::go.net.http_package.segment wild(@string name) => new segment(s: name, wild: true);

    var wildʗ1 = wild;
    global::go.net.http_package.segment multi(@string name) {
        var s = wildʗ1(name);
        s.multi = true;
        return s;
    }

    foreach (var (_, vᴛ1) in slice<TestParsePattern_type>([
        new("/", new pattern(segments: slice<global::go.net.http_package.segment>([multi(""u8)]))),
        new("/a", new pattern(segments: slice<global::go.net.http_package.segment>([lit("a")]))),
        new(
            "/a/",
            new pattern(segments: slice<global::go.net.http_package.segment>([lit("a"), multi(""u8)]))
        ),
        new("/path/to/something", new pattern(segments: slice<global::go.net.http_package.segment>([
            lit(pathˢ2), lit("to"), lit(somethingˢ)
        ])
        )),
        new(
            "/{w1}/lit/{w2}",
            new pattern(
                segments: slice<global::go.net.http_package.segment>([wild("w1"), lit(litˢ), wild("w2")])
            )
        ),
        new(
            "/{w1}/lit/{w2}/",
            new pattern(
                segments: slice<global::go.net.http_package.segment>([wild("w1"), lit(litˢ), wild("w2"), multi(""u8)])
            )
        ),
        new(
            "example.com/",
            new pattern(host: "example.com", segments: slice<global::go.net.http_package.segment>([multi(""u8)]))
        ),
        new(
            "GET /",
            new pattern(method: "GET", segments: slice<global::go.net.http_package.segment>([multi(""u8)]))
        ),
        new(
            "POST example.com/foo/{w}",
            new pattern(
                method: "POST",
                host: "example.com",
                segments: slice<global::go.net.http_package.segment>([lit(fooˢ), wild("w")])
            )
        ),
        new(
            "/{$}",
            new pattern(segments: slice<global::go.net.http_package.segment>([lit("/")]))
        ),
        new(
            "DELETE example.com/a/{foo12}/{$}",
            new pattern(method: "DELETE", host: "example.com", segments: slice<global::go.net.http_package.segment>([lit("a"), wild(foo12ˢ), lit("/")]))
        ),
        new(
            "/foo/{$}",
            new pattern(segments: slice<global::go.net.http_package.segment>([lit(fooˢ), lit("/")]))
        ),
        new(
            "/{a}/foo/{rest...}",
            new pattern(segments: slice<global::go.net.http_package.segment>([wild("a"), lit(fooˢ), multi(restˢ)]))
        ),
        new(
            "//",
            new pattern(segments: slice<global::go.net.http_package.segment>([lit(""u8), multi(""u8)]))
        ),
        new(
            "/foo///./../bar",
            new pattern(segments: slice<global::go.net.http_package.segment>([lit(fooˢ), lit(""u8), lit(""u8), lit("."), lit(".."), lit(barˢ2)]))
        ),
        new(
            "a.com/foo//",
            new pattern(host: "a.com", segments: slice<global::go.net.http_package.segment>([lit(fooˢ), lit(""u8), multi(""u8)]))
        ),
        new(
            "/%61%62/%7b/%",
            new pattern(segments: slice<global::go.net.http_package.segment>([lit("ab"), lit("{"), lit("%")]))
        ),
        // Allow multiple spaces matching regexp '[ \t]+' between method and path.
        new(
            "GET\t  /",
            new pattern(method: "GET", segments: slice<global::go.net.http_package.segment>([multi(""u8)]))
        ),
        new(
            "POST \t  example.com/foo/{w}",
            new pattern(
                method: "POST",
                host: "example.com",
                segments: slice<global::go.net.http_package.segment>([lit(fooˢ), wild("w")])
            )
        ),
        new(
            "DELETE    \texample.com/a/{foo12}/{$}",
            new pattern(method: "DELETE", host: "example.com", segments: slice<global::go.net.http_package.segment>([lit("a"), wild(foo12ˢ), lit("/")]))
        )
    ])) {
        ref var test = ref heap(new TestParsePattern_type(), out var Ꮡtest);
        test = vᴛ1;

        var got = mustParsePattern(new http_test_package.testing_TжTB(t), test.@in);
        if (!got.equal(Ꮡtest.of(TestParsePattern_type.Ꮡwant))) {
            t.Errorf("%q:\ngot  %#v\nwant %#v", test.@in, got.OrTypedNil(), Ꮡtest.of(TestParsePattern_type.Ꮡwant));
        }
    }
}

internal partial struct TestParsePatternError_type /*dyn*/ {
    internal @string @in;
    internal @string contains;
}

public static void TestParsePatternError(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestParsePatternError_type>([
        new("", "empty pattern"),
        new("A=B /", "at offset 0: invalid method"),
        new(" ", "at offset 1: host/path missing /"),
        new("/{w}x", "at offset 1: bad wildcard segment"),
        new("/x{w}", "at offset 1: bad wildcard segment"),
        new("/{wx", "at offset 1: bad wildcard segment"),
        new("/a/{/}/c", "at offset 3: bad wildcard segment"),
        new("/a/{%61}/c", "at offset 3: bad wildcard name"), // wildcard names aren't unescaped
        new("/{a$}", "at offset 1: bad wildcard name"),
        new("/{}", "at offset 1: empty wildcard"),
        new("POST a.com/x/{}/y", "at offset 13: empty wildcard"),
        new("/{...}", "at offset 1: empty wildcard"),
        new("/{$...}", "at offset 1: bad wildcard"),
        new("/{$}/", "at offset 1: {$} not at end"),
        new("/{$}/x", "at offset 1: {$} not at end"),
        new("/abc/{$}/x", "at offset 5: {$} not at end"),
        new("/{a...}/", "at offset 1: {...} wildcard not at end"),
        new("/{a...}/x", "at offset 1: {...} wildcard not at end"),
        new("{a}/b", "at offset 0: host contains '{' (missing initial '/'?)"),
        new("/a/{x}/b/{x...}", "at offset 9: duplicate wildcard name"),
        new("GET //", "at offset 4: non-CONNECT pattern with unclean path")
    ])) {
        var (_, err) = parsePattern(test.@in);
        if (err == nil || !strings.Contains(err.Error(), test.contains)) {
            t.Errorf("%q:\ngot %v, want error containing %q", test.@in, err, test.contains);
        }
    }
}

internal static bool equal(this ref global::go.net.http_package.pattern p1, ж<global::go.net.http_package.pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.DerefOrNull();

    return p1.method == p2.method && p1.host == p2.host && slices.Equal<slice<global::go.net.http_package.segment>, global::go.net.http_package.segment>(p1.segments, p2.segments);
}

internal static ж<global::go.net.http_package.pattern> mustParsePattern(testing.TB tb, @string s) {
    tb.Helper();
    var (p, err) = parsePattern(s);
    if (err != nil) {
        tb.Fatal(err);
    }
    return p;
}

internal partial struct TestCompareMethods_type /*dyn*/ {
    internal @string p1, p2;
    internal global::go.net.http_package.relationship want;
}

public static void TestCompareMethods(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestCompareMethods_type>([
        new("/", "/", equivalent),
        new("GET /", "GET /", equivalent),
        new("HEAD /", "HEAD /", equivalent),
        new("POST /", "POST /", equivalent),
        new("GET /", "POST /", disjoint),
        new("GET /", "/", moreSpecific),
        new("HEAD /", "/", moreSpecific),
        new("GET /", "HEAD /", moreGeneral)
    ])) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p2);
        global::go.net.http_package.relationship got = pat1.compareMethods(pat2);
        if (got != test.want) {
            t.Errorf("%s vs %s: got %s, want %s", test.p1, test.p2, got, test.want);
        }
        global::go.net.http_package.relationship got2 = pat2.compareMethods(pat1);
        global::go.net.http_package.relationship want2 = inverseRelationship(test.want);
        if (got2 != want2) {
            t.Errorf("%s vs %s: got %s, want %s", test.p2, test.p1, got2, want2);
        }
    }
}

internal partial struct TestComparePaths_type /*dyn*/ {
    internal @string p1, p2;
    internal global::go.net.http_package.relationship want;
}

public static void TestComparePaths(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestComparePaths_type>([
        // A non-final pattern segment can have one of two values: literal or
        // single wildcard. A final pattern segment can have one of 5: empty
        // (trailing slash), literal, dollar, single wildcard, or multi
        // wildcard. Trailing slash and multi wildcard are the same.

        // A literal should be more specific than anything it overlaps, except itself.
        new("/a", "/a", equivalent),
        new("/a", "/b", disjoint),
        new("/a", "/", moreSpecific),
        new("/a", "/{$}", disjoint),
        new("/a", "/{x}", moreSpecific),
        new("/a", "/{x...}", moreSpecific),

        // Adding a segment doesn't change that.
        new("/b/a", "/b/a", equivalent),
        new("/b/a", "/b/b", disjoint),
        new("/b/a", "/b/", moreSpecific),
        new("/b/a", "/b/{$}", disjoint),
        new("/b/a", "/b/{x}", moreSpecific),
        new("/b/a", "/b/{x...}", moreSpecific),
        new("/{z}/a", "/{z}/a", equivalent),
        new("/{z}/a", "/{z}/b", disjoint),
        new("/{z}/a", "/{z}/", moreSpecific),
        new("/{z}/a", "/{z}/{$}", disjoint),
        new("/{z}/a", "/{z}/{x}", moreSpecific),
        new("/{z}/a", "/{z}/{x...}", moreSpecific),

        // Single wildcard on left.
        new("/{z}", "/a", moreGeneral),
        new("/{z}", "/a/b", disjoint),
        new("/{z}", "/{$}", disjoint),
        new("/{z}", "/{x}", equivalent),
        new("/{z}", "/", moreSpecific),
        new("/{z}", "/{x...}", moreSpecific),
        new("/b/{z}", "/b/a", moreGeneral),
        new("/b/{z}", "/b/a/b", disjoint),
        new("/b/{z}", "/b/{$}", disjoint),
        new("/b/{z}", "/b/{x}", equivalent),
        new("/b/{z}", "/b/", moreSpecific),
        new("/b/{z}", "/b/{x...}", moreSpecific),

        // Trailing slash on left.
        new("/", "/a", moreGeneral),
        new("/", "/a/b", moreGeneral),
        new("/", "/{$}", moreGeneral),
        new("/", "/{x}", moreGeneral),
        new("/", "/", equivalent),
        new("/", "/{x...}", equivalent),

        new("/b/", "/b/a", moreGeneral),
        new("/b/", "/b/a/b", moreGeneral),
        new("/b/", "/b/{$}", moreGeneral),
        new("/b/", "/b/{x}", moreGeneral),
        new("/b/", "/b/", equivalent),
        new("/b/", "/b/{x...}", equivalent),

        new("/{z}/", "/{z}/a", moreGeneral),
        new("/{z}/", "/{z}/a/b", moreGeneral),
        new("/{z}/", "/{z}/{$}", moreGeneral),
        new("/{z}/", "/{z}/{x}", moreGeneral),
        new("/{z}/", "/{z}/", equivalent),
        new("/{z}/", "/a/", moreGeneral),
        new("/{z}/", "/{z}/{x...}", equivalent),
        new("/{z}/", "/a/{x...}", moreGeneral),
        new("/a/{z}/", "/{z}/a/", overlaps),
        new("/a/{z}/b/", "/{x}/c/{y...}", overlaps),

        // Multi wildcard on left.
        new("/{m...}", "/a", moreGeneral),
        new("/{m...}", "/a/b", moreGeneral),
        new("/{m...}", "/{$}", moreGeneral),
        new("/{m...}", "/{x}", moreGeneral),
        new("/{m...}", "/", equivalent),
        new("/{m...}", "/{x...}", equivalent),

        new("/b/{m...}", "/b/a", moreGeneral),
        new("/b/{m...}", "/b/a/b", moreGeneral),
        new("/b/{m...}", "/b/{$}", moreGeneral),
        new("/b/{m...}", "/b/{x}", moreGeneral),
        new("/b/{m...}", "/b/", equivalent),
        new("/b/{m...}", "/b/{x...}", equivalent),
        new("/b/{m...}", "/a/{x...}", disjoint),

        new("/{z}/{m...}", "/{z}/a", moreGeneral),
        new("/{z}/{m...}", "/{z}/a/b", moreGeneral),
        new("/{z}/{m...}", "/{z}/{$}", moreGeneral),
        new("/{z}/{m...}", "/{z}/{x}", moreGeneral),
        new("/{z}/{m...}", "/{w}/", equivalent),
        new("/{z}/{m...}", "/a/", moreGeneral),
        new("/{z}/{m...}", "/{z}/{x...}", equivalent),
        new("/{z}/{m...}", "/a/{x...}", moreGeneral),
        new("/a/{m...}", "/a/b/{y...}", moreGeneral),
        new("/a/{m...}", "/a/{x}/{y...}", moreGeneral),
        new("/a/{z}/{m...}", "/a/b/{y...}", moreGeneral),
        new("/a/{z}/{m...}", "/{z}/a/", overlaps),
        new("/a/{z}/{m...}", "/{z}/b/{y...}", overlaps),
        new("/a/{z}/b/{m...}", "/{x}/c/{y...}", overlaps),
        new("/a/{z}/a/{m...}", "/{x}/b", disjoint),

        // Dollar on left.
        new("/{$}", "/a", disjoint),
        new("/{$}", "/a/b", disjoint),
        new("/{$}", "/{$}", equivalent),
        new("/{$}", "/{x}", disjoint),
        new("/{$}", "/", moreSpecific),
        new("/{$}", "/{x...}", moreSpecific),

        new("/b/{$}", "/b", disjoint),
        new("/b/{$}", "/b/a", disjoint),
        new("/b/{$}", "/b/a/b", disjoint),
        new("/b/{$}", "/b/{$}", equivalent),
        new("/b/{$}", "/b/{x}", disjoint),
        new("/b/{$}", "/b/", moreSpecific),
        new("/b/{$}", "/b/{x...}", moreSpecific),
        new("/b/{$}", "/b/c/{x...}", disjoint),
        new("/b/{x}/a/{$}", "/{x}/c/{y...}", overlaps),
        new("/{x}/b/{$}", "/a/{x}/{y}", disjoint),
        new("/{x}/b/{$}", "/a/{x}/c", disjoint),

        new("/{z}/{$}", "/{z}/a", disjoint),
        new("/{z}/{$}", "/{z}/a/b", disjoint),
        new("/{z}/{$}", "/{z}/{$}", equivalent),
        new("/{z}/{$}", "/{z}/{x}", disjoint),
        new("/{z}/{$}", "/{z}/", moreSpecific),
        new("/{z}/{$}", "/a/", overlaps),
        new("/{z}/{$}", "/a/{x...}", overlaps),
        new("/{z}/{$}", "/{z}/{x...}", moreSpecific),
        new("/a/{z}/{$}", "/{z}/a/", overlaps)
    ])) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p2);
        {
            global::go.net.http_package.relationship g = pat1.comparePaths(pat1); if (g != equivalent) {
                t.Errorf("%s does not match itself; got %s", pat1.OrTypedNil(), g);
            }
        }
        {
            global::go.net.http_package.relationship g = pat2.comparePaths(pat2); if (g != equivalent) {
                t.Errorf("%s does not match itself; got %s", pat2.OrTypedNil(), g);
            }
        }
        global::go.net.http_package.relationship got = pat1.comparePaths(pat2);
        if (got != test.want) {
            t.Errorf("%s vs %s: got %s, want %s", test.p1, test.p2, got, test.want);
            t.Logf("pat1: %+v\n", pat1.segments);
            t.Logf("pat2: %+v\n", pat2.segments);
        }
        global::go.net.http_package.relationship want2 = inverseRelationship(test.want);
        global::go.net.http_package.relationship got2 = pat2.comparePaths(pat1);
        if (got2 != want2) {
            t.Errorf("%s vs %s: got %s, want %s", test.p2, test.p1, got2, want2);
        }
    }
}

internal partial struct TestConflictsWith_type /*dyn*/ {
    internal @string p1, p2;
    internal bool want;
}

public static void TestConflictsWith(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestConflictsWith_type>([
        new("/a", "/a", true),
        new("/a", "/ab", false),
        new("/a/b/cd", "/a/b/cd", true),
        new("/a/b/cd", "/a/b/c", false),
        new("/a/b/c", "/a/c/c", false),
        new("/{x}", "/{y}", true),
        new("/{x}", "/a", false), // more specific
        new("/{x}/{y}", "/{x}/a", false),
        new("/{x}/{y}", "/{x}/a/b", false),
        new("/{x}", "/a/{y}", false),
        new("/{x}/{y}", "/{x}/a/", false),
        new("/{x}", "/a/{y...}", false),           // more specific
        new("/{x}/a/{y}", "/{x}/a/{y...}", false), // more specific
        new("/{x}/{y}", "/{x}/a/{$}", false),      // more specific
        new("/{x}/{y}/{$}", "/{x}/a/{$}", false),
        new("/a/{x}", "/{x}/b", true),
        new("/", "GET /", false),
        new("/", "GET /foo", false),
        new("GET /", "GET /foo", false),
        new("GET /", "/foo", true),
        new("GET /foo", "HEAD /", true)
    ])) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p2);
        var got = pat1.conflictsWith(pat2);
        if (got != test.want) {
            t.Errorf("%q.ConflictsWith(%q) = %t, want %t",
                test.p1, test.p2, got, test.want);
        }
        // conflictsWith should be commutative.
        got = pat2.conflictsWith(pat1);
        if (got != test.want) {
            t.Errorf("%q.ConflictsWith(%q) = %t, want %t",
                test.p2, test.p1, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aYZˢ => ref aYZˢᵇ;
private static readonly @string aYZˢᵇ = "/a/{y}/{z...}"u8;
internal static ref readonly @string matchesTheSameRequestsAsˢ => ref matchesTheSameRequestsAsˢᵇ;
private static readonly @string matchesTheSameRequestsAsˢᵇ = "matches the same requests as"u8;

public static partial void TestRegisterConflict(ж<testing.T> t) {
    var mux = NewServeMux();
    @string pat1 = "/a/{x}/";
    {
        var errΔ1 = mux.registerErr(pat1, NotFoundHandler()); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    @string pat2 = aYZˢ;
    var err = mux.registerErr(pat2, NotFoundHandler());
    @string got = default;
    if (err == nil) {
        got = nilˢ;
    } else {
        got = err.Error();
    }
    @string want = matchesTheSameRequestsAsˢ;
    if (!strings.Contains(got, want)) {
        t.Errorf("got\n%s\nwant\n%s", got, want);
    }
}

internal partial struct TestDescribeConflict_type /*dyn*/ {
    internal @string p1, p2;
    internal @string want;
}

public static void TestDescribeConflict(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestDescribeConflict_type>([
        new("/a/{x}", "/a/{y}", "the same requests"),
        new("/", "/{m...}", "the same requests"),
        new("/a/{x}", "/{y}/b", "both match some paths"),
        new("/a", "GET /{x}", "matches more methods than GET /{x}, but has a more specific path pattern"),
        new("GET /a", "HEAD /", "matches more methods than HEAD /, but has a more specific path pattern"),
        new("POST /", "/a", "matches fewer methods than /a, but has a more general path pattern")
    ])) {
        @string got = describeConflict(mustParsePattern(new http_test_package.testing_TжTB(t), test.p1), mustParsePattern(new http_test_package.testing_TжTB(t), test.p2));
        if (!strings.Contains(got, test.want)) {
            t.Errorf("%s vs. %s:\ngot:\n%s\nwhich does not contain %q",
                test.p1, test.p2, got, test.want);
        }
    }
}

internal partial struct TestCommonPath_type /*dyn*/ {
    internal @string p1, p2;
    internal @string want;
}

public static void TestCommonPath(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestCommonPath_type>([
        new("/a/{x}", "/{x}/a", "/a/a"),
        new("/a/{z}/", "/{z}/a/", "/a/a/"),
        new("/a/{z}/{m...}", "/{z}/a/", "/a/a/"),
        new("/{z}/{$}", "/a/", "/a/"),
        new("/{z}/{$}", "/a/{x...}", "/a/"),
        new("/a/{z}/{$}", "/{z}/a/", "/a/a/"),
        new("/a/{x}/b/{y...}", "/{x}/c/{y...}", "/a/c/b/"),
        new("/a/{x}/b/", "/{x}/c/{y...}", "/a/c/b/"),
        new("/a/{x}/b/{$}", "/{x}/c/{y...}", "/a/c/b/"),
        new("/a/{z}/{x...}", "/{z}/b/{y...}", "/a/b/")
    ])) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p2);
        if (pat1.comparePaths(pat2) != overlaps) {
            t.Fatalf("%s does not overlap %s", test.p1, test.p2);
        }
        @string got = commonPath(ref (pat1).DerefOrNull(), ref (pat2).DerefOrNull());
        if (got != test.want) {
            t.Errorf("%s vs. %s: got %q, want %q", test.p1, test.p2, got, test.want);
        }
    }
}

internal partial struct TestDifferencePath_type /*dyn*/ {
    internal @string p1, p2;
    internal @string want;
}

public static void TestDifferencePath(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestDifferencePath_type>([
        new("/a/{x}", "/{x}/a", "/a/x"),
        new("/{x}/a", "/a/{x}", "/x/a"),
        new("/a/{z}/", "/{z}/a/", "/a/z/"),
        new("/{z}/a/", "/a/{z}/", "/z/a/"),
        new("/{a}/a/", "/a/{z}/", "/ax/a/"),
        new("/a/{z}/{x...}", "/{z}/b/{y...}", "/a/z/"),
        new("/{z}/b/{y...}", "/a/{z}/{x...}", "/z/b/"),
        new("/a/b/", "/a/b/c", "/a/b/"),
        new("/a/b/{x...}", "/a/b/c", "/a/b/"),
        new("/a/b/{x...}", "/a/b/c/d", "/a/b/"),
        new("/a/b/{x...}", "/a/b/c/d/", "/a/b/"),
        new("/a/{z}/{m...}", "/{z}/a/", "/a/z/"),
        new("/{z}/a/", "/a/{z}/{m...}", "/z/a/"),
        new("/{z}/{$}", "/a/", "/z/"),
        new("/a/", "/{z}/{$}", "/a/x"),
        new("/{z}/{$}", "/a/{x...}", "/z/"),
        new("/a/{foo...}", "/{z}/{$}", "/a/foo"),
        new("/a/{z}/{$}", "/{z}/a/", "/a/z/"),
        new("/{z}/a/", "/a/{z}/{$}", "/z/a/x"),
        new("/a/{x}/b/{y...}", "/{x}/c/{y...}", "/a/x/b/"),
        new("/{x}/c/{y...}", "/a/{x}/b/{y...}", "/x/c/"),
        new("/a/{c}/b/", "/{x}/c/{y...}", "/a/cx/b/"),
        new("/{x}/c/{y...}", "/a/{c}/b/", "/x/c/"),
        new("/a/{x}/b/{$}", "/{x}/c/{y...}", "/a/x/b/"),
        new("/{x}/c/{y...}", "/a/{x}/b/{$}", "/x/c/")
    ])) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(t), test.p2);
        global::go.net.http_package.relationship rel = pat1.comparePaths(pat2);
        if (rel != overlaps && rel != moreGeneral) {
            t.Fatalf("%s vs. %s are %s, need overlaps or moreGeneral", pat1.OrTypedNil(), pat2.OrTypedNil(), rel);
        }
        @string got = differencePath(ref (pat1).DerefOrNull(), ref (pat2).DerefOrNull());
        if (got != test.want) {
            t.Errorf("%s vs. %s: got %q, want %q", test.p1, test.p2, got, test.want);
        }
    }
}

} // end http_internal_test_package
