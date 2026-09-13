// Stack.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// Stack is a minimal last-in/first-out container built on a slice.
//
// The converter reaches for it wherever emission has to suspend what it is doing, work on
// something nested, and then resume exactly where it left off. The one live use today is
// Visitor.blocks (a Stack[*strings.Builder]): visitBlockStmt pushes the current output builder
// before it descends into a nested block and pops it back afterwards, so the inner block writes
// into its own builder without disturbing the outer one.
//
// The zero value is ready to use — `Stack[T]{}` needs no constructor, which is how every caller
// creates one (see packageStateOperations.resetPackageState).
type Stack[T any] struct {
	// items holds the stack contents with the TOP at the END of the slice, so Push/Pop are an
	// append and a re-slice rather than a shift of every element.
	items []T
}

// Len reports how many items the stack currently holds.
func (s *Stack[T]) Len() int {
	return len(s.items)
}

// IsEmpty reports whether the stack holds nothing, so callers can ask the question without
// comparing Len against a magic zero.
func (s *Stack[T]) IsEmpty() bool {
	return s.Len() == 0
}

// Push adds an item to the top of the stack.
func (s *Stack[T]) Push(item T) {
	// The top lives at the end of the slice, so a push is a plain append.
	s.items = append(s.items, item)
}

// Pop removes and returns the item from the top of the stack, and panics when the stack is empty.
//
// Use this when an empty stack means the converter's own bookkeeping is broken and there is no
// sensible way to continue — visitBlockStmt pops exactly once per push, so an empty stack there
// is a converter bug, not a data condition. Use TryPop when emptiness is an expected outcome.
func (s *Stack[T]) Pop() T {
	item, ok := s.TryPop()

	if !ok {
		panic("stack is empty")
	}

	return item
}

// TryPop removes and returns the item from the top of the stack, reporting false (and the zero
// value of T) when the stack is empty instead of panicking.
func (s *Stack[T]) TryPop() (T, bool) {
	if s.IsEmpty() {
		var zero T
		return zero, false
	}

	index := len(s.items) - 1
	item := s.items[index]
	s.items = s.items[:index]

	return item, true
}
