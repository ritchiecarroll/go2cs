# The closure regression's cure: glyph forwarders get box twins (record)

**G, 2026-11-24.** The cure for DATA-mn-row7-closure.md's regression. The twinned MnHttpClosure stopped parking
(parks 36 -> 0, peak OS threads 38-39 -> 56-57). P5's bisect (mailbox COORD/20261124T003208Z-P5.md) named the first
bad point: train 4's merge of the glyph seat, 027073dd1f. Branch claude/g-mn-closure-cure, off master 97cca467a2.

## Cause (read before any code)

A stack sample of the twinned closure under GO2CS_MN=1, taken 700 ms in, has six threads, one per client request,
all in the PLAIN `TransportжRoundTripper.RoundTrip`. Under it each runs plain `roundTrip`, a plain `select` and plain
`writeLoop`, and above it sit twin frames:

  mainꞏtwin -> Getꞏtwin -> Do⁔ꞏtwin -> do⁔ꞏtwin -> httpᴛClient.send (PLAIN) -> send⁔ -> rt.RoundTrip (plain) ...

`do⁔ꞏtwin` calls plain `send`, although `send⁔ꞏtwin(ж<Client> Ꮡc, …)` exists. Since the glyph seat, `send` is a
forwarder in package_info.cs:

  internal static (…) send(this ref Client c, ж<Request> Ꮡreq, time.Time deadline) => http_package.send⁔(ref c, …);

The N2 box-twin rule (TwinGenerator.BoxTwin.cs `BoxReceiverOf`) gives a `this ref T` method a box twin
`Mꞏtwin(this ж<T>, …)` only when it has a BLOCK body. An expression-bodied forwarder is refused `byref-recv`. With
no box twin, a caller holding the box cannot bind a twin and calls the forwarder plain. Everything under it then runs
plain, including the `goǃ` launches, so those goroutines become threads (thread-blocks about 80).

The closure's twin reports read `refused byref-recv=2283` at master, against 2 in row 6's arm S at the good point.

## The cure

- `BoxReceiverOf` accepts an expression body as well as a block, scanned for the box's name the same way.
- `TwinText` turns an expression-bodied box twin into a one-line block, `{ <prologue> return …; }` (or `{ …; }` for
  void), so the prologue `ref var t = ref Ꮡt.DerefOrNull();` has a statement to precede, and the line mapping holds.

## Predictions (written before any build or test)

| # | prediction | stop line |
|---|---|---|
| C1 | GenTests `AGlyphForwarderOverARefReceiverGetsABoxTwinAndItsCallerAwaitsIt` is RED at master (no `gꞏtwin(this ж<T>)`) and GREEN with the cure | green at master: the row does not test the cause |
| C2 | MnHttpClosure arm S (closure twinned via GoTwinsFor), 3 runs: EQUAL to Go; parks 30-40, steps 65-85, thread-blocks 0; peak OS threads <= 38 | parks < 20 or thread-blocks > 5: the cure is partial; read the stacks again |
| C3 | the closure's `refused byref-recv` falls from 2,283 to <= 10 | > 100: another forwarder shape is still refused |
| C4 | GenTests all green (808 + 1); GO2CS0017/0018/0021 = 0 on the closure | any |
| C5 | GolibTests unchanged (the cure is generator-only); repoguard rc=0 | any |

## Three more cures the twinned builds surfaced (2026-11-24, before the readings below)

With the forwarders twinned, calls that were plain before now park. That made two rewriter gaps reachable, and the
first fix had a gap of its own. Each broke a twinned build. Each is cured in this seat and has a GenTests row, red
first.

- **An awaited `for` INITIALIZER.** runtime's unwinder loops, `for (Ꮡu.initAt(…); u.valid(); Ꮡu.next())` (heapdump,
  mgcmark, windows stack), became `for ((await Ꮡu.initAtꞏtwin(…)); …)`: CS0201, six errors. A bare await cannot stay in
  the header either, because C# parses `for (await f(), …` as a declaration of a type named `await`. So when an
  initializer awaits, every initializer moves, in order, into the statement prelude (Rule S's spills). They run once,
  before the loop, as the header ran them, and the live refs are derived again after each.
  Row `TwinLoopTests.AParkingInitializerMovesBeforeTheLoopAndTheConditionReadsAfterIt`: red at master, because the
  loop source fails to compile with CS0201 and all six loop rows go red.
- **A LABELED statement's header.** crypto/x509's `candidateLoop: foreach (… in range(slice([new(…,
  opts.Roots.findPotentialParents(…)), new(…, opts.Intermediates.findPotentialParents(…))])))` read `opts` after the first
  await: CS9217. `EvaluatedWith` compared the read's statement with the visited statement by identity, and the visited
  one was the label, so the read was never counted. Labels are now looked through.
  Row `TwinLoopTests.ALabeledLoopsHeaderReadsItsRefLocalAfterAnAwaitThroughTheBox`: red at master (CS9217).
- **A DECLARING initializer stays in the header.** The first whole-stdlib twinned build with the two cures above
  failed on go/internal/gcimporter's `for ((nint i, nint n) = (0, r.of(…).Len()); i < n; i++)`: moved before the
  loop, `i` left the loop's scope and clashed with a sibling `foreach (var i …)` (CS0136, 2 errors on each target).
  Its await is nested in the tuple, so it parses in the header: an initializer that declares a variable is never moved.
  Row `TwinLoopTests.ADeclaringInitializerThatParksStaysInTheHeader`: red without this rule (CS0136).

## Readings

**C1 MET.** The glyph-forwarder row is red at master: the twins carry no `gꞏtwin(this ж<T>)`. It is green with the
cure.

**C3 MET.** The closure's `refused byref-recv` went from 2,283 to 0. Twinned rose from 5,079 to 6,171 (of 19,059
candidates), and await sites from 10,723 to 12,891.

**C4.** GenTests 817/817 (808 + the glyph row + the 2 loop rows + rows that landed since). On the closure,
GO2CS0017, GO2CS0018 and GO2CS0021 are all 0.

**C2.** MnHttpClosure arm S (closure twinned), 9 runs: every run EQUAL to Go, rc 0.

| reading | master 97cca467a2 | the cure | row 6 (11-21, the good point) |
|---|---|---|---|
| thread-blocks | 77-79 | **0** (all 9) | 0 |
| parks | 0 | 60-62 | 36 |
| steps | 20 | 98-100 | 74 |
| peak OS threads | 56-57 | 39-45 (median 40) | 38-39 |
| injections | 0-6 (two master builds, 6 runs) | 0-11 | 6 (one run read) |
| steps over 10 ms | 0-4 | 36-43 | 11-21 |

- thread-blocks: **MET**. Every goroutine of the closure runs on a carrier again.
- parks 30-40: **MISSED HIGH**. 60-62 parks: more of the closure is twinned than at row 6 (6,171 twins against
  3,350), so more of its blocking parks.
- peak OS threads <= 38: **MISSED**, by 1-7. Runs with 0 injections read 39-40. Each injection adds a carrier, and
  the cure's runs have 36-43 steps over 10 ms against row 6's 11-21, so injections, and with them carriers, vary from run
  to run. Twin count and step length moved together. Whether the long steps are first-call JIT of the larger twin set is
  NOT read here.

## Final gates, at the seat's code (all three cures)

- **The whole standard library twinned** (go2cs-stdlib.slnx, Release, `-p:GoStdlibTwins=true`): windows, linux and
  darwin, each rc 0, 0 errors, with GO2CS0017, GO2CS0018 and GO2CS0021 all 0.
- **GenTests:** 818/818.
- **GolibTests Release:** 3,152 passed, 0 failed, 53 skipped. The cure is generator-only; this is beyond the
  src/gen class's gates.
- **MnHttpClosure arm S, 3 runs at the final code:** EQUAL to Go; thread-blocks 0; parks 61; steps 99; peak OS
  threads 39, 41 and 40 (injections 6, 6, 5). The scores above stand.
- **Left to the train**, by the change-class rule (owner, 2026-11-24): CNR, the full `go test` and the `--mn`
  battery. One closure-shaped reading (R7-3, T4's input) is owed separately: the closure's peak threads, about 40,
  are still above R7-3's 30.
