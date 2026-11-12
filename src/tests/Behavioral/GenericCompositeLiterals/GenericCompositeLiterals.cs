namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Queue<T> {
    internal slice<T> items;
}

} partial class mainᴛQueue {

public static void Enqueue<T>(this ref Queue<T> q, T item) {
    q.items = append(q.items, item);
}

public static (T, bool) Dequeue<T>(this ref Queue<T> q) {
    T zero = GoZero<T>();
    if (len(q.items) == 0) {
        return (zero, false);
    }

    var item = q.items[0];
    q.items = q.items[1..];
    return (item, true);
}

public static nint Size<T>(this ref Queue<T> q) {
    return len(q.items);
}

} partial class main_package {

internal static void main() {
    var intQueue = new Queue<nint>();
    intQueue.Enqueue(10);
    intQueue.Enqueue(20);
    fmt.Printf("Int queue size: %d\n"u8, intQueue.Size());

    var stringQueue = new Queue<@string>(
        items: ["hello", "world"]
    );
    fmt.Printf("String queue size: %d\n"u8, stringQueue.Size());

    Queue<float64> floatQueue = new Queue<float64>(
        items: [3.14D, 2.71D, 1.618D]
    );
    fmt.Printf("Float queue size: %d\n"u8, floatQueue.Size());

    var queues = slice<any>([
        new Queue<nint>(),
        new Queue<@string>(),
        new Queue<bool>(items: [true, false])
    ]);
    fmt.Printf("Number of queues: %d\n"u8, len(queues));
}

} // end main_package
