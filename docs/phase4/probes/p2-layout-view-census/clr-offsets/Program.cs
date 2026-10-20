using System;
using System.Runtime.CompilerServices;
public readonly struct Arr<T> { public readonly T[] a; public readonly int low; public readonly int len; }
public struct MountPoint { public ushort SubstituteNameOffset, SubstituteNameLength, PrintNameOffset, PrintNameLength; public Arr<ushort> PathBuffer; }
public struct Inet4 { public byte Len; public byte Family; public ushort Port; public Arr<byte> Addr; public Arr<sbyte> Zero; }
public struct Func { public int entryOff, nameOff, args; public uint deferreturn, pcsp, pcfile, pcln, npcdata, cuOffset; public int startLine; public byte funcID, flag; public Arr<byte> _; public byte nfuncdata; }
public struct Sym { public ushort a,b,c,d; public uint Flags; public Arr<ushort> PathBuffer; }
static class P {
  static long Off<S,F>(ref S s, ref F f) => (long)Unsafe.ByteOffset(ref Unsafe.As<S,byte>(ref s), ref Unsafe.As<F,byte>(ref f));
  static void Main() {
    var m = new MountPoint(); Console.WriteLine($"MountPoint size={Unsafe.SizeOf<MountPoint>()} SNO={Off(ref m, ref m.SubstituteNameOffset)} PathBuffer={Off(ref m, ref m.PathBuffer)}  (Go: 0, 8)");
    var i = new Inet4(); Console.WriteLine($"Inet4 size={Unsafe.SizeOf<Inet4>()} Len={Off(ref i, ref i.Len)} Port={Off(ref i, ref i.Port)} Addr={Off(ref i, ref i.Addr)} Zero={Off(ref i, ref i.Zero)}  (Go: 0, 2, 4, 8)");
    var f = new Func(); Console.WriteLine($"_func size={Unsafe.SizeOf<Func>()} entryOff={Off(ref f, ref f.entryOff)} startLine={Off(ref f, ref f.startLine)} funcID={Off(ref f, ref f.funcID)} _={Off(ref f, ref f._)} nfuncdata={Off(ref f, ref f.nfuncdata)}  (Go: 0, 36, 40, 42, 43)");
    var s = new Sym(); Console.WriteLine($"Sym size={Unsafe.SizeOf<Sym>()} Flags={Off(ref s, ref s.Flags)} PathBuffer={Off(ref s, ref s.PathBuffer)}  (Go: 8, 12)");
  }
}
