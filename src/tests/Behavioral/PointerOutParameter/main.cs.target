namespace go;

using fmt = fmt_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class main_package {

internal static slice<@string> wellKnown = new @string[]{
    "S-1-5-18",
    "S-1-5-32-544",
    "S-1-1-0",
    "S-1-0-0"
}.slice();

internal static void Main() {
    foreach (var (_, s) in wellKnown) {
        var (sid, err) = syscall.StringToSid(s);
        if (err != nil) {
            fmt.Println("StringToSid error:", err);
            continue;
        }
        if (sid == nil) {
            fmt.Println("StringToSid returned a nil SID for", s);
            continue;
        }

        (var back, err) = sid.String();
        if (err != nil) {
            fmt.Println("SID.String error:", err);
            continue;
        }

        fmt.Println(s, "->", back, "roundtrip:", back == s, "len>0:", sid.Len() > 0);
    }

    {
        var (_, err) = syscall.StringToSid("not-a-sid"); if (err == nil) {
            fmt.Println("malformed SID rejected: false");
        } else {
            fmt.Println("malformed SID rejected: true");
        }
    }

    var (a, _) = syscall.StringToSid("S-1-5-32-545");
    var (b, _) = syscall.StringToSid("S-1-5-32-545");
    var (@as, _) = a.String();
    var (bs, _) = b.String();
    fmt.Println("stable:", @as == bs, @as == "S-1-5-32-545"u8);

    ref var name = ref heap<ж<uint16>>(out var Ꮡname);
    ref var bufType = ref heap(new uint32(), out var ᏑbufType);
    {
        var err = syscall.NetGetJoinInformation(nil, Ꮡname, ᏑbufType); if (err != nil) {
            fmt.Println("NetGetJoinInformation error:", err);
        } else if (name == nil) {
            fmt.Println("NetGetJoinInformation left its out-parameter nil");
        } else {
            nint n = 0;
            var first = (uint16)0;
            while (n < 256) {
                var c = ~(ж<uint16>)(uintptr)(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(name), 2 * n));
                if (c == 0) {
                    break;
                }
                if (n == 0) {
                    first = c;
                }
                n++;
            }
            fmt.Println("join name length in range:", n > 0 && n < 256,
                "first rune printable:", first >= 0x20 && first < 0x7f,
                "status in range:", bufType <= 4);
            syscall.NetApiBufferFree(name.Reinterpret<uint16, byte>());
        }
    }
}

} // end main_package
