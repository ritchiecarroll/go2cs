global using MutexWrap = go.ExportedAliasUnexportedTarget.aliaslib_package.mutexWrap;
global using CompareType = go.ExportedAliasUnexportedTarget.aliaslib_package.compareResult;

namespace go.ExportedAliasUnexportedTarget;

using sync = sync_package;

partial class aliaslib_package {

public partial struct mutexWrap {
    internal sync.Mutex @lock;
    internal bool disabled;
}

public static void Lock(this ж<mutexWrap> Ꮡmw) {
    ref var mw = ref Ꮡmw.DerefOrNull();

    if (!mw.disabled) {
        mw.@lock.Lock();
    }
}

public static void Unlock(this ж<mutexWrap> Ꮡmw) {
    ref var mw = ref Ꮡmw.DerefOrNull();

    if (!mw.disabled) {
        mw.@lock.Unlock();
    }
}

public static void Disable(this ref mutexWrap mw) {
    mw.disabled = true;
}

public partial struct compareResult /*num:nint*/;

internal static compareResult compareLess => /* iota - 1 */ -1;
internal static compareResult compareEqual => 0;
internal static compareResult compareGreater => 1;

public static nint Compare(nint a, nint b) {
    switch (ᐧ) {
    case {} when a < b: {
        return (nint)compareLess;
    }
    case {} when a > b: {
        return (nint)compareGreater;
    }}

    return (nint)compareEqual;
}

} // end aliaslib_package
