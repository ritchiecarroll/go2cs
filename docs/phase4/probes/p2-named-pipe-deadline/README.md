# p2-named-pipe-deadline — where does os TestVariousDeadlines1Proc/named_overlapped_pipe hang?

The i9's windows os read at `3bdac60827` (2026-10-17): `^TestVariousDeadlines1Proc$` runs to a 5 m
deadline and its `named_overlapped_pipe` subtests never report (new at Go 1.26,
`os/timeout_windows_test.go`). Even the test's own 5 s `max` timer never ends a subtest, which fits a
hang in a deferred `Close` after `t.Fatalf`, not only in the read itself.

This probe builds the test's pipe pair (`CreateNamedPipe` with `FILE_FLAG_OVERLAPPED` wrapped by
`os.NewFile`, the client end from `os.OpenFile` with `O_FILE_FLAG_OVERLAPPED`) and runs three steps,
each in its own goroutine under a 10 s guard, so a hang is reported by name and the next step still runs.

| step | what it does | Go | a hang here means |
|---|---|---|---|
| A | read deadline 50 ms, no writer | `i/o timeout` after ~50 ms | a timed-out pending read is never cancelled |
| B | one 5-byte write, one read | `n=5 "hello"` at once | a completion on the pipe never reaches the poller |
| C | `Close` under a pending read, no deadline | `file already closed` after ~100 ms | `Close`'s `CancelIoEx(h, nil)` does not wake the read |

## The static read behind A (unmeasured, hop `3bdac60827`)

- `syscall.readFile` and `writeFile` are converted, not hand-owned (`src/core/syscall/windows/zsyscall_windows.cs:1393`):
  the OVERLAPPED crosses as `(uintptr)&o.o`, and no GoAsyncIO operation record is registered for it.
- The hand-owned `CancelIoEx` (`src/core/syscall/windows/zsyscall_windows_wsa_impl.cs:668`) cancels a non-nil
  overlapped only through its GoAsyncIO record and answers `ERROR_NOT_FOUND` when there is none. `cancelIO` reads
  that as "already completed" and calls `runtime_pollWaitCanceled`, which waits for a completion
  (`src/core/internal/poll/windows/runtime_netpoll_impl.cs`, section 5). A pipe read with no data never completes,
  so the deadline path never returns.
- Separately, `&o.o` is an interior field of the pooled, reference-bearing `operation`, so the address the
  kernel holds is not pinned for the life of the I/O.

## Run

```
go2cs <this dir> <out dir>      # GOOS=windows emission
dotnet build -c Release <out dir> && <out dir>/bin/.../PipeProbe.exe
go run .                        # the Go oracle, on the same box
```

Windows only (`//go:build windows`); it does not build elsewhere by design.
