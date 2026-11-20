global using Tflock = go.modernc.org.libc_package.Tflockᴛ1;
global using Tstatx = go.modernc.org.sqlite.lib.sqlite3_package.Tstatxᴛ1;
global using Tstatx_timestamp = go.modernc.org.libc_package.Tstatx_timestampᴛ1;

namespace go.modernc.org.sqlite.lib;

partial class sqlite3_package {

partial struct Tstatxᴛ1 /*dyn*/ {
    public Tuint32_t Fstx_mask;
    public Tuint32_t Fstx_blksize;
    public Tuint64_t Fstx_attributes;
    public Tuint32_t Fstx_nlink;
    public Tuint32_t Fstx_uid;
    public Tuint32_t Fstx_gid;
    public Tuint16_t Fstx_mode;
    public array<Tuint16_t> F__pad0 = new(1);
    public Tuint64_t Fstx_ino;
    public Tuint64_t Fstx_size;
    public Tuint64_t Fstx_blocks;
    public Tuint64_t Fstx_attributes_mask;
    public Tstatx_timestamp Fstx_atime;
    public Tstatx_timestamp Fstx_btime;
    public Tstatx_timestamp Fstx_ctime;
    public Tstatx_timestamp Fstx_mtime;
    public Tuint32_t Fstx_rdev_major;
    public Tuint32_t Fstx_rdev_minor;
    public Tuint32_t Fstx_dev_major;
    public Tuint32_t Fstx_dev_minor;
    public array<Tuint64_t> F__pad1 = new(14);
}

internal static UntypedInt __PIE__ => 2;

internal static UntypedInt __pie__ => 2;

} // end sqlite3_package
