global using TunixFile = go.modernc.org.sqlite.lib.sqlite3_package.TunixFileᴛ1;

namespace go.modernc.org.sqlite.lib;

partial class sqlite3_package {

partial struct TunixFileᴛ1 /*dyn*/ {
    public uintptr FpMethod;
    public uintptr FpVfs;
    public uintptr FpInode;
    public int32 Fh;
    public uint8 FeFileLock;
    public uint16 FctrlFlags;
    public int32 FlastErrno;
    public uintptr FlockingContext;
    public uintptr FpPreallocatedUnused;
    public uintptr FzPath;
    public uintptr FpShm;
    public int32 FszChunk;
    public int32 FnFetchOut;
    public Tsqlite3_int64 FmmapSize;
    public Tsqlite3_int64 FmmapSizeActual;
    public Tsqlite3_int64 FmmapSizeMax;
    public uintptr FpMapRegion;
    public int32 FsectorSize;
    public int32 FdeviceCharacteristics;
}

} // end sqlite3_package
