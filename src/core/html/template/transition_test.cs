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

public static void TestFindEndTag(ж<testing.T> t) {
    var tests = slice<TestFindEndTag_tests>([
        new("", "tag", -1),
        new("hello </textarea> hello", "textarea", 6),
        new("hello </TEXTarea> hello", "textarea", 6),
        new("hello </textAREA>", "textarea", 6),
        new("hello </textarea", "textareax", -1),
        new("hello </textarea>", "tag", -1),
        new("hello tag </textarea", "tag", -1),
        new("hello </tag> </other> </textarea> <other>", "textarea", 22),
        new("</textarea> <other>", "textarea", 0),
        new("<div> </div> </TEXTAREA>", "textarea", 13),
        new("<div> </div> </TEXTAREA\t>", "textarea", 13),
        new("<div> </div> </TEXTAREA >", "textarea", 13),
        new("<div> </div> </TEXTAREAfoo", "textarea", -1),
        new("</TEXTAREAfoo </textarea>", "textarea", 14),
        new("<</script >", "script", 1),
        new("</script>", "textarea", -1)
    ]);
    foreach (var (_, test) in tests) {
        {
            nint got = indexTagEnd(slice<byte>(test.s), slice<byte>(test.tag)); if (test.want != got) {
                t.Errorf("%q/%q: want\n\t%d\nbut got\n\t%d", test.s, test.tag, test.want, got);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string textareaHelloHelloHelloˢ => ref textareaHelloHelloHelloˢᵇ;
private static readonly @string textareaHelloHelloHelloˢᵇ = "<textarea> Hello Hello Hello </textarea> "u8;
internal static ref readonly @string textareaPDearNameWithˢ => ref textareaPDearNameWithˢᵇ;
private static readonly @string textareaPDearNameWithˢᵇ = "<textarea> <p> Dear {{.Name}},\n{{with .Gift}}Thank you for the lovely {{.}}. {{end}}\nBest wishes. </p>\n</textarea>"u8;

internal partial struct BenchmarkTemplateSpecialTags_r /*dyn*/ {
    public @string Name, Gift;
}

public static void BenchmarkTemplateSpecialTags(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = new BenchmarkTemplateSpecialTags_r("Aunt Mildred", "bone china tea set");

    @string h1 = textareaHelloHelloHelloˢ;
    @string h2 = textareaPDearNameWithˢ;
    @string html = strings.Repeat(h1, 100) + h2 + strings.Repeat(h1, 100) + h2;

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < b.N; i++) {
        var (ᴛ1, ᴛ2) = New(fooˢ2).Parse(html);
        var tmpl = Must(ᴛ1, ᴛ2);
        {
            var err = tmpl.Execute(new template_test_package.bytes_BufferжWriter(Ꮡbuf), r); if (err != nil) {
                Ꮡb.Fatal(err);
            }
        }
        buf.Reset();
    }
}

} // end template_internal_test_package
