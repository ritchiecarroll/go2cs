namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Queue<T> {
    internal slice<T> items;
}

[GoRecv] public static void Enqueue<T>(this ref Queue<T> q, T item) {
    q.items = append(q.items, item);
}

[GoRecv] public static (T, bool) Dequeue<T>(this ref Queue<T> q) {
    T zero = GoZero<T>();
    if (len(q.items) == 0) {
        return (zero, false);
    }
    var item = q.items[0];
    q.items = q.items[1..];
    return (item, true);
}

[GoRecv] public static nint Size<T>(this ref Queue<T> q) {
    return len(q.items);
}

internal static void Main() {
    var intQueue = new Queue<nint>(nil);
    intQueue.Enqueue(10);
    intQueue.Enqueue(20);
    fmt.Printf("Int queue size: %d\n"u8, intQueue.Size());
    var stringQueue = new Queue<@string>(
        items: new @string[]{"hello"u8, "world"u8}.slice()
    );
    fmt.Printf("String queue size: %d\n"u8, stringQueue.Size());
    Queue<float64> floatQueue = new Queue<float64>(
        items: new float64[]{3.14D, 2.71D, 1.618D}.slice()
    );
    fmt.Printf("Float queue size: %d\n"u8, floatQueue.Size());
    var queues = new any[]{
        new Queue<nint>(nil),
        new Queue<@string>(nil),
        new Queue<bool>(items: new bool[]{true, false}.slice())
    }.slice();
    fmt.Printf("Number of queues: %d\n"u8, len(queues));
}

} // end main_package
