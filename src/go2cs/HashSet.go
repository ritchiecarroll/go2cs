// HashSet.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

type void struct{}

var member void

// HashSet represents a distinct collection of T values, i.e., a set.
// A HashSet is not sorted and will not contain duplicate elements.
// The methods of the HashSet are not intrinsically thread-safe procedures,
// to guarantee thread safety, you should initiate a lock before calling a method.
type HashSet[T comparable] map[T]void

// NewHashSet creates a new set containing all elements in the specified slice.
// Returns a new HashSet that contain the specified slice items.
func NewHashSet[T comparable](items []T) HashSet[T] {
	hs := make(HashSet[T], len(items))
	hs.UnionWith(items)
	return hs
}

// Add adds the specified element to a set.
// Returns true if item was added to the set; otherwise, false.
func (hs HashSet[T]) Add(item T) bool {
	if hs.Contains(item) {
		return false
	}

	hs[item] = member
	return true
}

// Remove removes the specified element from a set.
// Returns true if item was removed from the set; otherwise, false.
func (hs HashSet[T]) Remove(item T) bool {
	if hs.Contains(item) {
		delete(hs, item)
		return true
	}

	return false
}

// RemoveWhere removes all elements that match the conditions defined by the specified predicate from a set.
// Returns number of elements that were removed from the set.
func (hs HashSet[T]) RemoveWhere(predicate func(T) bool) int {
	var removedCount int

	for k := range hs {
		if predicate(k) && hs.Remove(k) {
			removedCount++
		}
	}

	return removedCount
}

// IsEmpty determines if set contains no elements.
// Returns true if the set is empty, i.e., has a length of zero; otherwise, false.
func (hs HashSet[T]) IsEmpty() bool {
	return len(hs) == 0
}

// Clear removes all elements from a set.
func (hs HashSet[T]) Clear() {
	for k := range hs {
		delete(hs, k)
	}
}

// Contains determines whether a set contains the specified element.
// Returns true if the set contains the specified element; otherwise, false.
func (hs HashSet[T]) Contains(item T) bool {
	_, ok := hs[item]
	return ok
}

// Keys copies the elements of a set to a slice.
// Returns a slice containing the elements of the set.
func (hs HashSet[T]) Keys() []T {
	keys := make([]T, len(hs))
	i := 0

	for k := range hs {
		keys[i] = k
		i++
	}

	return keys
}

// ExceptWith removes all elements in the specified slice from the current set.
func (hs HashSet[T]) ExceptWith(other []T) {
	for _, v := range other {
		delete(hs, v)
	}
}

// ExceptWithSet removes all elements in the specified set from the current set.
func (hs HashSet[T]) ExceptWithSet(other HashSet[T]) {
	hs.ExceptWith(other.Keys())
}

// SymmetricExceptWith modifies the current set to contain only elements that are present either in the set or in the specified slice, but not both.
func (hs HashSet[T]) SymmetricExceptWith(other []T) {
	if hs.IsEmpty() {
		// If set is empty, then symmetric difference is other
		hs.UnionWith(other)
		return
	}

	for _, v := range other {
		if !hs.Remove(v) {
			hs.Add(v)
		}
	}
}

// SymmetricExceptWithSet modifies the current set to contain only elements that are present either in the set or in the specified set, but not both.
func (hs HashSet[T]) SymmetricExceptWithSet(other HashSet[T]) {
	hs.SymmetricExceptWith(other.Keys())
}

// IntersectWith modifies the current set to contain only elements that are present in the set and in the specified slice.
func (hs HashSet[T]) IntersectWith(other []T) {
	hs.IntersectWithSet(NewHashSet(other))
}

// IntersectWithSet modifies the current set to contain only elements that are present in the set and in the specified set.
func (hs HashSet[T]) IntersectWithSet(other HashSet[T]) {
	// Intersection of anything with empty set is empty set, so return if count is 0
	if len(hs) == 0 {
		return
	}

	if len(other) == 0 {
		hs.Clear()
		return
	}

	for k := range hs {
		if !other.Contains(k) {
			hs.Remove(k)
		}
	}
}

// UnionWith modifies the current set to contain all elements that are present in the set, the specified slice, or both.
func (hs HashSet[T]) UnionWith(other []T) {
	for _, v := range other {
		hs.Add(v)
	}
}

// UnionWithSet modifies the current set to contain all elements that are present in the set, the specified set, or both.
func (hs HashSet[T]) UnionWithSet(other HashSet[T]) {
	hs.UnionWith(other.Keys())
}

// SetEquals determines whether a set and the specified slice contain the same elements.
// Returns true if the set is equal to other slice items; otherwise, false.
func (hs HashSet[T]) SetEquals(other []T) bool {
	if len(hs) != len(other) {
		return false
	}

	for _, v := range other {
		if !hs.Contains(v) {
			return false
		}
	}

	return true
}

// SetEqualsSet determines whether a set and the specified set contain the same elements.
// Returns true if the set is equal to other set; otherwise, false.
func (hs HashSet[T]) SetEqualsSet(other HashSet[T]) bool {
	return hs.SetEquals(other.Keys())
}

// Overlaps determines whether the current set and a specified slice share common elements.
// Returns true if the current set and other slice items share at least one common element; otherwise, false.
func (hs HashSet[T]) Overlaps(other []T) bool {
	if hs.IsEmpty() {
		return false
	}

	for _, v := range other {
		if hs.Contains(v) {
			return true
		}
	}

	return false
}

// OverlapsSet determines whether the current set and a specified set share common elements.
// Returns true if the current set and other set share at least one common element; otherwise, false.
func (hs HashSet[T]) OverlapsSet(other HashSet[T]) bool {
	return hs.Overlaps(other.Keys())
}

// IsSubsetOf determines whether a set is a subset of the specified slice.
// Returns true if the set is a subset of other slice items; otherwise, false.
func (hs HashSet[T]) IsSubsetOf(other []T) bool {
	return hs.IsSubsetOfSet(NewHashSet(other))
}

// IsSubsetOfSet determines whether a set is a subset of the specified set.
// Returns true if the set is a subset of other set; otherwise, false.
func (hs HashSet[T]) IsSubsetOfSet(other HashSet[T]) bool {
	// The empty set is a subset of any set
	if hs.IsEmpty() {
		return true
	}

	// If set has more elements than slice, then it can't be a subset
	if len(hs) > len(other) {
		return false
	}

	for k := range hs {
		if !other.Contains(k) {
			return false
		}
	}

	return true
}

// IsProperSubsetOf determines whether a set is a proper subset of the specified slice.
// Returns true if the set is a proper subset of other slice items; otherwise, false.
func (hs HashSet[T]) IsProperSubsetOf(other []T) bool {
	return hs.IsProperSubsetOfSet(NewHashSet(other))
}

// IsProperSubsetOfSet determines whether a set is a proper subset of the specified set.
// Returns true if the set is a proper subset of other set; otherwise, false.
func (hs HashSet[T]) IsProperSubsetOfSet(other HashSet[T]) bool {
	// The empty set is a proper subset of anything but the empty set
	if hs.IsEmpty() {
		return len(other) > 0
	}

	// If set has more or equal elements than slice, then it can't be a proper subset
	if len(hs) >= len(other) {
		return false
	}

	for k := range hs {
		if !other.Contains(k) {
			return false
		}
	}

	return true
}

// IsSupersetOf determines whether a set is a superset of the specified slice.
// Returns true if the set is a superset of other slice items; otherwise, false.
func (hs HashSet[T]) IsSupersetOf(other []T) bool {
	// If other is the empty set then this is a superset
	if len(other) == 0 {
		return true
	}

	// If slice has more elements than set, then it can't be a superset
	if len(other) > len(hs) {
		return false
	}

	for _, v := range other {
		if !hs.Contains(v) {
			return false
		}
	}

	return true
}

// IsSupersetOfSet determines whether a set is a superset of the specified set.
// Returns true if the set is a superset of other set; otherwise, false.
func (hs HashSet[T]) IsSupersetOfSet(other HashSet[T]) bool {
	return hs.IsSupersetOf(other.Keys())
}

// IsProperSupersetOf determines whether a set is a proper superset of the specified slice.
// Returns true if the set is a proper superset of other slice items; otherwise, false.
func (hs HashSet[T]) IsProperSupersetOf(other []T) bool {
	// The empty set is not a proper subset of any set
	if hs.IsEmpty() {
		return true
	}

	// If other is the empty set then this is a proper superset
	if len(other) == 0 {
		return true // Set has at least one element, based on prior check
	}

	// If slice has more or equal elements than set, then it can't be a proper superset
	if len(other) >= len(hs) {
		return false
	}

	for _, v := range other {
		if !hs.Contains(v) {
			return false
		}
	}

	return true
}

// IsProperSupersetOfSet determines whether a set is a proper superset of the specified set.
// Returns true if the set is a proper superset of other set; otherwise, false.
func (hs HashSet[T]) IsProperSupersetOfSet(other HashSet[T]) bool {
	return hs.IsProperSupersetOf(other.Keys())
}
