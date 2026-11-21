// Package sub sits below a directory named extension, so its C# namespace carries `extension` as a segment.
package sub

// Name reports which package answered.
func Name() string { return "extension/sub" }
