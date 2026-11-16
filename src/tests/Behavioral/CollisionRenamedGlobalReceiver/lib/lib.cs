namespace go.CollisionRenamedGlobalReceiver;

using atomic = sync.atomic_package;
using sync;

partial class lib_package {

public static readonly ж<atomic.Int32> ᏑΔTid = new StaticBox<atomic.Int32>(default(atomic.Int32));
public static ref atomic.Int32 ΔTid => ref ((StaticBox<atomic.Int32>)ᏑΔTid).ValueSlot;

partial struct T {
    public int32 ID;
}

public static int32 Tid(this ref T t) { return t.ID; }

public static int32 Next() { return ᏑΔTid.Add(1); }

} // end lib_package
