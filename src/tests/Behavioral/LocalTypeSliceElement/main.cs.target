namespace go;

using fmt = fmt_package;

partial class main_package {

internal partial struct process_entry /*dyn*/ {
    internal nint id;
    internal nint val;
}

internal static nint process() {
    slice<process_entry> entries = default!;
    entries = append(entries, new process_entry(1, 10));
    entries = append(entries, new process_entry(2, 20));
    entries = append(entries, new process_entry(3, 30));
    nint total = 0;
    foreach (var (_, e) in entries) {
        total += e.val * e.id;
    }
    return total;
}

internal partial struct arr_pair /*dyn*/ {
    internal nint a, b;
}

internal static nint arr() {
    array<arr_pair> ps = new(2);
    ps[0] = new arr_pair(1, 2);
    ps[1] = new arr_pair(3, 4);
    return ps[0].a + ps[0].b + ps[1].a + ps[1].b;
}

internal static void Main() {
    fmt.Println(process(), arr());
}

} // end main_package
