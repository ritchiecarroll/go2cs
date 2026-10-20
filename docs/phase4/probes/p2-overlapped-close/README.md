# Probe: the windows overlapped close crash and the 750ns deadline hang

Two defects the i9's net row met on 2026-10-19, in `TestVariousDeadlines1Proc` and the tests in flight
beside it. `main.go` runs each as its own mode, one process per mode.

## `race`: "Handle is not initialized" out of Close

Go 1.26's internal/poll takes every operation from one process-wide `sync.Pool`, so the key an
overlapped record is filed under moves between sockets. Two paths then retire one record:

- a submit on socket B whose pooled key still names a record bound to socket A
  (`GoAsyncIO.GetOrBindOperationState` retires and disposes it), and
- socket A's `pollClose` (`SocketBinding.Dispose`), which disposes every record A still tracks.

`OverlappedOp.Dispose` had no guard against the two running at once, so both freed the one control
block, and the second free threw `InvalidOperationException: Handle is not initialized` out of
`Overlapped.GetOverlappedFromNative`, through net's `Close`, into the test host. The mode opens and
closes 3,200 listener/connection pairs on 16 goroutines, each read under a short deadline, to make
the two retirements overlap. The race is timing-dependent: a clean run on the base does not clear it.

## `deadlines`: the run that never returns

`testVariousDeadlines`' client loop under `GOMAXPROCS(1)`, over 20 passes, with the step each run is
on. A run stuck in its read for 3 s has its read deadline set again, in the PAST, which expires the
mode at once without any timer:

- `UNSTUCK ... its armed deadline never expired the read` (exit 4): the deadline timer is the defect.
- `STILL STUCK ... the read is past its deadline wait` (exit 3): the read already timed out and is
  stuck in the cancel and harvest (`CancelIoEx`, then `waitCanceled` for the completion).

Inferred, not measured: up to 500ns the deadline is usually already past when `SetDeadline`
computes it, so the read never reaches the kernel; 750ns is the first timeout whose read is pending
when the deadline fires.

## Readings

Linux, uid 0, Microsoft .NET 10.0.12 / SDK 10.0.112, go1.26.9, at the fix: the C# output equals
Go's in both modes (`deadlines: 780 runs, all timed out`; `race: 3200 socket pairs closed`).
Neither defect's path exists on linux; the windows readings are the i9's.
