package lib

// largeInteger is unexported, as libc's is: an alias of an anonymous struct.
type largeInteger = struct {
	QuadPart int64
}

// SetFilePointerEx is exported, so its parameter type is reachable from other packages.
func SetFilePointerEx(distance largeInteger, moved *largeInteger) int64 {
	moved.QuadPart = distance.QuadPart * 2
	return distance.QuadPart + 1
}
