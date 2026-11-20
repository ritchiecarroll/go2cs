namespace go;

using CrossPkgLib = CrossPkgLib_package;

partial class CrossPkgBox_package {

partial struct Box {
    public CrossPkgLib.Status S;
}

public static Box New(nint code) {
    return new Box(S: new CrossPkgLib.Status(Code: code));
}

} // end CrossPkgBox_package
