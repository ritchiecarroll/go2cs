namespace go.NestedNamespaceReach.b;

using deep = go.NestedNamespaceReach.a.deep_package;
using go.NestedNamespaceReach.a;
using static outer_package;

partial class outer_package {

partial interface Wide :
    deep.Info
{
    error Close();
}

partial struct impl {
}

} partial class outerᴛimpl {

internal static @string Name(this impl _) { return "impl"; }
internal static error Close(this impl _) { return default; }

} partial class outer_package {

public static Wide Make() { return new impl(); }

} // end outer_package
