using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainedDelaunayTriangulation : MonoBehaviour
{
    [Header("Main Simulation Parameters")]
    public float minBound;
    public float maxBound;
    public float simulationSpeed = 0.1f;
    public int maxPointCount = 10;
    public float superTriangleScale = 3;

    protected List<Vector2> points = new();
    protected List<Triangle> triangles = new List<Triangle>();
    protected List<Triangle> finalTriangles = new List<Triangle>();
    protected Triangle superTriangle;
    protected float simulationScale;
    public List<Edge> constraints = new List<Edge>();

    public void Start()
    {
        // Define the points for the "L" shape
        points = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 2),
            new Vector2(2, 2),
            new Vector2(2, 3),
            new Vector2(0, 3),
        };

        // Define the edges that make up the "L" shape boundary
        List<Edge> constraints = new List<Edge>
        {
            new Edge(points[0], points[1]),
            new Edge(points[1], points[2]),
            new Edge(points[2], points[3]),
            new Edge(points[3], points[4]),
            new Edge(points[4], points[5]),
            new Edge(points[5], points[0]),
        };

        // Start the triangulation process
        StartCoroutine(TriangulateCoroutine());
    }

    protected IEnumerator TriangulateCoroutine()
    {
        triangles.Add(superTriangle);

        foreach (var point in points)
        {
            InsertPointWithConstraints(point);
            DrawTriangles(triangles, Color.blue);
            yield return new WaitForSeconds(simulationSpeed);
        }

        triangles.RemoveAll(triangle =>
            triangle.Contains(superTriangle.A)
            || triangle.Contains(superTriangle.B)
            || triangle.Contains(superTriangle.C)
        );

        finalTriangles = new List<Triangle>(triangles);
        triangles.Clear();
    }

    private void InsertPointWithConstraints(Vector2 point)
    {
        List<Triangle> badTriangles = new List<Triangle>();
        HashSet<Edge> boundaryEdges = new HashSet<Edge>();

        foreach (var triangle in triangles)
        {
            if (triangle.IsPointInCircumcircle(point))
            {
                badTriangles.Add(triangle);
                AddEdgeWithConstraints(boundaryEdges, triangle.A, triangle.B);
                AddEdgeWithConstraints(boundaryEdges, triangle.B, triangle.C);
                AddEdgeWithConstraints(boundaryEdges, triangle.C, triangle.A);
            }
        }

        foreach (var badTriangle in badTriangles)
        {
            triangles.Remove(badTriangle);
        }

        foreach (var edge in boundaryEdges)
        {
            if (!IsConstraintEdge(edge))
            {
                triangles.Add(new Triangle(edge.Start, edge.End, point));
            }
        }
    }

    private void AddEdgeWithConstraints(HashSet<Edge> edges, Vector2 start, Vector2 end)
    {
        var edge = new Edge(start, end);
        if (!edges.Add(edge))
            edges.Remove(edge);
    }

    private bool IsConstraintEdge(Edge edge)
    {
        foreach (var constraint in constraints)
        {
            if (edge.Equals(constraint) || DoEdgesIntersect(edge, constraint))
            {
                return true;
            }
        }
        return false;
    }

    private bool DoEdgesIntersect(Edge edge1, Edge edge2)
    {
        return DoLinesIntersect(edge1.Start, edge1.End, edge2.Start, edge2.End);
    }

    // Helper function to check if two lines intersect
    private bool DoLinesIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        int orientation1 = Orientation(p1, q1, p2);
        int orientation2 = Orientation(p1, q1, q2);
        int orientation3 = Orientation(p2, q2, p1);
        int orientation4 = Orientation(p2, q2, q1);

        // General case: the lines intersect if each segment straddles the other
        if (orientation1 != orientation2 && orientation3 != orientation4)
            return true;

        // Special cases: if the points are collinear, we check if they lie on each other
        if (orientation1 == 0 && OnSegment(p1, p2, q1))
            return true;
        if (orientation2 == 0 && OnSegment(p1, q2, q1))
            return true;
        if (orientation3 == 0 && OnSegment(p2, p1, q2))
            return true;
        if (orientation4 == 0 && OnSegment(p2, q1, q2))
            return true;

        return false;
    }

    // Find orientation of the ordered triplet (p, q, r)
    // 0 -> p, q and r are collinear
    // 1 -> Clockwise
    // 2 -> Counterclockwise
    private int Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        if (Mathf.Approximately(val, 0))
            return 0; // collinear
        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    // Check if point q lies on line segment pr
    private bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        return q.x <= Mathf.Max(p.x, r.x)
            && q.x >= Mathf.Min(p.x, r.x)
            && q.y <= Mathf.Max(p.y, r.y)
            && q.y >= Mathf.Min(p.y, r.y);
    }

    protected virtual Triangle GenerateSuperTriangle(float superTriangleScale, float boundScale)
    {
        float masterScale = boundScale * superTriangleScale;
        Vector2 a = new Vector2(-masterScale, -masterScale / 2);
        Vector2 b = new Vector2(masterScale, -masterScale / 2);
        Vector2 c = new Vector2(0, masterScale);
        return new Triangle(a, b, c);
    }

    protected virtual void GeneratePoints()
    {
        for (int i = 0; i < maxPointCount; i++)
        {
            float x = Random.Range(minBound, maxBound);
            float y = Random.Range(minBound, maxBound);
            points.Add(new Vector2(x, y));
        }
    }

    protected virtual void DrawTriangles(List<Triangle> trianglesToDraw, Color color)
    {
        foreach (var triangle in trianglesToDraw)
        {
            Vector3 tA = new Vector3(triangle.A.x, 0, triangle.A.y);
            Vector3 tB = new Vector3(triangle.B.x, 0, triangle.B.y);
            Vector3 tC = new Vector3(triangle.C.x, 0, triangle.C.y);
            Debug.DrawLine(tA, tB, color, 0.5f);
            Debug.DrawLine(tB, tC, color, 0.5f);
            Debug.DrawLine(tC, tA, color, 0.5f);
        }
    }
}
