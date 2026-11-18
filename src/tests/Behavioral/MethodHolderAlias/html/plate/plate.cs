namespace go.MethodHolderAlias.html;

using plate = go.MethodHolderAlias.text.plate_package;
using go.MethodHolderAlias.text;
using static plate_package;

partial class plate_package {

partial struct Plate {
    internal plate.Plate inner;
}

} partial class plateᴛPlate {

public static @string Render(this Plate p) { return plate_package.Render(p.inner.Name) + "+"u8 + plate.Render(p.inner.Name); }

} partial class plate_package {

public static @string Render(@string name) { return "html:"u8 + name; }

public static Plate New(@string name) { return new Plate(inner: new plate.Plate(Name: name)); }

} // end plate_package
