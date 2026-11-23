# Clash fixtures: package-main projects whose methods take the ⁔ form

Converter master ccca03abea vs claude/c1-glyph-methods 149dab1f70; unified diff of every differing file (GoPositionMap records omitted).

## ClearBuiltinShadow

```diff
--- master/ClearBuiltinShadow/main.cs
+++ g1/ClearBuiltinShadow/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -16,25 +15,19 @@
     internal slice<nint> data;
 }
 
-} partial class mainᴛbox {
-
-internal static void clear(this ref box b) { b.data = default; }
-
-} partial class main_package {
+internal static void clear⁔(/*this*/ ref box b) { b.data = default; }
 
 partial struct guard {
     internal error err;
 }
 
-} partial class mainᴛguard {
-
-internal static void recover(this ж<guard> Ꮡg) {
+internal static void recover⁔(/*this*/ ж<guard> Ꮡg) {
     GoFrame ᒐ = default;
     try {
         var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?
         ref var g = ref Ꮡg.DerefOrNull();
 
-        var e = builtin.recover(calledByDefer); if (e != nil) {
+        var e = recover(calledByDefer); if (e != nil) {
             g.err = fmt.Errorf("recovered: %v"u8, e);
         }
     }
@@ -42,7 +35,7 @@
     finally { ᒐ.Run(); }
 }
 
-internal static void run(this ж<guard> Ꮡg) {
+internal static void run⁔(/*this*/ ж<guard> Ꮡg) {
     GoFrame ᒐ = default;
     try {
         defer(Ꮡg.recover, ref ᒐ, recovers: true);
@@ -52,8 +45,6 @@
     finally { ᒐ.Run(); }
 }
 
-} partial class main_package {
-
 internal static void main() {
     var s = slice<nint>([1, 2, 3]);
     clear(s);
```

```diff
--- master/ClearBuiltinShadow/package_info.cs
+++ g1/ClearBuiltinShadow/package_info.cs
@@ -96,3 +96,17 @@
 [GoMethodHost(typeof(main_package.box)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbox;
 [GoMethodHost(typeof(main_package.guard)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛguard;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbox {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ref box b) => main_package.clear⁔(ref b);
+}
+partial class mainᴛguard {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void recover(this ж<guard> Ꮡg) => main_package.recover⁔(Ꮡg);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void run(this ж<guard> Ꮡg) => main_package.run⁔(Ꮡg);
+}
+// </MethodForwarders>
```

## CollisionFieldBoxAccessor

```diff
--- master/CollisionFieldBoxAccessor/main.cs
+++ g1/CollisionFieldBoxAccessor/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,11 +11,7 @@
 partial struct tagger {
 }
 
-} partial class mainᴛtagger {
-
-internal static void mark(this tagger _) { }
-
-} partial class main_package {
+internal static void mark⁔(/*this*/ tagger _) { }
 
 partial struct holder {
     internal nint mark;
```

```diff
--- master/CollisionFieldBoxAccessor/package_info.cs
+++ g1/CollisionFieldBoxAccessor/package_info.cs
@@ -96,3 +96,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.tagger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagger {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void mark(this tagger _) => main_package.mark⁔(_);
+}
+// </MethodForwarders>
```

## CollisionRenamedGlobalReceiver

```diff
--- master/CollisionRenamedGlobalReceiver/main.cs
+++ g1/CollisionRenamedGlobalReceiver/main.cs
@@ -5,7 +5,6 @@
 using lib = CollisionRenamedGlobalReceiver.lib_package;
 using CollisionRenamedGlobalReceiver;
 using sync;
-using static main_package;
 
 partial class main_package {
 
@@ -38,13 +37,9 @@
     internal int32 id;
 }
 
-} partial class mainᴛTLS {
+internal static int32 tid⁔(/*this*/ ref TLS tls) { return tls.id; }
 
-internal static int32 tid(this ref TLS tls) { return tls.id; }
-
-internal static nint count(this ref TLS tls) { return (nint)tls.id * 10; }
-
-} partial class main_package {
+internal static nint count⁔(/*this*/ ref TLS tls) { return (nint)tls.id * 10; }
 
 internal static void main() {
     var first = Ꮡtid.Add(1);
```

```diff
--- master/CollisionRenamedGlobalReceiver/package_info.cs
+++ g1/CollisionRenamedGlobalReceiver/package_info.cs
@@ -90,3 +90,14 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.TLS)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛTLS;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛTLS {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int32 tid(this ref TLS tls) => main_package.tid⁔(ref tls);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint count(this ref TLS tls) => main_package.count⁔(ref tls);
+}
+// </MethodForwarders>
```

## CollisionRenamedLocalBox

```diff
--- master/CollisionRenamedLocalBox/main.cs
+++ g1/CollisionRenamedLocalBox/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,11 +11,7 @@
 partial struct tagger {
 }
 
-} partial class mainᴛtagger {
-
-internal static void p(this tagger _) { }
-
-} partial class main_package {
+internal static void p⁔(/*this*/ tagger _) { }
 
 partial struct box {
     internal nint n;
@@ -42,11 +37,7 @@
 internal static readonly ж<box> Ꮡcounter = new StaticBox<box>(default(box));
 internal static ref box counter => ref ((StaticBox<box>)Ꮡcounter).ValueSlot;
 
-} partial class mainᴛtagger {
-
-internal static nint counter(this tagger _) { return 1; }
-
-} partial class main_package {
+internal static nint counter⁔(/*this*/ tagger _) { return 1; }
 
 internal static nint usesTypeP() {
     p pv = default;
```

```diff
--- master/CollisionRenamedLocalBox/package_info.cs
+++ g1/CollisionRenamedLocalBox/package_info.cs
@@ -93,3 +93,14 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.tagger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagger {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void p(this tagger _) => main_package.p⁔(_);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint counter(this tagger _) => main_package.counter⁔(_);
+}
+// </MethodForwarders>
```

## CrossPkgUser

```diff
--- master/CrossPkgUser/main.cs
+++ g1/CrossPkgUser/main.cs
@@ -5,7 +5,6 @@
 
 using fmt = fmt_package;
 using CrossPkgLib = CrossPkgLib_package;
-using static main_package;
 
 partial class main_package {
 
@@ -372,11 +371,7 @@
     nint Bump();
 }
 
-} partial class mainᴛtagged {
-
-internal static @string Meter(this tagged t) { return "tagged-meter"; }
-
-} partial class main_package {
+internal static @string Meter⁔(/*this*/ tagged t) { return "tagged-meter"; }
 
 partial struct counter {
     /*embed*/ public ж<CrossPkgLib_package.Meter> Meter;
```

```diff
--- master/CrossPkgUser/package_info.cs
+++ g1/CrossPkgUser/package_info.cs
@@ -140,3 +140,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.tagged)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagged;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagged {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Meter(this tagged t) => main_package.Meter⁔(t);
+}
+// </MethodForwarders>
```

## DeferTypelessReturns

```diff
--- master/DeferTypelessReturns/DeferTypelessReturns.cs
+++ g1/DeferTypelessReturns/DeferTypelessReturns.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -88,11 +87,9 @@
     internal nint id;
 }
 
-} partial class mainᴛres {
-
-internal static error close(this res h) {
+internal static error close⁔(/*this*/ res h) {
     fmt.Println("closed", h.id);
     return default;
 }
 
-}
+} // end main_package
```

```diff
--- master/DeferTypelessReturns/package_info.cs
+++ g1/DeferTypelessReturns/package_info.cs
@@ -89,3 +89,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.res)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛres;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛres {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static error close(this res h) => main_package.close⁔(h);
+}
+// </MethodForwarders>
```

## DotImportRenamedType

```diff
--- master/DotImportRenamedType/main.cs
+++ g1/DotImportRenamedType/main.cs
@@ -4,7 +4,6 @@
 using static DotImportRenamedType.renamedlib_package;
 using DotImportRenamedType;
 using renamedlib = DotImportRenamedType.renamedlib_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,14 +11,10 @@
     public @string Tag;
 }
 
-} partial class mainᴛLocal {
-
-public static @string Local(this Local l) {
+internal static @string Local⁔(/*this*/ Local l) {
     return "L:"u8 + l.Tag;
 }
 
-} partial class main_package {
-
 internal static void main() {
     var m = new Marker(Name: "alpha", Size: 3);
     fmt.Println("literal:", m.Marker());
```

```diff
--- master/DotImportRenamedType/package_info.cs
+++ g1/DotImportRenamedType/package_info.cs
@@ -88,3 +88,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.Local)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛLocal;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛLocal {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Local(this Local l) => main_package.Local⁔(l);
+}
+// </MethodForwarders>
```

## EmbeddedValuePointerMethod

```diff
--- master/EmbeddedValuePointerMethod/main.cs
+++ g1/EmbeddedValuePointerMethod/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -22,12 +21,8 @@
 
 partial struct flags /*num:uint8*/;
 
-} partial class mainᴛflags {
-
-internal static void set(this ref flags f, flags bit) { f |= (flags)(bit); }
-internal static void clear(this ref flags f, flags bit) { f &= unchecked((flags)~(flags)(bit)); }
-
-} partial class main_package {
+internal static void set⁔(/*this*/ ref flags f, flags bit) { f |= (flags)(bit); }
+internal static void clear⁔(/*this*/ ref flags f, flags bit) { f &= unchecked((flags)~(flags)(bit)); }
 
 partial struct chunk {
     internal uint16 inUse;
```

```diff
--- master/EmbeddedValuePointerMethod/package_info.cs
+++ g1/EmbeddedValuePointerMethod/package_info.cs
@@ -94,3 +94,14 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.flags)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛflags;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛflags {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set(this ref flags f, flags bit) => main_package.set⁔(ref f, bit);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void clear(this ref flags f, flags bit) => main_package.clear⁔(ref f, bit);
+}
+// </MethodForwarders>
```

## FieldNameTypeMethodCollision

```diff
--- master/FieldNameTypeMethodCollision/FieldNameTypeMethodCollision.cs
+++ g1/FieldNameTypeMethodCollision/FieldNameTypeMethodCollision.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,14 +9,10 @@
     internal @string label;
 }
 
-} partial class mainᴛevent {
-
-internal static Label Label(this @event e) {
+internal static Label Label⁔(/*this*/ @event e) {
     return new Label(ΔLabel: e.label, Resource: e.kind);
 }
 
-} partial class main_package {
-
 partial struct Label {
     public @string ΔLabel;
     public nint Resource;
```

```diff
--- master/FieldNameTypeMethodCollision/package_info.cs
+++ g1/FieldNameTypeMethodCollision/package_info.cs
@@ -88,3 +88,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.@event)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛevent;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛevent {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Label Label(this @event e) => main_package.Label⁔(e);
+}
+// </MethodForwarders>
```

## FuncMethodSameNameCollision

```diff
--- master/FuncMethodSameNameCollision/main.cs
+++ g1/FuncMethodSameNameCollision/main.cs
@@ -3,7 +3,6 @@
 using fmt = fmt_package;
 using shapes = FuncMethodSameNameCollision.shapes_package;
 using FuncMethodSameNameCollision;
-using static main_package;
 
 partial class main_package {
 
@@ -11,11 +10,7 @@
     public nint Depth;
 }
 
-} partial class mainᴛNode {
-
-public static bool Deep(this ref Node n) { return n.Depth > 2; }
-
-} partial class main_package {
+internal static bool Deep⁔(/*this*/ ref Node n) { return n.Depth > 2; }
 
 public static bool Deep(ж<Node> Ꮡn) {
     ref var n = ref Ꮡn.DerefOrNull();
```

```diff
--- master/FuncMethodSameNameCollision/package_info.cs
+++ g1/FuncMethodSameNameCollision/package_info.cs
@@ -87,3 +87,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.Node)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛNode;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛNode {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Deep(this ref Node n) => main_package.Deep⁔(ref n);
+}
+// </MethodForwarders>
```

## FuncValueMethodClash

```diff
--- master/FuncValueMethodClash/main.cs
+++ g1/FuncValueMethodClash/main.cs
@@ -3,18 +3,13 @@
 using fmt = fmt_package;
 using reflect = reflect_package;
 using time = time_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct T {
 }
 
-} partial class mainᴛT {
-
-public static @string Run(this T _) { return "method"; }
-
-} partial class main_package {
+internal static @string Run⁔(/*this*/ T _) { return "method"; }
 
 public static @string Run() { return "func"; }
 
```

```diff
--- master/FuncValueMethodClash/package_info.cs
+++ g1/FuncValueMethodClash/package_info.cs
@@ -89,3 +89,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.T)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛT;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛT {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Run(this T _) => main_package.Run⁔(_);
+}
+// </MethodForwarders>
```

## FuncVsMethodOverload

```diff
--- master/FuncVsMethodOverload/main.cs
+++ g1/FuncVsMethodOverload/main.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -14,17 +13,13 @@
     internal uint32 a, b;
 }
 
-} partial class mainᴛnih {
-
-internal static ж<nih> add(this ж<nih> Ꮡp, uintptr bytes) {
+internal static ж<nih> add⁔(/*this*/ ж<nih> Ꮡp, uintptr bytes) {
     if (bytes == 0) {
         return Ꮡp;
     }
     return @unsafe.Pointer.To<nih>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡp), bytes));
 }
 
-} partial class main_package {
-
 internal static partial uint32 step(ж<uint32> v) {
     var q = @unsafe.Pointer.To<uint32>(add(@unsafe.Pointer.FromPinnedBox(v), /* unsafe.Sizeof(uint32(0)) */ (uintptr)4));
     return q.Value;
```

```diff
--- master/FuncVsMethodOverload/package_info.cs
+++ g1/FuncVsMethodOverload/package_info.cs
@@ -92,3 +92,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.nih)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛnih;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛnih {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<nih> add(this ж<nih> Ꮡp, uintptr bytes) => main_package.add⁔(Ꮡp, bytes);
+}
+// </MethodForwarders>
```

## GenericStructFields

```diff
--- master/GenericStructFields/GenericStructFields.cs
+++ g1/GenericStructFields/GenericStructFields.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -79,8 +78,6 @@
 partial struct keeper {
 }
 
-} partial class mainᴛkeeper {
+internal static @string pool⁔(/*this*/ keeper k) { return "kept"; }
 
-internal static @string pool(this keeper k) { return "kept"; }
-
-}
+} // end main_package
```

```diff
--- master/GenericStructFields/package_info.cs
+++ g1/GenericStructFields/package_info.cs
@@ -92,3 +92,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.keeper)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛkeeper;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛkeeper {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string pool(this keeper k) => main_package.pool⁔(k);
+}
+// </MethodForwarders>
```

## GlobalShadowedByLocal

```diff
--- master/GlobalShadowedByLocal/main.cs
+++ g1/GlobalShadowedByLocal/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -15,11 +14,7 @@
     internal nint n;
 }
 
-} partial class mainᴛacquirer {
-
-internal static nint trace(this acquirer _) { return 1; }
-
-} partial class main_package {
+internal static nint trace⁔(/*this*/ acquirer _) { return 1; }
 
 internal static partial nint collisionGlobalShadow() {
     nint a = main_package.trace.addr;
```

```diff
--- master/GlobalShadowedByLocal/package_info.cs
+++ g1/GlobalShadowedByLocal/package_info.cs
@@ -94,3 +94,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.acquirer)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛacquirer;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛacquirer {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint trace(this acquirer _) => main_package.trace⁔(_);
+}
+// </MethodForwarders>
```

## GoCallVariations

```diff
--- master/GoCallVariations/GoCallVariations.cs
+++ g1/GoCallVariations/GoCallVariations.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using time = time_package;
-using static main_package;
 
 partial class main_package {
 
@@ -45,15 +44,11 @@
     internal nint total;
 }
 
-} partial class mainᴛaccum {
-
-internal static nint add(this ref accum a, nint n) {
+internal static nint add⁔(/*this*/ ref accum a, nint n) {
     a.total += n;
     return a.total;
 }
 
-} partial class main_package {
-
 internal static void bindAdd(ж<accum> a) {
     var add = (nint p1) => a.add(p1);
     fmt.Println("bound add:", add(5), add(7));
```

```diff
--- master/GoCallVariations/package_info.cs
+++ g1/GoCallVariations/package_info.cs
@@ -92,3 +92,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.accum)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛaccum;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛaccum {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint add(this ref accum a, nint n) => main_package.add⁔(ref a, n);
+}
+// </MethodForwarders>
```

## InterfaceCasting

```diff
--- master/InterfaceCasting/InterfaceCasting.cs
+++ g1/InterfaceCasting/InterfaceCasting.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -216,12 +215,8 @@
     internal nint num;
 }
 
-} partial class mainᴛbadge {
-
-internal static @string @string(this ref badge b) { return b.text; }
-internal static nint @int(this ref badge b) { return b.num; }
-
-} partial class main_package {
+internal static @string string⁔(/*this*/ ref badge b) { return b.text; }
+internal static nint int⁔(/*this*/ ref badge b) { return b.num; }
 
 internal static void replaceAnimal(ref Animal a) {
     a = new CatжAnimal(Ꮡ(new Cat()));
@@ -260,12 +255,8 @@
     internal @string name;
 }
 
-} partial class mainᴛfileRdr {
-
-internal static @string read(this fileRdr f) { return "read:"u8 + f.name; }
-internal static @string close(this fileRdr f) { return "close:"u8 + f.name; }
-
-} partial class main_package {
+internal static @string read⁔(/*this*/ fileRdr f) { return "read:"u8 + f.name; }
+internal static @string close⁔(/*this*/ fileRdr f) { return "close:"u8 + f.name; }
 
 internal static rdCloser open(@string name) { return new fileRdr(name); }
 
```

```diff
--- master/InterfaceCasting/package_info.cs
+++ g1/InterfaceCasting/package_info.cs
@@ -132,3 +132,18 @@
 [GoMethodHost(typeof(main_package.badge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbadge;
 [GoMethodHost(typeof(main_package.fileRdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfileRdr;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbadge {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string @string(this ref badge b) => main_package.string⁔(ref b);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint @int(this ref badge b) => main_package.int⁔(ref b);
+}
+partial class mainᴛfileRdr {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string read(this fileRdr f) => main_package.read⁔(f);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string close(this fileRdr f) => main_package.close⁔(f);
+}
+// </MethodForwarders>
```

## IotaEnum

```diff
--- master/IotaEnum/IotaEnum.cs
+++ g1/IotaEnum/IotaEnum.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -50,17 +49,13 @@
 public static Kind Struct => 25;
 public static Kind UnsafePointer => 26;
 
-} partial class mainᴛKind {
-
-public static @string String(this Kind k) {
+internal static @string String⁔(/*this*/ Kind k) {
     if ((nint)k < len(kindNames)) {
         return kindNames[k];
     }
     return kindNames[0];
 }
 
-} partial class main_package {
-
 [GoLazy] partial class lazyᴛkindNames { internal static slice<@string> kindNames = new golib.SparseArray<@string>{
     [Invalid] = "invalid",
     [Bool] = "bool",
@@ -132,32 +127,28 @@
     return TypeOf((ж<T>)nil).Elem();
 }
 
-} partial class mainᴛType {
+internal static Kind Kind⁔(/*this*/ ref Type t) { return (Kind)(t.Kind_ & KindMask); }
 
-public static Kind Kind(this ref Type t) { return (Kind)(t.Kind_ & KindMask); }
-
-public static bool HasName(this ref Type t) {
+internal static bool HasName⁔(/*this*/ ref Type t) {
     return (TFlag)(t.TFlag & TFlagNamed) != 0;
 }
 
-public static ж<Type> Elem(this ж<Type> Ꮡt) {
+internal static ж<Type> Elem⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     switch (t.Kind()) {
     case var ᴛ when ᴛ == Array: {
         var tt = Ꮡt.Reinterpret<Type, ArrayType>();
-        return tt.of(main_package.ArrayType.ᏑElem).Value;
+        return tt.of(ArrayType.ᏑElem).Value;
     }
     case var ᴛ when ᴛ == Map: {
         var tt = Ꮡt.Reinterpret<Type, MapType>();
-        return tt.of(main_package.MapType.ᏑElem).Value;
+        return tt.of(MapType.ᏑElem).Value;
     }}
 
     return default;
 }
 
-} partial class main_package {
-
 partial struct MapType {
     /*embed*/ public Type Type;
     public ж<Type> Key;
@@ -170,9 +161,7 @@
     public uint32 Flags;
 }
 
-} partial class mainᴛType {
-
-public static ж<MapType> MapType(this ж<Type> Ꮡt) {
+internal static ж<MapType> MapType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Map) {
@@ -181,8 +170,6 @@
     return Ꮡt.Reinterpret<Type, MapType>();
 }
 
-} partial class main_package {
-
 partial struct ArrayType {
     /*embed*/ public Type Type;
     public ж<Type> Elem;
@@ -190,9 +177,7 @@
     public uintptr Len;
 }
 
-} partial class mainᴛType {
-
-public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
+internal static ж<ArrayType> ArrayType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Array) {
@@ -201,8 +186,6 @@
     return Ꮡt.Reinterpret<Type, ArrayType>();
 }
 
-} partial class main_package {
-
 internal const nint seqFirst = iota;
 internal const nint seqSecond = 1;
 internal const nint seqAgain = /* iota */ 2;
```

```diff
--- master/IotaEnum/package_info.cs
+++ g1/IotaEnum/package_info.cs
@@ -96,3 +96,20 @@
 [GoMethodHost(typeof(main_package.Kind)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛKind;
 [GoMethodHost(typeof(main_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛType;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛKind {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Kind k) => main_package.String⁔(k);
+}
+partial class mainᴛType {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Kind Kind(this ref Type t) => main_package.Kind⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HasName(this ref Type t) => main_package.HasName⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Type> Elem(this ж<Type> Ꮡt) => main_package.Elem⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<MapType> MapType(this ж<Type> Ꮡt) => main_package.MapType⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) => main_package.ArrayType⁔(Ꮡt);
+}
+// </MethodForwarders>
```

## MethodHolderAlias

```diff
--- master/MethodHolderAlias/main.cs
+++ g1/MethodHolderAlias/main.cs
@@ -10,7 +10,6 @@
 using MethodHolderAlias.html;
 using geom = MethodHolderAlias.geom_package;
 using plate = MethodHolderAlias.html.plate_package;
-using static main_package;
 
 partial class main_package {
 
@@ -18,11 +17,7 @@
     internal geom.Shape s;
 }
 
-} partial class mainᴛfield {
-
-internal static nint Area(this field f) { return MethodHolderAlias.geom_package.Area(f.s.W.Len, f.s.H.Len) + (nint)Unit; }
-
-} partial class main_package {
+internal static nint Area⁔(/*this*/ field f) { return Area(f.s.W.Len, f.s.H.Len) + (nint)Unit; }
 
 internal static void main() {
     ref var s = ref heap<geom.Shape>(new Shape(W: new part.Side(Len: 4), H: new part.Side(Len: 5)), out var Ꮡs);
```

```diff
--- master/MethodHolderAlias/package_info.cs
+++ g1/MethodHolderAlias/package_info.cs
@@ -87,3 +87,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.field)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfield;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛfield {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Area(this field f) => main_package.Area⁔(f);
+}
+// </MethodForwarders>
```

## MethodHolderNames

```diff
--- master/MethodHolderNames/forms.cs
+++ g1/MethodHolderNames/forms.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -20,13 +19,11 @@
     internal slice<Tag> tags;
 }
 
-} partial class mainᴛChest {
+internal static Tag Tag⁔(/*this*/ ref Chest c) { return c.tag; }
 
-public static Tag Tag(this ref Chest c) { return c.tag; }
+internal static ж<nint> Seal⁔(/*this*/ ж<Chest> Ꮡc) { return Ꮡc.Ꮡtag.Ꮡinner.Ꮡn; }
 
-public static ж<nint> Seal(this ж<Chest> Ꮡc) { return Ꮡc.Ꮡtag.Ꮡinner.Ꮡn; }
-
-public static @string Forms(this ж<Chest> Ꮡc, any v) {
+internal static @string Forms⁔(/*this*/ ж<Chest> Ꮡc, any v) {
     ref var c = ref Ꮡc.DerefOrNull();
 
     var lit = new Tag(id: 1, Value: 2);
@@ -41,7 +38,7 @@
     var fresh = @new<Tag>();
     fresh.Value.Value = 9;
 
-    var valueAddr = ptr.of(main_package.Tag.ᏑValue);
+    var valueAddr = ptr.of(Tag.ᏑValue);
     valueAddr.Value += 4;
 
     var idAddr = Ꮡc.Ꮡtag.Ꮡid;
@@ -53,7 +50,7 @@
     @string kind = "other";
 
     switch (v.type()) {
-    case Tag _: {
+    case Tag: {
         kind = "tag";
         break;
     }
@@ -68,8 +65,6 @@
     return fmt.Sprint(lit.id, (~ptr).Value, zero.id, count, (~fresh).Value, c.tag.id, isTag, isPtr, kind, Ꮡc.Seal().Value);
 }
 
-} partial class main_package {
-
 internal static T pick<T>(slice<T> s, nint i) { return s[i]; }
 
 internal static void forms() {
```

```diff
--- master/MethodHolderNames/main.cs
+++ g1/MethodHolderNames/main.cs
@@ -3,8 +3,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
@@ -22,23 +20,21 @@
     internal nint n;
 }
 
-} partial class mainᴛBox {
+internal static nint Limit⁔(/*this*/ Box b) { return b.n + (nint)Limit; }
 
-public static nint Limit(this Box b) { return b.n + (nint)main.Limit; }
-
-public static nint Count(this ref Box b) {
-    main.Count++;
-    return b.n + main.Count;
+internal static nint Count⁔(/*this*/ ref Box b) {
+    Count++;
+    return b.n + Count;
 }
 
-public static Kind Kind(this Box b) { return (Kind)b.n; }
+internal static Kind Kind⁔(/*this*/ Box b) { return (Kind)b.n; }
 
-public static nint Scale(this Box b) {
-    var f = main.Scale;
-    return main.Scale(b.n) + f(1);
+internal static nint Scale⁔(/*this*/ Box b) {
+    var f = Scale;
+    return Scale(b.n) + f(1);
 }
 
-public static @string Describe(this Box b, any v) {
+internal static @string Describe⁔(/*this*/ Box b, any v) {
     switch (v.type()) {
     case Kind k: {
         return "a Kind: "u8 + k.String();
@@ -54,9 +50,9 @@
     return fmt.Sprint("other: ", v, " zero ", (Kind)0);
 }
 
-public static @string Class(this Box b, any v) {
+internal static @string Class⁔(/*this*/ Box b, any v) {
     switch (v.type()) {
-    case Kind _: {
+    case Kind: {
         return "kind";
     }
     case Box _:
@@ -67,34 +63,22 @@
     return "neither";
 }
 
-} partial class main_package {
-
 partial struct Pair<T> {
     internal T a, b;
 }
 
-} partial class mainᴛPair {
-
-public static nint Scale<T>(this Pair<T> p) { return main.Scale(2) + (nint)main.Limit; }
-
-public static nint Limit<T>(this ref Pair<T> p) { return main.Limit * 2; }
+internal static nint Scale⁔<T>(/*this*/ Pair<T> p) { return Scale(2) + (nint)Limit; }
 
-} partial class mainᴛBox {
+internal static nint Limit⁔<T>(/*this*/ ref Pair<T> p) { return Limit * 2; }
 
-public static nint Total(this Crate c) { return c.Limit() + (nint)main.Limit + main.Scale(c.n); }
-
-} partial class main_package {
+internal static nint Total⁔(/*this*/ Crate c) { return c.Limit() + (nint)Limit + Scale(c.n); }
 
 partial struct Shelf {
     /*embed*/ public Box Box;
     internal nint extra;
 }
 
-} partial class mainᴛShelf {
-
-public static nint Limit(this Shelf s) { return s.Box.Limit() + s.extra + (nint)main.Limit; }
-
-} partial class main_package {
+internal static nint Limit⁔(/*this*/ Shelf s) { return s.Box.Limit() + s.extra + (nint)Limit; }
 
 // Hoisted @string literals (single allocation; Go keeps these in RODATA)
 private static readonly object textˢ = (@string)"text"u8;
@@ -118,7 +102,7 @@
     var s = new Shelf(Box: new Box(n: 3), extra: 100);
     fmt.Println("embedded", s.Limit(), s.Scale(), s.Kind(), s.Box.Count());
 
-    var limit = ((Func<Box, nint>)(mainᴛBox.Limit));
+    var limit = ((Func<Box, nint>)(main_package.Limit⁔));
     var count = ((Func<ж<Box>, nint>)(mainᴛBox.Count));
     
     var bʗ1 = b;
```

```diff
--- master/MethodHolderNames/others.cs
+++ g1/MethodHolderNames/others.cs
@@ -4,27 +4,21 @@
 using bits = math.bits_package;
 using sort = sort_package;
 using math;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct words /*[]uint64*/;
 
-} partial class mainᴛwords {
+internal static nint Len⁔(/*this*/ words w) { return len(w); }
+internal static bool Less⁔(/*this*/ words w, nint i, nint j) { return w[i] < w[j]; }
+internal static void Swap⁔(/*this*/ words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
 
-internal static nint Len(this words w) { return len(w); }
-internal static bool Less(this words w, nint i, nint j) { return w[i] < w[j]; }
-internal static void Swap(this words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
+internal static nint cap⁔(/*this*/ words w) { return cap(w) + 1; }
+internal static nint bits⁔(/*this*/ words w) { return bits.Len64(w[0]); }
 
-internal static nint cap(this words w) { return builtin.cap(w) + 1; }
-internal static nint bits(this words w) { return math.bits_package.Len64(w[0]); }
+internal static void Sort⁔(/*this*/ words w) { Sort(w); }
 
-internal static void Sort(this words w) { main.Sort(w); }
-
-internal static nint Run(this words w) { return len(w); }
-
-} partial class main_package {
+internal static nint Run⁔(/*this*/ words w) { return len(w); }
 
 partial struct gauge {
     internal nint n;
```

```diff
--- master/MethodHolderNames/package_info.cs
+++ g1/MethodHolderNames/package_info.cs
@@ -111,3 +111,45 @@
 [GoMethodHost(typeof(main_package.meter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛmeter;
 [GoMethodHost(typeof(main_package.words)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛwords;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛBox {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Limit(this Box b) => main_package.Limit⁔(b);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Count(this ref Box b) => main_package.Count⁔(ref b);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Kind Kind(this Box b) => main_package.Kind⁔(b);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Scale(this Box b) => main_package.Scale⁔(b);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Describe(this Box b, any v) => main_package.Describe⁔(b, v);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Class(this Box b, any v) => main_package.Class⁔(b, v);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Total(this Crate c) => main_package.Total⁔(c);
+}
+partial class mainᴛChest {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Tag Tag(this ref Chest c) => main_package.Tag⁔(ref c);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<nint> Seal(this ж<Chest> Ꮡc) => main_package.Seal⁔(Ꮡc);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Forms(this ж<Chest> Ꮡc, any v) => main_package.Forms⁔(Ꮡc, v);
+}
+partial class mainᴛPair {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Scale<T>(this Pair<T> p) => main_package.Scale⁔<T>(p);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Limit<T>(this ref Pair<T> p) => main_package.Limit⁔<T>(ref p);
+}
+partial class mainᴛShelf {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Limit(this Shelf s) => main_package.Limit⁔(s);
+}
+partial class mainᴛmeter {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Limit(this meter m) => main_package.Limit⁔(m);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Scale(this meter m, nint main) => main_package.Scale⁔(m, main);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Count(this meter m) => main_package.Count⁔(m);
+}
+partial class mainᴛwords {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this words w) => main_package.Len⁔(w);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this words w, nint i, nint j) => main_package.Less⁔(w, i, j);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this words w, nint i, nint j) => main_package.Swap⁔(w, i, j);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint cap(this words w) => main_package.cap⁔(w);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint bits(this words w) => main_package.bits⁔(w);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Sort(this words w) => main_package.Sort⁔(w);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Run(this words w) => main_package.Run⁔(w);
+}
+// </MethodForwarders>
```

```diff
--- master/MethodHolderNames/shadow.cs
+++ g1/MethodHolderNames/shadow.cs
@@ -1,8 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,18 +8,14 @@
     internal nint main;
 }
 
-} partial class mainᴛmeter {
-
-internal static nint Limit(this meter m) {
+internal static nint Limit⁔(/*this*/ meter m) {
     nint main = m.main + 1;
-    return main + (nint)main_package.Limit;
+    return main + (nint)Limit;
 }
 
-internal static nint Scale(this meter m, nint main) { return main_package.Scale(main) + m.main; }
-
-internal static nint Count(this meter m) { return main.Count + m.main; }
+internal static nint Scale⁔(/*this*/ meter m, nint main) { return Scale(main) + m.main; }
 
-} partial class main_package {
+internal static nint Count⁔(/*this*/ meter m) { return Count + m.main; }
 
 internal static void shadows() {
     var m = new meter(main: 2);
```

## NamedMapMethodShadow

```diff
--- master/NamedMapMethodShadow/main.cs
+++ g1/NamedMapMethodShadow/main.cs
@@ -2,17 +2,12 @@
 
 using fmt = fmt_package;
 using strings = strings_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct Env /*map[@string, @string]*/;
 
-} partial class mainᴛEnv {
-
-public static void Set(this Env e, @string k, @string v) { e[strings.ToLower(k)] = v; }
-
-} partial class main_package {
+internal static void Set⁔(/*this*/ Env e, @string k, @string v) { e[strings.ToLower(k)] = v; }
 
 partial struct Bag /*map[@string, nint]*/;
 
@@ -39,11 +34,7 @@
 
 partial struct PEnv /*map[@string, @string]*/;
 
-} partial class mainᴛPEnv {
-
-public static void Set(this ref PEnv p, @string k, @string v) { (p)[strings.ToUpper(k)] = v; }
-
-} partial class main_package {
+internal static void Set⁔(/*this*/ ref PEnv p, @string k, @string v) { (p)[strings.ToUpper(k)] = v; }
 
 partial struct Uniq<T> /*map[T, EmptyStruct]*/;
 
@@ -66,11 +57,7 @@
 
 partial struct Hdr /*map[@string, slice<@string>]*/;
 
-} partial class mainᴛHdr {
-
-public static void Set(this Hdr h, @string k, @string v) { h[strings.ToUpper(k)] = [v]; }
-
-} partial class main_package {
+internal static void Set⁔(/*this*/ Hdr h, @string k, @string v) { h[strings.ToUpper(k)] = [v]; }
 
 internal partial struct main_named /*dyn*/ {
     internal map<@string, Env> m;
```

```diff
--- master/NamedMapMethodShadow/package_info.cs
+++ g1/NamedMapMethodShadow/package_info.cs
@@ -98,3 +98,19 @@
 [GoMethodHost(typeof(main_package.Hdr)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛHdr;
 [GoMethodHost(typeof(main_package.PEnv)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛPEnv;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛEnv {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this Env e, @string k, @string v) => main_package.Set⁔(e, k, v);
+}
+partial class mainᴛHdr {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this Hdr h, @string k, @string v) => main_package.Set⁔(h, k, v);
+}
+partial class mainᴛPEnv {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Set(this ref PEnv p, @string k, @string v) => main_package.Set⁔(ref p, k, v);
+}
+// </MethodForwarders>
```

## NamedMapRvalueIndexWrite

```diff
--- master/NamedMapRvalueIndexWrite/main.cs
+++ g1/NamedMapRvalueIndexWrite/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -13,11 +12,7 @@
     internal Header hdr;
 }
 
-} partial class mainᴛresponse {
-
-internal static Header Header(this ref response r) { return r.hdr; }
-
-} partial class main_package {
+internal static Header Header⁔(/*this*/ ref response r) { return r.hdr; }
 
 internal static ж<response> newResponse() { return Ꮡ(new response(hdr: new Header(new map<@string, slice<@string>>{}))); }
 
```

```diff
--- master/NamedMapRvalueIndexWrite/package_info.cs
+++ g1/NamedMapRvalueIndexWrite/package_info.cs
@@ -93,3 +93,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.response)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛresponse;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛresponse {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Header Header(this ref response r) => main_package.Header⁔(ref r);
+}
+// </MethodForwarders>
```

## NamedTypeOverStruct

```diff
--- master/NamedTypeOverStruct/NamedTypeOverStruct.cs
+++ g1/NamedTypeOverStruct/NamedTypeOverStruct.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -50,15 +49,11 @@
     internal int64 fd;
 }
 
-} partial class mainᴛfile {
-
-internal static int64 bump(this ref @file f) {
+internal static int64 bump⁔(/*this*/ ref @file f) {
     f.fd += 2;
     return f.fd;
 }
 
-} partial class main_package {
-
 partial struct mixed {
     public nint Pub;
     internal nint sec;
```

```diff
--- master/NamedTypeOverStruct/package_info.cs
+++ g1/NamedTypeOverStruct/package_info.cs
@@ -101,3 +101,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.@file)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfile;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛfile {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static int64 bump(this ref @file f) => main_package.bump⁔(ref f);
+}
+// </MethodForwarders>
```

## PointerReceiverPointerLocalField

```diff
--- master/PointerReceiverPointerLocalField/main.cs
+++ g1/PointerReceiverPointerLocalField/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -9,13 +8,9 @@
     internal nint n;
 }
 
-} partial class mainᴛslot {
-
-internal static void inc(this ref slot s) { s.n++; }
-internal static void add(this ref slot s, nint d) { s.n += d; }
-internal static nint get(this ref slot s) { return s.n; }
-
-} partial class main_package {
+internal static void inc⁔(/*this*/ ref slot s) { s.n++; }
+internal static void add⁔(/*this*/ ref slot s, nint d) { s.n += d; }
+internal static nint get⁔(/*this*/ ref slot s) { return s.n; }
 
 partial struct holder {
     internal slot s;
```

```diff
--- master/PointerReceiverPointerLocalField/package_info.cs
+++ g1/PointerReceiverPointerLocalField/package_info.cs
@@ -92,3 +92,15 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.slot)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛslot;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛslot {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void inc(this ref slot s) => main_package.inc⁔(ref s);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref slot s, nint d) => main_package.add⁔(ref s, d);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint get(this ref slot s) => main_package.get⁔(ref s);
+}
+// </MethodForwarders>
```

## PromotedForwarderTypeNameCollision

```diff
--- master/PromotedForwarderTypeNameCollision/main.cs
+++ g1/PromotedForwarderTypeNameCollision/main.cs
@@ -5,7 +5,6 @@
 using fmt = fmt_package;
 using reflect = reflect_package;
 using database.sql;
-using static main_package;
 
 partial class main_package {
 
@@ -24,59 +23,43 @@
     internal @string tag;
 }
 
-} partial class mainᴛbase {
-
-internal static Driver Driver(this ref @base b) { return new Driver(name: "driver:"u8 + b.tag); }
-internal static @string Name(this ref @base b) { return "base:"u8 + b.tag; }
-
-} partial class main_package {
+internal static Driver Driver⁔(/*this*/ ref @base b) { return new Driver(name: "driver:"u8 + b.tag); }
+internal static @string Name⁔(/*this*/ ref @base b) { return "base:"u8 + b.tag; }
 
 partial struct counting {
     /*embed*/ public Connector Connector;
     internal nint hits;
 }
 
-} partial class mainᴛcounting {
-
 // Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
 // interface field in *counting's method set; see the pointer-only satisfaction record.
-internal static Driver Driver(this counting recvᴛ) => recvᴛ.Connector.Driver();
+internal static Driver Driver⁔(/*this*/ counting recvᴛ) => recvᴛ.Connector.Driver();
 
-internal static @string Name(this ref counting c) {
+internal static @string Name⁔(/*this*/ ref counting c) {
     c.hits++;
     return fmt.Sprintf("counting(%d):%s"u8, c.hits, c.Connector.Name());
 }
 
-} partial class main_package {
-
 partial struct fbase {
 }
 
-} partial class mainᴛfbase {
-
-internal static (driver.Conn, error) Connect(this fbase _Δp0, context.Context _Δp1) { return (default, default); }
-internal static driver.Driver Driver(this fbase _) { return new DriverжDriver(Ꮡ(new Driver(name: "foreign"))); }
-
-} partial class main_package {
+internal static (driver.Conn, error) Connect⁔(/*this*/ fbase _Δp0, context.Context _Δp1) { return (default, default); }
+internal static driver.Driver Driver⁔(/*this*/ fbase _) { return new DriverжDriver(Ꮡ(new Driver(name: "foreign"))); }
 
 partial struct fcounting {
     /*embed*/ public database.sql.driver_package.Connector Connector;
     internal nint n;
 }
 
-} partial class mainᴛfcounting {
-
 // Go method set entry for the promoted 'Connector.Driver()' - provided ONLY by the embedded
 // interface field in *fcounting's method set; see the pointer-only satisfaction record.
-internal static driver.Driver Driver(this fcounting recvᴛ) => recvᴛ.Connector.Driver();
+internal static driver.Driver Driver⁔(/*this*/ fcounting recvᴛ) => recvᴛ.Connector.Driver();
 
-internal static (driver.Conn, error) Connect(this ref fcounting c, context.Context ctx) {
+internal static (driver.Conn, error) Connect⁔(/*this*/ ref fcounting c, context.Context ctx) {
     c.n++;
     return c.Connector.Connect(ctx);
 }
 
-} partial class main_package {
-
 internal static @string use(Connector c) { return c.Name() + " / "u8 + c.Driver().name; }
 
 internal static void main() {
```

```diff
--- master/PromotedForwarderTypeNameCollision/package_info.cs
+++ g1/PromotedForwarderTypeNameCollision/package_info.cs
@@ -10,6 +10,8 @@
 // importing type aliases at a namespace level.
 
 // <ImportedTypeAliases>
+using context = go.context_package;
+using driver = go.database.sql.driver_package;
 // </ImportedTypeAliases>
 
 using go;
@@ -108,3 +110,26 @@
 [GoMethodHost(typeof(main_package.fbase)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfbase;
 [GoMethodHost(typeof(main_package.fcounting)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛfcounting;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbase {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Driver Driver(this ref @base b) => main_package.Driver⁔(ref b);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref @base b) => main_package.Name⁔(ref b);
+}
+partial class mainᴛcounting {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Driver Driver(this counting recvᴛ) => main_package.Driver⁔(recvᴛ);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string Name(this ref counting c) => main_package.Name⁔(ref c);
+}
+partial class mainᴛfbase {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (driver.Conn, error) Connect(this fbase _Δp0, context.Context _Δp1) => main_package.Connect⁔(_Δp0, _Δp1);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static driver.Driver Driver(this fbase _) => main_package.Driver⁔(_);
+}
+partial class mainᴛfcounting {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static driver.Driver Driver(this fcounting recvᴛ) => main_package.Driver⁔(recvᴛ);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (driver.Conn, error) Connect(this ref fcounting c, context.Context ctx) => main_package.Connect⁔(ref c, ctx);
+}
+// </MethodForwarders>
```

## PtrKeyMapReceiverLookup

```diff
--- master/PtrKeyMapReceiverLookup/main.cs
+++ g1/PtrKeyMapReceiverLookup/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -13,9 +12,7 @@
     internal map<ж<conn>, @string> m;
 }
 
-} partial class mainᴛconn {
-
-internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
+internal static @string status⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     var (s, ok) = t.m[Ꮡc, ꟷ]; if (ok) {
@@ -24,19 +21,19 @@
     return "unknown";
 }
 
-internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
+internal static void rename⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     t.m[Ꮡc] = s;
 }
 
-internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
+internal static @string label⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     return t.m[Ꮡc];
 }
 
-internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) {
+internal static void close⁔(/*this*/ ж<conn> Ꮡc, ж<tracker> Ꮡt) {
     GoFrame ᒐ = default;
     try {
         ref var c = ref Ꮡc.DerefOrNull();
@@ -51,8 +48,6 @@
     finally { ᒐ.Run(); }
 }
 
-} partial class main_package {
-
 internal static void main() {
     var a = Ꮡ(new conn(id: 1));
     var b = Ꮡ(new conn(id: 2));
```

```diff
--- master/PtrKeyMapReceiverLookup/package_info.cs
+++ g1/PtrKeyMapReceiverLookup/package_info.cs
@@ -92,3 +92,16 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.conn)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛconn;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛconn {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string status(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => main_package.status⁔(Ꮡc, Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void rename(this ж<conn> Ꮡc, ж<tracker> Ꮡt, @string s) => main_package.rename⁔(Ꮡc, Ꮡt, s);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static @string label(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => main_package.label⁔(Ꮡc, Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void close(this ж<conn> Ꮡc, ж<tracker> Ꮡt) => main_package.close⁔(Ꮡc, Ꮡt);
+}
+// </MethodForwarders>
```

## RenamedReceiverBox

```diff
--- master/RenamedReceiverBox/main.cs
+++ g1/RenamedReceiverBox/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -12,11 +11,7 @@
 partial struct tagger {
 }
 
-} partial class mainᴛtagger {
-
-internal static void p(this tagger _) { }
-
-} partial class main_package {
+internal static void p⁔(/*this*/ tagger _) { }
 
 partial struct counter {
     internal nint n;
```

```diff
--- master/RenamedReceiverBox/package_info.cs
+++ g1/RenamedReceiverBox/package_info.cs
@@ -93,3 +93,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.tagger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛtagger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛtagger {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void p(this tagger _) => main_package.p⁔(_);
+}
+// </MethodForwarders>
```

## ReservedNameShadows

```diff
--- master/ReservedNameShadows/main.cs
+++ g1/ReservedNameShadows/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -58,11 +57,7 @@
     internal slice<nint> items;
 }
 
-} partial class mainᴛbox {
-
-internal static nint len(this ref box b) { return builtin.len(b.items) + 1; }
-
-} partial class main_package {
+internal static nint len⁔(/*this*/ ref box b) { return len(b.items) + 1; }
 
 internal static nint freeLen() {
     var s = slice<nint>([1, 2, 3]);
```

```diff
--- master/ReservedNameShadows/package_info.cs
+++ g1/ReservedNameShadows/package_info.cs
@@ -96,3 +96,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.box)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbox;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛbox {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint len(this ref box b) => main_package.len⁔(ref b);
+}
+// </MethodForwarders>
```

## ReservedTypeMethodCollision

```diff
--- master/ReservedTypeMethodCollision/main.cs
+++ g1/ReservedTypeMethodCollision/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -15,14 +14,10 @@
     internal nint @base;
 }
 
-} partial class mainᴛbuilder {
-
-internal static Δslice Δslice(this ref builder b, nint n) {
+internal static Δslice Δslice⁔(/*this*/ ref builder b, nint n) {
     return new Δslice(lo: b.@base, hi: b.@base + n);
 }
 
-} partial class main_package {
-
 internal static void main() {
     var b = Ꮡ(new builder(@base: 10));
     var s = b.Δslice(5);
@@ -41,9 +36,7 @@
     internal slice<nint> vals;
 }
 
-} partial class mainᴛSieve {
-
-public static nint Filter(this Sieve sv, Func<nint, bool> f) {
+internal static nint Filter⁔(/*this*/ Sieve sv, Func<nint, bool> f) {
     nint count = 0;
     foreach (var (_, v) in range(sv.vals)) {
         if (f(v)) {
@@ -53,4 +46,4 @@
     return count;
 }
 
-}
+} // end main_package
```

```diff
--- master/ReservedTypeMethodCollision/package_info.cs
+++ g1/ReservedTypeMethodCollision/package_info.cs
@@ -90,3 +90,16 @@
 [GoMethodHost(typeof(main_package.Sieve)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛSieve;
 [GoMethodHost(typeof(main_package.builder)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛbuilder;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛSieve {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Filter(this Sieve sv, Func<nint, bool> f) => main_package.Filter⁔(sv, f);
+}
+partial class mainᴛbuilder {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static Δslice Δslice(this ref builder b, nint n) => main_package.Δslice⁔(ref b, n);
+}
+// </MethodForwarders>
```

## ShadowedImportConstUser

```diff
--- master/ShadowedImportConstUser/main.cs
+++ g1/ShadowedImportConstUser/main.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using ShadowedImportConstLib = ShadowedImportConstLib_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,11 +9,7 @@
     internal nint level;
 }
 
-} partial class mainᴛgauge {
-
-internal static nint ShadowedImportConstLib(this gauge g) { return g.level; }
-
-} partial class main_package {
+internal static nint ShadowedImportConstLib⁔(/*this*/ gauge g) { return g.level; }
 
 internal static void main() {
     var g = new gauge(level: 3);
```

```diff
--- master/ShadowedImportConstUser/package_info.cs
+++ g1/ShadowedImportConstUser/package_info.cs
@@ -87,3 +87,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.gauge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛgauge;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛgauge {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint ShadowedImportConstLib(this gauge g) => main_package.ShadowedImportConstLib⁔(g);
+}
+// </MethodForwarders>
```

## ShadowedInterfaceEmbed

```diff
--- master/ShadowedInterfaceEmbed/main.cs
+++ g1/ShadowedInterfaceEmbed/main.cs
@@ -1,7 +1,6 @@
 namespace go;
 
 using fmt = fmt_package;
-using static main_package;
 
 partial class main_package {
 
@@ -9,11 +8,7 @@
     internal @string name;
 }
 
-} partial class mainᴛLogger {
-
-public static @string Handler(this Logger l) { return "logger:"u8 + l.name; }
-
-} partial class main_package {
+internal static @string Handler⁔(/*this*/ Logger l) { return "logger:"u8 + l.name; }
 
 partial interface Handler {
     bool Enabled(nint level);
```

```diff
--- master/ShadowedInterfaceEmbed/package_info.cs
+++ g1/ShadowedInterfaceEmbed/package_info.cs
@@ -94,3 +94,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.Logger)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛLogger;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛLogger {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Handler(this Logger l) => main_package.Handler⁔(l);
+}
+// </MethodForwarders>
```

## SortArrayType

```diff
--- master/SortArrayType/SortArrayType.cs
+++ g1/SortArrayType/SortArrayType.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using sort = sort_package;
-using static main_package;
 
 partial class main_package {
 
@@ -88,14 +87,10 @@
 
 internal static UntypedInt markerConst => 256;
 
-} partial class mainᴛPeopleByAge {
-
-internal static void sort(this PeopleByAge p) {
-    sort_package.Sort(p);
+internal static void sort⁔(/*this*/ PeopleByAge p) {
+    sort.Sort(p);
 }
 
-} partial class main_package {
-
 public static nint Len(this PeopleByShoeSize p) {
     return len(p);
 }
@@ -111,22 +106,18 @@
     return a;
 }
 
-} partial class mainᴛPeopleByAge {
-
-public static nint Len(this PeopleByAge p) {
+internal static nint Len⁔(/*this*/ PeopleByAge p) {
     return len(p);
 }
 
-public static void Swap(this PeopleByAge p, nint i, nint j) {
+internal static void Swap⁔(/*this*/ PeopleByAge p, nint i, nint j) {
     (p[i], p[j]) = (p[j], p[i]);
 }
 
-public static bool Less(this PeopleByAge p, nint i, nint j) {
+internal static bool Less⁔(/*this*/ PeopleByAge p, nint i, nint j) {
     return (p[i].Age < p[j].Age);
 }
 
-} partial class main_package {
-
 public static (nint E2, @string p) Testing() {
     nint E2 = default;
     @string p = default;
```

```diff
--- master/SortArrayType/package_info.cs
+++ g1/SortArrayType/package_info.cs
@@ -95,3 +95,16 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.PeopleByAge)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛPeopleByAge;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛPeopleByAge {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void sort(this PeopleByAge p) => main_package.sort⁔(p);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this PeopleByAge p) => main_package.Len⁔(p);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Swap(this PeopleByAge p, nint i, nint j) => main_package.Swap⁔(p, i, j);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Less(this PeopleByAge p, nint i, nint j) => main_package.Less⁔(p, i, j);
+}
+// </MethodForwarders>
```

## SortMethodSelfCapture

```diff
--- master/SortMethodSelfCapture/main.cs
+++ g1/SortMethodSelfCapture/main.cs
@@ -2,28 +2,18 @@
 
 using fmt = fmt_package;
 using sort = sort_package;
-using main = main_package;
-using static main_package;
 
 partial class main_package {
 
 partial struct words /*[]@string*/;
 
-} partial class mainᴛwords {
-
-internal static nint Len(this words w) { return len(w); }
-internal static bool Less(this words w, nint i, nint j) { return w[i] < w[j]; }
-internal static void Swap(this words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
-
-} partial class main_package {
+internal static nint Len⁔(/*this*/ words w) { return len(w); }
+internal static bool Less⁔(/*this*/ words w, nint i, nint j) { return w[i] < w[j]; }
+internal static void Swap⁔(/*this*/ words w, nint i, nint j) { (w[i], w[j]) = (w[j], w[i]); }
 
 public static void Sort(sort.Interface data) { sort.Sort(data); }
 
-} partial class mainᴛwords {
-
-internal static void Sort(this words w) { main.Sort(w); }
-
-} partial class main_package {
+internal static void Sort⁔(/*this*/ words w) { Sort(w); }
 
 internal static void main() {
     var ints = new sort.IntSlice(new nint[]{3, 1, 2}.slice());
```

```diff
--- master/SortMethodSelfCapture/package_info.cs
+++ g1/SortMethodSelfCapture/package_info.cs
@@ -89,3 +89,16 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.words)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛwords;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛwords {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static nint Len(this words w) => main_package.Len⁔(w);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static bool Less(this words w, nint i, nint j) => main_package.Less⁔(w, i, j);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Swap(this words w, nint i, nint j) => main_package.Swap⁔(w, i, j);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void Sort(this words w) => main_package.Sort⁔(w);
+}
+// </MethodForwarders>
```

## StdLibInternalAbi

```diff
--- master/StdLibInternalAbi/package_info.cs
+++ g1/StdLibInternalAbi/package_info.cs
@@ -145,3 +145,48 @@
 [GoMethodHost(typeof(main_package.Name)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛName;
 [GoMethodHost(typeof(main_package.Type)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛType;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛKind {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this Kind k) => main_package.String⁔(k);
+}
+partial class mainᴛName {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<byte> DataChecked(this Name n, nint off, @string whySafe) => main_package.DataChecked⁔(n, off, whySafe);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<byte> Data(this Name n, nint off) => main_package.Data⁔(n, off);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsExported(this Name n) => main_package.IsExported⁔(n);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HasTag(this Name n) => main_package.HasTag⁔(n);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsEmbedded(this Name n) => main_package.IsEmbedded⁔(n);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, nint) ReadVarint(this Name n, nint off) => main_package.ReadVarint⁔(n, off);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsBlank(this Name n) => main_package.IsBlank⁔(n);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Name(this Name n) => main_package.Name⁔(n);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string Tag(this Name n) => main_package.Tag⁔(n);
+}
+partial class mainᴛType {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static Kind Kind(this ref Type t) => main_package.Kind⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool HasName(this ref Type t) => main_package.HasName⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Pointers(this ref Type t) => main_package.Pointers⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IfaceIndir(this ref Type t) => main_package.IfaceIndir⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool IsDirectIface(this ref Type t) => main_package.IsDirectIface⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) => main_package.GcSlice⁔(ref t, begin, end);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ж<Type> Ꮡt) => main_package.Len⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Type> Common(this ж<Type> Ꮡt) => main_package.Common⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ChanDir ChanDir(this ж<Type> Ꮡt) => main_package.ChanDir⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) => main_package.Uncommon⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Type> Elem(this ж<Type> Ꮡt) => main_package.Elem⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<StructType> StructType(this ж<Type> Ꮡt) => main_package.StructType⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<MapType> MapType(this ж<Type> Ꮡt) => main_package.MapType⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) => main_package.ArrayType⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<FuncType> FuncType(this ж<Type> Ꮡt) => main_package.FuncType⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<InterfaceType> InterfaceType(this ж<Type> Ꮡt) => main_package.InterfaceType⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static uintptr Size(this ref Type t) => main_package.Size⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Align(this ref Type t) => main_package.Align⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint FieldAlign(this ref Type t) => main_package.FieldAlign⁔(ref t);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) => main_package.ExportedMethods⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint NumMethod(this ж<Type> Ꮡt) => main_package.NumMethod⁔(Ꮡt);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ж<Type> Key(this ж<Type> Ꮡt) => main_package.Key⁔(Ꮡt);
+}
+// </MethodForwarders>
```

```diff
--- master/StdLibInternalAbi/type.cs
+++ g1/StdLibInternalAbi/type.cs
@@ -5,7 +5,6 @@
 namespace go;
 
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -71,17 +70,13 @@
 
 partial struct TextOff /*num:int32*/;
 
-} partial class mainᴛKind {
-
-public static @string String(this Kind k) {
+internal static @string String⁔(/*this*/ Kind k) {
     if ((nint)k < len(kindNames)) {
         return kindNames[k];
     }
     return kindNames[0];
 }
 
-} partial class main_package {
-
 [GoLazy] partial class lazyᴛkindNames { internal static slice<@string> kindNames = new golib.SparseArray<@string>{
     [Invalid] = "invalid",
     [Bool] = "bool",
@@ -127,30 +122,26 @@
     return TypeOf((ж<T>)nil).Elem();
 }
 
-} partial class mainᴛType {
-
-public static Kind Kind(this ref Type t) { return (Kind)(t.Kind_ & KindMask); }
+internal static Kind Kind⁔(/*this*/ ref Type t) { return (Kind)(t.Kind_ & KindMask); }
 
-public static bool HasName(this ref Type t) {
+internal static bool HasName⁔(/*this*/ ref Type t) {
     return (TFlag)(t.TFlag & TFlagNamed) != 0;
 }
 
-public static bool Pointers(this ref Type t) { return t.PtrBytes != 0; }
+internal static bool Pointers⁔(/*this*/ ref Type t) { return t.PtrBytes != 0; }
 
-public static bool IfaceIndir(this ref Type t) {
+internal static bool IfaceIndir⁔(/*this*/ ref Type t) {
     return (Kind)(t.Kind_ & KindDirectIface) == 0;
 }
 
-public static bool IsDirectIface(this ref Type t) {
+internal static bool IsDirectIface⁔(/*this*/ ref Type t) {
     return (Kind)(t.Kind_ & KindDirectIface) != 0;
 }
 
-public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
+internal static slice<byte> GcSlice⁔(/*this*/ ref Type t, uintptr begin, uintptr end) {
     return @unsafe.Slice(t.GCData, (nint)end).slice((nint)(begin));
 }
 
-} partial class main_package {
-
 partial struct Method {
     public NameOff Name;
     public TypeOff Mtyp;
@@ -200,9 +191,7 @@
     public uintptr Len;
 }
 
-} partial class mainᴛType {
-
-public static nint Len(this ж<Type> Ꮡt) {
+internal static nint Len⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Array) {
@@ -211,12 +200,10 @@
     return 0;
 }
 
-public static ж<Type> Common(this ж<Type> Ꮡt) {
+internal static ж<Type> Common⁔(/*this*/ ж<Type> Ꮡt) {
     return Ꮡt;
 }
 
-} partial class main_package {
-
 partial struct ChanDir /*num:nint*/;
 
 public static ChanDir RecvDir => /* 1 << iota */ 1;
@@ -235,9 +222,7 @@
     internal UncommonType u;
 }
 
-} partial class mainᴛType {
-
-public static ChanDir ChanDir(this ж<Type> Ꮡt) {
+internal static ChanDir ChanDir⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Chan) {
@@ -247,8 +232,6 @@
     return InvalidDir;
 }
 
-} partial class main_package {
-
 internal partial struct Uncommon_u /*dyn*/ {
     /*embed*/ public PtrType PtrType;
     internal UncommonType u;
@@ -289,9 +272,7 @@
     internal UncommonType u;
 }
 
-} partial class mainᴛType {
-
-public static ж<UncommonType> Uncommon(this ж<Type> Ꮡt) {
+internal static ж<UncommonType> Uncommon⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
@@ -328,13 +309,13 @@
 
 }
 
-public static ж<Type> Elem(this ж<Type> Ꮡt) {
+internal static ж<Type> Elem⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     switch (t.Kind()) {
     case var ᴛ when ᴛ == Array: {
         var tt = Ꮡt.Reinterpret<Type, ArrayType>();
-        return tt.of(main_package.ArrayType.ᏑElem).Value;
+        return tt.of(ArrayType.ᏑElem).Value;
     }
     case var ᴛ when ᴛ == Chan: {
         var tt = Ꮡt.Reinterpret<Type, ChanType>();
@@ -342,7 +323,7 @@
     }
     case var ᴛ when ᴛ == Map: {
         var tt = Ꮡt.Reinterpret<Type, MapType>();
-        return tt.of(main_package.MapType.ᏑElem).Value;
+        return tt.of(MapType.ᏑElem).Value;
     }
     case var ᴛ when ᴛ == Pointer: {
         var tt = Ꮡt.Reinterpret<Type, PtrType>();
@@ -356,7 +337,7 @@
     return default;
 }
 
-public static ж<StructType> StructType(this ж<Type> Ꮡt) {
+internal static ж<StructType> StructType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Struct) {
@@ -365,7 +346,7 @@
     return Ꮡt.Reinterpret<Type, StructType>();
 }
 
-public static ж<MapType> MapType(this ж<Type> Ꮡt) {
+internal static ж<MapType> MapType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Map) {
@@ -374,7 +355,7 @@
     return Ꮡt.Reinterpret<Type, MapType>();
 }
 
-public static ж<ArrayType> ArrayType(this ж<Type> Ꮡt) {
+internal static ж<ArrayType> ArrayType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Array) {
@@ -383,7 +364,7 @@
     return Ꮡt.Reinterpret<Type, ArrayType>();
 }
 
-public static ж<FuncType> FuncType(this ж<Type> Ꮡt) {
+internal static ж<FuncType> FuncType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Func) {
@@ -392,7 +373,7 @@
     return Ꮡt.Reinterpret<Type, FuncType>();
 }
 
-public static ж<InterfaceType> InterfaceType(this ж<Type> Ꮡt) {
+internal static ж<InterfaceType> InterfaceType⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() != Interface) {
@@ -401,13 +382,11 @@
     return Ꮡt.Reinterpret<Type, InterfaceType>();
 }
 
-public static uintptr Size(this ref Type t) { return t.Size_; }
+internal static uintptr Size⁔(/*this*/ ref Type t) { return t.Size_; }
 
-public static nint Align(this ref Type t) { return (nint)t.Align_; }
+internal static nint Align⁔(/*this*/ ref Type t) { return (nint)t.Align_; }
 
-public static nint FieldAlign(this ref Type t) { return (nint)t.FieldAlign_; }
-
-} partial class main_package {
+internal static nint FieldAlign⁔(/*this*/ ref Type t) { return (nint)t.FieldAlign_; }
 
 partial struct InterfaceType {
     /*embed*/ public Type Type;
@@ -415,9 +394,7 @@
     public slice<Imethod> Methods;
 }
 
-} partial class mainᴛType {
-
-public static slice<Method> ExportedMethods(this ж<Type> Ꮡt) {
+internal static slice<Method> ExportedMethods⁔(/*this*/ ж<Type> Ꮡt) {
     var ut = Ꮡt.Uncommon();
     if (ut == nil) {
         return default;
@@ -425,7 +402,7 @@
     return ut.ExportedMethods();
 }
 
-public static nint NumMethod(this ж<Type> Ꮡt) {
+internal static nint NumMethod⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Interface) {
@@ -435,8 +412,6 @@
     return len(Ꮡt.ExportedMethods());
 }
 
-} partial class main_package {
-
 public static nint NumMethod(this ref InterfaceType t) { return len(t.Methods); }
 
 partial struct MapType {
@@ -467,9 +442,7 @@
     return (mt.Flags & 16) != 0;
 }
 
-} partial class mainᴛType {
-
-public static ж<Type> Key(this ж<Type> Ꮡt) {
+internal static ж<Type> Key⁔(/*this*/ ж<Type> Ꮡt) {
     ref var t = ref Ꮡt.DerefOrNull();
 
     if (t.Kind() == Map) {
@@ -478,8 +451,6 @@
     return default;
 }
 
-} partial class main_package {
-
 partial struct SliceType {
     /*embed*/ public Type Type;
     public ж<Type> Elem;
@@ -562,29 +533,27 @@
     public ж<byte> Bytes;
 }
 
-} partial class mainᴛName {
-
-public static ж<byte> DataChecked(this Name n, nint off, @string whySafe) {
+internal static ж<byte> DataChecked⁔(/*this*/ Name n, nint off, @string whySafe) {
     return @unsafe.Pointer.To<byte>(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, whySafe));
 }
 
-public static ж<byte> Data(this Name n, nint off) {
+internal static ж<byte> Data⁔(/*this*/ Name n, nint off) {
     return @unsafe.Pointer.To<byte>(addChecked(@unsafe.Pointer.FromPinnedBox(n.Bytes), (uintptr)off, "the runtime doesn't need to give you a reason"));
 }
 
-public static bool IsExported(this Name n) {
+internal static bool IsExported⁔(/*this*/ Name n) {
     return (byte)((n.Bytes.Value) & ((byte)(1 << 0))) != 0;
 }
 
-public static bool HasTag(this Name n) {
+internal static bool HasTag⁔(/*this*/ Name n) {
     return (byte)((n.Bytes.Value) & ((byte)(1 << 1))) != 0;
 }
 
-public static bool IsEmbedded(this Name n) {
+internal static bool IsEmbedded⁔(/*this*/ Name n) {
     return (byte)((n.Bytes.Value) & ((byte)(1 << 3))) != 0;
 }
 
-public static (nint, nint) ReadVarint(this Name n, nint off) {
+internal static (nint, nint) ReadVarint⁔(/*this*/ Name n, nint off) {
     nint v = 0;
     for (nint i = 0; ; i++) {
         var x = n.DataChecked(off + i, "read varint").Value;
@@ -595,7 +564,7 @@
     }
 }
 
-public static bool IsBlank(this Name n) {
+internal static bool IsBlank⁔(/*this*/ Name n) {
     if (n.Bytes == nil) {
         return false;
     }
@@ -603,8 +572,6 @@
     return l == 1 && n.Data(2).Value == '_';
 }
 
-} partial class main_package {
-
 internal static nint writeVarint(slice<byte> buf, nint n) {
     for (nint i = 0; ; i++) {
         var b = (byte)(n & 0x7f);
@@ -617,9 +584,7 @@
     }
 }
 
-} partial class mainᴛName {
-
-public static @string Name(this Name n) {
+internal static @string Name⁔(/*this*/ Name n) {
     if (n.Bytes == nil) {
         return "";
     }
@@ -627,7 +592,7 @@
     return @unsafe.String(n.DataChecked(1 + i, "non-empty string"), l);
 }
 
-public static @string Tag(this Name n) {
+internal static @string Tag⁔(/*this*/ Name n) {
     if (!n.HasTag()) {
         return "";
     }
@@ -636,8 +601,6 @@
     return @unsafe.String(n.DataChecked(1 + i + l + i2, "non-empty string"), l2);
 }
 
-} partial class main_package {
-
 public static Name NewName(@string n, @string tag, bool exported, bool embedded) {
     if (len(n) >= 1 << 29) {
         throw panic("abi.NewName: name too long: " + n[..1024] + "...");
```

## UnsafePointerArgPassing

```diff
--- master/UnsafePointerArgPassing/main.cs
+++ g1/UnsafePointerArgPassing/main.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -10,11 +9,7 @@
     internal nint n;
 }
 
-} partial class mainᴛcounter {
-
-internal static void add(this ref counter c, uintptr d) { c.n += (nint)d; }
-
-} partial class main_package {
+internal static void add⁔(/*this*/ ref counter c, uintptr d) { c.n += (nint)d; }
 
 internal static partial @unsafe.Pointer add(@unsafe.Pointer p, uintptr x) {
     return @unsafe.Add(p, x);
```

```diff
--- master/UnsafePointerArgPassing/package_info.cs
+++ g1/UnsafePointerArgPassing/package_info.cs
@@ -91,3 +91,13 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.counter)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛcounter;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛcounter {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void add(this ref counter c, uintptr d) => main_package.add⁔(ref c, d);
+}
+// </MethodForwarders>
```

## UnsafePointerReceiverCopy

```diff
--- master/UnsafePointerReceiverCopy/main.cs
+++ g1/UnsafePointerReceiverCopy/main.cs
@@ -2,7 +2,6 @@
 
 using fmt = fmt_package;
 using @unsafe = unsafe_package;
-using static main_package;
 
 partial class main_package {
 
@@ -19,54 +18,46 @@
 partial struct q {
 }
 
-} partial class mainᴛT {
-
-internal static void q(this ref T h) { }
-
-} partial class main_package {
+internal static void q⁔(/*this*/ ref T h) { }
 
 internal static void poke(@unsafe.Pointer p, nint v) { (@unsafe.Pointer.To<nint>(p)).Value = v; }
 
-} partial class mainᴛT {
-
-internal static void viaReceiver(this ж<T> Ꮡh) {
+internal static void viaReceiver⁔(/*this*/ ж<T> Ꮡh) {
     ref var h = ref Ꮡh.DerefOrNull();
 
     poke((uintptr)@unsafe.Pointer.FromRef(ref h), 7);
 }
 
-internal static void viaLocal(this ж<T> Ꮡh) {
+internal static void viaLocal⁔(/*this*/ ж<T> Ꮡh) {
     var p = Ꮡh;
     poke(@unsafe.Pointer.FromPinnedBox(p), 8);
 }
 
-internal static void viaParam(this ref T h, ж<T> Ꮡother) {
+internal static void viaParam⁔(/*this*/ ref T h, ж<T> Ꮡother) {
     ref var other = ref Ꮡother.DerefOrNull();
 
     poke((uintptr)@unsafe.Pointer.FromRef(ref other), 9);
 }
 
-internal static void viaFolded(this ж<T> Ꮡh) {
+internal static void viaFolded⁔(/*this*/ ж<T> Ꮡh) {
     var p = Ꮡh;
     (p.Reinterpret<T, nint>()).Value = 10;
 }
 
-internal static void viaField(this ref T h) { poke(@unsafe.Pointer.FromPinnedBox(hold.p), 11); }
+internal static void viaField⁔(/*this*/ ref T h) { poke(@unsafe.Pointer.FromPinnedBox(hold.p), 11); }
 
-internal static void viaRenamedReceiver(this ж<T> Ꮡq) {
+internal static void viaRenamedReceiver⁔(/*this*/ ж<T> Ꮡq) {
     ref var q = ref Ꮡq.DerefOrNull();
 
     poke((uintptr)@unsafe.Pointer.FromRef(ref q), 12);
 }
 
-internal static void viaLocalChain(this ж<T> Ꮡh) {
+internal static void viaLocalChain⁔(/*this*/ ж<T> Ꮡh) {
     var p = Ꮡh;
     var r = p;
     poke(@unsafe.Pointer.FromPinnedBox(r), 13);
 }
 
-} partial class main_package {
-
 internal static void main() {
     ref var a = ref heap(new T(), out var Ꮡa);
     ref var b = ref heap(new T(), out var Ꮡb);
```

```diff
--- master/UnsafePointerReceiverCopy/package_info.cs
+++ g1/UnsafePointerReceiverCopy/package_info.cs
@@ -89,3 +89,20 @@
 // <MethodHolders>
 [GoMethodHost(typeof(main_package.T)), global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)] public static partial class mainᴛT;
 // </MethodHolders>
+
+// A Go method's body sits in its package class beside its type, named with a trailing U+2054 (`String⁔`),
+// which no Go identifier can spell. The Go-named extension method its holder declares is a one-line
+// forwarder to that body: every call, interface and method set reads the forwarder, and the runtime names
+// the body's frame as the Go method.
+// <MethodForwarders>
+partial class mainᴛT {
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void q(this ref T h) => main_package.q⁔(ref h);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaReceiver(this ж<T> Ꮡh) => main_package.viaReceiver⁔(Ꮡh);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaLocal(this ж<T> Ꮡh) => main_package.viaLocal⁔(Ꮡh);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaParam(this ref T h, ж<T> Ꮡother) => main_package.viaParam⁔(ref h, Ꮡother);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaFolded(this ж<T> Ꮡh) => main_package.viaFolded⁔(Ꮡh);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaField(this ref T h) => main_package.viaField⁔(ref h);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaRenamedReceiver(this ж<T> Ꮡq) => main_package.viaRenamedReceiver⁔(Ꮡq);
+    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void viaLocalChain(this ж<T> Ꮡh) => main_package.viaLocalChain⁔(Ꮡh);
+}
+// </MethodForwarders>
```
