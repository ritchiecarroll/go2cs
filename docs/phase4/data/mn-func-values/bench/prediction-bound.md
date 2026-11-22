# N3 bench, bound-method arms: predictions (written 2026-11-13 22:58 CST, before the build)

Arms added to Program.cs (fix 2: a bound x.M gets a pair per value, its receiver evaluated once):
- B0p: today's bound method value on a pointer receiver (`Func<int,int> f = r.M`), created and called.
- D3bp: the same as a pair: receiver read once into a temporary, plain `t.M` + twin `t.MTwin`, pair, handed-out
  delegate bound to the pair; created and called.
- B0v: today's method value on a VALUE receiver: the struct copied once into a closure, one delegate.
- D3bv: the pair over a value receiver: ONE copy in one closure class shared by both halves, two lambdas, pair,
  handed-out delegate.

Predictions:
- P-b1: D3bp costs +10..30 ns over B0p on the JIT and under AOT (the closure literal's D3c delta was +16/+19).
- P-b2: D3bp allocates +160 B over B0p (two delegates + the 32 B pair + the handed-out delegate over one delegate:
  3 x 64 + 32 - 64 = 160). So S4's 160 B delta holds for the pointer-receiver bound case.
- P-b3: D3bv allocates +160 B over B0v too (the copy is shared, not doubled).
- P-b4: no arm over 100 ns (the stop line).
