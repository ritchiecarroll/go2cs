package main

import . "CollisionRenamedGlobalReceiver/lib"

// The dot-import form: the foreign var is a bare identifier here.
func viaDot() int32 {
	Tid.Add(100)
	return Tid.Load()
}
