namespace go.NestedNamespaceReach.b;

using deep = go.NestedNamespaceReach.a.deep_package;
using go.NestedNamespaceReach.a;

partial class outer_package {

partial interface Wide :
    deep.Info
{
    error Close();
}

partial struct impl {
}

internal static @string Name⁔(/*this*/ impl _) { return "impl"; }
internal static error Close⁔(/*this*/ impl _) { return default; }
public static Wide Make() { return new impl(); }

} // end outer_package
