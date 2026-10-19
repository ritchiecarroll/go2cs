namespace go.BlankImportSideEffects;

using registry = go.BlankImportSideEffects.registry_package;
using go.BlankImportSideEffects;

partial class pnglike_package {

[GoInit] internal static void init() {
    registry.Register("pnglike", "pnglike decoder");
}

} // end pnglike_package
