namespace go.NamedImportInitOrder;

using store = go.NamedImportInitOrder.store_package;
using go.NamedImportInitOrder;

partial class writer_package {

public static readonly ж<@string> ᏑName = new StaticBox<@string>("writer");
public static ref @string Name => ref ((StaticBox<@string>)ᏑName).ValueSlot;

[GoInit] internal static void init() {
    store.Value = "written-by-writer-init";
}

} // end writer_package
