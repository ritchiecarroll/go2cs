namespace go.github.com.itchyny;

partial class gojq_package {

partial struct stack {
    internal slice<block> data;
    internal nint index;
    internal nint limit;
}

partial struct block {
    internal any value;
    internal nint next;
}

internal static ж<stack> newStack() {
    return Ꮡ(new stack(index: -1, limit: -1));
}

internal static void push(this ref stack s, any v) {
    var b = new block(v, s.index);
    s.index = max(s.index, s.limit) + 1;
    if (s.index < len(s.data)) {
        s.data[s.index] = b;
    } else {
        s.data = builtin.append(s.data, b);
    }
}

internal static any pop(this ref stack s) {
    var b = s.data[s.index];
    s.index = b.next;
    return b.value;
}

internal static any top(this ref stack s) {
    return s.data[s.index].value;
}

internal static bool empty(this ref stack s) {
    return s.index < 0;
}

internal static (nint index, nint limit) save(this ref stack s) {
    nint index = default;
    nint limit = default;

    (index, limit) = (s.index, s.limit);
    if (s.index > s.limit) {
        s.limit = s.index;
    }
    return (index, limit);
}

internal static void restore(this ref stack s, nint index, nint limit) {
    (s.index, s.limit) = (index, limit);
}

} // end gojq_package
