namespace go.github.com.itchyny;

using __yyfmt__ = fmt_package;

partial class gojq_package {

internal static slice<ж<FuncDef>> reverseFuncDef(slice<ж<FuncDef>> xs) {
    for ((nint i, nint j) = (0, len(xs) - 1); i < j; (i, j) = (i + 1, j - 1)) {
        (xs[i], xs[j]) = (xs[j], xs[i]);
    }
    return xs;
}

internal static slice<ж<FuncDef>> prependFuncDef(slice<ж<FuncDef>> xs, ж<FuncDef> x) {
    xs = builtin.append(xs, (ж<FuncDef>)(nil));
    copy(xs[1..], xs);
    xs[0] = x;
    return xs;
}

partial struct yySymType {
    internal nint yys;
    internal any value;
    internal @string token;
    internal Operator @operator;
}

internal static UntypedInt tokAltOp => 57346;
internal static UntypedInt tokUpdateOp => 57347;
internal static UntypedInt tokDestAltOp => 57348;
internal static UntypedInt tokCompareOp => 57349;
internal static UntypedInt tokOrOp => 57350;
internal static UntypedInt tokAndOp => 57351;
internal static UntypedInt tokModule => 57352;
internal static UntypedInt tokImport => 57353;
internal static UntypedInt tokInclude => 57354;
internal static UntypedInt tokDef => 57355;
internal static UntypedInt tokAs => 57356;
internal static UntypedInt tokLabel => 57357;
internal static UntypedInt tokBreak => 57358;
internal static UntypedInt tokNull => 57359;
internal static UntypedInt tokTrue => 57360;
internal static UntypedInt tokFalse => 57361;
internal static UntypedInt tokIf => 57362;
internal static UntypedInt tokThen => 57363;
internal static UntypedInt tokElif => 57364;
internal static UntypedInt tokElse => 57365;
internal static UntypedInt tokEnd => 57366;
internal static UntypedInt tokTry => 57367;
internal static UntypedInt tokCatch => 57368;
internal static UntypedInt tokReduce => 57369;
internal static UntypedInt tokForeach => 57370;
internal static UntypedInt tokIdent => 57371;
internal static UntypedInt tokVariable => 57372;
internal static UntypedInt tokModuleIdent => 57373;
internal static UntypedInt tokModuleVariable => 57374;
internal static UntypedInt tokRecurse => 57375;
internal static UntypedInt tokIndex => 57376;
internal static UntypedInt tokNumber => 57377;
internal static UntypedInt tokFormat => 57378;
internal static UntypedInt tokString => 57379;
internal static UntypedInt tokStringStart => 57380;
internal static UntypedInt tokStringQuery => 57381;
internal static UntypedInt tokStringEnd => 57382;
internal static UntypedInt tokInvalid => 57383;
internal static UntypedInt tokInvalidEscapeSequence => 57384;
internal static UntypedInt tokUnterminatedString => 57385;
internal static UntypedInt tokFuncDefQuery => 57386;
internal static UntypedInt tokExpr => 57387;
internal static UntypedInt tokTerm => 57388;
internal static UntypedInt tokEmptyCatch => 57389;

[GoLazy] partial class lazyᴛyyToknames { internal static array<@string> yyToknames = new @string[]{
    "$end",
    "error",
    "$unk",
    "tokAltOp",
    "tokUpdateOp",
    "tokDestAltOp",
    "tokCompareOp",
    "tokOrOp",
    "tokAndOp",
    "tokModule",
    "tokImport",
    "tokInclude",
    "tokDef",
    "tokAs",
    "tokLabel",
    "tokBreak",
    "tokNull",
    "tokTrue",
    "tokFalse",
    "tokIf",
    "tokThen",
    "tokElif",
    "tokElse",
    "tokEnd",
    "tokTry",
    "tokCatch",
    "tokReduce",
    "tokForeach",
    "tokIdent",
    "tokVariable",
    "tokModuleIdent",
    "tokModuleVariable",
    "tokRecurse",
    "tokIndex",
    "tokNumber",
    "tokFormat",
    "tokString",
    "tokStringStart",
    "tokStringQuery",
    "tokStringEnd",
    "tokInvalid",
    "tokInvalidEscapeSequence",
    "tokUnterminatedString",
    "tokFuncDefQuery",
    "tokExpr",
    "tokTerm",
    "'|'",
    "','",
    "'+'",
    "'-'",
    "'*'",
    "'/'",
    "'%'",
    "'.'",
    "'?'",
    "tokEmptyCatch",
    "'['",
    "';'",
    "':'",
    "'('",
    "')'",
    "']'",
    "'{'",
    "'}'"
}.array(); }

internal static array<@string> yyStatenames = new @string[]{}.array();

internal static UntypedInt yyEofCode => 1;
internal static UntypedInt yyErrCode => 2;
internal static UntypedInt yyInitialStackSize => 16;

[GoLazy] partial class lazyᴛyyExca { internal static array<int16> yyExca = new int16[]{
    (int16)(-1), 1,
    1, (int16)(-1),
    (int16)(-2), 0,
    (int16)(-1), 139,
    5, 0,
    (int16)(-2), 27,
    (int16)(-1), 142,
    7, 0,
    (int16)(-2), 30,
    (int16)(-1), 194,
    59, 114,
    (int16)(-2), 49
}.array(); }

internal static UntypedInt yyPrivate => 57344;

internal static UntypedInt yyLast => 733;

[GoLazy] partial class lazyᴛyyAct { internal static array<int16> yyAct = new int16[]{
    79, 129, 174, 195, 103, 10, 210, 190, 175, 227,
    135, 32, 5, 158, 102, 108, 48, 93, 6, 123,
    243, 234, 50, 246, 233, 226, 225, 242, 110, 157,
    98, 264, 15, 99, 115, 122, 97, 245, 104, 120,
    224, 148, 149, 114, 37, 21, 22, 23, 33, 207,
    74, 253, 206, 34, 130, 35, 36, 39, 41, 40,
    42, 19, 20, 28, 31, 43, 44, 116, 117, 118,
    240, 229, 228, 92, 127, 160, 133, 29, 30, 159,
    126, 131, 18, 73, 75, 27, 136, 132, 38, 43,
    44, 26, 125, 95, 94, 204, 96, 153, 203, 124,
    156, 252, 198, 45, 239, 186, 161, 151, 74, 96,
    88, 89, 90, 150, 138, 139, 140, 141, 142, 143,
    144, 145, 146, 147, 137, 179, 180, 181, 183, 184,
    43, 44, 77, 188, 179, 180, 181, 191, 199, 50,
    173, 73, 75, 177, 192, 178, 185, 76, 74, 200,
    74, 119, 177, 202, 178, 205, 74, 80, 86, 87,
    88, 89, 90, 266, 214, 182, 91, 212, 216, 217,
    104, 6, 218, 209, 182, 165, 74, 74, 74, 219,
    6, 73, 75, 73, 75, 221, 222, 74, 223, 73,
    75, 134, 211, 211, 230, 74, 255, 281, 215, 3,
    279, 196, 197, 278, 136, 236, 231, 74, 238, 73,
    75, 73, 75, 170, 273, 171, 169, 74, 25, 74,
    73, 75, 74, 24, 237, 220, 162, 247, 73, 75,
    249, 250, 191, 176, 277, 248, 47, 17, 254, 192,
    73, 75, 251, 261, 262, 9, 256, 257, 101, 263,
    73, 75, 73, 75, 265, 73, 75, 259, 260, 78,
    267, 268, 164, 272, 244, 258, 271, 112, 113, 213,
    274, 275, 211, 211, 276, 74, 241, 155, 121, 74,
    280, 52, 53, 54, 55, 56, 57, 58, 59, 60,
    61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
    71, 72, 106, 107, 189, 187, 128, 201, 73, 75,
    43, 44, 73, 75, 7, 8, 4, 2, 74, 1,
    111, 0, 168, 270, 16, 13, 14, 37, 21, 22,
    23, 33, 0, 105, 13, 0, 34, 208, 35, 36,
    39, 41, 40, 42, 19, 20, 28, 31, 43, 44,
    0, 73, 75, 81, 82, 0, 85, 83, 84, 0,
    29, 30, 269, 167, 85, 18, 84, 0, 27, 0,
    154, 38, 0, 152, 26, 52, 53, 54, 55, 56,
    57, 58, 59, 60, 61, 62, 63, 64, 65, 66,
    67, 68, 69, 70, 71, 72, 106, 107, 86, 87,
    88, 89, 90, 0, 43, 44, 86, 87, 88, 89,
    90, 0, 74, 0, 0, 0, 11, 12, 16, 0,
    14, 37, 21, 22, 23, 33, 0, 105, 0, 0,
    34, 100, 35, 36, 39, 41, 40, 42, 19, 20,
    28, 31, 43, 44, 0, 73, 75, 0, 81, 82,
    0, 85, 83, 84, 29, 30, 232, 0, 166, 18,
    0, 0, 27, 0, 0, 38, 0, 16, 26, 14,
    37, 21, 22, 23, 33, 0, 0, 0, 0, 34,
    0, 35, 36, 39, 41, 40, 42, 19, 20, 28,
    31, 43, 44, 86, 87, 88, 89, 90, 0, 0,
    0, 0, 0, 29, 30, 85, 83, 84, 18, 0,
    0, 27, 0, 0, 38, 0, 235, 26, 16, 0,
    14, 37, 21, 22, 23, 33, 0, 0, 0, 0,
    34, 0, 35, 36, 39, 41, 40, 42, 19, 20,
    28, 31, 43, 44, 0, 74, 0, 86, 87, 88,
    89, 90, 163, 0, 29, 30, 0, 0, 0, 18,
    0, 0, 27, 0, 0, 38, 0, 109, 26, 16,
    0, 14, 37, 21, 22, 23, 33, 0, 73, 75,
    0, 34, 0, 35, 36, 39, 41, 40, 42, 19,
    20, 28, 31, 43, 44, 0, 0, 81, 82, 0,
    85, 83, 84, 0, 0, 29, 30, 0, 0, 0,
    18, 0, 0, 27, 0, 0, 38, 0, 0, 26,
    52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
    62, 63, 64, 65, 66, 67, 68, 69, 70, 71,
    72, 49, 86, 87, 88, 89, 90, 0, 0, 51,
    52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
    62, 63, 64, 65, 66, 67, 68, 69, 70, 71,
    72, 49, 85, 0, 0, 0, 172, 0, 0, 51,
    52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
    62, 63, 64, 65, 66, 67, 68, 69, 70, 71,
    72, 106, 194, 0, 0, 0, 46, 0, 0, 43,
    44, 0, 0, 0, 86, 87, 88, 89, 90, 0,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 193
}.array(); }

[GoLazy] partial class lazyᴛyyPact { internal static array<int16> yyPact = new int16[]{
    189, (int16)(-1000), (int16)(-1000), (int16)(-45), 405, 45, 642, (int16)(-1000), (int16)(-1000), (int16)(-1000),
    94, 110, 95, 556, 127, 593, 137, 39, 52, (int16)(-1000),
    (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-27), (int16)(-1000), 367, 505, (int16)(-1000), 28,
    28, 93, (int16)(-1000), 556, 28, 28, 28, 121, 556, (int16)(-1000),
    (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-29), (int16)(-1000), 40,
    33, 21, (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000),
    (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000),
    (int16)(-1000), (int16)(-1000), (int16)(-1000), 556, 24, 556, 177, (int16)(-45), (int16)(-1000), 94,
    77, 28, 28, 28, 28, 28, 28, 28, 28, 28,
    28, (int16)(-18), (int16)(-1000), (int16)(-1000), (int16)(-1000), 52, 311, (int16)(-1000), (int16)(-1000), 556,
    (int16)(-1000), (int16)(-35), (int16)(-1000), 20, 16, 556, (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000),
    164, 556, 39, 39, (int16)(-1000), 531, 149, 444, 349, (int16)(-1000),
    261, 176, (int16)(-1000), 612, 108, 108, 108, 94, 99, (int16)(-1000),
    (int16)(-1000), 24, 672, 163, 172, 44, (int16)(-1000), 556, 593, 498,
    357, 665, 109, 59, 59, (int16)(-1000), (int16)(-1000), (int16)(-1000), 556, 172,
    (int16)(-1000), (int16)(-1000), (int16)(-1000), 36, 556, (int16)(-9), 94, (int16)(-1000), 273, 28,
    28, 208, (int16)(-1000), 556, (int16)(-1000), 28, 24, 24, (int16)(-1000), (int16)(-1000),
    (int16)(-1000), 556, (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000), (int16)(-1000),
    (int16)(-1000), (int16)(-1000), 117, (int16)(-1000), (int16)(-1000), 556, 24, (int16)(-22), (int16)(-1000), (int16)(-39),
    (int16)(-1000), 13, 12, 556, (int16)(-1000), (int16)(-45), (int16)(-1000), (int16)(-1000), (int16)(-1000), 94,
    398, (int16)(-37), (int16)(-1000), (int16)(-1000), 454, 162, (int16)(-1000), 556, (int16)(-1000), (int16)(-1000),
    57, 593, 57, 11, 94, (int16)(-1000), (int16)(-33), (int16)(-40), 203, (int16)(-1000),
    (int16)(-25), (int16)(-1000), 94, (int16)(-1000), (int16)(-1000), 24, (int16)(-1000), 672, 24, 24,
    181, 43, (int16)(-1000), (int16)(-8), 172, (int16)(-1000), 134, (int16)(-1000), 94, 28,
    28, 235, 556, 556, (int16)(-1000), (int16)(-1000), 108, (int16)(-1000), (int16)(-1000), (int16)(-1000),
    (int16)(-1000), (int16)(-28), (int16)(-1000), 556, (int16)(-1000), (int16)(-1000), 57, 57, 139, 556,
    556, 304, 265, (int16)(-1000), 24, 205, (int16)(-1000), 193, 94, 556,
    556, (int16)(-1000), (int16)(-1000), 556, 173, 142, 94, (int16)(-1000), (int16)(-1000), 556,
    136, (int16)(-1000)
}.array(); }

[GoLazy] partial class lazyᴛyyPgo { internal static array<int16> yyPgo = new int16[]{
    0, 319, 317, 316, 315, 10, 314, 245, 320, 307,
    0, 306, 1, 305, 304, 7, 32, 237, 11, 278,
    17, 277, 276, 265, 262, 248, 14, 6, 2, 8,
    236, 16, 233, 225, 3, 223, 218, 15, 4
}.array(); }

[GoLazy] partial class lazyᴛyyR1 { internal static array<int8> yyR1 = new int8[]{
    0, 1, 2, 2, 3, 3, 4, 4, 5, 5,
    6, 6, 7, 7, 8, 8, 9, 9, 34, 34,
    10, 10, 10, 10, 10, 10, 16, 16, 16, 16,
    16, 16, 16, 16, 16, 16, 16, 11, 11, 12,
    12, 12, 13, 13, 14, 14, 15, 15, 15, 15,
    17, 17, 17, 17, 17, 17, 17, 17, 17, 17,
    17, 17, 17, 17, 17, 17, 17, 17, 17, 17,
    17, 17, 17, 17, 17, 17, 17, 17, 17, 17,
    17, 17, 17, 17, 18, 18, 19, 19, 19, 35,
    35, 36, 36, 20, 20, 20, 20, 20, 21, 21,
    22, 22, 23, 23, 24, 24, 25, 25, 26, 26,
    26, 26, 26, 38, 38, 38, 27, 27, 28, 28,
    28, 28, 28, 28, 28, 29, 29, 29, 30, 30,
    31, 31, 31, 32, 32, 33, 33, 37, 37, 37,
    37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
    37, 37, 37, 37, 37, 37, 37, 37
}.array(); }

[GoLazy] partial class lazyᴛyyR2 { internal static array<int8> yyR2 = new int8[]{
    0, 3, 0, 3, 0, 2, 6, 4, 0, 1,
    1, 1, 0, 2, 5, 8, 1, 3, 1, 1,
    2, 3, 5, 4, 3, 1, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 1, 1, 3, 1,
    3, 3, 1, 3, 1, 3, 3, 3, 5, 1,
    1, 1, 1, 2, 2, 1, 1, 1, 1, 4,
    1, 2, 3, 4, 2, 3, 1, 2, 2, 1,
    2, 1, 7, 3, 9, 9, 11, 2, 3, 2,
    2, 2, 3, 3, 1, 3, 0, 2, 4, 1,
    1, 1, 1, 2, 3, 4, 4, 5, 1, 3,
    0, 5, 0, 2, 0, 2, 1, 3, 3, 3,
    5, 1, 1, 1, 1, 1, 3, 1, 1, 1,
    1, 1, 1, 1, 1, 2, 3, 4, 1, 3,
    3, 3, 3, 2, 3, 1, 3, 1, 1, 1,
    1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    1, 1, 1, 1, 1, 1, 1, 1
}.array(); }

[GoLazy] partial class lazyᴛyyChk { internal static array<int16> yyChk = new int16[]{
    (int16)(-1000), (int16)(-1), (int16)(-2), 10, (int16)(-3), (int16)(-29), 63, (int16)(-6), (int16)(-4), (int16)(-7),
    (int16)(-10), 11, 12, (int16)(-8), 15, (int16)(-16), 13, (int16)(-17), 54, 33,
    34, 17, 18, 19, (int16)(-35), (int16)(-36), 63, 57, 35, 49,
    50, 36, (int16)(-18), 20, 25, 27, 28, 16, 60, 29,
    31, 30, 32, 37, 38, 58, 64, (int16)(-30), (int16)(-31), 29,
    (int16)(-37), 37, 8, 9, 10, 11, 12, 13, 14, 15,
    16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
    26, 27, 28, 47, 14, 48, 37, 37, (int16)(-7), (int16)(-10),
    30, 4, 5, 8, 9, 7, 49, 50, 51, 52,
    53, 29, 34, (int16)(-20), 55, 54, 57, (int16)(-20), (int16)(-18), 60,
    64, (int16)(-25), (int16)(-26), (int16)(-38), (int16)(-18), 60, 29, 30, (int16)(-37), 62,
    (int16)(-10), (int16)(-8), (int16)(-17), (int16)(-17), (int16)(-18), (int16)(-10), (int16)(-16), (int16)(-16), (int16)(-16), 30,
    (int16)(-10), (int16)(-19), 64, 48, 59, 59, 59, (int16)(-10), (int16)(-11), (int16)(-12),
    30, 57, 63, (int16)(-10), 14, (int16)(-5), (int16)(-29), 47, (int16)(-16), (int16)(-16),
    (int16)(-16), (int16)(-16), (int16)(-16), (int16)(-16), (int16)(-16), (int16)(-16), (int16)(-16), (int16)(-16), 59, 60,
    (int16)(-20), (int16)(-18), 62, (int16)(-10), 59, (int16)(-21), (int16)(-10), 64, 48, 59,
    59, (int16)(-10), 62, 21, (int16)(-24), 26, 14, 14, 61, 40,
    37, 39, 64, (int16)(-31), (int16)(-28), (int16)(-29), (int16)(-32), 35, 37, 17,
    18, 19, 57, (int16)(-28), (int16)(-28), 47, 6, (int16)(-13), (int16)(-12), (int16)(-14),
    (int16)(-15), (int16)(-38), (int16)(-18), 60, 30, (int16)(-34), 29, 30, 58, (int16)(-10),
    (int16)(-10), (int16)(-9), (int16)(-34), 62, 59, (int16)(-10), 61, 58, 64, (int16)(-26),
    (int16)(-27), (int16)(-16), (int16)(-27), 61, (int16)(-10), (int16)(-16), (int16)(-12), (int16)(-12), (int16)(-10), 62,
    (int16)(-33), (int16)(-28), (int16)(-10), (int16)(-12), 62, 48, 64, 48, 59, 59,
    (int16)(-10), (int16)(-5), 58, 61, 58, 62, (int16)(-10), 62, (int16)(-10), 47,
    59, (int16)(-22), 60, 60, 61, 62, 48, (int16)(-12), (int16)(-15), (int16)(-12),
    (int16)(-12), 61, 58, 59, (int16)(-34), 62, (int16)(-27), (int16)(-27), (int16)(-23), 22,
    23, (int16)(-10), (int16)(-10), (int16)(-28), 59, (int16)(-10), 24, (int16)(-10), (int16)(-10), 58,
    58, (int16)(-12), 58, 21, (int16)(-10), (int16)(-10), (int16)(-10), 61, 61, 58,
    (int16)(-10), 61
}.array(); }

[GoLazy] partial class lazyᴛyyDef { internal static array<int16> yyDef = new int16[]{
    2, (int16)(-2), 4, 0, 12, 0, 0, 1, 5, 10,
    11, 0, 0, 12, 0, 25, 0, 36, 50, 51,
    52, 55, 56, 57, 58, 60, 0, 0, 66, 0,
    0, 69, 71, 0, 0, 0, 0, 0, 0, 89,
    90, 91, 92, 84, 86, 3, 125, 0, 128, 0,
    0, 0, 137, 138, 139, 140, 141, 142, 143, 144,
    145, 146, 147, 148, 149, 150, 151, 152, 153, 154,
    155, 156, 157, 0, 0, 0, 0, 8, 13, 20,
    0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 79, 80, 81, 0, 0, 53, 54, 0,
    61, 0, 106, 111, 112, 0, 113, 114, 115, 64,
    0, 0, 67, 68, 70, 0, 104, 0, 0, 77,
    0, 0, 126, 0, 0, 0, 0, 21, 0, 37,
    39, 0, 0, 24, 0, 0, 9, 0, 26, (int16)(-2),
    28, 29, (int16)(-2), 31, 32, 33, 34, 35, 0, 0,
    82, 83, 93, 0, 0, 0, 98, 62, 0, 0,
    0, 0, 65, 0, 73, 0, 0, 0, 78, 85,
    87, 0, 127, 129, 130, 118, 119, 120, 121, 122,
    123, 124, 0, 131, 132, 0, 0, 0, 42, 0,
    44, 0, 0, 0, (int16)(-2), 8, 18, 19, 7, 23,
    0, 0, 16, 94, 0, 0, 59, 0, 63, 107,
    108, 117, 109, 0, 100, 105, 0, 0, 0, 133,
    0, 135, 22, 38, 40, 0, 41, 0, 0, 0,
    0, 0, 14, 0, 0, 95, 0, 96, 99, 0,
    0, 102, 0, 0, 88, 134, 0, 43, 45, 46,
    47, 0, 6, 0, 17, 97, 116, 110, 0, 0,
    0, 0, 0, 136, 0, 0, 72, 0, 103, 0,
    0, 48, 15, 0, 0, 0, 101, 74, 75, 0,
    0, 76
}.array(); }

[GoLazy] partial class lazyᴛyyTok1 { internal static array<int8> yyTok1 = new int8[]{
    1, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 53, 3, 3,
    60, 61, 51, 49, 48, 50, 54, 52, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 59, 58,
    3, 3, 3, 55, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 57, 3, 62, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
    3, 3, 3, 63, 47, 64
}.array(); }

[GoLazy] partial class lazyᴛyyTok2 { internal static array<int8> yyTok2 = new int8[]{
    2, 3, 4, 5, 6, 7, 8, 9, 10, 11,
    12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
    22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
    32, 33, 34, 35, 36, 37, 38, 39, 40, 41,
    42, 43, 44, 45, 46, 56
}.array(); }

internal static array<int8> yyTok3 = new int8[]{
    0
}.array();


partial struct yyErrorMessagesᴛ1 /*dyn*/ {
    internal nint state;
    internal nint token;
    internal @string msg;
}
internal static array<yyErrorMessagesᴛ1> yyErrorMessages = new yyErrorMessagesᴛ1[]{}.array();

internal static nint yyDebug = 0;
internal static bool yyErrorVerbose = false;

partial interface yyLexer {
    nint Lex(ж<yySymType> lval);
    void Error(@string s);
}

partial interface yyParser {
    nint Parse(yyLexer _);
    nint Lookahead();
}

partial struct yyParserImpl {
    internal yySymType lval;
    internal array<yySymType> stack = new(yyInitialStackSize);
    internal nint @char;
}

internal static nint Lookahead(this ref yyParserImpl p) {
    return p.@char;
}

internal static yyParser yyNewParser() {
    return new yyParserImplжyyParser(Ꮡ(new yyParserImpl()));
}

internal static UntypedInt yyFlag => -1000;

internal static @string yyTokname(nint c) {
    if (c >= 1 && c - 1 < len(yyToknames)) {
        if (yyToknames[c - 1] != "") {
            return yyToknames[c - 1];
        }
    }
    return __yyfmt__.Sprintf("tok-%v"u8, c);
}

internal static @string yyStatname(nint s) {
    if (s >= 0 && s < len(yyStatenames)) {
        if (yyStatenames[s] != "") {
            return yyStatenames[s];
        }
    }
    return __yyfmt__.Sprintf("state-%v"u8, s);
}

internal static @string yyErrorMessage(nint state, nint lookAhead) {
    const nint TOKSTART = 4;

    if (!yyErrorVerbose) {
        return "syntax error";
    }

    foreach (var (_, e) in range(yyErrorMessages.ΔRangeSnapshot())) {
        if (e.state == state && e.token == lookAhead) {
            return "syntax error: "u8 + e.msg;
        }
    }

    @string res = "syntax error: unexpected "u8 + yyTokname(lookAhead);

    var expected = new slice<nint>(0, 4);

    nint @base = (nint)yyPact[state];
    for (nint tok = TOKSTART; tok - 1 < len(yyToknames); tok++) {
        nint n = @base + tok; if (n >= 0 && n < yyLast && (nint)yyChk[(nint)yyAct[n]] == tok) {
            if (len(expected) == cap(expected)) {
                return res;
            }
            expected = builtin.append(expected, tok);
        }
    }

    if (yyDef[state] == -2) {
        nint i = 0;
        while (yyExca[i] != -1 || (nint)yyExca[i + 1] != state) {
            i += 2;
        }

        for (i += 2; yyExca[i] >= 0; i += 2) {
            nint tok = (nint)yyExca[i];
            if (tok < TOKSTART || yyExca[i + 1] == 0) {
                continue;
            }
            if (len(expected) == cap(expected)) {
                return res;
            }
            expected = builtin.append(expected, tok);
        }

        if (yyExca[i + 1] != 0) {
            return res;
        }
    }

    foreach (var (i, tok) in range(expected)) {
        if (i == 0) {
            res += ", expecting "u8;
        } else {
            res += " or "u8;
        }
        res += yyTokname(tok);
    }
    return res;
}

internal static (nint @char, nint token) yylex1(yyLexer lex, ж<yySymType> lval) {
    nint @char = default;
    nint token = default;

    token = 0;
    @char = lex.Lex(lval);
    if (@char <= 0) {
        token = (nint)yyTok1[0];
        goto @out;
    }
    if (@char < len(yyTok1)) {
        token = (nint)yyTok1[@char];
        goto @out;
    }
    if (@char >= yyPrivate) {
        if (@char < (nint)((nint)yyPrivate + len(yyTok2))) {
            token = (nint)yyTok2[@char - (nint)yyPrivate];
            goto @out;
        }
    }
    for (nint i = 0; i < len(yyTok3); i += 2) {
        token = (nint)yyTok3[i + 0];
        if (token == @char) {
            token = (nint)yyTok3[i + 1];
            goto @out;
        }
    }

@out:
    if (token == 0) {
        token = (nint)yyTok2[1];
    }
    if (yyDebug >= 3) {
        __yyfmt__.Printf("lex %s(%d)\n"u8, yyTokname(token), (nuint)@char);
    }
    return (@char, token);
}

internal static nint yyParse(yyLexer yylex) {
    return yyNewParser().Parse(yylex);
}

internal static nint Parse(this ж<yyParserImpl> Ꮡyyrcvr, yyLexer yylex) {
    GoFrame ᒐ = default;
    try {
        ref var yyrcvr = ref Ꮡyyrcvr.DerefOrNull();

        nint yyn = default;
        yySymType yyVAL = default;
        slice<yySymType> yyDollar = default;
        _ = yyDollar;
        var yyS = yyrcvr.stack[..];

        nint Nerrs = 0;
        nint Errflag = 0;
        nint yystate = 0;
        yyrcvr.@char = -1;
        nint yytoken = -1;
        defer(() => {
            yystate = -1;
            Ꮡyyrcvr.Value.@char = -1;
            yytoken = -1;
        }, ref ᒐ);
        nint yyp = -1;
        goto yystack;

ret0:
        return 0;

ret1:
        return 1;

yystack:
        if (yyDebug >= 4) {
            __yyfmt__.Printf("char %v in %v\n"u8, yyTokname(yytoken), yyStatname(yystate));
        }

        yyp++;
        if (yyp >= len(yyS)) {
            var nyys = new slice<yySymType>(len(yyS) * 2);
            copy(nyys, yyS);
            yyS = nyys;
        }
        yyS[yyp] = yyVAL;
        yyS[yyp].yys = yystate;

yynewstate:
        yyn = (nint)yyPact[yystate];
        if (yyn <= yyFlag) {
            goto yydefault;
        }
        if (yyrcvr.@char < 0) {
            (yyrcvr.@char, yytoken) = yylex1(yylex, Ꮡyyrcvr.Ꮡlval);
        }
        yyn += yytoken;
        if (yyn < 0 || yyn >= yyLast) {
            goto yydefault;
        }
        yyn = (nint)yyAct[yyn];
        if ((nint)yyChk[yyn] == yytoken) {
            yyrcvr.@char = -1;
            yytoken = -1;
            yyVAL = yyrcvr.lval;
            yystate = yyn;
            if (Errflag > 0) {
                Errflag--;
            }
            goto yystack;
        }

yydefault:
        yyn = (nint)yyDef[yystate];
        if (yyn == -2) {
            if (yyrcvr.@char < 0) {
                (yyrcvr.@char, yytoken) = yylex1(yylex, Ꮡyyrcvr.Ꮡlval);
            }

            nint xi = 0;
            for (;;) {
                if (yyExca[xi + 0] == -1 && (nint)yyExca[xi + 1] == yystate) {
                    break;
                }
                xi += 2;
            }
            for (xi += 2; ; xi += 2) {
                yyn = (nint)yyExca[xi + 0];
                if (yyn < 0 || yyn == yytoken) {
                    break;
                }
            }
            yyn = (nint)yyExca[xi + 1];
            if (yyn < 0) {
                goto ret0;
            }
        }
        if (yyn == 0) {
            var exprᴛ1 = Errflag;
            var matchᴛ1 = false;
            if (exprᴛ1 is 0) { matchᴛ1 = true;
                yylex.Error(yyErrorMessage(yystate, yytoken));
                Nerrs++;
                if (yyDebug >= 1) {
                    __yyfmt__.Printf("%s"u8, yyStatname(yystate));
                    __yyfmt__.Printf(" saw %s\n"u8, yyTokname(yytoken));
                }
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && (exprᴛ1 is 1 or 2)) {
                Errflag = 3;

                while (yyp >= 0) {
                    yyn = (nint)yyPact[yyS[yyp].yys] + (nint)yyErrCode;
                    if (yyn >= 0 && yyn < yyLast) {
                        yystate = (nint)yyAct[yyn];
                        if ((nint)yyChk[yystate] == yyErrCode) {
                            goto yystack;
                        }
                    }

                    if (yyDebug >= 2) {
                        __yyfmt__.Printf("error recovery pops state %d\n"u8, yyS[yyp].yys);
                    }
                    yyp--;
                }
                goto ret1;
            }
            else if (exprᴛ1 is 3) { matchᴛ1 = true;
                if (yyDebug >= 2) {
                    __yyfmt__.Printf("error recovery discards %s\n"u8, yyTokname(yytoken));
                }
                if (yytoken == yyEofCode) {
                    goto ret1;
                }
                yyrcvr.@char = -1;
                yytoken = -1;
                goto yynewstate;
            }

        }

        if (yyDebug >= 2) {
            __yyfmt__.Printf("reduce %v in:\n\t%v\n"u8, yyn, yyStatname(yystate));
        }

        nint yynt = yyn;
        nint yypt = yyp;
        _ = yypt;

        yyp -= (nint)yyR2[yyn];
        if (yyp + 1 >= len(yyS)) {
            var nyys = new slice<yySymType>(len(yyS) * 2);
            copy(nyys, yyS);
            yyS = nyys;
        }
        yyVAL = yyS[yyp + 1];

        yyn = (nint)yyR1[yyn];
        nint yyg = (nint)yyPgo[yyn];
        nint yyj = yyg + yyS[yyp].yys + 1;

        if (yyj >= yyLast) {
            yystate = (nint)yyAct[yyg];
        } else {
            yystate = (nint)yyAct[yyj];
            if ((nint)yyChk[yystate] != -yyn) {
                yystate = (nint)yyAct[yyg];
            }
        }
        switch (yynt) {
        case 1: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                var query = yyDollar[3].value._<ж<Query>>();
                query.Meta = yyDollar[1].value._<ж<ConstObject>>();
                query.Imports = yyDollar[2].value._<slice<ж<Import>>>();
                yylex._<yyLexer, ж<lexer>>().result = query;
            }
            break;
        }
        case 2: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = (ж<ConstObject>)nil;
            }
            break;
        }
        case 3: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = yyDollar[2].value;
            }
            break;
        }
        case 4: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = slice<ж<Import>>(default);
            }
            break;
        }
        case 5: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<Import>>>(), yyDollar[2].value._<ж<Import>>());
            }
            break;
        }
        case 6: {
            yyDollar = yyS.slice(yypt - 6, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Import(ImportPath: yyDollar[2].token, ImportAlias: yyDollar[4].token, Meta: yyDollar[5].value._<ж<ConstObject>>()));
            }
            break;
        }
        case 7: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Import(IncludePath: yyDollar[2].token, Meta: yyDollar[3].value._<ж<ConstObject>>()));
            }
            break;
        }
        case 8: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = (ж<ConstObject>)nil;
            }
            break;
        }
        case 10: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(FuncDefs: reverseFuncDef(yyDollar[1].value._<slice<ж<FuncDef>>>())));
            }
            break;
        }
        case 12: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = slice<ж<FuncDef>>(default);
            }
            break;
        }
        case 13: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[2].value._<slice<ж<FuncDef>>>(), yyDollar[1].value._<ж<FuncDef>>());
            }
            break;
        }
        case 14: {
            yyDollar = yyS.slice(yypt - 5, yypt + 1);
            {
                yyVAL.value = Ꮡ(new FuncDef(Name: yyDollar[2].token, Body: yyDollar[4].value._<ж<Query>>()));
            }
            break;
        }
        case 15: {
            yyDollar = yyS.slice(yypt - 8, yypt + 1);
            {
                yyVAL.value = Ꮡ(new FuncDef(yyDollar[2].token, yyDollar[4].value._<slice<@string>>(), yyDollar[7].value._<ж<Query>>()));
            }
            break;
        }
        case 16: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<@string>([yyDollar[1].token]);
            }
            break;
        }
        case 17: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<@string>>(), yyDollar[3].token);
            }
            break;
        }
        case 20: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                var query = yyDollar[2].value._<ж<Query>>();
                query.FuncDefs = prependFuncDef(query.FuncDefs, yyDollar[1].value._<ж<FuncDef>>());
                yyVAL.value = query.OrTypedNil();
            }
            break;
        }
        case 21: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpPipe, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 22: {
            yyDollar = yyS.slice(yypt - 5, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpPipe, Right: yyDollar[5].value._<ж<Query>>(), Patterns: yyDollar[3].value._<slice<ж<Pattern>>>()));
            }
            break;
        }
        case 23: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeLabel, Label: Ꮡ(new Label(yyDollar[2].token, yyDollar[4].value._<ж<Query>>()))))));
            }
            break;
        }
        case 24: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpComma, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 26: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: yyDollar[2].@operator, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 27: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: yyDollar[2].@operator, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 28: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpOr, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 29: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpAnd, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 30: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: yyDollar[2].@operator, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 31: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpAdd, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 32: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpSub, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 33: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpMul, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 34: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpDiv, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 35: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpMod, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 36: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Term: yyDollar[1].value._<ж<Term>>()));
            }
            break;
        }
        case 37: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<Pattern>>([yyDollar[1].value._<ж<Pattern>>()]);
            }
            break;
        }
        case 38: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<Pattern>>>(), yyDollar[3].value._<ж<Pattern>>());
            }
            break;
        }
        case 39: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Pattern(Name: yyDollar[1].token));
            }
            break;
        }
        case 40: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Pattern(Array: yyDollar[2].value._<slice<ж<Pattern>>>()));
            }
            break;
        }
        case 41: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Pattern(Object: yyDollar[2].value._<slice<ж<PatternObject>>>()));
            }
            break;
        }
        case 42: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<Pattern>>([yyDollar[1].value._<ж<Pattern>>()]);
            }
            break;
        }
        case 43: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<Pattern>>>(), yyDollar[3].value._<ж<Pattern>>());
            }
            break;
        }
        case 44: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<PatternObject>>([yyDollar[1].value._<ж<PatternObject>>()]);
            }
            break;
        }
        case 45: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<PatternObject>>>(), yyDollar[3].value._<ж<PatternObject>>());
            }
            break;
        }
        case 46: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new PatternObject(Key: yyDollar[1].token, Val: yyDollar[3].value._<ж<Pattern>>()));
            }
            break;
        }
        case 47: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new PatternObject(KeyString: yyDollar[1].value._<ж<ΔString>>(), Val: yyDollar[3].value._<ж<Pattern>>()));
            }
            break;
        }
        case 48: {
            yyDollar = yyS.slice(yypt - 5, yypt + 1);
            {
                yyVAL.value = Ꮡ(new PatternObject(KeyQuery: yyDollar[2].value._<ж<Query>>(), Val: yyDollar[5].value._<ж<Pattern>>()));
            }
            break;
        }
        case 49: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new PatternObject(Key: yyDollar[1].token));
            }
            break;
        }
        case 50: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeIdentity));
            }
            break;
        }
        case 51: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeRecurse));
            }
            break;
        }
        case 52: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeIndex, Index: Ꮡ(new Index(Name: yyDollar[1].token))));
            }
            break;
        }
        case 53: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                var suffix = yyDollar[2].value._<ж<Suffix>>();
                if (suffix.Iter) {
                    yyVAL.value = Ꮡ(new Term(Type: TermTypeIdentity, SuffixList: [suffix]));
                } else {
                    yyVAL.value = Ꮡ(new Term(Type: TermTypeIndex, Index: suffix.Index));
                }
            }
            break;
        }
        case 54: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeIndex, Index: Ꮡ(new Index(Str: yyDollar[2].value._<ж<ΔString>>()))));
            }
            break;
        }
        case 55: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeNull));
            }
            break;
        }
        case 56: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeTrue));
            }
            break;
        }
        case 57: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeFalse));
            }
            break;
        }
        case 58: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeFunc, Func: Ꮡ(new Func(Name: yyDollar[1].token))));
            }
            break;
        }
        case 59: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeFunc, Func: Ꮡ(new Func(Name: yyDollar[1].token, Args: yyDollar[3].value._<slice<ж<Query>>>()))));
            }
            break;
        }
        case 60: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeFunc, Func: Ꮡ(new Func(Name: yyDollar[1].token))));
            }
            break;
        }
        case 61: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeObject, Object: Ꮡ(new Object())));
            }
            break;
        }
        case 62: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeObject, Object: Ꮡ(new Object(yyDollar[2].value._<slice<ж<ObjectKeyVal>>>()))));
            }
            break;
        }
        case 63: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeObject, Object: Ꮡ(new Object(yyDollar[2].value._<slice<ж<ObjectKeyVal>>>()))));
            }
            break;
        }
        case 64: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeArray, Array: Ꮡ(new Array())));
            }
            break;
        }
        case 65: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeArray, Array: Ꮡ(new Array(yyDollar[2].value._<ж<Query>>()))));
            }
            break;
        }
        case 66: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeNumber, Number: yyDollar[1].token));
            }
            break;
        }
        case 67: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeUnary, Unary: Ꮡ(new Unary(OpAdd, yyDollar[2].value._<ж<Term>>()))));
            }
            break;
        }
        case 68: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeUnary, Unary: Ꮡ(new Unary(OpSub, yyDollar[2].value._<ж<Term>>()))));
            }
            break;
        }
        case 69: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeFormat, Format: yyDollar[1].token));
            }
            break;
        }
        case 70: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeFormat, Format: yyDollar[1].token, Str: yyDollar[2].value._<ж<ΔString>>()));
            }
            break;
        }
        case 71: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeString, Str: yyDollar[1].value._<ж<ΔString>>()));
            }
            break;
        }
        case 72: {
            yyDollar = yyS.slice(yypt - 7, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeIf, If: Ꮡ(new If(yyDollar[2].value._<ж<Query>>(), yyDollar[4].value._<ж<Query>>(), yyDollar[5].value._<slice<ж<IfElif>>>(), yyDollar[6].value._<ж<Query>>()))));
            }
            break;
        }
        case 73: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeTry, Try: Ꮡ(new Try(yyDollar[2].value._<ж<Query>>(), yyDollar[3].value._<ж<Query>>()))));
            }
            break;
        }
        case 74: {
            yyDollar = yyS.slice(yypt - 9, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeReduce, Reduce: Ꮡ(new Reduce(yyDollar[2].value._<ж<Query>>(), yyDollar[4].value._<ж<Pattern>>(), yyDollar[6].value._<ж<Query>>(), yyDollar[8].value._<ж<Query>>()))));
            }
            break;
        }
        case 75: {
            yyDollar = yyS.slice(yypt - 9, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeForeach, Foreach: Ꮡ(new Foreach(yyDollar[2].value._<ж<Query>>(), yyDollar[4].value._<ж<Pattern>>(), yyDollar[6].value._<ж<Query>>(), yyDollar[8].value._<ж<Query>>(), nil))));
            }
            break;
        }
        case 76: {
            yyDollar = yyS.slice(yypt - 11, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeForeach, Foreach: Ꮡ(new Foreach(yyDollar[2].value._<ж<Query>>(), yyDollar[4].value._<ж<Pattern>>(), yyDollar[6].value._<ж<Query>>(), yyDollar[8].value._<ж<Query>>(), yyDollar[10].value._<ж<Query>>()))));
            }
            break;
        }
        case 77: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeBreak, Break: yyDollar[2].token));
            }
            break;
        }
        case 78: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Term(Type: TermTypeQuery, Query: yyDollar[2].value._<ж<Query>>()));
            }
            break;
        }
        case 79: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyDollar[1].value._<ж<Term>>().SuffixList = builtin.append(yyDollar[1].value._<ж<Term>>().SuffixList, Ꮡ(new Suffix(Index: Ꮡ(new Index(Name: yyDollar[2].token)))));
            }
            break;
        }
        case 80: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyDollar[1].value._<ж<Term>>().SuffixList = builtin.append(yyDollar[1].value._<ж<Term>>().SuffixList, yyDollar[2].value._<ж<Suffix>>());
            }
            break;
        }
        case 81: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyDollar[1].value._<ж<Term>>().SuffixList = builtin.append(yyDollar[1].value._<ж<Term>>().SuffixList, Ꮡ(new Suffix(Optional: true)));
            }
            break;
        }
        case 82: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyDollar[1].value._<ж<Term>>().SuffixList = builtin.append(yyDollar[1].value._<ж<Term>>().SuffixList, yyDollar[3].value._<ж<Suffix>>());
            }
            break;
        }
        case 83: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyDollar[1].value._<ж<Term>>().SuffixList = builtin.append(yyDollar[1].value._<ж<Term>>().SuffixList, Ꮡ(new Suffix(Index: Ꮡ(new Index(Str: yyDollar[3].value._<ж<ΔString>>())))));
            }
            break;
        }
        case 84: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ΔString(Str: yyDollar[1].token));
            }
            break;
        }
        case 85: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ΔString(Queries: yyDollar[2].value._<slice<ж<Query>>>()));
            }
            break;
        }
        case 86: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = slice<ж<Query>>([]);
            }
            break;
        }
        case 87: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<Query>>>(), Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeString, Str: Ꮡ(new ΔString(Str: yyDollar[2].token)))))));
            }
            break;
        }
        case 88: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yylex._<yyLexer, ж<lexer>>().inString = true;
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<Query>>>(), Ꮡ(new Query(Term: Ꮡ(new Term(Type: TermTypeQuery, Query: yyDollar[3].value._<ж<Query>>())))));
            }
            break;
        }
        case 93: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Suffix(Iter: true));
            }
            break;
        }
        case 94: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Suffix(Index: Ꮡ(new Index(Start: yyDollar[2].value._<ж<Query>>()))));
            }
            break;
        }
        case 95: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Suffix(Index: Ꮡ(new Index(Start: yyDollar[2].value._<ж<Query>>(), IsSlice: true))));
            }
            break;
        }
        case 96: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Suffix(Index: Ꮡ(new Index(End: yyDollar[3].value._<ж<Query>>(), IsSlice: true))));
            }
            break;
        }
        case 97: {
            yyDollar = yyS.slice(yypt - 5, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Suffix(Index: Ꮡ(new Index(Start: yyDollar[2].value._<ж<Query>>(), End: yyDollar[4].value._<ж<Query>>(), IsSlice: true))));
            }
            break;
        }
        case 98: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<Query>>([yyDollar[1].value._<ж<Query>>()]);
            }
            break;
        }
        case 99: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<Query>>>(), yyDollar[3].value._<ж<Query>>());
            }
            break;
        }
        case 100: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = slice<ж<IfElif>>(default);
            }
            break;
        }
        case 101: {
            yyDollar = yyS.slice(yypt - 5, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<IfElif>>>(), Ꮡ(new IfElif(yyDollar[3].value._<ж<Query>>(), yyDollar[5].value._<ж<Query>>())));
            }
            break;
        }
        case 102: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = (ж<Query>)nil;
            }
            break;
        }
        case 103: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = yyDollar[2].value;
            }
            break;
        }
        case 104: {
            yyDollar = yyS.slice(yypt - 0, yypt + 1);
            {
                yyVAL.value = (ж<Query>)nil;
            }
            break;
        }
        case 105: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = yyDollar[2].value;
            }
            break;
        }
        case 106: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<ObjectKeyVal>>([yyDollar[1].value._<ж<ObjectKeyVal>>()]);
            }
            break;
        }
        case 107: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<ObjectKeyVal>>>(), yyDollar[3].value._<ж<ObjectKeyVal>>());
            }
            break;
        }
        case 108: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ObjectKeyVal(Key: yyDollar[1].token, Val: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 109: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ObjectKeyVal(KeyString: yyDollar[1].value._<ж<ΔString>>(), Val: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 110: {
            yyDollar = yyS.slice(yypt - 5, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ObjectKeyVal(KeyQuery: yyDollar[2].value._<ж<Query>>(), Val: yyDollar[5].value._<ж<Query>>()));
            }
            break;
        }
        case 111: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ObjectKeyVal(Key: yyDollar[1].token));
            }
            break;
        }
        case 112: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ObjectKeyVal(KeyString: yyDollar[1].value._<ж<ΔString>>()));
            }
            break;
        }
        case 116: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new Query(Left: yyDollar[1].value._<ж<Query>>(), Op: OpPipe, Right: yyDollar[3].value._<ж<Query>>()));
            }
            break;
        }
        case 118: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(Object: yyDollar[1].value._<ж<ConstObject>>()));
            }
            break;
        }
        case 119: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(Array: yyDollar[1].value._<ж<ConstArray>>()));
            }
            break;
        }
        case 120: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(Number: yyDollar[1].token));
            }
            break;
        }
        case 121: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(Str: yyDollar[1].token));
            }
            break;
        }
        case 122: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(Null: true));
            }
            break;
        }
        case 123: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(True: true));
            }
            break;
        }
        case 124: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstTerm(False: true));
            }
            break;
        }
        case 125: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstObject());
            }
            break;
        }
        case 126: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstObject(yyDollar[2].value._<slice<ж<ConstObjectKeyVal>>>()));
            }
            break;
        }
        case 127: {
            yyDollar = yyS.slice(yypt - 4, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstObject(yyDollar[2].value._<slice<ж<ConstObjectKeyVal>>>()));
            }
            break;
        }
        case 128: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<ConstObjectKeyVal>>([yyDollar[1].value._<ж<ConstObjectKeyVal>>()]);
            }
            break;
        }
        case 129: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<ConstObjectKeyVal>>>(), yyDollar[3].value._<ж<ConstObjectKeyVal>>());
            }
            break;
        }
        case 130: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstObjectKeyVal(Key: yyDollar[1].token, Val: yyDollar[3].value._<ж<ConstTerm>>()));
            }
            break;
        }
        case 131: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstObjectKeyVal(Key: yyDollar[1].token, Val: yyDollar[3].value._<ж<ConstTerm>>()));
            }
            break;
        }
        case 132: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstObjectKeyVal(KeyString: yyDollar[1].token, Val: yyDollar[3].value._<ж<ConstTerm>>()));
            }
            break;
        }
        case 133: {
            yyDollar = yyS.slice(yypt - 2, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstArray());
            }
            break;
        }
        case 134: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = Ꮡ(new ConstArray(yyDollar[2].value._<slice<ж<ConstTerm>>>()));
            }
            break;
        }
        case 135: {
            yyDollar = yyS.slice(yypt - 1, yypt + 1);
            {
                yyVAL.value = slice<ж<ConstTerm>>([yyDollar[1].value._<ж<ConstTerm>>()]);
            }
            break;
        }
        case 136: {
            yyDollar = yyS.slice(yypt - 3, yypt + 1);
            {
                yyVAL.value = builtin.append(yyDollar[1].value._<slice<ж<ConstTerm>>>(), yyDollar[3].value._<ж<ConstTerm>>());
            }
            break;
        }}

        goto yystack;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

} // end gojq_package
