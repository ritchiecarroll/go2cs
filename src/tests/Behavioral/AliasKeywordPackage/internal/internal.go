// Package internal is named by a C# keyword: a converted package that names one of its types imports it under an
// alias C# must spell @internal.
package internal

// NotForPublicUse is the marker go-json-experiment's internal package exports, in the same shape.
type NotForPublicUse struct{}

// Flag is a defined type another package aliases.
type Flag uint8
