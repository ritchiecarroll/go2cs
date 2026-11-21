// Package vertex is the Tour of Go's Vertex (methods), as a library package.
namespace go.tour;

using math = math_package;
using static vertex_package;

partial class vertex_package {

// Vertex is a point in the plane.
partial struct Vertex {
    public float64 X, Y;
}

} partial class vertex_package {

// Abs returns the distance of v from the origin.
[GoMethodBody("Abs")] internal static float64 Absᐟ(Vertex v) {
    return math.Sqrt(v.X * v.X + v.Y * v.Y);
}

// Scale multiplies both coordinates of v by f.
[GoMethodBody("Scale")] internal static void Scaleᐟ(ref Vertex v, float64 f) {
    v.X = v.X * f;
    v.Y = v.Y * f;
}

}
