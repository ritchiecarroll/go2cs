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

public static void TestWriter(ж<testing.T> Ꮡt) {
    testWriter(Ꮡt, false);
}

public static void TestWriterBinary(ж<testing.T> Ꮡt) {
    testWriter(Ꮡt, true);
}

internal partial struct testWriter_tests /*dyn*/ {
    internal @string @in, want, wantB;
}

internal static void testWriter(ж<testing.T> Ꮡt, bool binary) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new testWriter_tests[]{
        new(@in: ""u8, want: ""u8),
        new(@in: "foo bar"u8, want: "foo bar"u8),
        new(@in: "foo bar="u8, want: "foo bar=3D"u8),
        new(@in: "foo bar\r"u8, want: "foo bar\r\n"u8, wantB: "foo bar=0D"u8),
        new(@in: "foo bar\r\r"u8, want: "foo bar\r\n\r\n"u8, wantB: "foo bar=0D=0D"u8),
        new(@in: "foo bar\n"u8, want: "foo bar\r\n"u8, wantB: "foo bar=0A"u8),
        new(@in: "foo bar\r\n"u8, want: "foo bar\r\n"u8, wantB: "foo bar=0D=0A"u8),
        new(@in: "foo bar\r\r\n"u8, want: "foo bar\r\n\r\n"u8, wantB: "foo bar=0D=0D=0A"u8),
        new(@in: "foo bar "u8, want: "foo bar=20"u8),
        new(@in: "foo bar\t"u8, want: "foo bar=09"u8),
        new(@in: "foo bar  "u8, want: "foo bar =20"u8),
        new(@in: "foo bar \n"u8, want: "foo bar=20\r\n"u8, wantB: "foo bar =0A"u8),
        new(@in: "foo bar \r"u8, want: "foo bar=20\r\n"u8, wantB: "foo bar =0D"u8),
        new(@in: "foo bar \r\n"u8, want: "foo bar=20\r\n"u8, wantB: "foo bar =0D=0A"u8),
        new(@in: "foo bar  \n"u8, want: "foo bar =20\r\n"u8, wantB: "foo bar  =0A"u8),
        new(@in: "foo bar  \n "u8, want: "foo bar =20\r\n=20"u8, wantB: "foo bar  =0A=20"u8),
        new(@in: "¡Hola Señor!"u8, want: "=C2=A1Hola Se=C3=B1or!"u8),
        new(
            @in: "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~"u8,
            want: "\t !\"#$%&'()*+,-./ :;<>?@[\\]^_`{|}~"u8
        ),
        new(
            @in: strings.Repeat("a"u8, 75),
            want: strings.Repeat("a"u8, 75)
        ),
        new(
            @in: strings.Repeat("a"u8, 76),
            want: strings.Repeat("a"u8, 75) + "=\r\na"u8
        ),
        new(
            @in: strings.Repeat("a"u8, 72) + "="u8,
            want: strings.Repeat("a"u8, 72) + "=3D"u8
        ),
        new(
            @in: strings.Repeat("a"u8, 73) + "="u8,
            want: strings.Repeat("a"u8, 73) + "=\r\n=3D"u8
        ),
        new(
            @in: strings.Repeat("a"u8, 74) + "="u8,
            want: strings.Repeat("a"u8, 74) + "=\r\n=3D"u8
        ),
        new(
            @in: strings.Repeat("a"u8, 75) + "="u8,
            want: strings.Repeat("a"u8, 75) + "=\r\n=3D"u8
        ),
        new(
            @in: strings.Repeat(" "u8, 73),
            want: strings.Repeat(" "u8, 72) + "=20"u8
        ),
        new(
            @in: strings.Repeat(" "u8, 74),
            want: strings.Repeat(" "u8, 73) + "=\r\n=20"u8
        ),
        new(
            @in: strings.Repeat(" "u8, 75),
            want: strings.Repeat(" "u8, 74) + "=\r\n=20"u8
        ),
        new(
            @in: strings.Repeat(" "u8, 76),
            want: strings.Repeat(" "u8, 75) + "=\r\n=20"u8
        ),
        new(
            @in: strings.Repeat(" "u8, 77),
            want: strings.Repeat(" "u8, 75) + "=\r\n =20"u8
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        var buf = @new<strings.Builder>();
        var w = NewWriter(new quotedprintable_test_package.strings_BuilderжWriter(buf));
        @string want = tt.want;
        if (binary) {
            w.Value.Binary = true;
            if (tt.wantB != ""u8) {
                want = tt.wantB;
            }
        }
        {
            var (_, err) = w.Write(slice<byte>(tt.@in)); if (err != default!) {
                Ꮡt.Errorf("Write(%q): %v"u8, tt.@in, err);
                continue;
            }
        }
        {
            var err = w.Close(); if (err != default!) {
                Ꮡt.Errorf("Close(): %v"u8, err);
                continue;
            }
        }
        @string got = buf.String();
        if (got != want) {
            Ꮡt.Errorf("Write(%q), got:\n%q\nwant:\n%q"u8, tt.@in, got, want);
        }
    }
}

public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new quotedprintable_test_package.bytes_BufferжWriter(buf));
    {
        var (_, errΔ1) = w.Write(testMsg); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Write: %v"u8, errΔ1);
        }
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Close: %v"u8, errΔ2);
        }
    }
    var r = NewReader(new quotedprintable_test_package.bytes_BufferжReader(buf));
    var (gotBytes, err) = io.ReadAll(new quotedprintable_test_package.quotedprintable_ReaderжReader(r));
    if (err != default!) {
        Ꮡt.Fatalf("Error while reading from Reader: %v"u8, err);
    }
    @string got = ((@string)gotBytes);
    if (got != ((sstring)testMsg)) {
        Ꮡt.Errorf("Encoding and decoding changed the message, got:\n%s"u8, got);
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
