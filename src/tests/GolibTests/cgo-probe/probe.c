// probe.c - go2cs's cgo probe library (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.10, increment 4d).
//
// The exit-gate arms no system library gives on every OS, in one small library. Every hosted golib-tests leg builds it
// from this source with the OS's default pinned C toolchain (PLAN-cgo-interop §5.1: zig on linux and darwin,
// llvm-mingw on windows) into GO2CS_CGO_PROBE_DIR. No binary of it is committed. GolibTests binds it through the
// generated wrappers of GoCgoWrappers.<os>.golden.cs (src/go2cs's cgoProbeDecls declares it as cmd/cgo would), and
// CgoProbeTests calls it.

#include <stdint.h>
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

// The word it was handed, so the caller sees what crossed (design 20.4 S4: a uintptr_t parameter).
GO2CS_PROBE_EXPORT uintptr_t go2cs_probe_word(uintptr_t word)
{
    return word;
}

// The go-sqlite3 marshalling slice (DESIGN-cgo-sqlite-marshalling.md): go-sqlite3's sqlite3_go_col shape, a record with
// a void * member. go2cs_probe_col_fill writes ncol records, each ptr a C string C owns; go2cs_probe_col_len reads one
// record Go filled. go2cs_probe_fname returns a typedef of const char *, as sqlite3_db_filename does.
struct go2cs_probe_col
{
    int typ;
    long long i64;
    double f64;
    const void *ptr;
    int n;
};

typedef const char *go2cs_probe_fname_t;

GO2CS_PROBE_EXPORT void go2cs_probe_col_fill(struct go2cs_probe_col *cols, int ncol)
{
    static const char *words[] = { "alpha", "beta", "gamma" };
    static const int lengths[] = { 5, 4, 5 };

    for (int i = 0; i < ncol; i++)
    {
        cols[i].typ = i + 1;
        cols[i].i64 = 10 * (i + 1);
        cols[i].f64 = 0.5 * (i + 1);
        cols[i].ptr = words[i % 3];
        cols[i].n = lengths[i % 3];
    }
}

GO2CS_PROBE_EXPORT int go2cs_probe_col_len(const struct go2cs_probe_col *col)
{
    return col->n;
}

GO2CS_PROBE_EXPORT go2cs_probe_fname_t go2cs_probe_fname(void)
{
    return "main.db";
}

// The near misses of the S4 cure (P3's read, F1): a parameter cmd/cgo declares at a C typedef, beside a void pointer. A
// void pointer is what makes cmd/cgo write its check closure, which declares the constant at that typedef; a typed
// pointer to a C integer needs no check, takes no closure, and so never reaches the cast. uint64_t is a typedef of a
// typedef on glibc (over __uint64_t); go2cs_probe_color is a typedef'd enum.
typedef enum
{
    go2cs_probe_red = 1,
    go2cs_probe_green = 2,
    go2cs_probe_blue = 4
} go2cs_probe_color;

GO2CS_PROBE_EXPORT uint64_t go2cs_probe_wide_into(void *dst, uint64_t value)
{
    *(uint64_t *)dst = value;
    return value >> 32;
}

GO2CS_PROBE_EXPORT int go2cs_probe_color_into(void *dst, go2cs_probe_color color)
{
    *(int *)dst = (int)color * 10;
    return (int)color;
}

// A pointer C KEEPS past the call, and a later call that reads through it (cgo S6, design 21: legal in Go only when a
// runtime.Pinner pins the memory).
static void *go2cs_probe_kept_pointer;

GO2CS_PROBE_EXPORT void go2cs_probe_keep(void *pointer)
{
    go2cs_probe_kept_pointer = pointer;
}

GO2CS_PROBE_EXPORT int go2cs_probe_kept(void)
{
    return go2cs_probe_kept_pointer ? *(int *)go2cs_probe_kept_pointer : -1;
}

// The same, for a C STRUCT passed to a typed parameter (S6, P3's MUST 2): C keeps the struct pointer, a later call reads
// its second member. go2cs_probe_pair is the struct declared at the top of this file.
static struct go2cs_probe_pair *go2cs_probe_kept_pair_pointer;

GO2CS_PROBE_EXPORT void go2cs_probe_keep_pair(struct go2cs_probe_pair *pair)
{
    go2cs_probe_kept_pair_pointer = pair;
}

GO2CS_PROBE_EXPORT int go2cs_probe_kept_pair_b(void)
{
    return go2cs_probe_kept_pair_pointer ? go2cs_probe_kept_pair_pointer->b : -1;
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

// cgo Phase 3, increment 3.2 seat 2 (H's sizing item 1): words C RETURNS to Go through a stack copy the wrapper made.
// Each stores a pointer where Go reads it back: a member of a struct (a parser's cursor), an out-parameter's word, and a
// node's own next (a one-node ring, as a list's init leaves it).
struct go2cs_probe_cursor
{
    int n;
    char *cur;
};

// Item 1(a): a pointer MEMBER C writes into a struct it was handed. cursor->cur = buffer + k; returns k.
GO2CS_PROBE_EXPORT int go2cs_probe_cursor_set(struct go2cs_probe_cursor *cursor, char *buffer, int k)
{
    cursor->n = k;
    cursor->cur = buffer + k;
    return k;
}

// Item 1(a), the result's side: the same cursor, returned BY VALUE.
GO2CS_PROBE_EXPORT struct go2cs_probe_cursor go2cs_probe_cursor_at(char *buffer, int k)
{
    struct go2cs_probe_cursor cursor = { k, buffer + k };
    return cursor;
}

// Item 1(b): an out-parameter's word that is the pointer C was handed. *out = a; returns *a.
GO2CS_PROBE_EXPORT int go2cs_probe_pick(int *a, int **out)
{
    *out = a;
    return *a;
}

// Item 1(b), interior: the address of the second int of the pair C was handed. *out = &pair->b; returns pair->b.
GO2CS_PROBE_EXPORT int go2cs_probe_pick_b(struct go2cs_probe_pair *pair, int **out)
{
    *out = &pair->b;
    return pair->b;
}

// A member that is the struct's own address: node->next = node, as a ring's init leaves it.
GO2CS_PROBE_EXPORT void go2cs_probe_node_ring(struct go2cs_probe_node *node)
{
    node->next = node;
}

// Item 1(b)'s near-miss (H, 2026-11-24): the same address as an argument C was handed, through another pointee type.
// *out = (char *)a; returns *a.
GO2CS_PROBE_EXPORT int go2cs_probe_pick_as_char(int *a, char **out)
{
    *out = (char *)a;
    return *a;
}

// Item 2, the argv shape (.5 cgo, DESIGN-cgo-phase3-marshaling 18.12.3; H's verdict (A), the word mirror): a NULL-ended
// array of C strings. Returns count * 100 + the strings' total length, so a dropped or swapped word reads differently.
GO2CS_PROBE_EXPORT int go2cs_probe_argv_count(char **argv)
{
    int count = 0;
    int length = 0;

    for (; argv[count] != NULL; count++)
        for (const char *c = argv[count]; *c != 0; c++)
            length++;

    return count * 100 + length;
}

// The same through a void pointer: the void door's argv.
GO2CS_PROBE_EXPORT int go2cs_probe_argv_count_void(void *argv)
{
    return go2cs_probe_argv_count((char **)argv);
}

// C fills the first n words with C strings it owns, and leaves the rest alone.
GO2CS_PROBE_EXPORT int go2cs_probe_argv_fill(char **argv, int n)
{
    static char *const words[] = { "c0", "c11", "c222", "c3333" };

    for (int i = 0; i < n && i < 4; i++)
        argv[i] = words[i];

    return n;
}

// H's point 2: C stores the address of argv[2] into argv[0], a pointer into the array it was handed.
GO2CS_PROBE_EXPORT void go2cs_probe_argv_self(char **argv)
{
    argv[0] = (char *)&argv[2];
}

// C global variables (cgo item 3, C.x): an int, a struct with padding between its fields, and an errno-style int a
// call sets. Go reads and writes each in place; so does go2cs, through a box over the symbol's exported address.
struct go2cs_probe_gauge
{
    int count;
    long long total;
};

GO2CS_PROBE_EXPORT int go2cs_probe_counter = 3;
GO2CS_PROBE_EXPORT struct go2cs_probe_gauge go2cs_probe_gauge_now = { 1, 100 };
GO2CS_PROBE_EXPORT int go2cs_probe_last_error;

// Adds step to the counter and folds it into the gauge, as C code that owns these globals would. A negative step is
// refused: the counter is left as it was and go2cs_probe_last_error reads 22 (EINVAL's value on linux), as errno would.
GO2CS_PROBE_EXPORT int go2cs_probe_bump(int step)
{
    if (step < 0)
    {
        go2cs_probe_last_error = 22;
        return -1;
    }

    go2cs_probe_last_error = 0;
    go2cs_probe_counter += step;
    go2cs_probe_gauge_now.count++;
    go2cs_probe_gauge_now.total += go2cs_probe_counter;

    return go2cs_probe_counter;
}

// Door 1 (docs/phase4/DESIGN-cgo-struct-array-element.md): C handed a pointer to an element of a Go array of records
// may reach the whole array. go2cs_probe_col_back reads the record BEFORE the one it is handed; go2cs_probe_col_at
// returns the record k past it; go2cs_probe_col_poke_prev writes a scalar of the record before it and fills the record
// it is handed; go2cs_probe_col_slow pauses, then writes only the record it is handed, so another goroutine can write a
// different record of the same array during the call.
GO2CS_PROBE_EXPORT long long go2cs_probe_col_back(const struct go2cs_probe_col *col)
{
    return col[-1].i64;
}

GO2CS_PROBE_EXPORT struct go2cs_probe_col *go2cs_probe_col_at(struct go2cs_probe_col *col, int k)
{
    return col + k;
}

GO2CS_PROBE_EXPORT void go2cs_probe_col_poke_prev(struct go2cs_probe_col *col)
{
    col[-1].n = 7;
    go2cs_probe_col_fill(col, 1);
}

GO2CS_PROBE_EXPORT void go2cs_probe_col_slow(struct go2cs_probe_col *col, int micros)
{
    go2cs_probe_pause(micros);
    col->n = 5;
}
