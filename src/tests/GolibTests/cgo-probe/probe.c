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
