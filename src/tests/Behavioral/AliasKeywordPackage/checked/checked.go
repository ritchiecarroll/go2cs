// Package checked is named by another C# keyword (checked is reserved, as internal is).
package checked

// Token is a defined type another package's method names in its signature.
type Token int

// Twice doubles a token.
func (t Token) Twice() Token { return t * 2 }
