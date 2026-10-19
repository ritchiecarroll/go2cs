namespace go.BlankImportSideEffects;

using registry = go.BlankImportSideEffects.registry_package;
using go.BlankImportSideEffects;

partial class jpeglike_package {

[GoInit] internal static void init() {
    registry.Register("jpeglike", "jpeglike decoder");
}

} // end jpeglike_package
