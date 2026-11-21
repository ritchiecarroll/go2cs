namespace go.CollisionRenamedGlobalReceiver;

using atomic = sync.atomic_package;
using sync;

partial class lib_package {

public static readonly ж<atomic.Int32> ᏑTid = new StaticBox<atomic.Int32>(default(atomic.Int32));
public static ref atomic.Int32 Tid => ref ((StaticBox<atomic.Int32>)ᏑTid).ValueSlot;

partial struct T {
    public int32 ID;
}

internal static int32 Tid⁔(/*this*/ ref T t) { return t.ID; }

public static int32 Next() { return ᏑTid.Add(1); }

} // end lib_package
