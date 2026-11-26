namespace go.PackageSuffixCollision;

using st = strings_package;

partial class shadow_package {

internal static @string strings = "shadowed";

public static @string Get() { return st.ToUpper(strings); }

} // end shadow_package
