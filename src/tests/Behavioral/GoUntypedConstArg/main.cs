namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static UntypedInt poly => 0x82f63b78;

internal static channel<uint32> done = new channel<uint32>(1);

internal static uint32 compute(uint32 p) {
    var r = (uint32)(p ^ 0xffffffffU);
    done.ᐸꟷ(r);
    return r;
}

internal static partial void Main() {
    goǃ(ᴛ1 => compute(ᴛ1), (uint32)(poly));
    var stored = ᐸꟷ(done);
    fmt.Printf("%#x\n"u8, stored);
}

} // end main_package
