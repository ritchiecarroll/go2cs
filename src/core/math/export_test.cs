// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.math_package;

partial class math_internal_test_package {

// Export internal functions for testing.
public static readonly ж<Func<float64, float64>> ᏑExpGo = new StaticBox<Func<float64, float64>>(exp);
public static ref Func<float64, float64> ExpGo => ref ((StaticBox<Func<float64, float64>>)ᏑExpGo).ValueSlot;

public static readonly ж<Func<float64, float64>> ᏑExp2Go = new StaticBox<Func<float64, float64>>(exp2);
public static ref Func<float64, float64> Exp2Go => ref ((StaticBox<Func<float64, float64>>)ᏑExp2Go).ValueSlot;

public static readonly ж<Func<float64, float64, float64>> ᏑHypotGo = new StaticBox<Func<float64, float64, float64>>(hypot);
public static ref Func<float64, float64, float64> HypotGo => ref ((StaticBox<Func<float64, float64, float64>>)ᏑHypotGo).ValueSlot;

public static readonly ж<Func<float64, float64>> ᏑSqrtGo = new StaticBox<Func<float64, float64>>(sqrt);
public static ref Func<float64, float64> SqrtGo => ref ((StaticBox<Func<float64, float64>>)ᏑSqrtGo).ValueSlot;

public static readonly ж<Func<float64, (uint64, float64)>> ᏑTrigReduce = new StaticBox<Func<float64, (uint64, float64)>>(default(Func<float64, (uint64, float64)>));
public static ref Func<float64, (uint64, float64)> TrigReduce => ref ((StaticBox<Func<float64, (uint64, float64)>>)ᏑTrigReduce).ValueSlot;
internal static void initᴛTrigReduce() { TrigReduce = trigReduce; }

public static UntypedInt ReduceThreshold => /* reduceThreshold */ 536870912;

} // end math_internal_test_package
