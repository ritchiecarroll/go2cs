// WsaSendtoRoundTrip guards `syscall.WSASendto` — the Windows DATAGRAM SEND wrapper, hand-owned in
// syscall/windows/zsyscall_windows_wsa_impl.cs and sized in docs/phase4/DESIGN-windows-udp-send.md.
//
// WHY A GUARD AND NOT A ROSTER ROW. Nothing else in the corpus can reach this function on Windows,
// and Go's own suite structurally cannot. Its sole consumer is internal/poll.FD.WriteTo, whose
// net-side callers are IPConn and UnixConn — and net's testableNetwork returns false for
// unix/unixgram on windows outright and requires Getuid()==0 for ip/ip4/ip6. UDPConn never arrives
// either: UDPConn.writeTo switches on fd.family into writeToInet4/writeToInet6, the pair hand-owned
// in internal/syscall/windows. So this program drives the function directly, which is also why it
// passes a NIL overlapped: a socket created outside internal/poll is bound to no completion port,
// and that is the synchronous arm of the hand-own.
//
// WHAT WAS WRONG, and why each line below checks a VALUE rather than the absence of a fault. The
// generated body carried four defects in one statement:
//
//	bufs        -- a MANAGED WSABuf, whose Buf is a `ж<byte>` reference where native WSABUF wants a
//	               raw CHAR*, so the descriptor the kernel read was neither the right layout nor the
//	               right size.
//	to          -- the address returned by `sockaddr()`, which that method's own body says is not a
//	               native image and that every in-package caller reaching the kernel must build with
//	               writeNativeSockaddr instead.
//	sent        -- an interior field address inside a reference-bearing container, which golib's
//	               address model cannot hold still, so the kernel's byte count landed in storage
//	               nothing owns.
//	overlapped  -- the same box kind, and additionally the operation's kernel-side identity.
//
// ⚠ THE PRE-FIX SIGNATURE IS A REFUSED CALL, NOT A CRASH, and that is why nothing here asserts a
// fault. Since golib's pointer-to-scalar operator began minting an ORDER TOKEN whenever a box's
// pinnable storage answers null — true of every field or element reference rooted in a
// reference-bearing allocation — three of those four arguments were not wrong addresses but
// unmapped numbers, and Winsock answered WSAEFAULT. A control asserting "it faults without the fix"
// would be asserting something false. So the assertions are: the call reports no error, the byte
// count it wrote is the payload's length, the bytes arrive intact, and the peer address the
// receiver reports equals the sender's own — which is the one that cannot be faked, because the
// ephemeral port is host-chosen and a wrong-layout address image cannot reproduce it.
//
// The RECEIVER is ordinary `net`, deliberately: its read path is the already-proven WSARecvFrom
// hand-own, so it is not what is under test here. Nothing host-varying is printed — ports appear
// only as a relationship between two of them — so the output is identical on any host and between
// Go and the conversion.
package main

import (
	"fmt"
	"net"
	"syscall"
	"time"
)

func main() {
	roundTrip()
	zeroLengthDatagram()
	fmt.Println("done")
}

// roundTrip is the guard's core: one datagram sent through syscall.WSASendto from a raw socket, and
// every property of it checked from the other end.
func roundTrip() {
	server, err := net.ListenPacket("udp4", "127.0.0.1:0")
	if err != nil {
		fmt.Println("roundtrip: listen failed")
		return
	}
	defer server.Close()

	dst, ok := sockaddrOf(server.LocalAddr())
	if !ok {
		fmt.Println("roundtrip: server address is not IPv4")
		return
	}

	sender, err := newSender()
	if err != nil {
		fmt.Println("roundtrip: sender setup failed")
		return
	}
	defer syscall.Closesocket(sender)

	local, err := syscall.Getsockname(sender)
	if err != nil {
		fmt.Println("roundtrip: getsockname failed")
		return
	}
	mine, ok := local.(*syscall.SockaddrInet4)
	if !ok {
		fmt.Println("roundtrip: sender address is not IPv4")
		return
	}

	payload := []byte("wsasendto-payload")
	sent, err := send(sender, payload, dst)
	fmt.Println("roundtrip: send reported no error:", err == nil)
	fmt.Println("roundtrip: byte count equals payload:", int(sent) == len(payload))

	if err := server.SetReadDeadline(time.Now().Add(5 * time.Second)); err != nil {
		fmt.Println("roundtrip: deadline failed")
		return
	}
	buf := make([]byte, 64)
	n, from, err := server.ReadFrom(buf)
	if err != nil {
		fmt.Println("roundtrip: readfrom failed")
		return
	}
	fmt.Println("roundtrip: bytes arrived intact:", string(buf[:n]) == string(payload))

	peer, ok := from.(*net.UDPAddr)
	if !ok {
		fmt.Println("roundtrip: peer address is not UDP")
		return
	}
	// The strongest check here: the receiver's idea of who sent the datagram must equal the sender's
	// own idea of its address, octet for octet and port for port. The port is ephemeral, so no
	// wrong-layout image can produce it by accident.
	fmt.Println("roundtrip: peer host matches sender's own:", sameIPv4(peer.IP, mine.Addr))
	fmt.Println("roundtrip: peer port matches sender's own:", peer.Port == mine.Port)
}

// zeroLengthDatagram is a real UDP case with no payload at all, where a reported count of zero must
// mean "a datagram of no bytes was sent" and not "nothing happened".
func zeroLengthDatagram() {
	server, err := net.ListenPacket("udp4", "127.0.0.1:0")
	if err != nil {
		fmt.Println("zerolen: listen failed")
		return
	}
	defer server.Close()

	dst, ok := sockaddrOf(server.LocalAddr())
	if !ok {
		fmt.Println("zerolen: server address is not IPv4")
		return
	}

	sender, err := newSender()
	if err != nil {
		fmt.Println("zerolen: sender setup failed")
		return
	}
	defer syscall.Closesocket(sender)

	// A WSABUF whose Len is zero still needs a Buf the kernel may read zero bytes through, which is
	// what internal/poll's own zero-byte arm passes.
	one := []byte{0}
	buf := syscall.WSABuf{Len: 0, Buf: &one[0]}
	var sent uint32
	err = syscall.WSASendto(sender, &buf, 1, &sent, 0, dst, nil, nil)
	fmt.Println("zerolen: send reported no error:", err == nil)
	fmt.Println("zerolen: byte count is zero:", sent == 0)

	if err := server.SetReadDeadline(time.Now().Add(5 * time.Second)); err != nil {
		fmt.Println("zerolen: deadline failed")
		return
	}
	n, _, err := server.ReadFrom(make([]byte, 8))
	fmt.Println("zerolen: a datagram arrived:", err == nil)
	fmt.Println("zerolen: it carried no bytes:", n == 0)
}

// newSender is a raw UDP socket bound to an ephemeral loopback port — nothing between the payload
// and WSASendto, and no completion port, which is what makes the nil overlapped legitimate.
func newSender() (syscall.Handle, error) {
	s, err := syscall.Socket(syscall.AF_INET, syscall.SOCK_DGRAM, 0)
	if err != nil {
		return syscall.InvalidHandle, err
	}
	if err = syscall.Bind(s, &syscall.SockaddrInet4{Port: 0, Addr: [4]byte{127, 0, 0, 1}}); err != nil {
		syscall.Closesocket(s)
		return syscall.InvalidHandle, err
	}
	return s, nil
}

// send is the call under test.
func send(s syscall.Handle, payload []byte, to syscall.Sockaddr) (uint32, error) {
	buf := syscall.WSABuf{Len: uint32(len(payload)), Buf: &payload[0]}
	var sent uint32
	err := syscall.WSASendto(s, &buf, 1, &sent, 0, to, nil, nil)
	return sent, err
}

func sockaddrOf(addr net.Addr) (*syscall.SockaddrInet4, bool) {
	ua, ok := addr.(*net.UDPAddr)
	if !ok {
		return nil, false
	}
	ip := ua.IP.To4()
	if ip == nil {
		return nil, false
	}
	sa := &syscall.SockaddrInet4{Port: ua.Port}
	copy(sa.Addr[:], ip)
	return sa, true
}

func sameIPv4(ip net.IP, raw [4]byte) bool {
	four := ip.To4()
	if four == nil {
		return false
	}
	for i := 0; i < 4; i++ {
		if four[i] != raw[i] {
			return false
		}
	}
	return true
}
