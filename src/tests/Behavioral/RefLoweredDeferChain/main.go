package main

import "fmt"

// A POINTER PARAMETER that Phase-A ref-lowering lowers to `ref T` has NO BOX, so any emission that
// renders it as `Ꮡname` names nothing (CS0103). Every ordinary use already respects that; a DEFER
// or GO method group did not, because those render their callee through a LAMBDA conversion context
// and that path asked "is this a ref RECEIVER" where the question is "does this ident have a box at
// all". The guard's own comment at the site records the identical CS0103 for a ref receiver (flate
// init's method value) — ref-lowering later made a ref-lowered PARAMETER box-less the same way.
//
// The shape is 1.24 os/root_openat.go's doInRoot, reproduced faithfully: a pointer parameter whose
// POINTEE carries a POINTER field, with the deferred call's receiver reached THROUGH that field.
// The field being already a pointer is what makes the analysis lower the parameter — it takes no new
// address, so the use is value-level — and it is exactly the case with no coverage before 1.24: every
// pre-1.24 instance of this shape reaches the method through a VALUE field (a mutex), which the
// existing X3 veto catches and which therefore never lowered.
//
// ⚠ THIS ASSERTS VALUES, NOT MERELY THAT IT COMPILES. Each function prints what it observed, compared
// against `go run`, so an emission that compiled while deferring on the WRONG receiver still fails.

type root struct {
	fd   int
	done chan int // non-nil ONLY for the `go` row, so its goroutine's effect is OBSERVABLE
}

func (r *root) release() {
	r.fd = -r.fd

	if r.done != nil {
		r.done <- r.fd
	}
}

type Root struct{ root *root }

// ⚠ THE TYPE PARAMETER IS FAITHFULNESS, NOT NECESSITY -- measured, so nobody has to wonder. os.doInRoot
// is generic and this reproduces it; a NON-generic function of the same shape emits the IDENTICAL
// `defer(Ꮡr.Value.root.decref, ...)` on the pre-fix converter. Kept because the project's precedent is
// to carry the real site's shape (SwitchPointerSentinelCase keeps its leading default and its loop for
// the same reason), but it exercises no extra path and a future reader should not infer that it does.
//
// deferChain is the defect shape: `r` lowers to a ref parameter, and the deferred receiver `r.root`
// is reached through a POINTER field. The deferred call must run at return and must act on the SAME
// root the body read — so the printed pair is (value seen in body, value after the defer ran).
func deferChain[T any](r *Root, mark T) (seen int) {
	defer r.root.release()
	seen = r.root.fd
	return
}

// goChain is the TWIN: `visitGoStmt` renders through the same lambda context, so the same box form
// was emitted there. Synchronised so the output is deterministic — an output-compared guard cannot
// carry a race, and a row that waits without observing is worse than no row.
func goChain(r *Root) int {
	go r.root.release()
	return <-r.root.done
}

// CONTROL A — the same chain in an IMMEDIATE call. It renders the ref form and always did; it is
// here so a fix that changed ordinary calls too would fail rather than pass quietly.
func immediateChain(r *Root) int {
	r.root.release()
	return r.root.fd
}

// CONTROL B — the parameter IS the receiver, directly. The analysis already refuses to lower this
// (receiverUseKeptReason's "ptr-receiver-defer-go"), so the parameter keeps its box and the emission
// is unchanged. A fix that lowered it would break this row.
func directReceiver(p *root) (before int) {
	defer p.release()
	return p.fd
}

// CONTROL C — the chain's base is a LOCAL, which HAS a box. The emission snapshots the base into a
// capture temp and derefs it; that is correct and must stay untouched.
func localChain() int {
	r := &Root{root: &root{fd: 7}}
	defer r.root.release()
	return r.root.fd
}

func main() {
	a := &Root{root: &root{fd: 3}}
	fmt.Println(deferChain[int](a, 0), a.root.fd) // 3 then negated by the defer

	b := &Root{root: &root{fd: 5, done: make(chan int)}}
	fmt.Println(goChain(b), b.root.fd)

	c := &Root{root: &root{fd: 11}}
	fmt.Println(immediateChain(c))

	d := &root{fd: 13}
	fmt.Println(directReceiver(d), d.fd)

	fmt.Println(localChain())
}
