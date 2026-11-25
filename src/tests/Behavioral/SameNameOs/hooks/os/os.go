// Package os shares its NAME with the standard library's os, which publishes a FileInfo alias of io/fs's
// interface, and declares a FileInfo of its own: the importer's two `os.FileInfo` keys name different types.
package os

import (
	"io/fs"
	"time"
)

type FileInfo struct {
	name string
	size int64
}

func NewFileInfo(name string, size int64) *FileInfo { return &FileInfo{name, size} }

func (f *FileInfo) Name() string       { return f.name }
func (f *FileInfo) Size() int64        { return f.size }
func (f *FileInfo) Mode() fs.FileMode  { return 0o644 }
func (f *FileInfo) ModTime() time.Time { return time.Time{} }
func (f *FileInfo) IsDir() bool        { return false }
func (f *FileInfo) Sys() any           { return nil }
