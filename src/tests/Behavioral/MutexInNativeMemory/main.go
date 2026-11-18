// Guards a sync.Mutex that lives in memory the Go heap does not own (sqlite-cs W9a). sqlite's lib/mutex.go
// callocs `mutex{sync.Mutex; cnt, id int32; recursive bool}` and reaches it only through
// (*mutex)(unsafe.Pointer(m)); libc's pthread mutex overlay holds a sync.Mutex the same way. go2cs's Mutex held
// a managed semaphore reference, so reading one out of native memory was refused; it now has Go's eight bytes.
//
// An anonymous mmap stands in for calloc. Every shape is sqlite's: the fast mutex, the recursive mutex whose
// owner id is written through &(*mutex)(p).id (W9b), TryLock and the recursive tryEnter, and a counter that
// lives in the same native page, incremented under the mutex by 2, 4 and 8 goroutines.
//
// PLATFORM-EXCLUSIVE: [GoPlatformExclusive("linux")], following AddressOfFieldThroughConversionNative. An anonymous
// mmap by raw system call number is not on Windows.
package main

import (
	"fmt"
	"sync"
	"sync/atomic"
	"syscall"
	"unsafe"
)

type mutex struct {
	sync.Mutex
	cnt       int32
	id        int32
	recursive bool
}

func alloc() uintptr {
	p, _, errno := syscall.Syscall6(syscall.SYS_MMAP, 0, 4096, syscall.PROT_READ|syscall.PROT_WRITE, syscall.MAP_ANON|syscall.MAP_PRIVATE, ^uintptr(0), 0)
	if errno != 0 {
		panic(errno)
	}
	return p
}

func enter(m uintptr, id int32) {
	if !(*mutex)(unsafe.Pointer(m)).recursive {
		(*mutex)(unsafe.Pointer(m)).Lock()
		atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, id)
		return
	}
	if atomic.LoadInt32(&(*mutex)(unsafe.Pointer(m)).id) == id {
		(*mutex)(unsafe.Pointer(m)).cnt++
		return
	}
	(*mutex)(unsafe.Pointer(m)).Lock()
	atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, id)
	(*mutex)(unsafe.Pointer(m)).cnt = 1
}

// tryEnter is sqlite's mutexTry: a recursive mutex already held by this id is re-entered without TryLock.
func tryEnter(m uintptr, id int32) bool {
	if (*mutex)(unsafe.Pointer(m)).recursive && atomic.LoadInt32(&(*mutex)(unsafe.Pointer(m)).id) == id {
		(*mutex)(unsafe.Pointer(m)).cnt++
		return true
	}
	if !(*mutex)(unsafe.Pointer(m)).TryLock() {
		return false
	}
	atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, id)
	(*mutex)(unsafe.Pointer(m)).cnt = 1
	return true
}

func leave(m uintptr) {
	if !(*mutex)(unsafe.Pointer(m)).recursive {
		atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, 0)
		(*mutex)(unsafe.Pointer(m)).Unlock()
		return
	}
	(*mutex)(unsafe.Pointer(m)).cnt--
	if (*mutex)(unsafe.Pointer(m)).cnt == 0 {
		atomic.StoreInt32(&(*mutex)(unsafe.Pointer(m)).id, 0)
		(*mutex)(unsafe.Pointer(m)).Unlock()
	}
}

// count runs n goroutines that each add 20000 to a counter at offset 64 of the mutex's own native page.
func count(m uintptr, n int) int64 {
	total := (*int64)(unsafe.Pointer(m + 64))
	*total = 0
	var wg sync.WaitGroup
	for g := 1; g <= n; g++ {
		wg.Add(1)
		go func(id int32) {
			defer wg.Done()
			for i := 0; i < 20000; i++ {
				enter(m, id)
				*total++
				leave(m)
			}
		}(int32(g))
	}
	wg.Wait()
	return *total
}

func main() {
	fast := alloc()
	rec := alloc()
	(*mutex)(unsafe.Pointer(rec)).recursive = true
	fmt.Println("sizeof mutex:", unsafe.Sizeof(mutex{}), "sizeof sync.Mutex:", unsafe.Sizeof(sync.Mutex{}))

	enter(fast, 1)
	fmt.Println("fast: trylock while held:", (*mutex)(unsafe.Pointer(fast)).TryLock())
	leave(fast)
	fmt.Println("fast: trylock when free:", (*mutex)(unsafe.Pointer(fast)).TryLock())
	(*mutex)(unsafe.Pointer(fast)).Unlock()

	enter(rec, 2)
	enter(rec, 2)
	fmt.Println("recursive: cnt after two enters:", (*mutex)(unsafe.Pointer(rec)).cnt)
	fmt.Println("recursive: tryEnter by the owner:", tryEnter(rec, 2), (*mutex)(unsafe.Pointer(rec)).cnt)
	fmt.Println("recursive: tryEnter by another id:", tryEnter(rec, 3))
	leave(rec)
	leave(rec)
	leave(rec)
	fmt.Println("recursive: free again:", atomic.LoadInt32(&(*mutex)(unsafe.Pointer(rec)).id), tryEnter(rec, 3))
	leave(rec)

	for _, n := range []int{2, 4, 8} {
		fmt.Println("fast, goroutines", n, "total:", count(fast, n))
	}
	for _, n := range []int{2, 4, 8} {
		fmt.Println("recursive, goroutines", n, "total:", count(rec, n))
	}
}
