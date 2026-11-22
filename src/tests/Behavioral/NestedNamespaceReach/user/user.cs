namespace go.NestedNamespaceReach;

using x = go.NestedNamespaceReach.child.x_package;
using go.NestedNamespaceReach.child;
using Δchild = go.NestedNamespaceReach.child_package;

partial class user_package {

internal static T use<T>(T v) { return v; }

internal static Δchild.T made = use(x.Make());

public static nint Value() { return made.V + 1; }

} // end user_package
