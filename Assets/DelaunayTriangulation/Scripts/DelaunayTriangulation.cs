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

        superTriangle = GenerateSuperTriangle(triangleScale, simulationScale, simulationPosition);

        GeneratePoints();

        Triangulate();
        DrawTriangles();
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
        triangles.Add(superTriangle);

        foreach (var point in points)
        {
            InsertPoint(point);
        }
    }

    private void InsertPoint(Vector2 point)
    {
        List<Triangle> badTriangles = new();

        //find bad triangles (triangles with circumcircle containing points)
        foreach (var triangle in triangles)
        {
            if (triangle.IsPointInCircumcircle(point))
            {
                badTriangles.Add(triangle);
            }
        }

        //remove bad triangles
        foreach (var badTriangle in badTriangles)
        {
            triangles.Remove(badTriangle);
        }

        //create new triangles from the point to the edges of the bad triangles
        HashSet<Edge> edges = new HashSet<Edge>();

        foreach (var badTriangle in badTriangles)
        {
            edges.Add(new Edge(badTriangle.A, badTriangle.B));
            edges.Add(new Edge(badTriangle.B, badTriangle.C));
            edges.Add(new Edge(badTriangle.C, badTriangle.A));
        }

        //create new triangles
        foreach (var edge in edges)
        {
            triangles.Add(new Triangle(edge.Start, edge.End, point));
        }
    }

    //for now this is a perfect square
    private Vector2[] CreateSimulationBounds(Vector2 scalar)
    {
        //creates a constant square
        Vector2[] bounds =
        {
            // constant basic square unit (1)
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1),
        };

        for (int i = 0; i < bounds.Length; i++)
        {
            // and just scale the square dynamically here
            bounds[i] *= scalar;
        }

        return bounds;
    }

    void DrawTriangles()
    {
        foreach (var triangle in triangles)
        {
            Vector3 tA = new(triangle.A.x, 0, triangle.A.y);
            Vector3 tB = new(triangle.B.x, 0, triangle.B.y);
            Vector3 tC = new(triangle.C.x, 0, triangle.C.y);

            Debug.DrawLine(tA, tB, Color.red, 100f);
            Debug.DrawLine(tB, tC, Color.red, 100f);
            Debug.DrawLine(tC, tA, Color.red, 100f);
        }
    }

    //for debugging purpose only
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && points != null)
        {
            //bounds
            Gizmos.color = Color.green;
            foreach (Vector2 bound in CreateSimulationBounds(new Vector2(minBound.x, maxBound.x)))
            {
                Gizmos.DrawSphere(new Vector3(bound.x, 0, bound.y), 0.2f);
            }

            //debug super triangle
            Gizmos.color = Color.red;

            Vector3 tA = new(superTriangle.A.x, 0, superTriangle.A.y);
            Vector3 tB = new(superTriangle.B.x, 0, superTriangle.B.y);
            Vector3 tC = new(superTriangle.C.x, 0, superTriangle.C.y);

            Gizmos.DrawLine(tA, tB);
            Gizmos.DrawLine(tB, tC);
            Gizmos.DrawLine(tC, tA);

            //points within the simulation bound
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
