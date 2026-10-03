// The code behind docs/ConsumingGoFromCSharp.md: every snippet on that page is taken from here. Each
// check prints "ok" or "FAIL"; run-csharp-consumer.ps1 requires every one to print "ok" and the exit code 0.
using System;
using System.Threading;
using go;                                   // golib: @string, slice<T>, map<K,V>, channel<T>, ж<T>, error
using go.github.com.google;                 // uuid_package (and its extension methods)
using go.github.com.ritchiecarroll;         // hashset_package (and its extension methods)
using static go.builtin;                    // nil, len, goǃ, Ꮡ
using errors = go.errors_package;

static class Program
{
    static int passed, failed;

    static void Check(string what, bool ok)
    {
        Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {what}");
        if (ok) passed++; else failed++;
        if (!ok) Environment.ExitCode = 1;
    }

    static void Main()
    {
        try
        {
            Run();
        }
        finally
        {
            // The runner pins this line, so a check that stops running is noticed as surely as one that fails.
            Console.WriteLine($"checks: {passed} ok, {failed} failed");
        }
    }

    static void Run()
    {
        // 1. Calling a Go function, and strings.
        uuid_package.UUID id = uuid_package.New();
        @string text = id.String();         // a Go string comes back as @string
        string s = text;                    // and converts to System.String implicitly
        Check("1 New().String() is a 36-character UUID", s.Length == 36);
        @string fromCSharp = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";   // System.String converts the other way
        Check("1 a C# string passes where Go takes a string", uuid_package.Validate(fromCSharp) == nil);

        // 2. Multiple results and errors.
        var (parsed, err) = uuid_package.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        Check("2 Parse returns (UUID, error) as a tuple; err is nil", err == nil);
        Check("2 the parsed UUID round-trips", (string)parsed.String() == "6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        var (_, bad) = uuid_package.Parse("not-a-uuid");
        Check("2 a bad input returns a non-nil error with a message", bad != nil && bad.Error().ToString().Length > 0);
        error mine = errors.New("made in C#");
        Check("2 C# can create a Go error", (string)mine.Error() == "made in C#");

        // 3. Arrays: UUID is a Go [16]byte.
        Check("3 a fixed array indexes like one", parsed[0] == 0x6b && len(parsed) == 16);
        Span<byte> view = parsed.ToSpan();
        Check("3 ToSpan views the 16 bytes", view.Length == 16 && view[15] == 0xc8);

        // 4. Slices: T[] in, a view, and a copy back out.
        int[] numbers = [1, 2, 3];
        slice<int> shared = numbers;        // wraps the same array: no copy
        shared[0] = 10;
        Check("4 a slice made from a C# array shares that array", numbers[0] == 10);
        int[] copy = shared;                // converting back to T[] copies
        copy[1] = 20;
        Check("4 converting a slice to T[] copies", shared[1] == 2);
        Span<int> span = shared.ToSpan();   // a view, no copy
        span[2] = 30;
        Check("4 ToSpan views the slice's elements", numbers[2] == 30);

        // 5. Generics and maps: HashSet[T] is a Go map type.
        var set = hashset_package.NewHashSet<int>(new[] { 1, 2, 3 });
        Check("5 Add returns false for an existing element", !set.Add(2));
        Check("5 Add returns true for a new element", set.Add(4));
        Check("5 len works on the set", len(set) == 4);
        var sum = 0;
        foreach (var (key, _) in set) sum += key;   // range over a Go map: (key, value) pairs
        Check("5 ranging over the map visits every key", sum == 10);
        slice<int> keys = set.Keys();
        Check("5 Keys returns a slice of every element", len(keys) == 4);

        // 6. Pointers: a pointer-receiver method, and ж<T>.
        var target = new uuid_package.UUID();
        error uerr = target.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
        Check("6 a pointer-receiver method updates a local through ref", uerr == nil && target[0] == 0x6b);
        ж<uuid_package.UUID> boxed = Ꮡ(new uuid_package.UUID());
        boxed.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
        Check("6 the same method takes a ж<T> (Go *T)", boxed.Value[15] == 0xc8);

        // 7. Channels and goroutines from C#.
        var results = new channel<int>(1);
        goǃ(() => results.Send(42));        // a Go goroutine, started from C#
        Check("7 a goroutine started from C# sends on a channel", results.Receive() == 42);

        var done = new channel<bool>(0);
        var seen = 0;
        for (var i = 0; i < 4; i++)
        {
            goǃ(() => { Interlocked.Increment(ref seen); done.Send(true); });
        }
        for (var i = 0; i < 4; i++) done.Receive();
        Check("7 an unbuffered channel waits for every goroutine", seen == 4);

        // 8. Panics: a Go panic reaches C# as an exception.
        try
        {
            uuid_package.MustParse("not-a-uuid");
            Check("8 MustParse panics on bad input", false);
        }
        catch (PanicException panic)
        {
            Check("8 a Go panic is caught as PanicException", panic.Message.Length > 0);
        }
    }
}
