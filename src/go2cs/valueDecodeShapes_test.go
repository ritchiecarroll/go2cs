// valueDecodeShapes_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// B2's end-to-end reading (COORD 2026-11-15 17:56Z and 18:04Z): libc's and sqlite's shapes that G1 gives Go layouts
// (pthread_mutex_t, pthread_cond_t, the 14-word union, in6_addr, sockaddr_in6, ifreq, Bitvec; all admitted to inline
// arrays) and TBF_ctx (kept out, read through D2's GoValue), each read through a reinterpret of Go storage as ccgo code
// reads them: fields, whole values and whole writes. Every read runs under recover, so a refusal prints its text and
// the run goes on. The converted program must print what go prints.
//
// Measured: at the base (W14) the program died in init on TBF_ctx's B0' refusal; at B2's first cut every admitted
// shape read as go did and TBF_ctx decoded with an EMPTY FP, the field spelled through the alias TBF_key that had no
// `= new(18)` (fixed by aliasArrayFieldZero_test.go's cut). GO2CS_INLINE_BUILD=1 runs it.
var valueDecodeShapesModule = map[string]string{"main.go": `package main

// G1's admitted shapes (libc v1.80.1 / sqlite lib v1.62.0, linux/amd64), each read through a reinterpret over Go
// storage the way ccgo code reads them: field reads, whole-value reads and whole-value writes. Every read runs under
// recover, so a refusal prints its panic text and the run goes on.

import (
	"fmt"
	"unsafe"
)

type Tuint8_t = uint8
type Tuint16_t = uint16
type Tuint32_t = uint32
type Tsa_family_t = uint16
type Tin_port_t = uint16
type Tu32 = uint32
type Tu8 = uint8

type Tpthread_mutex_t = struct {
	F__u struct {
		F__vi [0][10]int32
		F__p  [0][5]uintptr
		F__i  [10]int32
	}
}

type Tpthread_cond_t = struct {
	F__u struct {
		F__vi [0][12]int32
		F__p  [0][6]uintptr
		F__i  [12]int32
	}
}

type Tunion14 = struct {
	F__u struct {
		F__vi [0][14]int32
		F__s  [0][7]uint64
		F__i  [14]int32
	}
}

type Tin6_addr = struct {
	F__in6_union struct {
		F__s6_addr16 [0][8]Tuint16_t
		F__s6_addr32 [0][4]Tuint32_t
		F__s6_addr   [16]Tuint8_t
	}
}

type Tsockaddr_in6 = struct {
	Fsin6_family   Tsa_family_t
	Fsin6_port     Tin_port_t
	Fsin6_flowinfo Tuint32_t
	Fsin6_addr     Tin6_addr
	Fsin6_scope_id Tuint32_t
}

type Tsockaddr = struct {
	Fsa_family Tsa_family_t
	Fsa_data   [14]int8
}

type Tifmap = struct {
	Fmem_start uint64
	Fmem_end   uint64
	Fbase_addr uint16
	Firq       uint8
	Fdma       uint8
	Fport      uint8
}

type Tifreq = struct {
	Fifr_ifrn struct {
		Fifrn_name [16]int8
	}
	Fifr_ifru struct {
		Fifru_dstaddr   [0]Tsockaddr
		Fifru_broadaddr [0]Tsockaddr
		Fifru_netmask   [0]Tsockaddr
		Fifru_hwaddr    [0]Tsockaddr
		Fifru_flags     [0]int16
		Fifru_ivalue    [0]int32
		Fifru_mtu       [0]int32
		Fifru_map       [0]Tifmap
		Fifru_slave     [0][16]int8
		Fifru_newname   [0][16]int8
		Fifru_data      [0]uintptr
		Fifru_addr      Tsockaddr
		F__ccgo_pad12   [8]byte
	}
}

type TBitvec = struct {
	FiSize    Tu32
	FnSet     Tu32
	FiDivisor Tu32
	Fu        struct {
		FaHash   [0][124]Tu32
		FapSub   [0][62]uintptr
		FaBitmap [496]Tu8
	}
}

type TBF_word = uint32

type TBF_key = [18]TBF_word

type TBF_ctx = struct {
	FPS [0][1042]TBF_word
	Fs  struct {
		FP TBF_key
		FS [4][256]TBF_word
	}
}

var bfInit = [1042]uint32{1, 2, 3, 18: 19, 1041: 99}

var bfState = *(*TBF_ctx)(unsafe.Pointer(&bfInit))

var buf [1024]byte

func fill() {
	for i := range buf {
		buf[i] = byte(i*7 + 1)
	}
}

func try(name string, read func() string) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Printf("%s PANIC %v\n", name, r)
		}
	}()

	fmt.Printf("%s %s\n", name, read())
}

func main() {
	fmt.Println("sizes", unsafe.Sizeof(Tpthread_mutex_t{}), unsafe.Sizeof(Tpthread_cond_t{}), unsafe.Sizeof(Tunion14{}),
		unsafe.Sizeof(Tin6_addr{}), unsafe.Sizeof(Tsockaddr_in6{}), unsafe.Sizeof(Tifreq{}), unsafe.Sizeof(TBitvec{}))

	fill()
	try("mutex.field", func() string {
		p := (*Tpthread_mutex_t)(unsafe.Pointer(&buf))
		return fmt.Sprint(p.F__u.F__i[3])
	})
	try("mutex.whole", func() string {
		v := *(*Tpthread_mutex_t)(unsafe.Pointer(&buf))
		return fmt.Sprint(v.F__u.F__i[1])
	})
	try("mutex.write", func() string {
		*(*Tpthread_mutex_t)(unsafe.Pointer(&buf)) = Tpthread_mutex_t{}
		return fmt.Sprint(buf[12], buf[39], buf[40])
	})

	fill()
	try("cond.field", func() string {
		p := (*Tpthread_cond_t)(unsafe.Pointer(&buf))
		return fmt.Sprint(p.F__u.F__i[11])
	})
	try("cond.write", func() string {
		*(*Tpthread_cond_t)(unsafe.Pointer(&buf)) = Tpthread_cond_t{}
		return fmt.Sprint(buf[0], buf[47], buf[48])
	})

	fill()
	try("u14.write", func() string {
		*(*Tunion14)(unsafe.Pointer(&buf)) = Tunion14{}
		return fmt.Sprint(buf[0], buf[55], buf[56])
	})

	fill()
	try("in6.whole", func() string {
		v := *(*Tin6_addr)(unsafe.Pointer(&buf))
		return fmt.Sprint(v.F__in6_union.F__s6_addr[5])
	})
	try("in6.field", func() string {
		p := (*Tin6_addr)(unsafe.Pointer(&buf))
		return fmt.Sprint(p.F__in6_union.F__s6_addr[2])
	})

	fill()
	try("sin6.field", func() string {
		p := (*Tsockaddr_in6)(unsafe.Pointer(&buf))
		return fmt.Sprint(p.Fsin6_port, p.Fsin6_scope_id, p.Fsin6_addr.F__in6_union.F__s6_addr[0])
	})
	try("sin6.write", func() string {
		*(*Tsockaddr_in6)(unsafe.Pointer(&buf)) = Tsockaddr_in6{Fsin6_family: 10}
		return fmt.Sprint(buf[0], buf[1], buf[27], buf[28])
	})

	fill()
	try("ifreq.field", func() string {
		p := (*Tifreq)(unsafe.Pointer(&buf))
		return fmt.Sprint(p.Fifr_ifrn.Fifrn_name[2], p.Fifr_ifru.Fifru_addr.Fsa_family)
	})

	fill()
	try("bitvec.field", func() string {
		p := (*TBitvec)(unsafe.Pointer(&buf))
		return fmt.Sprint(p.FiSize, p.FnSet, p.FiDivisor, p.Fu.FaBitmap[3])
	})

	try("tbf.seed", func() string {
		return fmt.Sprint(bfState.Fs.FP[0], bfState.Fs.FP[2], bfState.Fs.FS[0][0], bfState.Fs.FS[3][255])
	})
}
`}

func TestValueDecodeShapesReadAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	want := goRunInlineModule(t, "decodeshapes", valueDecodeShapesModule)
	out := convertInlineModule(t, "decodeshapes", valueDecodeShapesModule)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints:\n%s\nREADING go2cs prints:\n%s", want, got)

	if got != want || want == "" {
		t.Fatalf("the two runs differ, or printed nothing:\n   go:\n%s\ngo2cs:\n%s", want, got)
	}
}
