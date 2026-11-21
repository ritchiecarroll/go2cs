// Package vertex is the Tour of Go's Vertex (methods), as a library package.
namespace go.tour;

using math = math_package;
using static vertex_package;

partial class vertex_package {

// Vertex is a point in the plane.
partial struct Vertex {
    public float64 X, Y;
}

} partial class vertexᴛVertex {

// Abs returns the distance of v from the origin.
public static float64 Abs(this Vertex v) {
    return math.Sqrt(v.X * v.X + v.Y * v.Y);
}

// Scale multiplies both coordinates of v by f.
public static void Scale(this ref Vertex v, float64 f) {
    v.X = v.X * f;
    v.Y = v.Y * f;
}

}
