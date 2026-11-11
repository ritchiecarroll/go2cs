// ChildDeadline.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;

namespace GolibTests;

/// <summary>
/// One wall-clock budget for a test that runs child processes, so a slow or stuck child fails the test BY NAME
/// before the CI's hang watchdog fires.
/// </summary>
/// <remarks>
/// The os-matrix golib-tests stage runs <c>dotnet test --blame-hang-timeout 10m</c>: when no test finishes for ten
/// minutes it dumps the test host and kills it, and every later test in the run goes unreported. A child's own
/// timeout cannot prevent that when it is longer than the watchdog (a 30-minute build budget never fires before a
/// 10-minute watchdog). Each child therefore gets the smaller of its own cap and what is left of the test's budget.
/// The budget stays well under ten minutes, because <see cref="ChildTimeoutDiagnostics.DescribeAndKill"/> writes
/// and reads dumps after a timeout and must finish before the watchdog does.
/// </remarks>
internal sealed class ChildDeadline(int budgetMs)
{
    private readonly Stopwatch m_clock = Stopwatch.StartNew();

    /// <summary>The whole test's budget, in milliseconds.</summary>
    public int BudgetMs => budgetMs;

    /// <summary>
    /// The timeout for the next child: its own cap, or what is left of the budget when that is less (at least 1 ms,
    /// so an exhausted budget still fails the child by name instead of waiting without limit).
    /// </summary>
    public int For(int capMs) => (int)Math.Max(1, Math.Min(capMs, budgetMs - m_clock.ElapsedMilliseconds));
}
