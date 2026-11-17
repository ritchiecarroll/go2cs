namespace go.MethodHolderAlias.text;

using plate = plate_package;
using static plate_package;

partial class plate_package {

partial struct Plate {
    public @string Name;
}

} partial class plateᴛPlate {

public static @string Render(this Plate p) { return plate.Render(p.Name); }

} partial class plate_package {

public static @string Render(@string name) { return "text:"u8 + name; }

} // end plate_package
