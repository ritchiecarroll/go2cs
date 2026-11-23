namespace go.MethodHolderAlias.html;

using plate = go.MethodHolderAlias.text.plate_package;
using go.MethodHolderAlias.text;

partial class plate_package {

partial struct Plate {
    internal plate.Plate inner;
}

internal static @string Render⁔(/*this*/ Plate p) { return Render(p.inner.Name) + "+"u8 + plate.Render(p.inner.Name); }

public static @string Render(@string name) { return "html:"u8 + name; }

public static Plate New(@string name) { return new Plate(inner: new plate.Plate(Name: name)); }

} // end plate_package
