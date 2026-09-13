# go.sync.atomic

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).

[![Tests](https://img.shields.io/badge/Tests-108%2F108_validated-brightgreen?logo=go)](https://go2cs.net/validation/1.23.12.3/sync.atomic.html) [![Docs](https://img.shields.io/badge/Docs-@1.23.12-00ADD8?logo=go)](https://pkg.go.dev/sync/atomic@go1.23.12)\
[![Source](https://img.shields.io/badge/Source-@1.23.12-00ADD8?logo=go)](https://github.com/golang/go/tree/go1.23.12/src/sync/atomic) [![Source](https://img.shields.io/badge/Source-@1.23.12.3-512BD4?logo=dotnet)](https://github.com/ritchiecarroll/go2cs/tree/nuget-1.23.12.3/src/core/sync/atomic)

Package atomic provides low-level atomic memory primitives useful for implementing synchronization algorithms.

These functions require great care to be used correctly. Except for special, low-level applications, synchronization is better done with channels or the facilities of the [sync](https://pkg.go.dev/sync@go1.23.12) package. Share memory by communicating; don't communicate by sharing memory.

The swap operation, implemented by the SwapT functions, is the atomic equivalent of:

	old = *addr
	*addr = new
	return old

The compare-and-swap operation, implemented by the CompareAndSwapT functions, is the atomic equivalent of:

	if *addr == old {
		*addr = new
		return true
	}
	return false

The add operation, implemented by the AddT functions, is the atomic equivalent of:

	*addr += delta
	return *addr

The load and store operations, implemented by the LoadT and StoreT functions, are the atomic equivalents of "return \*addr" and "\*addr = val".

In the terminology of [the Go memory model](https://go.dev/ref/mem), if the effect of an atomic operation A is observed by atomic operation B, then A “synchronizes before” B. Additionally, all the atomic operations executed in a program behave as though executed in some sequentially consistent order. This definition provides the same semantics as C++'s sequentially consistent atomics and Java's volatile variables.

---

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/LICENSE) file.

Artwork licensed under Creative Commons Attribution 3.0; see [artwork attribution](https://github.com/ritchiecarroll/go2cs/blob/master/docs/images/README).
