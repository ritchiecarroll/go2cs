// SPDX-License-Identifier: MIT
// Copyright (c) 2021-2026 The go2cs Authors

namespace go.github.com.ritchiecarroll;

partial class hashset_package {

partial struct HashSet<T> /*map[T, EmptyStruct]*/;

public static HashSet<T> NewHashSet<T>(slice<T> items) {
    var hs = new HashSet<T>(len(items));
    hs.UnionWith(items);
    return hs;
}

public static bool Add<T>(this HashSet<T> hs, T item) {
    if (hs.Contains(item)) {
        return false;
    }
    hs[item] = new EmptyStruct();
    return true;
}

public static bool Remove<T>(this HashSet<T> hs, T item) {
    if (hs.Contains(item)) {
        delete(hs, item);
        return true;
    }
    return false;
}

public static nint RemoveWhere<T>(this HashSet<T> hs, Func<T, bool> predicate) {
    nint removedCount = default!;
    foreach (var (k, _) in hs) {
        if (predicate(k) && hs.Remove(k)) {
            removedCount++;
        }
    }
    return removedCount;
}

public static bool IsEmpty<T>(this HashSet<T> hs) {
    return len(hs) == 0;
}

public static void Clear<T>(this HashSet<T> hs) {
    clear(hs);
}

public static bool Contains<T>(this HashSet<T> hs, T item) {
    var (_, ok) = hs[item, ꟷ];
    return ok;
}

public static slice<T> Keys<T>(this HashSet<T> hs) {
    var keys = new slice<T>(len(hs));
    nint i = 0;
    foreach (var (k, _) in hs) {
        keys[i] = k;
        i++;
    }
    return keys;
}

public static void ExceptWith<T>(this HashSet<T> hs, slice<T> other) {
    foreach (var (_, v) in other) {
        delete(hs, v);
    }
}

public static void ExceptWithSet<T>(this HashSet<T> hs, HashSet<T> other) {
    hs.ExceptWith(other.Keys());
}

public static void SymmetricExceptWith<T>(this HashSet<T> hs, slice<T> other) {
    hs.SymmetricExceptWithSet(NewHashSet<T>(other));
}

public static void SymmetricExceptWithSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (hs.IsEmpty()) {
        hs.UnionWithSet(other);
        return;
    }
    foreach (var (_, v) in other.Keys()) {
        if (!hs.Remove(v)) {
            hs.Add(v);
        }
    }
}

public static void IntersectWith<T>(this HashSet<T> hs, slice<T> other) {
    hs.IntersectWithSet(NewHashSet<T>(other));
}

public static void IntersectWithSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (len(hs) == 0) {
        return;
    }
    if (len(other) == 0) {
        hs.Clear();
        return;
    }
    foreach (var (k, _) in hs) {
        if (!other.Contains(k)) {
            hs.Remove(k);
        }
    }
}

public static void UnionWith<T>(this HashSet<T> hs, slice<T> other) {
    foreach (var (_, v) in other) {
        hs.Add(v);
    }
}

public static void UnionWithSet<T>(this HashSet<T> hs, HashSet<T> other) {
    hs.UnionWith(other.Keys());
}

public static bool SetEquals<T>(this HashSet<T> hs, slice<T> other) {
    return hs.SetEqualsSet(NewHashSet<T>(other));
}

public static bool SetEqualsSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (len(hs) != len(other)) {
        return false;
    }
    foreach (var (k, _) in other) {
        if (!hs.Contains(k)) {
            return false;
        }
    }
    return true;
}

public static bool Overlaps<T>(this HashSet<T> hs, slice<T> other) {
    if (hs.IsEmpty()) {
        return false;
    }
    foreach (var (_, v) in other) {
        if (hs.Contains(v)) {
            return true;
        }
    }
    return false;
}

public static bool OverlapsSet<T>(this HashSet<T> hs, HashSet<T> other) {
    return hs.Overlaps(other.Keys());
}

public static bool IsSubsetOf<T>(this HashSet<T> hs, slice<T> other) {
    return hs.IsSubsetOfSet(NewHashSet<T>(other));
}

public static bool IsSubsetOfSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (hs.IsEmpty()) {
        return true;
    }
    if (len(hs) > len(other)) {
        return false;
    }
    foreach (var (k, _) in hs) {
        if (!other.Contains(k)) {
            return false;
        }
    }
    return true;
}

public static bool IsProperSubsetOf<T>(this HashSet<T> hs, slice<T> other) {
    return hs.IsProperSubsetOfSet(NewHashSet<T>(other));
}

public static bool IsProperSubsetOfSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (hs.IsEmpty()) {
        return len(other) > 0;
    }
    if (len(hs) >= len(other)) {
        return false;
    }
    foreach (var (k, _) in hs) {
        if (!other.Contains(k)) {
            return false;
        }
    }
    return true;
}

public static bool IsSupersetOf<T>(this HashSet<T> hs, slice<T> other) {
    return hs.IsSupersetOfSet(NewHashSet<T>(other));
}

public static bool IsSupersetOfSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (len(other) == 0) {
        return true;
    }
    if (len(other) > len(hs)) {
        return false;
    }
    foreach (var (k, _) in other) {
        if (!hs.Contains(k)) {
            return false;
        }
    }
    return true;
}

public static bool IsProperSupersetOf<T>(this HashSet<T> hs, slice<T> other) {
    return hs.IsProperSupersetOfSet(NewHashSet<T>(other));
}

public static bool IsProperSupersetOfSet<T>(this HashSet<T> hs, HashSet<T> other) {
    if (hs.IsEmpty()) {
        return false;
    }
    if (len(other) == 0) {
        return true;
    }
    if (len(other) >= len(hs)) {
        return false;
    }
    foreach (var (k, _) in other) {
        if (!hs.Contains(k)) {
            return false;
        }
    }
    return true;
}

} // end hashset_package
