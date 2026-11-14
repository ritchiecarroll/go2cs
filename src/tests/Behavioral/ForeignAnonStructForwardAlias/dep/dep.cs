global using TagLC_ID = go.ForeignAnonStructForwardAlias.dep_package.TagLC_IDᴛ1;

namespace go.ForeignAnonStructForwardAlias;

partial class dep_package {

public partial struct TagLC_IDᴛ1 /*dyn*/ {
    public uint16 FwLanguage;
    public uint16 FwCountry;
}

public static TagLC_ID Make() { return new TagLC_ID(FwLanguage: 1, FwCountry: 2); }

} // end dep_package
