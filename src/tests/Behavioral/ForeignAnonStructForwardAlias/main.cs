global using LC_ID = go.ForeignAnonStructForwardAlias.dep_package.TagLC_IDᴛ1;
global using TLC_ID = go.ForeignAnonStructForwardAlias.dep_package.TagLC_IDᴛ1;

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;
using dep = ForeignAnonStructForwardAlias.dep_package;
using ForeignAnonStructForwardAlias;

partial class main_package {

internal static void main() {
    TLC_ID t = dep.Make();
    var l = new LC_ID(FwLanguage: 9, FwCountry: 3);
    depꓸTagLC_ID shared = l;
    fmt.Println(t.FwLanguage, t.FwCountry, l.FwLanguage, shared.FwCountry, /* unsafe.Sizeof(l) */ (uintptr)4);
}

} // end main_package
