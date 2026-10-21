// The code behind docs/ConsumingGoFromCSharp.md: every snippet on that page is taken from here. Each
// check prints "ok" or "FAIL"; run-csharp-consumer.ps1 requires every one to print "ok" and the exit code 0.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using go;                                   // golib, and the methods of every Go package whose import path has no slash
using static go.builtin;                    // Go's built-ins: nil, len, append, delete, @new, goǃ, Ꮡ
using go.github.com.google;                 // the methods of github.com/google/uuid
using go.github.com.ritchiecarroll;         // the methods of github.com/ritchiecarroll/hashset

using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using utf8 = go.unicode.utf8_package;
using uuid = go.github.com.google.uuid_package;
using hashset = go.github.com.ritchiecarroll.hashset_package;

using static go.strings_package;            // the alternate form: bare ToUpper(..)

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

    // 6. A C# function in the Go shape: a nil error means success.
    static error CheckName(@string name)
    {
        if (len(name) == 0) return errors.New("empty name");
        return null;                        // a nil error: write null (or default)
    }

    // 6. The bridge to exceptions, where the rest of a C# program expects them.
    static nint ParsePort(@string text)
    {
        var (port, err) = strconv.Atoi(text);
        if (err != nil) throw new FormatException(err.Error());
        return port;
    }

    static void Run()
    {
        // 1. A five-minute start.
        @string shout = strings.ToUpper("hello, gopher");
        fmt.Println(shout, len(shout));     // HELLO, GOPHER 13
        Check("1 strings.ToUpper and fmt.Println", fmt.Sprintln(shout, len(shout)) == "HELLO, GOPHER 13\n");

        // 2. Imports: the alias form, the alternates, and where Go methods come from.
        @string bare = ToUpper("gopher");                       // using static go.strings_package;
        @string qualified = strings_package.ToUpper("gopher");  // using go;
        Check("2 using static and the package class reach the same function", bare == "GOPHER" && qualified == "GOPHER");
        time.Duration timeout = 90 * time.ΔSecond;
        @string shown = timeout.String();   // a Go method: found through `using go;`
        Check("2 a Go method is an extension method found through the namespace", shown == "1m30s");
        var log = new CollectingWriter();
        fmt.Fprintf(log, "%d gophers\n", 3);    // Go calls the C# Write
        Check("2 a C# class implements a Go interface", log.Text.ToString() == "3 gophers\n");

        // 3. Numbers: Go's int is nint.
        var size = len("héllo");            // nint, Go's int: 6 bytes
        int small = (int)len("héllo");      // C# int is Go's int32, so cast when you need one
        Check("3 len returns nint", size == 6 && small == 6);
        var (num, aerr) = strconv.Atoi("42");   // (nint, error)
        Check("3 strconv.Atoi returns (nint, error)", aerr == nil && num == 42);
        slice<nint> nums = new nint[] { 1, 2, 3 };
        nums = append(nums, 4, 5);              // C# int literals append to a slice<nint>
        Check("3 append to a slice<nint>", len(nums) == 5 && nums[4] == 5);
        slice<long> big = new long[] { 5 };
        big = append(big, 6, 7);                // and to any Go integer slice
        Check("3 append takes C# int literals for a slice<long>", len(big) == 3 && big[2] == 7);

        // 4. Strings.
        @string hello = "héllo, 世界";      // System.String converts to @string (UTF-16 to UTF-8)
        string back = hello;                // and back (UTF-8 to UTF-16)
        Check("4 len counts UTF-8 bytes, as in Go", len(hello) == 14);
        Check("4 utf8.RuneCountInString counts runes", utf8.RuneCountInString(hello) == 9);
        Check("4 an @string converts back to System.String", back == "héllo, 世界");
        Check("4 an @string compares with a C# string literal", hello == "héllo, 世界");
        Check("4 a u8 literal passes straight in", strings.HasSuffix(hello, "世界"u8));

        // 5. Printing Go values.
        error diskFull = errors.New("disk full");
        time.Duration wait = 1500 * time.Millisecond;
        fmt.Println(diskFull, wait);        // disk full 1.5s
        Check("5 fmt prints Go values as Go does", fmt.Sprint(diskFull, " ", wait) == "disk full 1.5s");
        Console.WriteLine($"{diskFull.Error()} after {wait.String()}");    // disk full after 1.5s
        Check("5 Error() and String() give the Go text", diskFull.Error() == "disk full" && wait.String() == "1.5s");
        Console.WriteLine($"{diskFull} after {wait}");                    // disk full after 1.5s
        Check("5 ToString() gives the Go text too", $"{diskFull} after {wait}" == "disk full after 1.5s");
        slice<nint> scores = new nint[] { 7, 8, 9 };
        Check("5 fmt formats a slice<T> as Go does", fmt.Sprintf("%v", scores) == "[7 8 9]");

        // 6. Errors are values.
        var (n, err) = strconv.Atoi("42");
        if (err != nil)
        {
            Console.WriteLine($"not a number: {err.Error()}");
        }
        Check("6 a nil error means success", err == nil && n == 42);
        var (_, bad) = strconv.Atoi("forty-two");
        error wrapped = fmt.Errorf("reading config: %w", bad);
        Check("6 fmt.Errorf wraps with %w", wrapped.Error() == "reading config: strconv.Atoi: parsing \"forty-two\": invalid syntax");
        Check("6 errors.Is sees through the wrap", errors.Is(wrapped, strconv.ErrSyntax));
        Check("6 a C# function returns a nil error as null", CheckName("gopher") == nil && CheckName("") != nil);
        try
        {
            ParsePort("eighty");
            Check("6 ParsePort throws on bad input", false);
        }
        catch (FormatException ex)
        {
            Check("6 an error becomes an exception where C# wants one", ex.Message.Contains("invalid syntax"));
        }
        nint port = strconv.Atoi("8080").Must();    // the value, or a GoErrorException
        Check("6 Must returns the value of a nil-error result", port == 8080);
        try
        {
            strconv.Atoi("eighty").Must();
            Check("6 Must throws on a non-nil error", false);
        }
        catch (GoErrorException ex) when (errors.Is(ex.Err, strconv.ErrSyntax))
        {
            Check("6 GoErrorException carries the original Go error", ex.Message.Contains("invalid syntax"));
        }

        // 7. Panics and exceptions.
        try
        {
            uuid.MustParse("not-a-uuid");
            Check("7 MustParse panics on bad input", false);
        }
        catch (PanicException panic)
        {
            // panic.State is the value Go passed to panic; panic.Message is its text
            Check("7 a Go panic is caught as PanicException", panic.State is not null && panic.Message.StartsWith("uuid: Parse(not-a-uuid)"));
        }
        try
        {
            fmt.Sprint(new NotReady());     // fmt calls the C# String method, under a Go recover()
            Check("7 the C# exception reaches the caller", false);
        }
        catch (InvalidOperationException ex)
        {
            Check("7 a C# exception passes through Go code, and its recover(), unchanged", ex.Message == "not ready");
        }
        var once = @new<sync.Once>();
        try
        {
            once.Do(() => throw new InvalidOperationException("first call fails"));
        }
        catch (InvalidOperationException) { }
        var ranAgain = false;
        once.Do(() => ranAgain = true);     // Once's deferred calls ran, so it is done, as after a Go panic
        Check("7 Go's deferred calls run while a C# exception passes", !ranAgain);

        // 8. Slices.
        slice<@string> words = strings.Fields("  the quick  brown fox ");
        var lengths = new StringBuilder();
        foreach (var (i, w) in words)       // Go: for i, w := range words
        {
            lengths.Append($"{i}:{len(w)} ");
        }
        Check("8 foreach gives (index, value) pairs", lengths.ToString() == "0:3 1:5 2:5 3:3 ");
        nint letters = 0;
        foreach (var (_, w) in words) letters += len(w);    // Go: for _, w := range words
        Check("8 (_, v) takes the values", letters == 16);
        IEnumerable<@string> seq = words;   // name the element type, then LINQ works
        Check("8 LINQ over IEnumerable<T>", seq.Count(w => len(w) == 5) == 2);
        List<@string> list = new(words);
        @string[] array = words.ToArray();
        Check("8 a slice copies into a List<T> or T[]", list.Count == 4 && array.Length == 4);
        Check("8 strings.Join takes the slice back", strings.Join(words, "-") == "the-quick-brown-fox");

        int[] numbers = [1, 2, 3];
        slice<int> shared = numbers;        // wraps the same array: no copy
        shared[0] = 10;
        Check("8 a slice made from a C# array shares that array", numbers[0] == 10);
        int[] copy = shared;                // converting back to T[] copies
        copy[1] = 20;
        Check("8 converting a slice to T[] copies", shared[1] == 2);
        Span<int> span = shared.ToSpan();   // a view, no copy
        span[2] = 30;
        Check("8 ToSpan views the slice's elements", numbers[2] == 30);

        // 9. Maps.
        var ages = new map<@string, nint> { ["ann"] = 31, ["bob"] = 42 };
        nint unknown = ages["zed"];         // a missing key gives the zero value
        Check("9 a missing key reads as the zero value", unknown == 0);
        var (age, ok) = ages["ann", ꟷ];     // Go: age, ok := ages["ann"]
        Check("9 the comma-ok read", ok && age == 31);
        delete(ages, "bob");
        Check("9 delete removes a key", len(ages) == 1);
        nint total = 0;
        foreach (var (name, years) in ages) total += years;   // Go: for name, years := range ages
        Check("9 ranging over a map visits each entry", total == 31);
        IDictionary<@string, nint> dict = ages;   // a Go map is an IDictionary<K, V>
        Check("9 a Go map works as an IDictionary", dict.ContainsKey("ann") && dict.Count == 1);

        // 10. Pointers.
        var sb = @new<strings.Builder>();   // Go: sb := new(strings.Builder)
        sb.WriteString("go");
        sb.WriteString("pher");
        Check("10 @new<T>() gives a ж<T> that every method takes", sb.String() == "gopher");
        var target = new uuid.UUID();
        error uerr = target.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
        Check("10 a pointer method updates a local, passed by reference", uerr == nil && target[0] == 0x6b);
        ж<uuid.UUID> boxed = Ꮡ(new uuid.UUID());    // Go: boxed := &uuid.UUID{}
        boxed.UnmarshalText("6ba7b810-9dad-11d1-80b4-00c04fd430c8"u8.ToArray());
        Check("10 the same method takes a ж<T>; .Value reads it", boxed.Value[15] == 0xc8);
        nint count = 1;
        ж<nint> counter = Ꮡ(count);         // a new box holding a copy of count
        counter.Value = 2;
        Check("10 Ꮡ(value) boxes a copy of the value", count == 1 && counter.Value == 2);

        // 11. Goroutines and channels.
        var results = new channel<nint>(1);
        goǃ(() => results.Send(42));        // Go: go func() { results <- 42 }()
        Check("11 a goroutine started from C# sends on a channel", results.Receive() == 42);

        var wg = @new<sync.WaitGroup>();
        var squares = new channel<nint>(3);
        for (nint i = 1; i <= 3; i++)
        {
            wg.Add(1);
            goǃ(k => { squares.Send(k * k); wg.Done(); }, i);   // i is passed now, as in Go's go f(i)
        }
        wg.Wait();
        squares.Close();
        nint sum = 0;
        foreach (var sq in squares) sum += sq;  // Go: for sq := range squares, until it is closed
        Check("11 a WaitGroup, Close, and ranging over a channel", sum == 14);

        var mailbox = new channel<@string>(1);
        Check("11 TrySend does not wait", mailbox.TrySend("hi") && !mailbox.TrySend("again"));
        Check("11 TryReceive does not wait", mailbox.TryReceive(out var got) && got == "hi" && !mailbox.TryReceive(out _));

        var done = new channel<bool>(0);
        var seen = 0;
        for (var g = 0; g < 4; g++)
        {
            goǃ(() => { Interlocked.Increment(ref seen); done.Send(true); });
        }
        for (var g = 0; g < 4; g++) done.Receive();
        Check("11 an unbuffered channel waits for every goroutine", seen == 4);

        var replies = new channel<nint>(0);
        Task<nint> reply = replies.ReceiveAsync();      // awaitable: no thread waits for the sender
        goǃ(() => replies.Send(99));
        Check("11 ReceiveAsync completes when a goroutine sends", reply.Wait(10000) && reply.Result == 99);
        using (var cts = new CancellationTokenSource())
        {
            Task<nint> abandoned = replies.ReceiveAsync(cts.Token);
            cts.Cancel();
            ((Task)abandoned).ContinueWith(_ => { }).Wait(10000);
            Check("11 a canceled ReceiveAsync leaves the channel and takes nothing", abandoned.IsCanceled && !replies.TrySend(1));
        }

        // 12. A real third-party module: github.com/google/uuid.
        uuid.UUID id = uuid.New();
        @string text = id.String();
        Check("12 New().String() is a 36-character UUID", len(text) == 36);
        Check("12 a C# string passes where Go takes a string", uuid.Validate("6ba7b810-9dad-11d1-80b4-00c04fd430c8") == nil);
        var (parsed, perr) = uuid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        Check("12 Parse returns (UUID, error) as a tuple", perr == nil && parsed.String() == "6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        var (_, badId) = uuid.Parse("not-a-uuid");
        Check("12 a bad input returns a non-nil error", badId != nil && len(badId.Error()) > 0);
        Check("12 UUID is a Go [16]byte and indexes like one", parsed[0] == 0x6b && len(parsed) == 16);
        Span<byte> view = parsed.ToSpan();
        Check("12 ToSpan views the 16 bytes", view.Length == 16 && view[15] == 0xc8);
        uuid.UUID copied = parsed.Clone();  // Go: copied := parsed
        copied[0] = 0;
        Check("12 Clone copies a Go array value", parsed[0] == 0x6b && copied[0] == 0);
        uuid.ΔVersion version = parsed.Version();   // Go: uuid.Version
        Check("12 a Δ name: uuid.Version is uuid.ΔVersion", version.String() == "VERSION_1");

        // 13. Generics: github.com/ritchiecarroll/hashset.
        var set = hashset.NewHashSet<int>(new[] { 1, 2, 3 });   // Go: hashset.NewHashSet[int32]
        Check("13 Add returns false for an existing element", !set.Add(2));
        Check("13 Add returns true for a new element", set.Add(4));
        Check("13 len works on the set", len(set) == 4);
        var keySum = 0;
        foreach (var (key, _) in set) keySum += key;   // a Go map: (key, value) pairs
        Check("13 ranging over the set visits every element", keySum == 10);
        slice<int> keys = set.Keys();
        Check("13 Keys returns a slice of every element", len(keys) == 4);
        IReadOnlySet<int> members = set.AsReadOnlySet();    // a live, read-only view of the keys
        Check("13 AsReadOnlySet passes the set where an IReadOnlySet goes", members.Contains(4) && members.IsSubsetOf(new[] { 1, 2, 3, 4, 5 }));
        map<@string, EmptyStruct> visited = nil;
        Check("13 a nil Go map is the empty set", visited.AsReadOnlySet().Count == 0 && !visited.AsReadOnlySet().Contains("go"));
    }

    // 2. A C# class implementing a Go interface: io.Writer.
    sealed class CollectingWriter : io.Writer
    {
        public readonly StringBuilder Text = new();

        public (nint n, error err) Write(slice<byte> p)
        {
            string chunk = (@string)p;      // Go: string(p)
            Text.Append(chunk);
            return (len(p), null);
        }
    }

    // 7. A fmt.Stringer whose String method throws a C# exception.
    sealed class NotReady : fmt.Stringer
    {
        public @string String() => throw new InvalidOperationException("not ready");
    }
}
