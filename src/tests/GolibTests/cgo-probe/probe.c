// probe.c - go2cs's cgo probe library (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.10, increment 4d).
//
// The exit-gate arms no system library gives on every OS, in one small library. Every hosted golib-tests leg builds it
// from this source with the OS's default pinned C toolchain (PLAN-cgo-interop §5.1: zig on linux and darwin,
// llvm-mingw on windows) into GO2CS_CGO_PROBE_DIR. No binary of it is committed. GolibTests binds it through the
// generated wrappers of GoCgoWrappers.<os>.golden.cs (src/go2cs's cgoProbeDecls declares it as cmd/cgo would), and
// CgoProbeTests calls it.

#include <stdlib.h>

#if defined(_WIN32)
#define GO2CS_PROBE_EXPORT __declspec(dllexport)
#else
#define GO2CS_PROBE_EXPORT __attribute__((visibility("default")))
#endif

struct go2cs_probe_pair
{
    int a;
    int b;
};

// A union inside a struct: cmd/cgo gives the union as its bytes, at its offset, after the padding that aligns it.
struct go2cs_probe_tagged
{
    int tag;
    union
    {
        long long l;
        double d;
    } u;
};

// Two doubles: a homogeneous floating-point aggregate on arm64, two SSE eightbytes on x64. By pointer only; by value
// §11.7 refuses it until it is measured per RID.
struct go2cs_probe_hfa
{
    double x;
    double y;
};

// Eight bytes by value both ways: §11.7's stand-in for inet_ntoa on windows, whose in_addr is a union there.
GO2CS_PROBE_EXPORT struct go2cs_probe_pair go2cs_probe_pair(int a, int b)
{
    struct go2cs_probe_pair pair = { a, b };
    return pair;
}

// Ordered, so a swapped pair reads differently: (3, 4) is 3004.
GO2CS_PROBE_EXPORT int go2cs_probe_pair_sum(struct go2cs_probe_pair pair)
{
    return pair.a * 1000 + pair.b;
}

// Aliasing (§5.2): called with dst == x, the second store reads x->a AFTER the first wrote dst->a, so one shared object
// and two separate copies give different results, and the result says which pointers C was handed.
GO2CS_PROBE_EXPORT int go2cs_probe_add_into(struct go2cs_probe_pair *dst, const struct go2cs_probe_pair *x, const struct go2cs_probe_pair *y)
{
    dst->a = x->a + y->a;
    dst->b = x->a + y->b;
    return dst == x;
}

// The struct image door's mixed case (cgo Phase 3, increment 3.4, DESIGN-cgo-phase3-marshaling 19.9): a struct and an
// int that may name one Go value. Go hands C one memory; go2cs refuses rather than hand C two copies of it.
GO2CS_PROBE_EXPORT int go2cs_probe_pair_and_int(struct go2cs_probe_pair *dst, int *n)
{
    dst->a += *n;
    *n = dst->b;
    return dst->a;
}

// Doubles the union's member the tag names, and returns the tag.
GO2CS_PROBE_EXPORT int go2cs_probe_tagged_double(struct go2cs_probe_tagged *tagged)
{
    if (tagged->tag == 0)
        tagged->u.l *= 2;
    else
        tagged->u.d *= 2;

    return tagged->tag;
}

GO2CS_PROBE_EXPORT double go2cs_probe_hfa_scale(struct go2cs_probe_hfa *hfa, double k)
{
    hfa->x *= k;
    hfa->y *= k;
    return hfa->x + hfa->y;
}

// strtol's own errno, set inside the C library this probe links (COORD's ruling (c) on §11.10's errno arm): the probe
// takes only a native char*, so Phase 2's surface binds it.
GO2CS_PROBE_EXPORT long go2cs_probe_strtol(const char *s, int base)
{
    return strtol(s, 0, base);
}

// The Phase 2 fixture's four probes (src/go2cs/testdata/cgo/phase2/fixture.go declares them, increment 5b): a struct
// holding a pointer, which cmd/cgo's pointer-check closure guards, by pointer and inside a struct by value, and a by-value
// pair beside an argument evaluated after it.
struct go2cs_probe_node
{
    int value;
    struct go2cs_probe_node *next;
};

struct go2cs_probe_link
{
    int value;
    struct go2cs_probe_node *node;
};

// The values along a list, plus base.
GO2CS_PROBE_EXPORT long go2cs_probe_node_sum(struct go2cs_probe_node *node, long base)
{
    for (; node; node = node->next)
        base += node->value;

    return base;
}

GO2CS_PROBE_EXPORT void go2cs_probe_node_touch(struct go2cs_probe_node *node)
{
    if (node)
        node->value++;
}

GO2CS_PROBE_EXPORT int go2cs_probe_link_value(struct go2cs_probe_link link)
{
    return link.value + (link.node ? link.node->value : 0);
}

// Ordered, so the pair read is the one passed: (1, 2) scaled by 2 is 2002.
GO2CS_PROBE_EXPORT int go2cs_probe_pair_scaled(struct go2cs_probe_pair pair, int k)
{
    return (pair.a * 1000 + pair.b) * k;
}

// A C string C owns: a Phase 2 program cannot make one (C.CString is Phase 3), so the exit gate's puts arm prints this.
GO2CS_PROBE_EXPORT const char *go2cs_probe_text(void)
{
    return "go2cs probe text";
}

// Phase 3 increment 3.1's probes (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16): C strings and plain memory, taken,
// returned and filled. Whatever this library allocates is released by go2cs_probe_release, never by the caller's own
// free: on windows the two may be different C runtimes, each with its own heap.

// A copy of text in memory this library allocates, or NULL for NULL.
GO2CS_PROBE_EXPORT char *go2cs_probe_text_copy(const char *text)
{
    if (!text)
        return 0;

    size_t length = 0;

    while (text[length])
        length++;

    char *copy = malloc(length + 1);

    if (!copy)
        return 0;

    for (size_t i = 0; i <= length; i++)
        copy[i] = text[i];

    return copy;
}

// Writes as much of the probe text as size holds, with its NUL, into the caller's buffer, and returns its length.
GO2CS_PROBE_EXPORT int go2cs_probe_text_fill(char *buffer, int size)
{
    const char *text = go2cs_probe_text();
    int length = 0;

    if (!buffer || size <= 0)
        return 0;

    while (length < size - 1 && text[length])
    {
        buffer[length] = text[length];
        length++;
    }

    buffer[length] = 0;
    return length;
}

// The sum of count bytes, each as an unsigned value: what C read behind a void pointer.
GO2CS_PROBE_EXPORT long go2cs_probe_sum(const void *bytes, int count)
{
    const unsigned char *p = bytes;
    long sum = 0;

    for (int i = 0; p && i < count; i++)
        sum += p[i];

    return sum;
}

// The pointer it was handed, so the caller sees what crossed.
GO2CS_PROBE_EXPORT void *go2cs_probe_same(void *pointer)
{
    return pointer;
}

// An empty call log of 32 bytes, zeroed.
GO2CS_PROBE_EXPORT char *go2cs_probe_log_new(void)
{
    return calloc(32, 1);
}

// Appends one digit to a call log: the order the calls ran in, and the value each was handed.
GO2CS_PROBE_EXPORT void go2cs_probe_log_put(char *log, int value)
{
    int length = 0;

    if (!log)
        return;

    while (log[length])
        length++;

    if (length < 31)
        log[length] = (char)('0' + value % 10);
}

// Releases memory this library allocated.
GO2CS_PROBE_EXPORT void go2cs_probe_release(void *pointer)
{
    free(pointer);
}

// cgo Phase 3 increment 3.2's first reds (docs/phase4/DESIGN-cgo-phase3-marshaling.md section 6): C memory that Go
// reads. A node holds a pointer field, as addrinfo does; the address is a struct of scalars, as sockaddr_in is.
struct go2cs_probe_entry
{
    int family;
    char *name;
    struct go2cs_probe_entry *next;
};

struct go2cs_probe_address
{
    unsigned short family;
    unsigned short port;
    unsigned int addr;
};

// A two-node list in C memory: (2, "a") then (10, "b").
GO2CS_PROBE_EXPORT struct go2cs_probe_entry *go2cs_probe_list(void)
{
    struct go2cs_probe_entry *second = calloc(1, sizeof *second);
    struct go2cs_probe_entry *first = calloc(1, sizeof *first);
    char *a = calloc(2, 1);
    char *b = calloc(2, 1);

    a[0] = 'a';
    b[0] = 'b';
    second->family = 10;
    second->name = b;
    first->family = 2;
    first->name = a;
    first->next = second;

    return first;
}

// Releases a list go2cs_probe_list made.
GO2CS_PROBE_EXPORT void go2cs_probe_list_release(struct go2cs_probe_entry *node)
{
    while (node)
    {
        struct go2cs_probe_entry *next = node->next;
        free(node->name);
        free(node);
        node = next;
    }
}

// The family of the node after node, read in C through node->next: -1 when there is none. Increment 3.2's store arms
// (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9, D-4) read back what Go stored into the field.
GO2CS_PROBE_EXPORT int go2cs_probe_next_family(struct go2cs_probe_entry *node)
{
    return node->next ? node->next->family : -1;
}

// An address in C memory: family 2, port 8080, addr 0x7f000001. Released by go2cs_probe_release.
GO2CS_PROBE_EXPORT void *go2cs_probe_address(void)
{
    struct go2cs_probe_address *address = calloc(1, sizeof *address);

    address->family = 2;
    address->port = 8080;
    address->addr = 0x7f000001u;

    return address;
}

// Increment 3.2 seat 1b's pressure arm (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18 item 4, 18.1 COORD (b)): fills
// size bytes one at a time, pausing between them, so that a compacting collection on another thread runs while C writes
// Go memory the wrapper holds pinned. Byte i is 'a' + i % 26. Returns size.
#if defined(_WIN32)
__declspec(dllimport) void __stdcall Sleep(unsigned long milliseconds);

static void go2cs_probe_pause(int micros)
{
    Sleep((unsigned long)(micros >= 1000 ? micros / 1000 : 1));
}
#else
#include <time.h>

static void go2cs_probe_pause(int micros)
{
    struct timespec pause = { micros / 1000000, (long)(micros % 1000000) * 1000L };
    nanosleep(&pause, NULL);
}
#endif

GO2CS_PROBE_EXPORT int go2cs_probe_slow_fill(char *buffer, int size, int micros)
{
    for (int i = 0; i < size; i++)
    {
        buffer[i] = (char)('a' + i % 26);
        go2cs_probe_pause(micros);
    }

    return size;
}
