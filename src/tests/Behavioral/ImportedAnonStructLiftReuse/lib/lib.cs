global using largeInteger = go.ImportedAnonStructLiftReuse.lib_package.largeIntegerᴛ1;

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

} // end lib_package
