// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class reflect_package {

// VisibleFields returns all the visible fields in t, which must be a
// struct type. A field is defined as visible if it's accessible
// directly with a FieldByName call. The returned fields include fields
// inside anonymous struct members and unexported fields. They follow
// the same order found in the struct, with anonymous fields followed
// immediately by their promoted fields.
//
// For each element e of the returned slice, the corresponding field
// can be retrieved from a value v of type t by calling v.FieldByIndex(e.Index).
public static slice<StructField> VisibleFields(ΔType t) {
    if (t == default!) {
        throw panic("reflect: VisibleFields(nil)");
    }
    if (t.Kind() != Struct) {
        throw panic("reflect.VisibleFields of non-struct type");
    }
    var w = Ꮡ(new visibleFieldsWalker(
        byName: new map<@string, nint>(),
        visiting: new map<ΔType, bool>(),
        fields: new slice<StructField>(0, t.NumField()),
        index: new slice<nint>(0, 2)
    ));
    w.walk(t);
    // Remove all the fields that have been hidden.
    // Use an in-place removal that avoids copying in
    // the common case that there are no hidden fields.
    nint j = 0;
    foreach (var (i, _) in (~w).fields) {
        var f = Ꮡ((~w).fields, i);
        if ((~f).Name == ""u8) {
            continue;
        }
        if (i != j) {
            // A field has been removed. We need to shuffle
            // all the subsequent elements up.
            w.Value.fields[j] = f.Value;
        }
        j++;
    }
    return (~w).fields.slice(0, j);
}

[GoType] partial struct visibleFieldsWalker {
    internal map<@string, nint> byName;
    internal map<ΔType, bool> visiting;
    internal slice<StructField> fields;
    internal slice<nint> index;
}

// walk walks all the fields in the struct type t, visiting
// fields in index preorder and appending them to w.fields
// (this maintains the required ordering).
// Fields that have been overridden have their
// Name field cleared.
[GoRecv] internal static void walk(this ref visibleFieldsWalker w, ΔType t) {
    if (w.visiting[t]) {
        return;
    }
    w.visiting[t] = true;
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        w.index = builtin.append(w.index, i);
        var add = true;
        {
            var (oldIndex, ok) = w.byName[f.Name, ꟷ]; if (ok) {
                var old = Ꮡ(w.fields, oldIndex);
                if (len(w.index) == len((~old).Index)){
                    // Fields with the same name at the same depth
                    // cancel one another out. Set the field name
                    // to empty to signify that has happened, and
                    // there's no need to add this field.
                    old.Value.Name = ""u8;
                    add = false;
                } else 
                if (len(w.index) < len((~old).Index)){
                    // The old field loses because it's deeper than the new one.
                    old.Value.Name = ""u8;
                } else {
                    // The old field wins because it's shallower than the new one.
                    add = false;
                }
            }
        }
        if (add) {
            // Copy the index so that it's not overwritten
            // by the other appends.
            f.Index = builtin.appendꓸꓸꓸ(slice<nint>(default!), w.index);
            w.byName[f.Name] = len(w.fields);
            w.fields = builtin.append(w.fields, f);
        }
        if (f.Anonymous) {
            if (f.Type.Kind() == ΔPointer) {
                f.Type = f.Type.Elem();
            }
            if (f.Type.Kind() == Struct) {
                w.walk(f.Type);
            }
        }
        w.index = w.index.slice(0, len(w.index) - 1);
    }
    delete(w.visiting, t);
}

} // end reflect_package
