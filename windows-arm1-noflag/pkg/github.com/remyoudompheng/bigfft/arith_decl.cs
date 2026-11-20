// Copyright 2010 The Go Authors.  All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using Word = go.math.big_package.Word;

namespace go.github.com.remyoudompheng;

using big = math.big_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using math;

partial class bigfft_package {

internal static partial Word /*c*/ addVV(slice<Word> z, slice<Word> x, slice<Word> y);

internal static partial Word /*c*/ subVV(slice<Word> z, slice<Word> x, slice<Word> y);

internal static partial Word /*c*/ addVW(slice<Word> z, slice<Word> x, Word y);

internal static partial Word /*c*/ subVW(slice<Word> z, slice<Word> x, Word y);

internal static partial Word /*c*/ shlVU(slice<Word> z, slice<Word> x, nuint s);

internal static partial Word /*c*/ mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r);

internal static partial Word /*c*/ addMulVVW(slice<Word> z, slice<Word> x, Word y);

} // end bigfft_package
