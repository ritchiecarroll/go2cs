# p1-reflect-method-arms: do the generated reflect method arms stay out of a program that never asks for them?

Seat `claude/p1-reflect-method-arms`, first increment (G's ruling, 2026-10-19). go2cs-gen writes golib's
`IGoMethodSet` on every value type with exported Go methods: per method, the unbound func reflect's
`Type.Method(i).Func` hands out and the binder behind `Value.Method(i)`, as compiled delegates. The arms are reached
only through that interface slot, which only reflect's method paths call, through the type's zero value. A program
that reaches no reflect method access should therefore compile none of them under Native AOT.

Two programs, identical except for one line:

- `nocall/`: types with value and pointer methods (a plain struct, a generic struct, a named float with `String`),
  printed through `fmt`. `fmt` reaches reflect, but nothing calls `Type.Method`, `Value.Method` or `MethodByName`.
- `onecall/`: the same, plus one `reflect.ValueOf(p).MethodByName("Describe").Call(...)`.

Each is published four ways in total: Native AOT default publish, linux-x64, with the arms ON (the seat) and OFF (the
generator's one emission line switched off, restored after).

## Prediction (written before any measurement)

- `nocall`, ON minus OFF: within 16 KB. Only the interface type and its interface-map entries on the constructed types
  may appear; no arm body. FALSIFIER: more than 16 KB means something roots the arms. The suspects, in order: the
  zero-instance lookup (`GoTypeOps.Uninitialized`), then `ΔValue.live`'s `LazyMethodFunc` test, which every reflect
  read passes through (it should fold away, because `LazyMethodFunc` is sealed and only `rtype.Method` constructs one).
- `onecall`, ON minus OFF: between +100 KB and +2 MB. The interface call is now reachable, so every CONSTRUCTED value
  type with exported methods compiles its arms: this program's own types, and every standard-library value type the
  program constructs (the reflect bridge's own types among them). The +127 KB the generic method source measured for
  13 closed methods is the scale for this program's own types alone; the standard library's share is the unknown, and
  that is why the range is wide.
- Output: both programs print what Go prints, ON and OFF (`onecall`'s `y[p=9]` included).

## Measurement

(appended after the runs)
