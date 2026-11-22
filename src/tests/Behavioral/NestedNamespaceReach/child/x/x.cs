namespace go.NestedNamespaceReach.child;

using child = go.NestedNamespaceReach.child_package;
using go.NestedNamespaceReach;

partial class x_package {

public static child.T Make() { return new child.T(V: 41); }

} // end x_package
