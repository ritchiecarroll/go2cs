global using Encoder = go.RenamedTypeAliasReexport.lib_package.ΔEncoder;
global using Kind = go.RenamedTypeAliasReexport.lib_package.ΔKind;
global using Tok = go.RenamedTypeAliasReexport.lib_package.Tok;
global using Token = object;

namespace go.RenamedTypeAliasReexport;

using lib = go.RenamedTypeAliasReexport.lib_package;
using go.RenamedTypeAliasReexport;

partial class wrap_package {

public static ж<Encoder> NewEncoder(nint n) { return lib.NewEncoder(n); }

public static Tok NewTok(libꓸKind k) { return lib.NewTok(k); }

public static libꓸToken Read() { return new lib.Reader().Token(); }

} // end wrap_package
