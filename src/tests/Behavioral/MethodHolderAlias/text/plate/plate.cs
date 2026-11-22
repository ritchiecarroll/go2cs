namespace go.MethodHolderAlias.text;

partial class plate_package {

partial struct Plate {
    public @string Name;
}

internal static @string Render⁔(/*this*/ Plate p) { return Render(p.Name); }

public static @string Render(@string name) { return "text:"u8 + name; }

} // end plate_package
