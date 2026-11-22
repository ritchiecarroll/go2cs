namespace go.NilFuncTypeSwitch;

using static typelib_package;

partial class typelib_package {

public delegate nint Step(nint _);

} partial class typelibᴛStep {

public static @string Name(this Step s) { return "step"; }

} partial class typelib_package {

public delegate nint StepE(nint _);

} partial class typelibᴛStepE {

public static @string Name(this StepE x) { return "stepe"; }

public static @string Error(this StepE x) { return "stepe error"; }

} partial class typelib_package {

partial struct Box {
    public @string Tag;
}

} partial class typelibᴛBox {

public static @string Name(this ref Box b) { return "box"; }

} partial class typelib_package {

public static Step NewStep() {
    return (nint x) => x + 1;
}

} // end typelib_package
