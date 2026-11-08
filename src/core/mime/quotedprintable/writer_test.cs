// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.mime;

using bytes = bytes_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using static go.mime.quotedprintable_package;

partial class quotedprintable_internal_test_package {

public static void TestWriter(ж<testing.T> t) {
    testWriter(t, false);
}

public static void TestWriterBinary(ж<testing.T> t) {
    testWriter(t, true);
}

internal partial struct testWriter_tests /*dyn*/ {
    internal @string @in, want, wantB;
}

internal static void testWriter(ж<testing.T> t, bool binary) {
    var tests = slice<testWriter_tests>([
        new(@in: "", want: ""),
        new(@in: "foo bar", want: "foo bar"),
        new(@in: "foo bar=", want: "foo bar=3D"),
        new(@in: "foo bar\r", want: "foo bar\r\n", wantB: "foo bar=0D"),
        new(@in: "foo bar\r\r", want: "foo bar\r\n\r\n", wantB: "foo bar=0D=0D"),
        new(@in: "foo bar\n", want: "foo bar\r\n", wantB: "foo bar=0A"),
        new(@in: "foo bar\r\n", want: "foo bar\r\n", wantB: "foo bar=0D=0A"),
        new(@in: "foo bar\r\r\n", want: "foo bar\r\n\r\n", wantB: "foo bar=0D=0D=0A"),
        new(@in: "foo bar ", want: "foo bar=20"),
        new(@in: "foo bar\t", want: "foo bar=09"),
        new(@in: "foo bar  ", want: "foo bar =20"),
        new(@in: "foo bar \n", want: "foo bar=20\r\n", wantB: "foo bar =0A"),
        new(@in: "foo bar \r", want: "foo bar=20\r\n", wantB: "foo bar =0D"),
        new(@in: "foo bar \r\n", want: "foo bar=20\r\n", wantB: "foo bar =0D=0A"),
        new(@in: "foo bar  \n", want: "foo bar =20\r\n", wantB: "foo bar  =0A"),
        new(@in: "foo bar  \n ", want: "foo bar =20\r\n=20", wantB: "foo bar  =0A=20"),
        new(@in: "¡Hola Señor!", want: "=C2=A1Hola Se=C3=B1or!"),
        new(
            @in: "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~",
            want: "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~"
        ),
        new(
            @in: strings.Repeat("a", 75),
            want: strings.Repeat("a", 75)
        ),
        new(
            @in: strings.Repeat("a", 76),
            want: strings.Repeat("a", 75) + "=\r\na"u8
        ),
        new(
            @in: strings.Repeat("a", 72) + "="u8,
            want: strings.Repeat("a", 72) + "=3D"u8
        ),
        new(
            @in: strings.Repeat("a", 73) + "="u8,
            want: strings.Repeat("a", 73) + "=\r\n=3D"u8
        ),
        new(
            @in: strings.Repeat("a", 74) + "="u8,
            want: strings.Repeat("a", 74) + "=\r\n=3D"u8
        ),
        new(
            @in: strings.Repeat("a", 75) + "="u8,
            want: strings.Repeat("a", 75) + "=\r\n=3D"u8
        ),
        new(
            @in: strings.Repeat(" ", 73),
            want: strings.Repeat(" ", 72) + "=20"u8
        ),
        new(
            @in: strings.Repeat(" ", 74),
            want: strings.Repeat(" ", 73) + "=\r\n=20"u8
        ),
        new(
            @in: strings.Repeat(" ", 75),
            want: strings.Repeat(" ", 74) + "=\r\n=20"u8
        ),
        new(
            @in: strings.Repeat(" ", 76),
            want: strings.Repeat(" ", 75) + "=\r\n=20"u8
        ),
        new(
            @in: strings.Repeat(" ", 77),
            want: strings.Repeat(" ", 75) + "=\r\n =20"u8
        )
    ]);

    foreach (var (_, tt) in range(tests)) {
        var buf = @new<strings.Builder>();
        var w = NewWriter(new quotedprintable_test_package.strings_BuilderжWriter(buf));

        @string want = tt.want;
        if (binary) {
            w.Binary = true;
            if (tt.wantB != ""u8) {
                want = tt.wantB;
            }
        }

        {
            var (_, err) = w.Write(slice<byte>(tt.@in)); if (err != nil) {
                t.Errorf("Write(%q): %v", tt.@in, err);
                continue;
            }
        }
        {
            var err = w.Close(); if (err != nil) {
                t.Errorf("Close(): %v", err);
                continue;
            }
        }
        @string got = buf.String();
        if (got != want) {
            t.Errorf("Write(%q), got:\n%q\nwant:\n%q", tt.@in, got, want);
        }
    }
}

public static void TestRoundTrip(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new quotedprintable_test_package.bytes_BufferжWriter(buf));
    var (_, errΔ1) = w.Write(testMsg); if (errΔ1 != nil) {
        t.Fatalf("Write: %v", errΔ1);
    }
    var errΔ2 = w.Close(); if (errΔ2 != nil) {
        t.Fatalf("Close: %v", errΔ2);
    }

    var r = NewReader(new quotedprintable_test_package.bytes_BufferжReader(buf));
    var (gotBytes, err) = io.ReadAll(new quotedprintable_test_package.quotedprintable_ReaderжReader(r));
    if (err != nil) {
        t.Fatalf("Error while reading from Reader: %v", err);
    }
    @string got = (@string)gotBytes;
    if (got != ((sstring)testMsg)) {
        t.Errorf("Encoding and decoding changed the message, got:\n%s", got);
    }
}

// From https://fr.wikipedia.org/wiki/Quoted-Printable
internal static slice<byte> testMsg = slice<byte>((@string)("Quoted-Printable (QP) est un format d'encodage de données codées sur 8 bits, qui utilise exclusivement les caractères alphanumériques imprimables du code ASCII (7 bits).\r\n"u8 + "\r\n"u8 + "En effet, les différents codages comprennent de nombreux caractères qui ne sont pas représentables en ASCII (par exemple les caractères accentués), ainsi que des caractères dits « non-imprimables ».\r\n"u8 + "\r\n"u8 + "L'encodage Quoted-Printable permet de remédier à ce problème, en procédant de la manière suivante :\r\n"u8 + "\r\n"u8 + "Un octet correspondant à un caractère imprimable de l'ASCII sauf le signe égal (donc un caractère de code ASCII entre 33 et 60 ou entre 62 et 126) ou aux caractères de saut de ligne (codes ASCII 13 et 10) ou une suite de tabulations et espaces non situées en fin de ligne (de codes ASCII respectifs 9 et 32) est représenté tel quel.\r\n"u8 + "Un octet qui ne correspond pas à la définition ci-dessus (caractère non imprimable de l'ASCII, tabulation ou espaces non suivies d'un caractère imprimable avant la fin de la ligne ou signe égal) est représenté par un signe égal, suivi de son numéro, exprimé en hexadécimal.\r\n"u8 + "Enfin, un signe égal suivi par un saut de ligne (donc la suite des trois caractères de codes ASCII 61, 13 et 10) peut être inséré n'importe où, afin de limiter la taille des lignes produites si nécessaire. Une limite de 76 caractères par ligne est généralement respectée.\r\n"u8));

public static void BenchmarkWriter(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var w = NewWriter(io.Discard);
        w.Write(testMsg);
        w.Close();
    }
}

} // end quotedprintable_internal_test_package
