namespace go.PromotedTargetParam;

partial class targetlib_package {

[GoType] partial struct Finder {
    public @string Name;
}

public static @string Match(this Finder f, @string target) {
    return f.Name + "=>"u8 + target;
}

[GoRecv] public static void Retarget(this ref Finder f, @string target) {
    f.Name = target;
}

} // end targetlib_package
