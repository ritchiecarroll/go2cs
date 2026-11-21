namespace go.go.kwlib;

using static scoped_package;

partial class scoped_package {

public static @string Name() { return "scoped"; }

partial struct Kind /*num:nint*/;

} partial class scopedᴛKind {

public static Kind Double(this Kind k) { return k * 2; }

}
