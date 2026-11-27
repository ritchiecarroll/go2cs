// fixture.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The e1census controls (e1census_test.go). Each function's comment states whether the census must count it.
package fixture

import (
	"runtime"
	"sync"
)

// NonGenericParker parks directly; it is not generic, so it is never counted.
func NonGenericParker(c chan int) { c <- 1 }

// sendHelper parks; it is the transitive path for GenericParker.
func sendHelper(c chan int) { c <- 2 }

// GenericParker parks only through sendHelper: counted.
func GenericParker[T any](v T, c chan int) T {
	sendHelper(c)
	return v
}

// quiet does not park.
func quiet(n int) int { return n + 1 }

// GenericQuiet only calls a function that does not park: not counted.
func GenericQuiet[T any](v T) T {
	_ = quiet(1)
	return v
}

// Box is a generic type.
type Box[T any] struct {
	mu sync.Mutex
	v  T
}

// Get parks through the sync.Mutex.Lock wait primitive: counted (a method of a generic type).
func (b *Box[T]) Get() T {
	b.mu.Lock()
	defer b.mu.Unlock()
	return b.v
}

// GenericDynamic calls a func value and nothing else: not counted, in the dynamic bucket.
func GenericDynamic[T any](f func() T) T { return f() }

// GenericFinalizer reaches a park only inside the Go runtime (SetFinalizer starts the finalizer goroutine): counted only
// when the census follows calls into package runtime.
func GenericFinalizer[T any](p *T) {
	runtime.SetFinalizer(p, func(*T) {})
}
