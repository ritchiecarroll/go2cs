namespace go;

using CrossPkgLib = CrossPkgLib_package;

partial class CrossPkgFuncLib_package {

public delegate bool Picker(CrossPkgLib.Status _);

public static bool Hot(CrossPkgLib.Status st) {
    return st.Code > 10;
}

public static nint Count(Picker pick) {
    nint n = 0;

    foreach (var (_, st) in range(slice<CrossPkgLib.Status>([new(Code: 5), new(Code: 15), new(Code: 25)]))) {
        if (pick(st)) {
            n++;
        }
    }

    return n;
}

} // end CrossPkgFuncLib_package
