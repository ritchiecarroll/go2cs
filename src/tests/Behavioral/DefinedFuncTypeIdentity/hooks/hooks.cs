namespace go.DefinedFuncTypeIdentity;

partial class hooks_package {

public delegate nint Handler(nint _);

public static nint Apply(Handler h, nint x) { return h(x); }

public static nint Raw(Func<nint, nint> f, nint x) { return f(x); }

public static Handler Make(nint k) { return [GoFunc("Make.func1")] (nint x) => x + k; }

} // end hooks_package
