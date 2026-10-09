// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.html.template_package;

partial class template_internal_test_package {

internal partial struct TestFindEndTag_tests /*dyn*/ {
    internal @string s, tag;
    internal nint want;
}

public static void TestFindEndTag(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestFindEndTag_tests[]{
        new(""u8, "tag"u8, -1),
        new("hello </textarea> hello"u8, "textarea"u8, 6),
        new("hello </TEXTarea> hello"u8, "textarea"u8, 6),
        new("hello </textAREA>"u8, "textarea"u8, 6),
        new("hello </textarea"u8, "textareax"u8, -1),
        new("hello </textarea>"u8, "tag"u8, -1),
        new("hello tag </textarea"u8, "tag"u8, -1),
        new("hello </tag> </other> </textarea> <other>"u8, "textarea"u8, 22),
        new("</textarea> <other>"u8, "textarea"u8, 0),
        new("<div> </div> </TEXTAREA>"u8, "textarea"u8, 13),
        new("<div> </div> </TEXTAREA\t>"u8, "textarea"u8, 13),
        new("<div> </div> </TEXTAREA >"u8, "textarea"u8, 13),
        new("<div> </div> </TEXTAREAfoo"u8, "textarea"u8, -1),
        new("</TEXTAREAfoo </textarea>"u8, "textarea"u8, 14),
        new("<</script >"u8, "script"u8, 1),
        new("</script>"u8, "textarea"u8, -1)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            nint got = indexTagEnd(slice<byte>(test.s), slice<byte>(test.tag)); if (test.want != got) {
                Ꮡt.Errorf("%q/%q: want\n\t%d\nbut got\n\t%d"u8, test.s, test.tag, test.want, got);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string textareaHelloHelloHelloˢ = "<textarea> Hello Hello Hello </textarea> "u8;
internal static readonly @string textareaPDearNameWithˢ = "<textarea> <p> Dear {{.Name}},\n{{with .Gift}}Thank you for the lovely {{.}}. {{end}}\nBest wishes. </p>\n</textarea>"u8;

internal partial struct BenchmarkTemplateSpecialTags_r /*dyn*/ {
    public @string Name, Gift;
}

public static void BenchmarkTemplateSpecialTags(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = new BenchmarkTemplateSpecialTags_r("Aunt Mildred"u8, "bone china tea set"u8);
    @string h1 = textareaHelloHelloHelloˢ;
    @string h2 = textareaPDearNameWithˢ;
    @string html = strings.Repeat(h1, 100) + h2 + strings.Repeat(h1, 100) + h2;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        var (ᴛ1, ᴛ2) = New(fooˢ2).Parse(html);
        var tmpl = Must(ᴛ1, ᴛ2);
        {
            var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), r); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
        buf.Reset();
    }
}

} // end template_internal_test_package
