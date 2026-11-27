global using localTone = go.RenamedTypeAliasReexportRecords.tone_package.Tone;

namespace go;

using fmt = fmt_package;
using tonealias = RenamedTypeAliasReexportRecords.tonealias_package;
using RenamedTypeAliasReexportRecords;
using tone = RenamedTypeAliasReexportRecords.tone_package;

partial class main_package {

partial interface toner :
    fmt.Stringer
{
    nint Tone();
}

partial interface raiser {
    void Raise(nint by);
    @string Label();
}

internal static @string describe(toner t) { return fmt.Sprintf("%s -> %d"u8, t.String(), t.Tone()); }

internal static void main() {
    toner t = new tone_Toneᴠtoner(((tone.Tone)3));
    fmt.Println(describe(t));
    fmt.Println(describe(new tone_Toneᴠtoner(((tone.Tone)4))));
    fmt.Println(t);

    localTone lt = 5;
    fmt.Println(describe(new tone_Toneᴠtoner(lt)));

    ref var p = ref heap<RenamedTypeAliasReexportRecords.tone_package.Pitch>(lt.Pitch(), out var Ꮡp);
    raiser r = new tone_Pitchжraiser(Ꮡp);
    r.Raise(2);
    fmt.Println(r.Label());

    var hz = Ꮡp.ᏑHz;
    hz.Value += 3;
    var evalᴛ1 = r.Label();
    fmt.Println(p.Hz, evalᴛ1);
}

} // end main_package
