namespace go.NilFuncTypeSwitch;

partial class typelib_package {

public delegate nint Step(nint _);

public static @string Name(this Step s) { return "step"; }

public delegate nint StepE(nint _);

public static @string Name(this StepE x) { return "stepe"; }

public static @string Error(this StepE x) { return "stepe error"; }

partial struct Box {
    public @string Tag;
}

public static @string Name(this ref Box b) { return "box"; }

public static Step NewStep() {
    return (nint x) => x + 1;
}

} // end typelib_package
