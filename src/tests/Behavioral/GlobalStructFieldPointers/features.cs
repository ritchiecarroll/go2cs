namespace go;

partial class main_package {


partial struct Featuresᴛ1 /*dyn*/ {
    public bool HasFast;
    public bool HasWide;
    public nint Level;
}
public static readonly ж<Featuresᴛ1> ᏑFeatures = new StaticBox<Featuresᴛ1>(default(Featuresᴛ1));
public static ref Featuresᴛ1 Features => ref ((StaticBox<Featuresᴛ1>)ᏑFeatures).ValueSlot;

partial struct option {
    internal @string name;
    internal ж<bool> flag;
}

} // end main_package
