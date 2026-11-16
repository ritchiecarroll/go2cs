global using localTone = go.RenamedTypeAliasReexportRecords.tone_package.ΔTone;

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
    toner t = new tone_ΔToneᴠtoner(((tone.ΔTone)3));
    fmt.Println(describe(t));
    fmt.Println(describe(new tone_ΔToneᴠtoner(((tone.ΔTone)4))));
    fmt.Println(t);

    localTone lt = 5;
    fmt.Println(describe(new tone_ΔToneᴠtoner(lt)));

    ref var p = ref heap<RenamedTypeAliasReexportRecords.tone_package.ΔPitch>(lt.Pitch(), out var Ꮡp);
    raiser r = new tone_ΔPitchжraiser(Ꮡp);
    r.Raise(2);
    fmt.Println(r.Label());

    var hz = Ꮡp.ᏑHz;
    hz.Value += 3;
    fmt.Println(p.Hz, r.Label());
}

} // end main_package
