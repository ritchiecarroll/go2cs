// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using html = html_package;
using strings = strings_package;
using testing = testing_package;
using static go.html.template_package;

partial class template_internal_test_package {

public static void TestHTMLNospaceEscaper(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string input = ("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !""#$%&'()*+,-./"u8 + @"0123456789:;<=>?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + "`abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\x7f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\ufdec\U0001D11E"u8 + ((@string)(new byte[]{0x65, 0x72, 0x72, 0x6f, 0x6e, 0x65, 0x6f, 0x75, 0x73, 0x96, 0x30}))); // keep at the end
    @string want = ("&#xfffd;\x01\x02\x03\x04\x05\x06\x07"u8 + "\x08&#9;&#10;&#11;&#12;&#13;\x0E\x0F"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17"u8 + "\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @"&#32;!&#34;#$%&amp;&#39;()*&#43;,-./"u8 + @"0123456789:;&lt;&#61;&gt;?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + @"&#96;abcdefghijklmno"u8 + @"pqrstuvwxyz{|}~"u8 + "\u007f"u8 + "\u00A0\u0100\u2028\u2029\ufeff&#xfdec;\U0001D11E"u8 + "erroneous&#xfffd;0"u8); // keep at the end
    @string got = htmlNospaceEscaper(input);
    if (got != want) {
        Ꮡt.Errorf("encode: want\n\t%q\nbut got\n\t%q"u8, want, got);
    }
    var r = strings.NewReplacer("\x00"u8, "\ufffd", ((@string)(new byte[]{0x96})), "\ufffd");
    (got, want) = (html.UnescapeString(got), r.Replace(input));
    if (want != got) {
        Ꮡt.Errorf("decode: want\n\t%q\nbut got\n\t%q"u8, want, got);
    }
}

internal partial struct TestStripTags_tests /*dyn*/ {
    internal @string input, want;
}

public static void TestStripTags(ж<testing.T> Ꮡt) {
    var tests = new TestStripTags_tests[]{
        new(""u8, ""u8),
        new("Hello, World!"u8, "Hello, World!"u8),
        new("foo&amp;bar"u8, "foo&amp;bar"u8),
        new(@"Hello <a href=""www.example.com/"">World</a>!"u8, "Hello World!"u8),
        new("Foo <textarea>Bar</textarea> Baz"u8, "Foo Bar Baz"u8),
        new("Foo <!-- Bar --> Baz"u8, "Foo  Baz"u8),
        new("<"u8, "<"u8),
        new("foo < bar"u8, "foo < bar"u8),
        new(@"Foo<script type=""text/javascript"">alert(1337)</script>Bar"u8, "FooBar"u8),
        new(@"Foo<div title=""1>2"">Bar"u8, "FooBar"u8),
        new(@"I <3 Ponies!"u8, @"I <3 Ponies!"u8),
        new(@"<script>foo()</script>"u8, @""u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string got = stripTags(test.input); if (got != test.want) {
                Ꮡt.Errorf("%q: want %q, got %q"u8, test.input, test.want, got);
            }
        }
    }
}

public static void BenchmarkHTMLNospaceEscaper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        htmlNospaceEscaper(theIQuickISpanStyleColorˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object theQuickBrownFoxJumpsˢ2 = (@string)"The_quick,_brown_fox_jumps_over_the_lazy_dog."u8;

public static void BenchmarkHTMLNospaceEscaperNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        htmlNospaceEscaper(theQuickBrownFoxJumpsˢ2);
    }
}

public static void BenchmarkStripTags(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        stripTags(theIQuickISpanStyleColorˢ);
    }
}

public static void BenchmarkStripTagsNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        stripTags(theQuickBrownFoxJumpsˢ);
    }
}

} // end template_internal_test_package
