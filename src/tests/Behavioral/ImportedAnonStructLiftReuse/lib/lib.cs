global using largeInteger = go.ImportedAnonStructLiftReuse.lib_package.largeIntegerᴛ1;
global using rect = go.ImportedAnonStructLiftReuse.lib_package.rectᴛ1;

namespace go.ImportedAnonStructLiftReuse;

partial class lib_package {

public partial struct largeIntegerᴛ1 /*dyn*/ {
    public int64 QuadPart;
}

public static int64 SetFilePointerEx(largeInteger distance, ж<largeInteger> Ꮡmoved) {
    ref var moved = ref Ꮡmoved.DerefOrNull();

    moved.QuadPart = distance.QuadPart * 2;
    return distance.QuadPart + 1;
}

public partial struct rectᴛ1 /*dyn*/ {
    public int32 W;
    public int32 H;
}

public static int64 Area(rect r) { return (int64)r.W * (int64)r.H; }

} // end lib_package
