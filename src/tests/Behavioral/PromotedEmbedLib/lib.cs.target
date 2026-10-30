namespace go;

partial class PromotedEmbedLib_package {

partial struct Reading {
    public nint Sum;
}

partial struct common {
    internal nint sum;
}

internal static void Add(this ж<common> Ꮡc, nint n) {
    var p = Ꮡc.Ꮡsum;
    p.Value += n;
}

internal static Reading Report(this ж<common> Ꮡc) {
    var p = Ꮡc.Ꮡsum;
    return new Reading(Sum: p.Value);
}

partial struct Counter {
    /*embed*/ internal common common;
    public @string Label;
}

public static ж<Counter> New(@string label) {
    return Ꮡ(new Counter(Label: label));
}

} // end PromotedEmbedLib_package
