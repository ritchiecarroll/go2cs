global using Ttimeval = go.modernc.org.libc_package.Ttimevalᴛ1;
global using TunixInodeInfo = go.modernc.org.sqlite.lib.sqlite3_package.TunixInodeInfoᴛ1;

namespace go.modernc.org.sqlite.lib;

partial class sqlite3_package {

partial struct TunixInodeInfoᴛ1 /*dyn*/ {
    public TunixFileId FfileId;
    public uintptr FpLockMutex;
    public int32 FnShared;
    public int32 FnLock;
    public uint8 FeFileLock;
    public uint8 FbProcessLock;
    public uint8 FbLockIsReadOnly;
    public int32 FhLock;
    public uintptr FpUnused;
    public int32 FnRef;
    public uintptr FpShmNode;
    public uintptr FpNext;
    public uintptr FpPrev;
}

} // end sqlite3_package
