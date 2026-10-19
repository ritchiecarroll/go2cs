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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object stringToSidErrorˢ = (@string)"StringToSid error:"u8;
private static readonly object stringToSidReturnedANilˢ = (@string)"StringToSid returned a nil SID for"u8;
private static readonly object sidStringErrorˢ = (@string)"SID.String error:"u8;
private static readonly object roundtripˢ = (@string)"roundtrip:"u8;
private static readonly object len0ˢ = (@string)"len>0:"u8;
private static readonly object malformedSidRejectedˢ = (@string)"malformed SID rejected: false"u8;
private static readonly object malformedSidRejectedTrueˢ = (@string)"malformed SID rejected: true"u8;
private static readonly object stableˢ = (@string)"stable:"u8;
private static readonly object netGetJoinInformationˢ = (@string)"NetGetJoinInformation error:"u8;
private static readonly object netGetJoinInformationˢ2 = (@string)"NetGetJoinInformation left its out-parameter nil"u8;
private static readonly object joinNameLengthInRangeˢ = (@string)"join name length in range:"u8;
private static readonly object firstRunePrintableˢ = (@string)"first rune printable:"u8;
private static readonly object statusInRangeˢ = (@string)"status in range:"u8;

internal static void Main() {
    foreach (var (_, s) in wellKnown) {
        var (sid, err) = syscall.StringToSid(s);
        if (err != default!) {
            fmt.Println(stringToSidErrorˢ, err);
            continue;
        }
        if (sid == nil) {
            fmt.Println(stringToSidReturnedANilˢ, s);
            continue;
        }

        (var back, err) = sid.String();
        if (err != default!) {
            fmt.Println(sidStringErrorˢ, err);
            continue;
        }

        fmt.Println(s, (@string)"->"u8, back, roundtripˢ, back == s, len0ˢ, sid.Len() > 0);
    }

    {
        var (_, err) = syscall.StringToSid("not-a-sid"); if (err == default!){
            fmt.Println(malformedSidRejectedˢ);
        } else {
            fmt.Println(malformedSidRejectedTrueˢ);
        }
    }

    var (a, _) = syscall.StringToSid("S-1-5-32-545");
    var (b, _) = syscall.StringToSid("S-1-5-32-545");
    var (@as, _) = a.String();
    var (bs, _) = b.String();
    fmt.Println(stableˢ, @as == bs, @as == "S-1-5-32-545"u8);

    ref var name = ref heap<ж<uint16>>(out var Ꮡname);
    ref var bufType = ref heap(new uint32(), out var ᏑbufType);
    {
        var err = syscall.NetGetJoinInformation(nil, Ꮡname, ᏑbufType); if (err != default!){
            fmt.Println(netGetJoinInformationˢ, err);
        } else 
        if (name == nil){
            fmt.Println(netGetJoinInformationˢ2);
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
            fmt.Println(joinNameLengthInRangeˢ, n > 0 && n < 256,
                firstRunePrintableˢ, first >= 0x20 && first < 0x7f,
                statusInRangeˢ, bufType <= 4);
            syscall.NetApiBufferFree(name.Reinterpret<uint16, byte>());
        }
    }
}

} // end main_package
