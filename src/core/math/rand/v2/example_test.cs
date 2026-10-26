// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using fmt = fmt_package;
using rand = global::go.math.rand.rand_package;
using os = os_package;
using strings = strings_package;
using tabwriter = text.tabwriter_package;
using time = time_package;
using global::go.math.rand;
using io = io_package;
using static global::go.math.rand.rand_internal_test_package;
using text;

partial class rand_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object magic8BallSaysˢ = (@string)"Magic 8-Ball says:"u8;

// These tests serve as an example but also make sure we don't change
// the output of the random number generator when given a fixed seed.
public static void Example() {
    var answers = new @string[]{
        "It is certain"u8,
        "It is decidedly so"u8,
        "Without a doubt"u8,
        "Yes definitely"u8,
        "You may rely on it"u8,
        "As I see it yes"u8,
        "Most likely"u8,
        "Outlook good"u8,
        "Yes"u8,
        "Signs point to yes"u8,
        "Reply hazy try again"u8,
        "Ask again later"u8,
        "Better not tell you now"u8,
        "Cannot predict now"u8,
        "Concentrate and ask again"u8,
        "Don't count on it"u8,
        "My reply is no"u8,
        "My sources say no"u8,
        "Outlook not so good"u8,
        "Very doubtful"u8
    }.slice();
    fmt.Println(magic8BallSaysˢ, answers[rand.IntN(len(answers))]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string float32ˢ = "Float32"u8;
internal static readonly @string float64ˢ = "Float64"u8;
internal static readonly @string expFloat64ˢ = "ExpFloat64"u8;
internal static readonly @string normFloat64ˢ = "NormFloat64"u8;
internal static readonly @string int32ˢ = "Int32"u8;
internal static readonly @string int64ˢ = "Int64"u8;
internal static readonly @string uint32ˢ = "Uint32"u8;
internal static readonly @string intN10ˢ = "IntN(10)"u8;
internal static readonly @string int32N10ˢ = "Int32N(10)"u8;
internal static readonly @string int64N10ˢ = "Int64N(10)"u8;
internal static readonly @string permˢ = "Perm"u8;

// This example shows the use of each of the methods on a *Rand.
// The use of the global functions is the same, without the receiver.
public static void Example_rand() {
    GoFrame ᒐ = default;
    try {
        // Create and seed the generator.
        // Typically a non-fixed seed should be used, such as Uint64(), Uint64().
        // Using a fixed seed will produce the same output on every run.
        var r = rand.New(new rand.PCGжSource(rand.NewPCG(1, 2)));

        // The tabwriter here helps us generate aligned output.
        var w = tabwriter.NewWriter(new os.FileжWriter(os.Stdout), 1, 1, 1, (rune)' ', 0);
        var wʗ1 = w;
        defer(() => wʗ1.Flush(), ref ᒐ);
        var wʗ2 = w;
        void show(@string name, any v1, any v2, any v3) {
            fmt.Fprintf(new rand_test_package.tabwriter_WriterжWriter(wʗ2), "%s\t%v\t%v\t%v\n"u8, name, v1, v2, v3);
        }

        // Float32 and Float64 values are in [0, 1).
        show(float32ˢ, r.Float32(), r.Float32(), r.Float32());
        show(float64ˢ, r.Float64(), r.Float64(), r.Float64());

        // ExpFloat64 values have an average of 1 but decay exponentially.
        show(expFloat64ˢ, r.ExpFloat64(), r.ExpFloat64(), r.ExpFloat64());

        // NormFloat64 values have an average of 0 and a standard deviation of 1.
        show(normFloat64ˢ, r.NormFloat64(), r.NormFloat64(), r.NormFloat64());

        // Int32, Int64, and Uint32 generate values of the given width.
        // The Int method (not shown) is like either Int32 or Int64
        // depending on the size of 'int'.
        show(int32ˢ, r.Int32(), r.Int32(), r.Int32());
        show(int64ˢ, r.Int64(), r.Int64(), r.Int64());
        show(uint32ˢ, r.Uint32(), r.Uint32(), r.Uint32());

        // IntN, Int32N, and Int64N limit their output to be < n.
        // They do so more carefully than using r.Int()%n.
        show(intN10ˢ, r.IntN(10), r.IntN(10), r.IntN(10));
        show(int32N10ˢ, r.Int32N(10), r.Int32N(10), r.Int32N(10));
        show(int64N10ˢ, r.Int64N(10), r.Int64N(10), r.Int64N(10));

        // Perm generates a random permutation of the numbers [0, n).
        show(permˢ, r.Perm(5), r.Perm(5), r.Perm(5));
        // Output:
        // Float32     0.95955694          0.8076733            0.8135684
        // Float64     0.4297927436037299  0.797802349388613    0.3883664855410056
        // ExpFloat64  0.43463410545541104 0.5513632046504593   0.7426404617374481
        // NormFloat64 -0.9303318111676635 -0.04750789419852852 0.22248301107582735
        // Int32       2020777787          260808523            851126509
        // Int64       5231057920893523323 4257872588489500903  158397175702351138
        // Uint32      314478343           1418758728           208955345
        // IntN(10)    6                   2                    0
        // Int32N(10)  3                   7                    7
        // Int64N(10)  8                   9                    4
        // Perm        [0 3 1 4 2]         [4 1 2 0 3]          [4 3 2 0 1]
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void ExamplePerm() {
    foreach (var (_, value) in rand.Perm(3)) {
        fmt.Println(value);
    }

    // Unordered output: 1
    // 2
    // 0
}

public static void ExampleN() {
    // Print an int64 in the half-open interval [0, 100).
    fmt.Println(rand.N((int64)100));

    // Sleep for a random duration between 0 and 100 milliseconds.
    time.Sleep(rand.N(100 * time.Millisecond));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inkRunsFromTheCornersOfˢ = "ink runs from the corners of my mouth"u8;

public static void ExampleShuffle() {
    var words = strings.Fields(inkRunsFromTheCornersOfˢ);
    var wordsʗ1 = words;
    rand.Shuffle(len(words), (nint i, nint j) => {
        (wordsʗ1[i], wordsʗ1[j]) = (wordsʗ1[j], wordsʗ1[i]);
    });
    fmt.Println(words);
}

public static void ExampleShuffle_slicesInUnison() {
    var numbers = slice<byte>("12345"u8);
    var letters = slice<byte>("ABCDE"u8);
    // Shuffle numbers, swapping corresponding entries in letters at the same time.
    var lettersʗ1 = letters;
    var numbersʗ1 = numbers;
    rand.Shuffle(len(numbers), (nint i, nint j) => {
        (numbersʗ1[i], numbersʗ1[j]) = (numbersʗ1[j], numbersʗ1[i]);
        (lettersʗ1[i], lettersʗ1[j]) = (lettersʗ1[j], lettersʗ1[i]);
    });
    foreach (var i in range(numbers)) {
        fmt.Printf("%c: %c\n"u8, letters[i], numbers[i]);
    }
}

public static void ExampleIntN() {
    fmt.Println(rand.IntN(100));
    fmt.Println(rand.IntN(100));
    fmt.Println(rand.IntN(100));
}

} // end rand_test_package
