namespace go.NamedImportInitOrder;

using store = go.NamedImportInitOrder.store_package;
using go.NamedImportInitOrder;

partial class writer_package {

public static @string Name = "writer";

[GoInit] internal static void init() {
    store.Value = "written-by-writer-init";
}

} // end writer_package
