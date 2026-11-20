namespace go.modernc.org.libc;

partial class time_package {

public static readonly ж<map<@string, EmptyStruct>> ᏑCAPI = new StaticBox<map<@string, EmptyStruct>>(new map<@string, EmptyStruct>{
    ["daylight"] = new(),
    ["timezone"] = new(),
    ["tzname"] = new()
});
public static ref map<@string, EmptyStruct> CAPI => ref ((StaticBox<map<@string, EmptyStruct>>)ᏑCAPI).ValueSlot;

} // end time_package
