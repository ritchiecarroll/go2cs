namespace go;

using CrossPkgLib = CrossPkgLib_package;

partial class CrossPkgFuncLib_package {

// type Picker is a methodless func type — rendered inline as its base delegate

public static bool Hot(CrossPkgLibꓸStatus st) {
    return st.Code > 10;
}

public static nint Count(Func<CrossPkgLibꓸStatus, bool> pick) {
    nint n = 0;

    foreach (var (_, st) in slice<CrossPkgLibꓸStatus>([new(Code: 5), new(Code: 15), new(Code: 25)])) {
        if (pick(st)) {
            n++;
        }
    }

    return n;
}

} // end CrossPkgFuncLib_package
