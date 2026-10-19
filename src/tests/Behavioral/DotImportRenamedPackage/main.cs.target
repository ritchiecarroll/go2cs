namespace go;

using fmt = fmt_package;
using static sync_package;
using Δsync = sync_package;

partial class main_package {

internal static bool ready(ж<Δsync.Mutex> Ꮡmu) {
    return Ꮡmu != nil;
}

internal static void Main() {
    ref var mu = ref heap(new Δsync.Mutex(), out var Ꮡmu);
    fmt.Println("mutex ready:", ready(Ꮡmu));
}

} // end main_package
