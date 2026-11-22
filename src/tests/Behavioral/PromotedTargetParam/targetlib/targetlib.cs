namespace go.PromotedTargetParam;

partial class targetlib_package {

partial struct Finder {
    public @string Name;
}

internal static @string Match⁔(/*this*/ Finder f, @string target) {
    return f.Name + "=>"u8 + target;
}

internal static void Retarget⁔(/*this*/ ref Finder f, @string target) {
    f.Name = target;
}

} // end targetlib_package
