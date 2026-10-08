namespace go;

partial class main_package {


partial struct Featuresᴛ1 /*dyn*/ {
    public bool HasFast;
    public bool HasWide;
    public nint Level;
}
public static ж<Featuresᴛ1> ᏑFeatures = new StandardBox<Featuresᴛ1>(default(Featuresᴛ1));
public static ref Featuresᴛ1 Features => ref ᏑFeatures.Value;

partial struct option {
    internal @string name;
    internal ж<bool> flag;
}

} // end main_package
