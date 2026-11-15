global using X_iobuf = go.ForeignAnonStructAliasElemAddress.stdio_package.X_iobufᴛ1;
global using FILE = go.ForeignAnonStructAliasElemAddress.stdio_package.X_iobufᴛ1;

namespace go.ForeignAnonStructAliasElemAddress;

partial class stdio_package {

public static UntypedInt X_IOB_ENTRIES => 3;

partial struct X_iobufᴛ1 /*dyn*/ {
    public uintptr F_ptr;
    public int32 F_cnt;
    public array<byte> F__ccgo_pad1 = new(4);
    public int32 F_flag;
}

} // end stdio_package
