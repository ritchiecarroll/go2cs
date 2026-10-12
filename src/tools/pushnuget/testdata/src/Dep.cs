// Dep: the dependency the App fixtures compile against. Two builds differ ONLY in Salt's value, so they have one
// surface and two MVIDs -- the shape of a go.* dependency rebuilt for another GoTargetOS.
namespace Dep;

public struct Alpha { public int X; }
public struct Bravo { public int X; }

internal static class Salt { internal const string Value = "SALT"; }
