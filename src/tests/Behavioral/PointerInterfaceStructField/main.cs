namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Describer {
    @string Describe();
}

partial struct Setting {
    internal @string name;
    internal nint value;
}

public static @string Describe(this ref Setting s) {
    return fmt.Sprintf("%s=%d"u8, s.name, s.value);
}

partial struct holder {
    internal Describer d;
    internal @string label;
}

internal static void assignDescriber(ref holder h, ж<Setting> s) {
    h.d = new SettingжDescriber(s);
}

internal static readonly ж<Setting> ᏑglobalSetting = new StaticBox<Setting>(new Setting(name: "verbosity", value: 3));
internal static ref Setting globalSetting => ref ((StaticBox<Setting>)ᏑglobalSetting).ValueSlot;

internal static void Main() {
    var h = Ꮡ(new holder(new SettingжDescriber(ᏑglobalSetting), "positional"));
    fmt.Println((~h).label, (~h).d.Describe());

    ref var local = ref heap<Setting>(new Setting(name: "count", value: 7), out var Ꮡlocal);
    var h2 = new holder(d: new SettingжDescriber(Ꮡlocal), label: "keyed");
    fmt.Println(h2.label, h2.d.Describe());

    ref var replacement = ref heap<Setting>(new Setting(name: "assigned", value: 11), out var Ꮡreplacement);
    assignDescriber(ref (h).DerefOrNull(), Ꮡreplacement);
    replacement.value = 12;
    fmt.Println((~h).label, (~h).d.Describe());
}

} // end main_package
