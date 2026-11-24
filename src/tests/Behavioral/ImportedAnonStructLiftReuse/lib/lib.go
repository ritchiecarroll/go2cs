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

// rect is spelled with int32 fields here; the importer spells the identical struct through its own alias of
// int32.
type rect = struct {
	W int32
	H int32
}

// Area is exported, so its rect parameter is reachable from other packages.
func Area(r rect) int64 { return int64(r.W) * int64(r.H) }
