// The extended GoFuncCookie, prototyped (DESIGN-sqlite-cs-funcptr.md 3.x, revised per the Fable review).
// ONE func-word story: a STATIC function (a delegate with no target, which is every converted top-level Go func) owns ONE
// word for the process, keyed on its method and held strongly, as Go's static funcval is; a closure keeps today's word
// per delegate INSTANCE, held weakly. One band, one counter, one lock. Recovery rebinds across delegate types.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

public sealed class FuncWords
{
    const ulong Base = 0xC000_8000_0000_0000UL, BandMask = 0xFFFF_8000_0000_0000UL;   // GoFuncCookie's band, unchanged
    const int StrideShift = 4;
    readonly object m_gate = new();
    readonly Dictionary<RuntimeMethodHandle, nuint> m_staticWord = new();
    readonly ConditionalWeakTable<Delegate, StrongBox<nuint>> m_instanceWord = new();
    readonly Dictionary<nuint, Delegate> m_static = new();
    readonly Dictionary<nuint, WeakReference<Delegate>> m_instance = new();
    ulong m_next;

    public static bool IsWord(nuint w) => ((ulong)w & BandMask) == Base;

    public nuint Of(Delegate? fn)
    {
        if (fn is null) return 0;
        if (IntPtr.Size != 8) throw new PlatformNotSupportedException("func words need a 64-bit runtime");
        lock (m_gate)
        {
            if (fn.Target is null)
            {
                RuntimeMethodHandle h = fn.Method.MethodHandle;
                if (m_staticWord.TryGetValue(h, out nuint w)) return w;
                w = Next();
                m_staticWord.Add(h, w);
                m_static.Add(w, fn);
                return w;
            }
            if (m_instanceWord.TryGetValue(fn, out var box)) return box.Value;
            nuint word = Next();
            m_instanceWord.Add(fn, new StrongBox<nuint>(word));
            m_instance[word] = new WeakReference<Delegate>(fn);
            return word;
        }
    }

    nuint Next()
    {
        ulong index = ++m_next;
        if (index >= 1UL << (47 - StrideShift)) throw new InvalidOperationException("func word band exhausted");
        return (nuint)(Base + (index << StrideShift));
    }

    public TDst? Func<TDst>(nuint w) where TDst : Delegate
    {
        if (w == 0) return null;
        Delegate? fn;
        lock (m_gate)
        {
            if (!m_static.TryGetValue(w, out fn) && !(m_instance.TryGetValue(w, out var wr) && wr.TryGetTarget(out fn))) fn = null;
        }
        if (fn is null) throw new InvalidOperationException($"func word 0x{(ulong)w:x} names no live func");
        if (fn is TDst typed) return typed;
        if (Delegate.CreateDelegate(typeof(TDst), fn.Target, fn.Method, throwOnBindFailure: false) is TDst rebound) return rebound;   // GoFuncCookie's rebind
        throw new InvalidCastException($"func word 0x{(ulong)w:x} holds {fn.GetType().Name}, which cannot be read as {typeof(TDst).Name}");
    }
}

public delegate int Cmp(object t, nuint a);
public delegate int CmpOther(object t, nuint a, nuint b);

public static class Program
{
    public static int F0(object t, nuint a) => 0;
    public static int F1(object t, nuint a) => 1;
    public static int F2(object t, nuint a) => 2;
    public static int F3(object t, nuint a) => 3;
    public static int F4(object t, nuint a) => 4;
    public static int F5(object t, nuint a) => 5;
    public static int F6(object t, nuint a) => 6;
    public static int F7(object t, nuint a) => 7;
    public static int F8(object t, nuint a) => 8;
    public static int F9(object t, nuint a) => 9;
    public static int F10(object t, nuint a) => 10;
    public static int F11(object t, nuint a) => 11;
    public static int F12(object t, nuint a) => 12;
    public static int F13(object t, nuint a) => 13;
    public static int F14(object t, nuint a) => 14;
    public static int F15(object t, nuint a) => 15;
    public static int F16(object t, nuint a) => 16;
    public static int F17(object t, nuint a) => 17;
    public static int F18(object t, nuint a) => 18;
    public static int F19(object t, nuint a) => 19;
    public static int F20(object t, nuint a) => 20;
    public static int F21(object t, nuint a) => 21;
    public static int F22(object t, nuint a) => 22;
    public static int F23(object t, nuint a) => 23;
    public static int F24(object t, nuint a) => 24;
    public static int F25(object t, nuint a) => 25;
    public static int F26(object t, nuint a) => 26;
    public static int F27(object t, nuint a) => 27;
    public static int F28(object t, nuint a) => 28;
    public static int F29(object t, nuint a) => 29;
    public static int F30(object t, nuint a) => 30;
    public static int F31(object t, nuint a) => 31;
    public static int F32(object t, nuint a) => 32;
    public static int F33(object t, nuint a) => 33;
    public static int F34(object t, nuint a) => 34;
    public static int F35(object t, nuint a) => 35;
    public static int F36(object t, nuint a) => 36;
    public static int F37(object t, nuint a) => 37;
    public static int F38(object t, nuint a) => 38;
    public static int F39(object t, nuint a) => 39;
    public static int F40(object t, nuint a) => 40;
    public static int F41(object t, nuint a) => 41;
    public static int F42(object t, nuint a) => 42;
    public static int F43(object t, nuint a) => 43;
    public static int F44(object t, nuint a) => 44;
    public static int F45(object t, nuint a) => 45;
    public static int F46(object t, nuint a) => 46;
    public static int F47(object t, nuint a) => 47;
    public static int F48(object t, nuint a) => 48;
    public static int F49(object t, nuint a) => 49;
    public static int F50(object t, nuint a) => 50;
    public static int F51(object t, nuint a) => 51;
    public static int F52(object t, nuint a) => 52;
    public static int F53(object t, nuint a) => 53;
    public static int F54(object t, nuint a) => 54;
    public static int F55(object t, nuint a) => 55;
    public static int F56(object t, nuint a) => 56;
    public static int F57(object t, nuint a) => 57;
    public static int F58(object t, nuint a) => 58;
    public static int F59(object t, nuint a) => 59;
    public static int F60(object t, nuint a) => 60;
    public static int F61(object t, nuint a) => 61;
    public static int F62(object t, nuint a) => 62;
    public static int F63(object t, nuint a) => 63;
    public static int F64(object t, nuint a) => 64;
    public static int F65(object t, nuint a) => 65;
    public static int F66(object t, nuint a) => 66;
    public static int F67(object t, nuint a) => 67;
    public static int F68(object t, nuint a) => 68;
    public static int F69(object t, nuint a) => 69;
    public static int F70(object t, nuint a) => 70;
    public static int F71(object t, nuint a) => 71;
    public static int F72(object t, nuint a) => 72;
    public static int F73(object t, nuint a) => 73;
    public static int F74(object t, nuint a) => 74;
    public static int F75(object t, nuint a) => 75;
    public static int F76(object t, nuint a) => 76;
    public static int F77(object t, nuint a) => 77;
    public static int F78(object t, nuint a) => 78;
    public static int F79(object t, nuint a) => 79;
    public static int F80(object t, nuint a) => 80;
    public static int F81(object t, nuint a) => 81;
    public static int F82(object t, nuint a) => 82;
    public static int F83(object t, nuint a) => 83;
    public static int F84(object t, nuint a) => 84;
    public static int F85(object t, nuint a) => 85;
    public static int F86(object t, nuint a) => 86;
    public static int F87(object t, nuint a) => 87;
    public static int F88(object t, nuint a) => 88;
    public static int F89(object t, nuint a) => 89;
    public static int F90(object t, nuint a) => 90;
    public static int F91(object t, nuint a) => 91;
    public static int F92(object t, nuint a) => 92;
    public static int F93(object t, nuint a) => 93;
    public static int F94(object t, nuint a) => 94;
    public static int F95(object t, nuint a) => 95;
    public static int F96(object t, nuint a) => 96;
    public static int F97(object t, nuint a) => 97;
    public static int F98(object t, nuint a) => 98;
    public static int F99(object t, nuint a) => 99;
    public static int F100(object t, nuint a) => 100;
    public static int F101(object t, nuint a) => 101;
    public static int F102(object t, nuint a) => 102;
    public static int F103(object t, nuint a) => 103;
    public static int F104(object t, nuint a) => 104;
    public static int F105(object t, nuint a) => 105;
    public static int F106(object t, nuint a) => 106;
    public static int F107(object t, nuint a) => 107;
    public static int F108(object t, nuint a) => 108;
    public static int F109(object t, nuint a) => 109;
    public static int F110(object t, nuint a) => 110;
    public static int F111(object t, nuint a) => 111;
    public static int F112(object t, nuint a) => 112;
    public static int F113(object t, nuint a) => 113;
    public static int F114(object t, nuint a) => 114;
    public static int F115(object t, nuint a) => 115;
    public static int F116(object t, nuint a) => 116;
    public static int F117(object t, nuint a) => 117;
    public static int F118(object t, nuint a) => 118;
    public static int F119(object t, nuint a) => 119;
    public static int F120(object t, nuint a) => 120;
    public static int F121(object t, nuint a) => 121;
    public static int F122(object t, nuint a) => 122;
    public static int F123(object t, nuint a) => 123;
    public static int F124(object t, nuint a) => 124;
    public static int F125(object t, nuint a) => 125;
    public static int F126(object t, nuint a) => 126;
    public static int F127(object t, nuint a) => 127;
    public static int F128(object t, nuint a) => 128;
    public static int F129(object t, nuint a) => 129;
    public static int F130(object t, nuint a) => 130;
    public static int F131(object t, nuint a) => 131;
    public static int F132(object t, nuint a) => 132;
    public static int F133(object t, nuint a) => 133;
    public static int F134(object t, nuint a) => 134;
    public static int F135(object t, nuint a) => 135;
    public static int F136(object t, nuint a) => 136;
    public static int F137(object t, nuint a) => 137;
    public static int F138(object t, nuint a) => 138;
    public static int F139(object t, nuint a) => 139;
    public static int F140(object t, nuint a) => 140;
    public static int F141(object t, nuint a) => 141;
    public static int F142(object t, nuint a) => 142;
    public static int F143(object t, nuint a) => 143;
    public static int F144(object t, nuint a) => 144;
    public static int F145(object t, nuint a) => 145;
    public static int F146(object t, nuint a) => 146;
    public static int F147(object t, nuint a) => 147;
    public static int F148(object t, nuint a) => 148;
    public static int F149(object t, nuint a) => 149;
    public static int F150(object t, nuint a) => 150;
    public static int F151(object t, nuint a) => 151;
    public static int F152(object t, nuint a) => 152;
    public static int F153(object t, nuint a) => 153;
    public static int F154(object t, nuint a) => 154;
    public static int F155(object t, nuint a) => 155;
    public static int F156(object t, nuint a) => 156;
    public static int F157(object t, nuint a) => 157;
    public static int F158(object t, nuint a) => 158;
    public static int F159(object t, nuint a) => 159;
    public static int F160(object t, nuint a) => 160;
    public static int F161(object t, nuint a) => 161;
    public static int F162(object t, nuint a) => 162;
    public static int F163(object t, nuint a) => 163;
    public static int F164(object t, nuint a) => 164;
    public static int F165(object t, nuint a) => 165;
    public static int F166(object t, nuint a) => 166;
    public static int F167(object t, nuint a) => 167;
    public static int F168(object t, nuint a) => 168;
    public static int F169(object t, nuint a) => 169;
    public static int F170(object t, nuint a) => 170;
    public static int F171(object t, nuint a) => 171;
    public static int F172(object t, nuint a) => 172;
    public static int F173(object t, nuint a) => 173;
    public static int F174(object t, nuint a) => 174;
    public static int F175(object t, nuint a) => 175;
    public static int F176(object t, nuint a) => 176;
    public static int F177(object t, nuint a) => 177;
    public static int F178(object t, nuint a) => 178;
    public static int F179(object t, nuint a) => 179;
    public static int F180(object t, nuint a) => 180;
    public static int F181(object t, nuint a) => 181;
    public static int F182(object t, nuint a) => 182;
    public static int F183(object t, nuint a) => 183;
    public static int F184(object t, nuint a) => 184;
    public static int F185(object t, nuint a) => 185;
    public static int F186(object t, nuint a) => 186;
    public static int F187(object t, nuint a) => 187;
    public static int F188(object t, nuint a) => 188;
    public static int F189(object t, nuint a) => 189;
    public static int F190(object t, nuint a) => 190;
    public static int F191(object t, nuint a) => 191;
    public static int F192(object t, nuint a) => 192;
    public static int F193(object t, nuint a) => 193;
    public static int F194(object t, nuint a) => 194;
    public static int F195(object t, nuint a) => 195;
    public static int F196(object t, nuint a) => 196;
    public static int F197(object t, nuint a) => 197;
    public static int F198(object t, nuint a) => 198;
    public static int F199(object t, nuint a) => 199;
    static readonly Delegate[] Funcs = [
        new Func<object, nuint, int>(F0),
        new Func<object, nuint, int>(F1),
        new Func<object, nuint, int>(F2),
        new Func<object, nuint, int>(F3),
        new Func<object, nuint, int>(F4),
        new Func<object, nuint, int>(F5),
        new Func<object, nuint, int>(F6),
        new Func<object, nuint, int>(F7),
        new Func<object, nuint, int>(F8),
        new Func<object, nuint, int>(F9),
        new Func<object, nuint, int>(F10),
        new Func<object, nuint, int>(F11),
        new Func<object, nuint, int>(F12),
        new Func<object, nuint, int>(F13),
        new Func<object, nuint, int>(F14),
        new Func<object, nuint, int>(F15),
        new Func<object, nuint, int>(F16),
        new Func<object, nuint, int>(F17),
        new Func<object, nuint, int>(F18),
        new Func<object, nuint, int>(F19),
        new Func<object, nuint, int>(F20),
        new Func<object, nuint, int>(F21),
        new Func<object, nuint, int>(F22),
        new Func<object, nuint, int>(F23),
        new Func<object, nuint, int>(F24),
        new Func<object, nuint, int>(F25),
        new Func<object, nuint, int>(F26),
        new Func<object, nuint, int>(F27),
        new Func<object, nuint, int>(F28),
        new Func<object, nuint, int>(F29),
        new Func<object, nuint, int>(F30),
        new Func<object, nuint, int>(F31),
        new Func<object, nuint, int>(F32),
        new Func<object, nuint, int>(F33),
        new Func<object, nuint, int>(F34),
        new Func<object, nuint, int>(F35),
        new Func<object, nuint, int>(F36),
        new Func<object, nuint, int>(F37),
        new Func<object, nuint, int>(F38),
        new Func<object, nuint, int>(F39),
        new Func<object, nuint, int>(F40),
        new Func<object, nuint, int>(F41),
        new Func<object, nuint, int>(F42),
        new Func<object, nuint, int>(F43),
        new Func<object, nuint, int>(F44),
        new Func<object, nuint, int>(F45),
        new Func<object, nuint, int>(F46),
        new Func<object, nuint, int>(F47),
        new Func<object, nuint, int>(F48),
        new Func<object, nuint, int>(F49),
        new Func<object, nuint, int>(F50),
        new Func<object, nuint, int>(F51),
        new Func<object, nuint, int>(F52),
        new Func<object, nuint, int>(F53),
        new Func<object, nuint, int>(F54),
        new Func<object, nuint, int>(F55),
        new Func<object, nuint, int>(F56),
        new Func<object, nuint, int>(F57),
        new Func<object, nuint, int>(F58),
        new Func<object, nuint, int>(F59),
        new Func<object, nuint, int>(F60),
        new Func<object, nuint, int>(F61),
        new Func<object, nuint, int>(F62),
        new Func<object, nuint, int>(F63),
        new Func<object, nuint, int>(F64),
        new Func<object, nuint, int>(F65),
        new Func<object, nuint, int>(F66),
        new Func<object, nuint, int>(F67),
        new Func<object, nuint, int>(F68),
        new Func<object, nuint, int>(F69),
        new Func<object, nuint, int>(F70),
        new Func<object, nuint, int>(F71),
        new Func<object, nuint, int>(F72),
        new Func<object, nuint, int>(F73),
        new Func<object, nuint, int>(F74),
        new Func<object, nuint, int>(F75),
        new Func<object, nuint, int>(F76),
        new Func<object, nuint, int>(F77),
        new Func<object, nuint, int>(F78),
        new Func<object, nuint, int>(F79),
        new Func<object, nuint, int>(F80),
        new Func<object, nuint, int>(F81),
        new Func<object, nuint, int>(F82),
        new Func<object, nuint, int>(F83),
        new Func<object, nuint, int>(F84),
        new Func<object, nuint, int>(F85),
        new Func<object, nuint, int>(F86),
        new Func<object, nuint, int>(F87),
        new Func<object, nuint, int>(F88),
        new Func<object, nuint, int>(F89),
        new Func<object, nuint, int>(F90),
        new Func<object, nuint, int>(F91),
        new Func<object, nuint, int>(F92),
        new Func<object, nuint, int>(F93),
        new Func<object, nuint, int>(F94),
        new Func<object, nuint, int>(F95),
        new Func<object, nuint, int>(F96),
        new Func<object, nuint, int>(F97),
        new Func<object, nuint, int>(F98),
        new Func<object, nuint, int>(F99),
        new Func<object, nuint, int>(F100),
        new Func<object, nuint, int>(F101),
        new Func<object, nuint, int>(F102),
        new Func<object, nuint, int>(F103),
        new Func<object, nuint, int>(F104),
        new Func<object, nuint, int>(F105),
        new Func<object, nuint, int>(F106),
        new Func<object, nuint, int>(F107),
        new Func<object, nuint, int>(F108),
        new Func<object, nuint, int>(F109),
        new Func<object, nuint, int>(F110),
        new Func<object, nuint, int>(F111),
        new Func<object, nuint, int>(F112),
        new Func<object, nuint, int>(F113),
        new Func<object, nuint, int>(F114),
        new Func<object, nuint, int>(F115),
        new Func<object, nuint, int>(F116),
        new Func<object, nuint, int>(F117),
        new Func<object, nuint, int>(F118),
        new Func<object, nuint, int>(F119),
        new Func<object, nuint, int>(F120),
        new Func<object, nuint, int>(F121),
        new Func<object, nuint, int>(F122),
        new Func<object, nuint, int>(F123),
        new Func<object, nuint, int>(F124),
        new Func<object, nuint, int>(F125),
        new Func<object, nuint, int>(F126),
        new Func<object, nuint, int>(F127),
        new Func<object, nuint, int>(F128),
        new Func<object, nuint, int>(F129),
        new Func<object, nuint, int>(F130),
        new Func<object, nuint, int>(F131),
        new Func<object, nuint, int>(F132),
        new Func<object, nuint, int>(F133),
        new Func<object, nuint, int>(F134),
        new Func<object, nuint, int>(F135),
        new Func<object, nuint, int>(F136),
        new Func<object, nuint, int>(F137),
        new Func<object, nuint, int>(F138),
        new Func<object, nuint, int>(F139),
        new Func<object, nuint, int>(F140),
        new Func<object, nuint, int>(F141),
        new Func<object, nuint, int>(F142),
        new Func<object, nuint, int>(F143),
        new Func<object, nuint, int>(F144),
        new Func<object, nuint, int>(F145),
        new Func<object, nuint, int>(F146),
        new Func<object, nuint, int>(F147),
        new Func<object, nuint, int>(F148),
        new Func<object, nuint, int>(F149),
        new Func<object, nuint, int>(F150),
        new Func<object, nuint, int>(F151),
        new Func<object, nuint, int>(F152),
        new Func<object, nuint, int>(F153),
        new Func<object, nuint, int>(F154),
        new Func<object, nuint, int>(F155),
        new Func<object, nuint, int>(F156),
        new Func<object, nuint, int>(F157),
        new Func<object, nuint, int>(F158),
        new Func<object, nuint, int>(F159),
        new Func<object, nuint, int>(F160),
        new Func<object, nuint, int>(F161),
        new Func<object, nuint, int>(F162),
        new Func<object, nuint, int>(F163),
        new Func<object, nuint, int>(F164),
        new Func<object, nuint, int>(F165),
        new Func<object, nuint, int>(F166),
        new Func<object, nuint, int>(F167),
        new Func<object, nuint, int>(F168),
        new Func<object, nuint, int>(F169),
        new Func<object, nuint, int>(F170),
        new Func<object, nuint, int>(F171),
        new Func<object, nuint, int>(F172),
        new Func<object, nuint, int>(F173),
        new Func<object, nuint, int>(F174),
        new Func<object, nuint, int>(F175),
        new Func<object, nuint, int>(F176),
        new Func<object, nuint, int>(F177),
        new Func<object, nuint, int>(F178),
        new Func<object, nuint, int>(F179),
        new Func<object, nuint, int>(F180),
        new Func<object, nuint, int>(F181),
        new Func<object, nuint, int>(F182),
        new Func<object, nuint, int>(F183),
        new Func<object, nuint, int>(F184),
        new Func<object, nuint, int>(F185),
        new Func<object, nuint, int>(F186),
        new Func<object, nuint, int>(F187),
        new Func<object, nuint, int>(F188),
        new Func<object, nuint, int>(F189),
        new Func<object, nuint, int>(F190),
        new Func<object, nuint, int>(F191),
        new Func<object, nuint, int>(F192),
        new Func<object, nuint, int>(F193),
        new Func<object, nuint, int>(F194),
        new Func<object, nuint, int>(F195),
        new Func<object, nuint, int>(F196),
        new Func<object, nuint, int>(F197),
        new Func<object, nuint, int>(F198),
        new Func<object, nuint, int>(F199),
    ];
    static readonly Delegate[] Named = [
        new Cmp(F0),
        new Cmp(F1),
        new Cmp(F2),
        new Cmp(F3),
        new Cmp(F4),
        new Cmp(F5),
        new Cmp(F6),
        new Cmp(F7),
        new Cmp(F8),
        new Cmp(F9),
        new Cmp(F10),
        new Cmp(F11),
        new Cmp(F12),
        new Cmp(F13),
        new Cmp(F14),
        new Cmp(F15),
        new Cmp(F16),
        new Cmp(F17),
        new Cmp(F18),
        new Cmp(F19),
        new Cmp(F20),
        new Cmp(F21),
        new Cmp(F22),
        new Cmp(F23),
        new Cmp(F24),
        new Cmp(F25),
        new Cmp(F26),
        new Cmp(F27),
        new Cmp(F28),
        new Cmp(F29),
        new Cmp(F30),
        new Cmp(F31),
        new Cmp(F32),
        new Cmp(F33),
        new Cmp(F34),
        new Cmp(F35),
        new Cmp(F36),
        new Cmp(F37),
        new Cmp(F38),
        new Cmp(F39),
        new Cmp(F40),
        new Cmp(F41),
        new Cmp(F42),
        new Cmp(F43),
        new Cmp(F44),
        new Cmp(F45),
        new Cmp(F46),
        new Cmp(F47),
        new Cmp(F48),
        new Cmp(F49),
        new Cmp(F50),
        new Cmp(F51),
        new Cmp(F52),
        new Cmp(F53),
        new Cmp(F54),
        new Cmp(F55),
        new Cmp(F56),
        new Cmp(F57),
        new Cmp(F58),
        new Cmp(F59),
        new Cmp(F60),
        new Cmp(F61),
        new Cmp(F62),
        new Cmp(F63),
        new Cmp(F64),
        new Cmp(F65),
        new Cmp(F66),
        new Cmp(F67),
        new Cmp(F68),
        new Cmp(F69),
        new Cmp(F70),
        new Cmp(F71),
        new Cmp(F72),
        new Cmp(F73),
        new Cmp(F74),
        new Cmp(F75),
        new Cmp(F76),
        new Cmp(F77),
        new Cmp(F78),
        new Cmp(F79),
        new Cmp(F80),
        new Cmp(F81),
        new Cmp(F82),
        new Cmp(F83),
        new Cmp(F84),
        new Cmp(F85),
        new Cmp(F86),
        new Cmp(F87),
        new Cmp(F88),
        new Cmp(F89),
        new Cmp(F90),
        new Cmp(F91),
        new Cmp(F92),
        new Cmp(F93),
        new Cmp(F94),
        new Cmp(F95),
        new Cmp(F96),
        new Cmp(F97),
        new Cmp(F98),
        new Cmp(F99),
        new Cmp(F100),
        new Cmp(F101),
        new Cmp(F102),
        new Cmp(F103),
        new Cmp(F104),
        new Cmp(F105),
        new Cmp(F106),
        new Cmp(F107),
        new Cmp(F108),
        new Cmp(F109),
        new Cmp(F110),
        new Cmp(F111),
        new Cmp(F112),
        new Cmp(F113),
        new Cmp(F114),
        new Cmp(F115),
        new Cmp(F116),
        new Cmp(F117),
        new Cmp(F118),
        new Cmp(F119),
        new Cmp(F120),
        new Cmp(F121),
        new Cmp(F122),
        new Cmp(F123),
        new Cmp(F124),
        new Cmp(F125),
        new Cmp(F126),
        new Cmp(F127),
        new Cmp(F128),
        new Cmp(F129),
        new Cmp(F130),
        new Cmp(F131),
        new Cmp(F132),
        new Cmp(F133),
        new Cmp(F134),
        new Cmp(F135),
        new Cmp(F136),
        new Cmp(F137),
        new Cmp(F138),
        new Cmp(F139),
        new Cmp(F140),
        new Cmp(F141),
        new Cmp(F142),
        new Cmp(F143),
        new Cmp(F144),
        new Cmp(F145),
        new Cmp(F146),
        new Cmp(F147),
        new Cmp(F148),
        new Cmp(F149),
        new Cmp(F150),
        new Cmp(F151),
        new Cmp(F152),
        new Cmp(F153),
        new Cmp(F154),
        new Cmp(F155),
        new Cmp(F156),
        new Cmp(F157),
        new Cmp(F158),
        new Cmp(F159),
        new Cmp(F160),
        new Cmp(F161),
        new Cmp(F162),
        new Cmp(F163),
        new Cmp(F164),
        new Cmp(F165),
        new Cmp(F166),
        new Cmp(F167),
        new Cmp(F168),
        new Cmp(F169),
        new Cmp(F170),
        new Cmp(F171),
        new Cmp(F172),
        new Cmp(F173),
        new Cmp(F174),
        new Cmp(F175),
        new Cmp(F176),
        new Cmp(F177),
        new Cmp(F178),
        new Cmp(F179),
        new Cmp(F180),
        new Cmp(F181),
        new Cmp(F182),
        new Cmp(F183),
        new Cmp(F184),
        new Cmp(F185),
        new Cmp(F186),
        new Cmp(F187),
        new Cmp(F188),
        new Cmp(F189),
        new Cmp(F190),
        new Cmp(F191),
        new Cmp(F192),
        new Cmp(F193),
        new Cmp(F194),
        new Cmp(F195),
        new Cmp(F196),
        new Cmp(F197),
        new Cmp(F198),
        new Cmp(F199),
    ];
    static int fail;
    static void Check(string what, bool ok, string detail = "") { Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {what} {detail}"); if (!ok) fail++; }

    public static int Main()
    {
        var r = new FuncWords();
        // identity by METHOD for a static function, across delegate types and sites
        nuint a = r.Of(new Func<object, nuint, int>(F7)), b = r.Of(new Cmp(F7)), c = r.Of((Func<object, nuint, int>)F8);
        Check("one word per static function, whatever the delegate type (Func<> or a named delegate) or site", a == b && a != c);
        Check("the word is in GoFuncCookie's band", FuncWords.IsWord(a));
        Check("recovery as the minted type", r.Func<Func<object, nuint, int>>(a)!(null!, 0) == 7);
        Check("recovery as ANOTHER delegate type of the same signature REBINDS (TLARGE_INTEGER lifted twice)", r.Func<Cmp>(a)!(null!, 0) == 7);
        string bad; try { r.Func<CmpOther>(a); bad = "no exception"; } catch (Exception e) { bad = e.GetType().Name; }
        Check("a signature that cannot bind is refused by name", bad == "InvalidCastException", bad);
        // a closure keeps today's per-instance, weak word
        int k = 3; Func<object, nuint, int> cl1 = (t, x) => k, cl2 = (t, x) => k;
        Check("two closures get two words; one closure answers one word", r.Of(cl1) != r.Of(cl2) && r.Of(cl1) == r.Of(cl1));

        // THE RACE: 64 threads mint the same 200 functions in different orders and through two delegate types, on a fresh
        // registry each round. Every thread must see one word per function, and two functions never share one.
        int rounds = 300, collisions = 0, splits = 0;
        for (int round = 0; round < rounds; round++)
        {
            var reg = new FuncWords();
            var seen = new nuint[64][];
            var go = new ManualResetEventSlim();
            var threads = new Thread[64];
            for (int t = 0; t < 64; t++)
            {
                int tt = t;
                threads[t] = new Thread(() =>
                {
                    var mine = new nuint[Funcs.Length];
                    go.Wait();
                    for (int j = 0; j < Funcs.Length; j++)
                    {
                        int i = (j * 37 + tt * 11) % Funcs.Length;
                        mine[i] = reg.Of(tt % 2 == 0 ? Funcs[i] : Named[i]);
                    }
                    seen[tt] = mine;
                });
                threads[t].Start();
            }
            go.Set();
            foreach (var th in threads) th.Join();
            for (int i = 0; i < Funcs.Length; i++)
                for (int t = 1; t < 64; t++)
                    if (seen[t][i] != seen[0][i]) splits++;
            var all = new HashSet<nuint>(seen[0]);
            if (all.Count != Funcs.Length) collisions++;
        }
        Check($"race: {rounds} rounds x 64 threads x {Funcs.Length} functions, no function split across two words", splits == 0, $"splits {splits}");
        Check($"race: no two functions ever share a word", collisions == 0, $"rounds with a collision {collisions}");

        // cost, single thread, warm
        var f0 = (Func<object, nuint, int>)F0;
        nuint w0 = r.Of(f0);
        for (int pass = 0; pass < 2; pass++)
        {
            var sw = Stopwatch.StartNew(); nuint x = 0;
            for (int i = 0; i < 200_000; i++) x ^= r.Of(f0);
            double mint = sw.Elapsed.TotalNanoseconds / 200_000;
            sw.Restart(); int y = 0;
            for (int i = 0; i < 200_000; i++) y += r.Func<Func<object, nuint, int>>(w0)!(null!, 1);
            double rec = sw.Elapsed.TotalNanoseconds / 200_000;
            sw.Restart();
            for (int i = 0; i < 200_000; i++) y += r.Func<Cmp>(w0)!(null!, 1);
            double rebind = sw.Elapsed.TotalNanoseconds / 200_000;
            Console.WriteLine($"COST pass {pass}: mint {mint:F0} ns; recover and call {rec:F0} ns; recover with a REBIND and call {rebind:F0} ns ({x}, {y})");
        }
        Console.WriteLine($"failed: {fail}");
        return fail;
    }
}
