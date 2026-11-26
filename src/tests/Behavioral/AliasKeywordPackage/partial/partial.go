// Package partial is named by a contextual C# word that stays legal at member start (a return type), so its alias must
// stay bare: the near-miss, where a widening of the escaper shows as a change in this project's golden.
package partial

// Part is a defined type named in flags' method signature.
type Part int
