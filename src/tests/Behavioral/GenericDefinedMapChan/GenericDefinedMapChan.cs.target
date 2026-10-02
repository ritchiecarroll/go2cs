namespace go;

using fmt = fmt_package;
using sort = sort_package;

partial class main_package {

[GoType] public partial struct @void {
}

[GoType("map[T, @void]")] partial struct Set<T>;

[GoType("map[T, EmptyStruct]")] partial struct Bag<T>;

[GoType("map[K, V]")] partial struct Index<K, V>;

[GoType("chan T")] partial struct Pipe<T>;

[GoType("[]T")] partial struct Stack<T>;

public static Set<T> NewSet<T>(params Span<T> itemsʗp) {
    var items = itemsʗp.sslice();

    var s = new Set<T>(len(items));
    foreach (var (_, item) in items) {
        s.Add(item);
    }
    return s;
}

public static bool Add<T>(this Set<T> s, T item) {
    {
        var (_, ok) = s[item, ꟷ]; if (ok) {
            return false;
        }
    }
    s[item] = new @void(nil);
    return true;
}

public static bool Has<T>(this Set<T> s, T item) {
    var (_, ok) = s[item, ꟷ];
    return ok;
}

[GoRecv] public static void Reset<T>(this ref Set<T> s) {
    s = new Set<T>(0);
}

public static void Put<T>(this Bag<T> b, T item) {
    b[item] = new EmptyStruct();
}

[GoRecv] public static void Drop<T>(this ref Bag<T> b, T item) {
    delete(b, item);
}

public static (V, bool) Get<K, V>(this Index<K, V> ix, K key) {
    var (value, ok) = ix[key, ꟷ];
    return (value, ok);
}

[GoRecv] public static void Put<K, V>(this ref Index<K, V> ix, K key, V value) {
    if (ix == default!) {
        ix = new Index<K, V>(0);
    }
    (ix)[key] = value;
}

public static void Send<T>(this Pipe<T> p, T value) {
    p.ᐸꟷ(value);
}

[GoRecv] public static slice<T> Drain<T>(this ref Pipe<T> p) {
    close<T>(p);
    slice<T> @out = default!;
    foreach (var value in p) {
        @out = append(@out, value);
    }
    return @out;
}

[GoRecv] public static void Push<T>(this ref Stack<T> s, T value) {
    s = append(s, value);
}

public static T Peek<T>(this Stack<T> s) {
    return s[len(s) - 1];
}

public static slice<T> Keys<T>(this Set<T> s) {
    var keys = new slice<T>(0, len(s));
    foreach (var (key, _) in s) {
        keys = append(keys, key);
    }
    return keys;
}

public static slice<T> Keys<T>(this Bag<T> b) {
    var keys = new slice<T>(0, len(b));
    foreach (var (key, _) in b) {
        keys = append(keys, key);
    }
    return keys;
}

public static slice<K> Keys<K, V>(this Index<K, V> ix) {
    var keys = new slice<K>(0, len(ix));
    foreach (var (key, _) in ix) {
        keys = append(keys, key);
    }
    return keys;
}

internal static slice<nint> sortedInts(slice<nint> values) {
    sort.Ints(values);
    return values;
}

internal static slice<@string> sortedStrings(slice<@string> values) {
    sort.Strings(values);
    return values;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object setIntˢ = (@string)"Set[int]:"u8;
private static readonly object setIntAfterResetˢ = (@string)"Set[int] after Reset:"u8;
private static readonly object setStringˢ = (@string)"Set[string]:"u8;
private static readonly object bagIntˢ = (@string)"Bag[int]:"u8;
private static readonly object bagStringˢ = (@string)"Bag[string]:"u8;
private static readonly object indexStringIntˢ = (@string)"Index[string, int]:"u8;
private static readonly @string twoˢ = "two"u8;
private static readonly object indexIntStringˢ = (@string)"Index[int, string]:"u8;
private static readonly object pipeIntˢ = (@string)"Pipe[int]:"u8;
private static readonly object pipeStringˢ = (@string)"Pipe[string]:"u8;
private static readonly object stackIntˢ = (@string)"Stack[int]:"u8;
private static readonly object stackStringˢ = (@string)"Stack[string]:"u8;

internal static void Main() {
    var ints = NewSet<nint>(3, 1, 2, 3);
    fmt.Println(setIntˢ, len(ints), ints.Has(2), ints.Has(9), ints.Add(4), ints.Add(4), sortedInts(ints.Keys()));
    ints.Reset();
    fmt.Println(setIntAfterResetˢ, len(ints), ints == default!);
    var words = NewSet<@string>((@string)"go", (@string)"cs");
    fmt.Println(setStringˢ, len(words), words.Has("go"u8), sortedStrings(words.Keys()));
    var bag = new Bag<nint>(2);
    bag.Put(7);
    bag.Put(8);
    bag.Drop(7);
    fmt.Println(bagIntˢ, len(bag), sortedInts(bag.Keys()));
    Bag<@string> names = new Bag<@string>(new map<@string, EmptyStruct>{["a"u8] = new()});
    names.Put("b"u8);
    fmt.Println(bagStringˢ, sortedStrings(names.Keys()));
    Index<@string, nint> ages = default!;
    ages.Put("x"u8, 1);
    ages.Put("y"u8, 2);
    var (age, ok) = ages.Get("y"u8);
    var (_, missing) = ages.Get("z"u8);
    fmt.Println(indexStringIntˢ, age, ok, missing, sortedStrings(ages.Keys()));
    var labels = new Index<nint, @string>(new map<nint, @string>{[1] = "one"u8});
    labels.Put(2, twoˢ);
    var (label, _) = labels.Get(2);
    fmt.Println(indexIntStringˢ, label, sortedInts(labels.Keys()));
    var pipe = new Pipe<nint>(3);
    pipe.Send(1);
    pipe.Send(2);
    fmt.Println(pipeIntˢ, cap(pipe), len(pipe), pipe.Drain());
    var texts = new Pipe<@string>(1);
    texts.Send("hi"u8);
    fmt.Println(pipeStringˢ, texts.Drain());
    var stack = new Stack<nint>(0, 4);
    stack.Push(5);
    stack.Push(6);
    fmt.Println(stackIntˢ, len(stack), cap(stack), stack.Peek());
    Stack<@string> tags = default!;
    tags.Push("t"u8);
    fmt.Println(stackStringˢ, tags.Peek(), len(tags));
}

} // end main_package
