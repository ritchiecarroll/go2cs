namespace go.NilFuncInterfaceReads;

partial class typelib_package {

public delegate nint Step(nint _);

internal static @string Name⁔(/*this*/ Step s) { return "step"; }

public delegate nint StepE(nint _);

internal static @string Name⁔(/*this*/ StepE x) { return "stepe"; }

internal static @string Error⁔(/*this*/ StepE x) { return "stepe error"; }

partial struct Box {
    public @string Tag;
}

internal static @string Name⁔(/*this*/ ref Box b) { return "box"; }

public static Step NewStep() {
    return [GoFunc("NewStep.func1")] (nint x) => x + 1;
}

} // end typelib_package
