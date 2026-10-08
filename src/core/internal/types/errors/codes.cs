// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.types;

partial class errors_package {

partial struct Code /*num:nint*/;

//go:generate go run golang.org/x/tools/cmd/stringer@latest -type Code codes.go
// This file defines the error codes that can be produced during type-checking.
// Collectively, these codes provide an identifier that may be used to
// implement special handling for certain types of errors.
//
// Error code values should not be changed: add new codes at the end.
//
// Error codes should be fine-grained enough that the exact nature of the error
// can be easily determined, but coarse enough that they are not an
// implementation detail of the type checking algorithm. As a rule-of-thumb,
// errors should be considered equivalent if there is a theoretical refactoring
// of the type checker in which they are emitted in exactly one place. For
// example, the type checker emits different error messages for "too many
// arguments" and "too few arguments", but one can imagine an alternative type
// checker where this check instead just emits a single "wrong number of
// arguments", so these errors should have the same code.
//
// Error code names should be as brief as possible while retaining accuracy and
// distinctiveness. In most cases names should start with an adjective
// describing the nature of the error (e.g. "invalid", "unused", "misplaced"),
// and end with a noun identifying the relevant language object. For example,
// "_DuplicateDecl" or "_InvalidSliceExpr". For brevity, naming follows the
// convention that "bad" implies a problem with syntax, and "invalid" implies a
// problem with types.
public static Code InvalidSyntaxTree => -1;

internal static Code _ᴛ2ʗ => /* iota */ 0;
public static Code Test => 1;
public static Code BlankPkgName => 2;
public static Code MismatchedPkgName => 3;
public static Code InvalidPkgUse => 4;
public static Code BadImportPath => 5;
public static Code BrokenImport => 6;
public static Code ImportCRenamed => 7;
public static Code UnusedImport => 8;
public static Code InvalidInitCycle => 9;
public static Code DuplicateDecl => 10;
public static Code InvalidDeclCycle => 11;
public static Code InvalidTypeCycle => 12;
public static Code InvalidConstInit => 13;
public static Code InvalidConstVal => 14;
public static Code InvalidConstType => 15;
public static Code UntypedNilUse => 16;
public static Code WrongAssignCount => 17;
public static Code UnassignableOperand => 18;
public static Code NoNewVar => 19;
public static Code MultiValAssignOp => 20;
public static Code InvalidIfaceAssign => 21;
public static Code InvalidChanAssign => 22;
public static Code IncompatibleAssign => 23;
public static Code UnaddressableFieldAssign => 24;
public static Code NotAType => 25;
public static Code InvalidArrayLen => 26;
public static Code BlankIfaceMethod => 27;
public static Code IncomparableMapKey => 28;
internal static Code _ᴛ3ʗ => 29; // not used anymore
public static Code InvalidPtrEmbed => 30;
public static Code BadRecv => 31;
public static Code InvalidRecv => 32;
public static Code DuplicateFieldAndMethod => 33;
public static Code DuplicateMethod => 34;
public static Code InvalidBlank => 35;
public static Code InvalidIota => 36;
public static Code MissingInitBody => 37;
public static Code InvalidInitSig => 38;
public static Code InvalidInitDecl => 39;
public static Code InvalidMainDecl => 40;
public static Code TooManyValues => 41;
public static Code NotAnExpr => 42;
public static Code TruncatedFloat => 43;
public static Code NumericOverflow => 44;
public static Code UndefinedOp => 45;
public static Code MismatchedTypes => 46;
public static Code DivByZero => 47;
public static Code NonNumericIncDec => 48;
public static Code UnaddressableOperand => 49;
public static Code InvalidIndirection => 50;
public static Code NonIndexableOperand => 51;
public static Code InvalidIndex => 52;
public static Code SwappedSliceIndices => 53;
public static Code NonSliceableOperand => 54;
public static Code InvalidSliceExpr => 55;
public static Code InvalidShiftCount => 56;
public static Code InvalidShiftOperand => 57;
public static Code InvalidReceive => 58;
public static Code InvalidSend => 59;
public static Code DuplicateLitKey => 60;
public static Code MissingLitKey => 61;
public static Code InvalidLitIndex => 62;
public static Code OversizeArrayLit => 63;
public static Code MixedStructLit => 64;
public static Code InvalidStructLit => 65;
public static Code MissingLitField => 66;
public static Code DuplicateLitField => 67;
public static Code UnexportedLitField => 68;
public static Code InvalidLitField => 69;
public static Code UntypedLit => 70;
public static Code InvalidLit => 71;
public static Code AmbiguousSelector => 72;
public static Code UndeclaredImportedName => 73;
public static Code UnexportedName => 74;
public static Code UndeclaredName => 75;
public static Code MissingFieldOrMethod => 76;
public static Code BadDotDotDotSyntax => 77;
public static Code NonVariadicDotDotDot => 78;
public static Code MisplacedDotDotDot => 79;
internal static Code _ᴛ4ʗ => 80; // InvalidDotDotDotOperand was removed.
public static Code InvalidDotDotDot => 81;
public static Code UncalledBuiltin => 82;
public static Code InvalidAppend => 83;
public static Code InvalidCap => 84;
public static Code InvalidClose => 85;
public static Code InvalidCopy => 86;
public static Code InvalidComplex => 87;
public static Code InvalidDelete => 88;
public static Code InvalidImag => 89;
public static Code InvalidLen => 90;
public static Code SwappedMakeArgs => 91;
public static Code InvalidMake => 92;
public static Code InvalidReal => 93;
public static Code InvalidAssert => 94;
public static Code ImpossibleAssert => 95;
public static Code InvalidConversion => 96;
public static Code InvalidUntypedConversion => 97;
public static Code BadOffsetofSyntax => 98;
public static Code InvalidOffsetof => 99;
public static Code UnusedExpr => 100;
public static Code UnusedVar => 101;
public static Code MissingReturn => 102;
public static Code WrongResultCount => 103;
public static Code OutOfScopeResult => 104;
public static Code InvalidCond => 105;
public static Code InvalidPostDecl => 106;
internal static Code _ᴛ5ʗ => 107; // InvalidChanRange was removed.
public static Code InvalidIterVar => 108;
public static Code InvalidRangeExpr => 109;
public static Code MisplacedBreak => 110;
public static Code MisplacedContinue => 111;
public static Code MisplacedFallthrough => 112;
public static Code DuplicateCase => 113;
public static Code DuplicateDefault => 114;
public static Code BadTypeKeyword => 115;
public static Code InvalidTypeSwitch => 116;
public static Code InvalidExprSwitch => 117;
public static Code InvalidSelectCase => 118;
public static Code UndeclaredLabel => 119;
public static Code DuplicateLabel => 120;
public static Code MisplacedLabel => 121;
public static Code UnusedLabel => 122;
public static Code JumpOverDecl => 123;
public static Code JumpIntoBlock => 124;
public static Code InvalidMethodExpr => 125;
public static Code WrongArgCount => 126;
public static Code InvalidCall => 127;
public static Code UnusedResults => 128;
public static Code InvalidDefer => 129;
public static Code InvalidGo => 130;
// All codes below were added in Go 1.17.
public static Code BadDecl => 131;
public static Code RepeatedDecl => 132;
public static Code InvalidUnsafeAdd => 133;
public static Code InvalidUnsafeSlice => 134;
// All codes below were added in Go 1.18.
public static Code UnsupportedFeature => 135;
public static Code NotAGenericType => 136;
public static Code WrongTypeArgCount => 137;
public static Code CannotInferTypeArgs => 138;
public static Code InvalidTypeArg => 139; // arguments? InferenceFailed
public static Code InvalidInstanceCycle => 140;
public static Code InvalidUnion => 141;
public static Code MisplacedConstraintIface => 142;
public static Code InvalidMethodTypeParams => 143;
public static Code MisplacedTypeParam => 144;
public static Code InvalidUnsafeSliceData => 145;
public static Code InvalidUnsafeString => 146;
internal static Code _ᴛ6ʗ => 147; // not used anymore
public static Code InvalidClear => 148;
public static Code TypeTooLarge => 149;
public static Code InvalidMinMaxOperand => 150;
public static Code TooNew => 151;

} // end errors_package
