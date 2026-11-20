namespace go.PromotedTargetParam;

using static targetlib_package;

partial class targetlib_package {

partial struct Finder {
    public @string Name;
}

} partial class targetlibᴛFinder {

public static @string Match(this Finder f, @string target) {
    return f.Name + "=>"u8 + target;
}

public static void Retarget(this ref Finder f, @string target) {
    f.Name = target;
}

} // end targetlibᴛFinder
