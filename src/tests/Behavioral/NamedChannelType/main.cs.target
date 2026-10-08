namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct closeWaiter /*chan EmptyStruct*/;

internal static void Init(this ref closeWaiter cw) {
    cw = new closeWaiter(0);
}

internal static void Close(this closeWaiter cw) {
    close<EmptyStruct>(cw);
}

internal static void Wait(this closeWaiter cw) {
    ᐸꟷ<EmptyStruct>(cw);
}

partial struct intQueue /*chan nint*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object waitedˢ = (@string)"waited"u8;

internal static partial void Main() {
    closeWaiter cw = default!;
    cw.Init();
    cw.Close();
    cw.Wait();
    fmt.Println(waitedˢ);
    var q = new intQueue(3);
    q.ᐸꟷ(10);
    q.ᐸꟷ(20);
    fmt.Println(len(q), cap(q));
    nint v = ᐸꟷ<nint>(q);
    fmt.Println(v);
    (v, var ok) = ᐸꟷ<nint>(q, ꟷ);
    fmt.Println(v, ok);
    close<nint>(q);
    (v, ok) = ᐸꟷ<nint>(q, ꟷ);
    fmt.Println(v, ok);
    var drain = new intQueue(2);
    drain.ᐸꟷ(7);
    drain.ᐸꟷ(8);
    close<nint>(drain);
    nint sum = 0;
    foreach (var x in drain) {
        sum += x;
    }
    fmt.Println(sum);
    var sel = new intQueue(1);
    sel.ᐸꟷ(42);
    var selᴛ1 = sel;
    switch (select(ᐸꟷ<nint>(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var y): {
        fmt.Println(y);
        break;
    }}
    var pipe = new intQueue(0);
    fmt.Println(len(pipe), cap(pipe));
    var pipeʗ1 = pipe;
    goǃ(() => {
        pipeʗ1.ᐸꟷ(77);
    });
    fmt.Println(ᐸꟷ<nint>(pipe));
}

} // end main_package
