using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    public Vector2 minBound;
    public Vector2 maxBound;

    public int maxPointCount = 10;

    private List<Vector2> points = new List<Vector2>();
    private List<Triangle> triangles = new List<Triangle>();
    private Triangle superTriangle;

    public Vector2 simulationPosition;
    public float triangleScale = 3;

    private float simulationScale;

    private void Start()
    {
        simulationScale = maxBound.x;

        // Generate super triangle
        superTriangle = GenerateSuperTriangle(triangleScale, simulationScale, simulationPosition);

        // Generate bounds and add them as points for triangulation
        var bounds = CreateSimulationBounds(new Vector2(minBound.x, maxBound.x));

        // Generate random points within the bounds
        GeneratePoints();

        // Perform the triangulation
        Triangulate();

        // Draw resulting triangles
        DrawTriangles();
    }

    private Triangle GenerateSuperTriangle(
        float triangleScale,
        float boundScale,
        Vector2 triangleTransform
    )
    {
        //initial value
        float direction = 1;
        float masterScale = boundScale * triangleScale;

        //creating super triangle template
        //right bottom
        Vector2 a = new Vector2(
            -direction * masterScale + triangleTransform.x,
            -masterScale / 2 + triangleTransform.y
        );

        //left bottom
        Vector2 b = new Vector2(
            direction * masterScale + triangleTransform.x,
            -masterScale / 2 + triangleTransform.y
        );

        //top
        Vector2 c = new Vector2(triangleTransform.x, triangleTransform.y + masterScale);

        return new Triangle(a, b, c);
    }

    private void Triangulate()
    {
        // Add the super triangle initially to contain all points
        triangles.Add(superTriangle);

        // Insert each point (including bounds and random points) into the triangulation
        foreach (var point in points)
        {
            InsertPoint(point);
        }

        // Remove triangles that contain any vertex of the super triangle
        triangles.RemoveAll(triangle =>
            triangle.Contains(superTriangle.A)
            || triangle.Contains(superTriangle.B)
            || triangle.Contains(superTriangle.C)
        );
    }

    private void InsertPoint(Vector2 point)
    {
        List<Triangle> badTriangles = new List<Triangle>();
        HashSet<Edge> boundaryEdges = new HashSet<Edge>();

        // Step 1: Find all "bad" triangles whose circumcircles contain the point.
        foreach (var triangle in triangles)
        {
            if (triangle.IsPointInCircumcircle(point))
            {
                badTriangles.Add(triangle);

                // Add edges of the bad triangle, potentially creating boundary edges
                AddEdge(boundaryEdges, triangle.A, triangle.B);
                AddEdge(boundaryEdges, triangle.B, triangle.C);
                AddEdge(boundaryEdges, triangle.C, triangle.A);
            }
        }

        // Step 2: Remove bad triangles from the main list
        foreach (var badTriangle in badTriangles)
        {
            triangles.Remove(badTriangle);
        }

        // Step 3: Create new triangles from the point to each boundary edge
        foreach (var edge in boundaryEdges)
        {
            triangles.Add(new Triangle(edge.Start, edge.End, point));
        }
    }

    // Utility method to add an edge to the set, removing it if it already exists (since it would be internal).
    private void AddEdge(HashSet<Edge> edges, Vector2 start, Vector2 end)
    {
        var edge = new Edge(start, end);

        // If the edge is already in the set, remove it (since it's internal)
        if (!edges.Add(edge))
        {
            edges.Remove(edge);
        }
    }

    // Modify this method to add bounds as points and allow them to be triangulated.
    private List<Vector2> CreateSimulationBounds(Vector2 scalar)
    {
        List<Vector2> bounds = new List<Vector2>
        {
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1),
        };

        // Scale bounds dynamically and add to points list for triangulation.
        for (int i = 0; i < bounds.Count; i++)
        {
            bounds[i] *= scalar;
            points.Add(bounds[i]); // Add bounds directly to points for triangulation
        }

        return bounds;
    }

    private void GeneratePoints()
    {
        for (int i = 0; i < maxPointCount; i++)
        {
            float x = Random.Range(minBound.x, maxBound.x);
            float y = Random.Range(minBound.y, maxBound.y);

            Vector2 newPoint = new Vector2(x, y);
            points.Add(newPoint);
        }
    }

    void DrawTriangles()
    {
        foreach (var triangle in triangles)
        {
            Vector3 tA = new Vector3(triangle.A.x, 0, triangle.A.y);
            Vector3 tB = new Vector3(triangle.B.x, 0, triangle.B.y);
            Vector3 tC = new Vector3(triangle.C.x, 0, triangle.C.y);

            Debug.DrawLine(tA, tB, Color.red, 100f);
            Debug.DrawLine(tB, tC, Color.red, 100f);
            Debug.DrawLine(tC, tA, Color.red, 100f);
        }
    }

    // Optional debugging to visualize the super triangle and points
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && points != null)
        {
            Gizmos.color = Color.red;

            Vector3 tA = new Vector3(superTriangle.A.x, 0, superTriangle.A.y);
            Vector3 tB = new Vector3(superTriangle.B.x, 0, superTriangle.B.y);
            Vector3 tC = new Vector3(superTriangle.C.x, 0, superTriangle.C.y);

            Gizmos.DrawLine(tA, tB);
            Gizmos.DrawLine(tB, tC);
            Gizmos.DrawLine(tC, tA);

            // Visualize points within the simulation bound
            Gizmos.color = Color.red;
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.2f);
            }
        }
    }
}

public class Triangle
{
    public Vector2 A { get; }
    public Vector2 B { get; }
    public Vector2 C { get; }

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a;
        B = b;
        C = c;
    }

    public bool IsPointInCircumcircle(Vector2 point)
    {
        float ax = A.x - point.x;
        float ay = A.y - point.y;

        float bx = B.x - point.x;
        float by = B.y - point.y;

        float cx = C.x - point.x;
        float cy = C.y - point.y;

        float det =
            (ax * ax + ay * ay) * (bx * cy - by * cx)
            - (bx * bx + by * by) * (ax * cy - ay * cx)
            + (cx * cx + cy * cy) * (ax * by - ay * bx);

        return det > 0;
    }

    public bool Contains(Vector2 point)
    {
        return (A == point || B == point || C == point);
    }
}

public class Edge
{
    public Vector2 Start { get; }
    public Vector2 End { get; }

    public Edge(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }

    public override bool Equals(object obj)
    {
        if (obj is Edge edge)
        {
            return (Start == edge.Start && End == edge.End)
                || (Start == edge.End && End == edge.Start);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Start.GetHashCode() * End.GetHashCode();
    }
}
