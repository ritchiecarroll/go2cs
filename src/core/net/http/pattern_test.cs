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
internal static readonly @string somethingˢ = "something"u8;
internal static readonly @string litˢ = "lit"u8;
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string foo12ˢ = "foo12"u8;
internal static readonly @string restˢ = "rest"u8;
internal static readonly @string barˢ2 = "bar"u8;

[GoType("dyn")] internal partial struct TestParsePattern_type {
    internal @string @in;
    internal global::go.net.http_package.pattern want;
}

public static void TestParsePattern(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    global::go.net.http_package.segment lit(@string name) => new segment(s: name);
    global::go.net.http_package.segment wild(@string name) => new segment(s: name, wild: true);
    var wildʗ1 = wild;
    global::go.net.http_package.segment multi(@string name) {
        var s = wildʗ1(name);
        s.multi = true;
        return s;
    }
    foreach (var (_, vᴛ1) in new TestParsePattern_type[]{
        new("/"u8, new pattern(segments: new global::go.net.http_package.segment[]{multi(""u8)}.slice())),
        new("/a"u8, new pattern(segments: new global::go.net.http_package.segment[]{lit("a"u8)}.slice())),
        new(
            "/a/"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{lit("a"u8), multi(""u8)}.slice())
        ),
        new("/path/to/something"u8, new pattern(segments: new global::go.net.http_package.segment[]{
            lit(pathˢ2), lit("to"u8), lit(somethingˢ)
        }.slice()
        )),
        new(
            "/{w1}/lit/{w2}"u8,
            new pattern(
                segments: new global::go.net.http_package.segment[]{wild("w1"u8), lit(litˢ), wild("w2"u8)}.slice()
            )
        ),
        new(
            "/{w1}/lit/{w2}/"u8,
            new pattern(
                segments: new global::go.net.http_package.segment[]{wild("w1"u8), lit(litˢ), wild("w2"u8), multi(""u8)}.slice()
            )
        ),
        new(
            "example.com/"u8,
            new pattern(host: "example.com"u8, segments: new global::go.net.http_package.segment[]{multi(""u8)}.slice())
        ),
        new(
            "GET /"u8,
            new pattern(method: "GET"u8, segments: new global::go.net.http_package.segment[]{multi(""u8)}.slice())
        ),
        new(
            "POST example.com/foo/{w}"u8,
            new pattern(
                method: "POST"u8,
                host: "example.com"u8,
                segments: new global::go.net.http_package.segment[]{lit(fooˢ), wild("w"u8)}.slice()
            )
        ),
        new(
            "/{$}"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{lit("/"u8)}.slice())
        ),
        new(
            "DELETE example.com/a/{foo12}/{$}"u8,
            new pattern(method: "DELETE"u8, host: "example.com"u8, segments: new global::go.net.http_package.segment[]{lit("a"u8), wild(foo12ˢ), lit("/"u8)}.slice())
        ),
        new(
            "/foo/{$}"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{lit(fooˢ), lit("/"u8)}.slice())
        ),
        new(
            "/{a}/foo/{rest...}"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{wild("a"u8), lit(fooˢ), multi(restˢ)}.slice())
        ),
        new(
            "//"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{lit(""u8), multi(""u8)}.slice())
        ),
        new(
            "/foo///./../bar"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{lit(fooˢ), lit(""u8), lit(""u8), lit("."u8), lit(".."u8), lit(barˢ2)}.slice())
        ),
        new(
            "a.com/foo//"u8,
            new pattern(host: "a.com"u8, segments: new global::go.net.http_package.segment[]{lit(fooˢ), lit(""u8), multi(""u8)}.slice())
        ),
        new(
            "/%61%62/%7b/%"u8,
            new pattern(segments: new global::go.net.http_package.segment[]{lit("ab"u8), lit("{"u8), lit("%"u8)}.slice())
        ), // Allow multiple spaces matching regexp '[ \t]+' between method and path.

        new(
            "GET\t  /"u8,
            new pattern(method: "GET"u8, segments: new global::go.net.http_package.segment[]{multi(""u8)}.slice())
        ),
        new(
            "POST \t  example.com/foo/{w}"u8,
            new pattern(
                method: "POST"u8,
                host: "example.com"u8,
                segments: new global::go.net.http_package.segment[]{lit(fooˢ), wild("w"u8)}.slice()
            )
        ),
        new(
            "DELETE    \texample.com/a/{foo12}/{$}"u8,
            new pattern(method: "DELETE"u8, host: "example.com"u8, segments: new global::go.net.http_package.segment[]{lit("a"u8), wild(foo12ˢ), lit("/"u8)}.slice())
        )
    }.slice()) {
        ref var test = ref heap(new TestParsePattern_type(), out var Ꮡtest);
        test = vᴛ1;

        var got = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.@in);
        if (!got.equal(Ꮡtest.of(TestParsePattern_type.Ꮡwant))) {
            Ꮡt.Errorf("%q:\ngot  %#v\nwant %#v"u8, test.@in, got.OrTypedNil(), Ꮡtest.of(TestParsePattern_type.Ꮡwant));
        }
    }
}

[GoType("dyn")] internal partial struct TestParsePatternError_type {
    internal @string @in;
    internal @string contains;
}

public static void TestParsePatternError(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestParsePatternError_type[]{
        new(""u8, "empty pattern"u8),
        new("A=B /"u8, "at offset 0: invalid method"u8),
        new(" "u8, "at offset 1: host/path missing /"u8),
        new("/{w}x"u8, "at offset 1: bad wildcard segment"u8),
        new("/x{w}"u8, "at offset 1: bad wildcard segment"u8),
        new("/{wx"u8, "at offset 1: bad wildcard segment"u8),
        new("/a/{/}/c"u8, "at offset 3: bad wildcard segment"u8),
        new("/a/{%61}/c"u8, "at offset 3: bad wildcard name"u8), // wildcard names aren't unescaped

        new("/{a$}"u8, "at offset 1: bad wildcard name"u8),
        new("/{}"u8, "at offset 1: empty wildcard"u8),
        new("POST a.com/x/{}/y"u8, "at offset 13: empty wildcard"u8),
        new("/{...}"u8, "at offset 1: empty wildcard"u8),
        new("/{$...}"u8, "at offset 1: bad wildcard"u8),
        new("/{$}/"u8, "at offset 1: {$} not at end"u8),
        new("/{$}/x"u8, "at offset 1: {$} not at end"u8),
        new("/abc/{$}/x"u8, "at offset 5: {$} not at end"u8),
        new("/{a...}/"u8, "at offset 1: {...} wildcard not at end"u8),
        new("/{a...}/x"u8, "at offset 1: {...} wildcard not at end"u8),
        new("{a}/b"u8, "at offset 0: host contains '{' (missing initial '/'?)"u8),
        new("/a/{x}/b/{x...}"u8, "at offset 9: duplicate wildcard name"u8),
        new("GET //"u8, "at offset 4: non-CONNECT pattern with unclean path"u8)
    }.slice()) {
        var (_, err) = parsePattern(test.@in);
        if (err == default! || !strings.Contains(err.Error(), test.contains)) {
            Ꮡt.Errorf("%q:\ngot %v, want error containing %q"u8, test.@in, err, test.contains);
        }
    }
}

[GoRecv] internal static bool equal(this ref global::go.net.http_package.pattern p1, ж<global::go.net.http_package.pattern> Ꮡp2) {
    ref var p2 = ref Ꮡp2.DerefOrNull();

    return p1.method == p2.method && p1.host == p2.host && slices.Equal<slice<global::go.net.http_package.segment>, global::go.net.http_package.segment>(p1.segments, p2.segments);
}

internal static ж<global::go.net.http_package.pattern> mustParsePattern(testing.TB tb, @string s) {
    tb.Helper();
    var (p, err) = parsePattern(s);
    if (err != default!) {
        tb.Fatal(err);
    }
    return p;
}

[GoType("dyn")] internal partial struct TestCompareMethods_type {
    internal @string p1, p2;
    internal global::go.net.http_package.relationship want;
}

public static void TestCompareMethods(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestCompareMethods_type[]{
        new("/"u8, "/"u8, equivalent),
        new("GET /"u8, "GET /"u8, equivalent),
        new("HEAD /"u8, "HEAD /"u8, equivalent),
        new("POST /"u8, "POST /"u8, equivalent),
        new("GET /"u8, "POST /"u8, disjoint),
        new("GET /"u8, "/"u8, moreSpecific),
        new("HEAD /"u8, "/"u8, moreSpecific),
        new("GET /"u8, "HEAD /"u8, moreGeneral)
    }.slice()) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p2);
        global::go.net.http_package.relationship got = pat1.compareMethods(pat2);
        if (got != test.want) {
            Ꮡt.Errorf("%s vs %s: got %s, want %s"u8, test.p1, test.p2, got, test.want);
        }
        global::go.net.http_package.relationship got2 = pat2.compareMethods(pat1);
        global::go.net.http_package.relationship want2 = inverseRelationship(test.want);
        if (got2 != want2) {
            Ꮡt.Errorf("%s vs %s: got %s, want %s"u8, test.p2, test.p1, got2, want2);
        }
    }
}

[GoType("dyn")] internal partial struct TestComparePaths_type {
    internal @string p1, p2;
    internal global::go.net.http_package.relationship want;
}

public static void TestComparePaths(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestComparePaths_type[]{ // A non-final pattern segment can have one of two values: literal or
 // single wildcard. A final pattern segment can have one of 5: empty
 // (trailing slash), literal, dollar, single wildcard, or multi
 // wildcard. Trailing slash and multi wildcard are the same.
 // A literal should be more specific than anything it overlaps, except itself.

        new("/a"u8, "/a"u8, equivalent),
        new("/a"u8, "/b"u8, disjoint),
        new("/a"u8, "/"u8, moreSpecific),
        new("/a"u8, "/{$}"u8, disjoint),
        new("/a"u8, "/{x}"u8, moreSpecific),
        new("/a"u8, "/{x...}"u8, moreSpecific), // Adding a segment doesn't change that.

        new("/b/a"u8, "/b/a"u8, equivalent),
        new("/b/a"u8, "/b/b"u8, disjoint),
        new("/b/a"u8, "/b/"u8, moreSpecific),
        new("/b/a"u8, "/b/{$}"u8, disjoint),
        new("/b/a"u8, "/b/{x}"u8, moreSpecific),
        new("/b/a"u8, "/b/{x...}"u8, moreSpecific),
        new("/{z}/a"u8, "/{z}/a"u8, equivalent),
        new("/{z}/a"u8, "/{z}/b"u8, disjoint),
        new("/{z}/a"u8, "/{z}/"u8, moreSpecific),
        new("/{z}/a"u8, "/{z}/{$}"u8, disjoint),
        new("/{z}/a"u8, "/{z}/{x}"u8, moreSpecific),
        new("/{z}/a"u8, "/{z}/{x...}"u8, moreSpecific), // Single wildcard on left.

        new("/{z}"u8, "/a"u8, moreGeneral),
        new("/{z}"u8, "/a/b"u8, disjoint),
        new("/{z}"u8, "/{$}"u8, disjoint),
        new("/{z}"u8, "/{x}"u8, equivalent),
        new("/{z}"u8, "/"u8, moreSpecific),
        new("/{z}"u8, "/{x...}"u8, moreSpecific),
        new("/b/{z}"u8, "/b/a"u8, moreGeneral),
        new("/b/{z}"u8, "/b/a/b"u8, disjoint),
        new("/b/{z}"u8, "/b/{$}"u8, disjoint),
        new("/b/{z}"u8, "/b/{x}"u8, equivalent),
        new("/b/{z}"u8, "/b/"u8, moreSpecific),
        new("/b/{z}"u8, "/b/{x...}"u8, moreSpecific), // Trailing slash on left.

        new("/"u8, "/a"u8, moreGeneral),
        new("/"u8, "/a/b"u8, moreGeneral),
        new("/"u8, "/{$}"u8, moreGeneral),
        new("/"u8, "/{x}"u8, moreGeneral),
        new("/"u8, "/"u8, equivalent),
        new("/"u8, "/{x...}"u8, equivalent),
        new("/b/"u8, "/b/a"u8, moreGeneral),
        new("/b/"u8, "/b/a/b"u8, moreGeneral),
        new("/b/"u8, "/b/{$}"u8, moreGeneral),
        new("/b/"u8, "/b/{x}"u8, moreGeneral),
        new("/b/"u8, "/b/"u8, equivalent),
        new("/b/"u8, "/b/{x...}"u8, equivalent),
        new("/{z}/"u8, "/{z}/a"u8, moreGeneral),
        new("/{z}/"u8, "/{z}/a/b"u8, moreGeneral),
        new("/{z}/"u8, "/{z}/{$}"u8, moreGeneral),
        new("/{z}/"u8, "/{z}/{x}"u8, moreGeneral),
        new("/{z}/"u8, "/{z}/"u8, equivalent),
        new("/{z}/"u8, "/a/"u8, moreGeneral),
        new("/{z}/"u8, "/{z}/{x...}"u8, equivalent),
        new("/{z}/"u8, "/a/{x...}"u8, moreGeneral),
        new("/a/{z}/"u8, "/{z}/a/"u8, overlaps),
        new("/a/{z}/b/"u8, "/{x}/c/{y...}"u8, overlaps), // Multi wildcard on left.

        new("/{m...}"u8, "/a"u8, moreGeneral),
        new("/{m...}"u8, "/a/b"u8, moreGeneral),
        new("/{m...}"u8, "/{$}"u8, moreGeneral),
        new("/{m...}"u8, "/{x}"u8, moreGeneral),
        new("/{m...}"u8, "/"u8, equivalent),
        new("/{m...}"u8, "/{x...}"u8, equivalent),
        new("/b/{m...}"u8, "/b/a"u8, moreGeneral),
        new("/b/{m...}"u8, "/b/a/b"u8, moreGeneral),
        new("/b/{m...}"u8, "/b/{$}"u8, moreGeneral),
        new("/b/{m...}"u8, "/b/{x}"u8, moreGeneral),
        new("/b/{m...}"u8, "/b/"u8, equivalent),
        new("/b/{m...}"u8, "/b/{x...}"u8, equivalent),
        new("/b/{m...}"u8, "/a/{x...}"u8, disjoint),
        new("/{z}/{m...}"u8, "/{z}/a"u8, moreGeneral),
        new("/{z}/{m...}"u8, "/{z}/a/b"u8, moreGeneral),
        new("/{z}/{m...}"u8, "/{z}/{$}"u8, moreGeneral),
        new("/{z}/{m...}"u8, "/{z}/{x}"u8, moreGeneral),
        new("/{z}/{m...}"u8, "/{w}/"u8, equivalent),
        new("/{z}/{m...}"u8, "/a/"u8, moreGeneral),
        new("/{z}/{m...}"u8, "/{z}/{x...}"u8, equivalent),
        new("/{z}/{m...}"u8, "/a/{x...}"u8, moreGeneral),
        new("/a/{m...}"u8, "/a/b/{y...}"u8, moreGeneral),
        new("/a/{m...}"u8, "/a/{x}/{y...}"u8, moreGeneral),
        new("/a/{z}/{m...}"u8, "/a/b/{y...}"u8, moreGeneral),
        new("/a/{z}/{m...}"u8, "/{z}/a/"u8, overlaps),
        new("/a/{z}/{m...}"u8, "/{z}/b/{y...}"u8, overlaps),
        new("/a/{z}/b/{m...}"u8, "/{x}/c/{y...}"u8, overlaps),
        new("/a/{z}/a/{m...}"u8, "/{x}/b"u8, disjoint), // Dollar on left.

        new("/{$}"u8, "/a"u8, disjoint),
        new("/{$}"u8, "/a/b"u8, disjoint),
        new("/{$}"u8, "/{$}"u8, equivalent),
        new("/{$}"u8, "/{x}"u8, disjoint),
        new("/{$}"u8, "/"u8, moreSpecific),
        new("/{$}"u8, "/{x...}"u8, moreSpecific),
        new("/b/{$}"u8, "/b"u8, disjoint),
        new("/b/{$}"u8, "/b/a"u8, disjoint),
        new("/b/{$}"u8, "/b/a/b"u8, disjoint),
        new("/b/{$}"u8, "/b/{$}"u8, equivalent),
        new("/b/{$}"u8, "/b/{x}"u8, disjoint),
        new("/b/{$}"u8, "/b/"u8, moreSpecific),
        new("/b/{$}"u8, "/b/{x...}"u8, moreSpecific),
        new("/b/{$}"u8, "/b/c/{x...}"u8, disjoint),
        new("/b/{x}/a/{$}"u8, "/{x}/c/{y...}"u8, overlaps),
        new("/{x}/b/{$}"u8, "/a/{x}/{y}"u8, disjoint),
        new("/{x}/b/{$}"u8, "/a/{x}/c"u8, disjoint),
        new("/{z}/{$}"u8, "/{z}/a"u8, disjoint),
        new("/{z}/{$}"u8, "/{z}/a/b"u8, disjoint),
        new("/{z}/{$}"u8, "/{z}/{$}"u8, equivalent),
        new("/{z}/{$}"u8, "/{z}/{x}"u8, disjoint),
        new("/{z}/{$}"u8, "/{z}/"u8, moreSpecific),
        new("/{z}/{$}"u8, "/a/"u8, overlaps),
        new("/{z}/{$}"u8, "/a/{x...}"u8, overlaps),
        new("/{z}/{$}"u8, "/{z}/{x...}"u8, moreSpecific),
        new("/a/{z}/{$}"u8, "/{z}/a/"u8, overlaps)
    }.slice()) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p2);
        {
            global::go.net.http_package.relationship g = pat1.comparePaths(pat1); if (g != equivalent) {
                Ꮡt.Errorf("%s does not match itself; got %s"u8, pat1.OrTypedNil(), g);
            }
        }
        {
            global::go.net.http_package.relationship g = pat2.comparePaths(pat2); if (g != equivalent) {
                Ꮡt.Errorf("%s does not match itself; got %s"u8, pat2.OrTypedNil(), g);
            }
        }
        global::go.net.http_package.relationship got = pat1.comparePaths(pat2);
        if (got != test.want) {
            Ꮡt.Errorf("%s vs %s: got %s, want %s"u8, test.p1, test.p2, got, test.want);
            Ꮡt.Logf("pat1: %+v\n"u8, (~pat1).segments);
            Ꮡt.Logf("pat2: %+v\n"u8, (~pat2).segments);
        }
        global::go.net.http_package.relationship want2 = inverseRelationship(test.want);
        global::go.net.http_package.relationship got2 = pat2.comparePaths(pat1);
        if (got2 != want2) {
            Ꮡt.Errorf("%s vs %s: got %s, want %s"u8, test.p2, test.p1, got2, want2);
        }
    }
}

[GoType("dyn")] internal partial struct TestConflictsWith_type {
    internal @string p1, p2;
    internal bool want;
}

public static void TestConflictsWith(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestConflictsWith_type[]{
        new("/a"u8, "/a"u8, true),
        new("/a"u8, "/ab"u8, false),
        new("/a/b/cd"u8, "/a/b/cd"u8, true),
        new("/a/b/cd"u8, "/a/b/c"u8, false),
        new("/a/b/c"u8, "/a/c/c"u8, false),
        new("/{x}"u8, "/{y}"u8, true),
        new("/{x}"u8, "/a"u8, false), // more specific

        new("/{x}/{y}"u8, "/{x}/a"u8, false),
        new("/{x}/{y}"u8, "/{x}/a/b"u8, false),
        new("/{x}"u8, "/a/{y}"u8, false),
        new("/{x}/{y}"u8, "/{x}/a/"u8, false),
        new("/{x}"u8, "/a/{y...}"u8, false), // more specific

        new("/{x}/a/{y}"u8, "/{x}/a/{y...}"u8, false), // more specific

        new("/{x}/{y}"u8, "/{x}/a/{$}"u8, false), // more specific

        new("/{x}/{y}/{$}"u8, "/{x}/a/{$}"u8, false),
        new("/a/{x}"u8, "/{x}/b"u8, true),
        new("/"u8, "GET /"u8, false),
        new("/"u8, "GET /foo"u8, false),
        new("GET /"u8, "GET /foo"u8, false),
        new("GET /"u8, "/foo"u8, true),
        new("GET /foo"u8, "HEAD /"u8, true)
    }.slice()) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p2);
        var got = pat1.conflictsWith(pat2);
        if (got != test.want) {
            Ꮡt.Errorf("%q.ConflictsWith(%q) = %t, want %t"u8,
                test.p1, test.p2, got, test.want);
        }
        // conflictsWith should be commutative.
        got = pat2.conflictsWith(pat1);
        if (got != test.want) {
            Ꮡt.Errorf("%q.ConflictsWith(%q) = %t, want %t"u8,
                test.p2, test.p1, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aYZˢ = "/a/{y}/{z...}"u8;
internal static readonly @string matchesTheSameRequestsAsˢ = "matches the same requests as"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestRegisterConflict(ж<testing.T> Ꮡt) {
    var mux = NewServeMux();
    @string pat1 = "/a/{x}/"u8;
    {
        var errΔ1 = mux.registerErr(pat1, NotFoundHandler()); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string pat2 = aYZˢ;
    var err = mux.registerErr(pat2, NotFoundHandler());
    @string got = default!;
    if (err == default!){
        got = nilˢ;
    } else {
        got = err.Error();
    }
    @string want = matchesTheSameRequestsAsˢ;
    if (!strings.Contains(got, want)) {
        Ꮡt.Errorf("got\n%s\nwant\n%s"u8, got, want);
    }
}

[GoType("dyn")] internal partial struct TestDescribeConflict_type {
    internal @string p1, p2;
    internal @string want;
}

public static void TestDescribeConflict(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestDescribeConflict_type[]{
        new("/a/{x}"u8, "/a/{y}"u8, "the same requests"u8),
        new("/"u8, "/{m...}"u8, "the same requests"u8),
        new("/a/{x}"u8, "/{y}/b"u8, "both match some paths"u8),
        new("/a"u8, "GET /{x}"u8, "matches more methods than GET /{x}, but has a more specific path pattern"u8),
        new("GET /a"u8, "HEAD /"u8, "matches more methods than HEAD /, but has a more specific path pattern"u8),
        new("POST /"u8, "/a"u8, "matches fewer methods than /a, but has a more general path pattern"u8)
    }.slice()) {
        @string got = describeConflict(mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p1), mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p2));
        if (!strings.Contains(got, test.want)) {
            Ꮡt.Errorf("%s vs. %s:\ngot:\n%s\nwhich does not contain %q"u8,
                test.p1, test.p2, got, test.want);
        }
    }
}

[GoType("dyn")] internal partial struct TestCommonPath_type {
    internal @string p1, p2;
    internal @string want;
}

public static void TestCommonPath(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestCommonPath_type[]{
        new("/a/{x}"u8, "/{x}/a"u8, "/a/a"u8),
        new("/a/{z}/"u8, "/{z}/a/"u8, "/a/a/"u8),
        new("/a/{z}/{m...}"u8, "/{z}/a/"u8, "/a/a/"u8),
        new("/{z}/{$}"u8, "/a/"u8, "/a/"u8),
        new("/{z}/{$}"u8, "/a/{x...}"u8, "/a/"u8),
        new("/a/{z}/{$}"u8, "/{z}/a/"u8, "/a/a/"u8),
        new("/a/{x}/b/{y...}"u8, "/{x}/c/{y...}"u8, "/a/c/b/"u8),
        new("/a/{x}/b/"u8, "/{x}/c/{y...}"u8, "/a/c/b/"u8),
        new("/a/{x}/b/{$}"u8, "/{x}/c/{y...}"u8, "/a/c/b/"u8),
        new("/a/{z}/{x...}"u8, "/{z}/b/{y...}"u8, "/a/b/"u8)
    }.slice()) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p2);
        if (pat1.comparePaths(pat2) != overlaps) {
            Ꮡt.Fatalf("%s does not overlap %s"u8, test.p1, test.p2);
        }
        @string got = commonPath(ref (pat1).DerefOrNull(), ref (pat2).DerefOrNull());
        if (got != test.want) {
            Ꮡt.Errorf("%s vs. %s: got %q, want %q"u8, test.p1, test.p2, got, test.want);
        }
    }
}

[GoType("dyn")] internal partial struct TestDifferencePath_type {
    internal @string p1, p2;
    internal @string want;
}

public static void TestDifferencePath(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestDifferencePath_type[]{
        new("/a/{x}"u8, "/{x}/a"u8, "/a/x"u8),
        new("/{x}/a"u8, "/a/{x}"u8, "/x/a"u8),
        new("/a/{z}/"u8, "/{z}/a/"u8, "/a/z/"u8),
        new("/{z}/a/"u8, "/a/{z}/"u8, "/z/a/"u8),
        new("/{a}/a/"u8, "/a/{z}/"u8, "/ax/a/"u8),
        new("/a/{z}/{x...}"u8, "/{z}/b/{y...}"u8, "/a/z/"u8),
        new("/{z}/b/{y...}"u8, "/a/{z}/{x...}"u8, "/z/b/"u8),
        new("/a/b/"u8, "/a/b/c"u8, "/a/b/"u8),
        new("/a/b/{x...}"u8, "/a/b/c"u8, "/a/b/"u8),
        new("/a/b/{x...}"u8, "/a/b/c/d"u8, "/a/b/"u8),
        new("/a/b/{x...}"u8, "/a/b/c/d/"u8, "/a/b/"u8),
        new("/a/{z}/{m...}"u8, "/{z}/a/"u8, "/a/z/"u8),
        new("/{z}/a/"u8, "/a/{z}/{m...}"u8, "/z/a/"u8),
        new("/{z}/{$}"u8, "/a/"u8, "/z/"u8),
        new("/a/"u8, "/{z}/{$}"u8, "/a/x"u8),
        new("/{z}/{$}"u8, "/a/{x...}"u8, "/z/"u8),
        new("/a/{foo...}"u8, "/{z}/{$}"u8, "/a/foo"u8),
        new("/a/{z}/{$}"u8, "/{z}/a/"u8, "/a/z/"u8),
        new("/{z}/a/"u8, "/a/{z}/{$}"u8, "/z/a/x"u8),
        new("/a/{x}/b/{y...}"u8, "/{x}/c/{y...}"u8, "/a/x/b/"u8),
        new("/{x}/c/{y...}"u8, "/a/{x}/b/{y...}"u8, "/x/c/"u8),
        new("/a/{c}/b/"u8, "/{x}/c/{y...}"u8, "/a/cx/b/"u8),
        new("/{x}/c/{y...}"u8, "/a/{c}/b/"u8, "/x/c/"u8),
        new("/a/{x}/b/{$}"u8, "/{x}/c/{y...}"u8, "/a/x/b/"u8),
        new("/{x}/c/{y...}"u8, "/a/{x}/b/{$}"u8, "/x/c/"u8)
    }.slice()) {
        var pat1 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p1);
        var pat2 = mustParsePattern(new http_test_package.testing_TжTB(Ꮡt), test.p2);
        global::go.net.http_package.relationship rel = pat1.comparePaths(pat2);
        if (rel != overlaps && rel != moreGeneral) {
            Ꮡt.Fatalf("%s vs. %s are %s, need overlaps or moreGeneral"u8, pat1.OrTypedNil(), pat2.OrTypedNil(), rel);
        }
        @string got = differencePath(ref (pat1).DerefOrNull(), ref (pat2).DerefOrNull());
        if (got != test.want) {
            Ꮡt.Errorf("%s vs. %s: got %q, want %q"u8, test.p1, test.p2, got, test.want);
        }
    }
}

} // end http_internal_test_package
