using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainedDelaunayTriangulation : TriangulationAlgorithm
{
    public List<Edge> constraints = new List<Edge>();

    private List<Edge> boundaryEdges = new List<Edge>();

    public void Start()
    {
        InitiateVariables();
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

    private void Update()
    {
        // Draw final triangles after simulation
        if (finalTriangles.Count > 0)
        {
            DrawTriangles(finalTriangles, Color.red);
        }
    }

    public override IEnumerator TriangulateCoroutine()
    {
        // Add the super triangle initially to contain all points
        triangles.Add(superTriangle);

        // Insert each point one by one with a delay to visualize
        foreach (var point in points)
        {
            InsertPointRespectingBoundary(point);

            // Draw the current state of triangles
            DrawTriangles(triangles, Color.yellow);

            // Wait for visualization
            yield return new WaitForSeconds(simulationSpeed);
        }

        // Remove triangles that contain any vertex of the super triangle
        triangles.RemoveAll(triangle =>
            triangle.Contains(superTriangle.A)
            || triangle.Contains(superTriangle.B)
            || triangle.Contains(superTriangle.C)
        );

        // Save the final triangles for continuous display
        finalTriangles = new List<Triangle>(triangles);

        // Clear the temporary triangles list
        triangles.Clear();
    }

    private void InsertPointWithConstraints(Vector2 point)
    {
        List<Triangle> badTriangles = new List<Triangle>();
        HashSet<Edge> boundaryEdges = new HashSet<Edge>();

        // Find all "bad" triangles whose circumcircles contain the point
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

        // Remove bad triangles from the main list
        foreach (var badTriangle in badTriangles)
        {
            triangles.Remove(badTriangle);
        }

        // Now ensure we respect constraints on the edges
        // Check the boundary edges and prevent crossing the constraints
        foreach (var edge in boundaryEdges)
        {
            if (!IsConstrainedEdge(edge)) // Ensure it's part of your L-shape boundary
            {
                triangles.Add(new Triangle(edge.Start, edge.End, point));
            }
        }
    }

    private void InsertPointRespectingBoundary(Vector2 point)
    {
        List<Triangle> badTriangles = new List<Triangle>();
        HashSet<Edge> boundaryEdgesToAdd = new HashSet<Edge>();

        // Find all "bad" triangles whose circumcircles contain the point
        foreach (var triangle in triangles)
        {
            if (triangle.IsPointInCircumcircle(point))
            {
                bool isBoundaryTriangle = false;

                // Check if any edge of the triangle is part of the boundary
                if (IsBoundaryTriangle(triangle))
                {
                    isBoundaryTriangle = true;
                }

                if (!isBoundaryTriangle)
                {
                    badTriangles.Add(triangle);

                    // Add edges of the bad triangle, potentially creating boundary edges
                    AddEdge(boundaryEdgesToAdd, triangle.A, triangle.B);
                    AddEdge(boundaryEdgesToAdd, triangle.B, triangle.C);
                    AddEdge(boundaryEdgesToAdd, triangle.C, triangle.A);
                }
            }
        }

        // Remove bad triangles from the main list
        foreach (var badTriangle in badTriangles)
        {
            triangles.Remove(badTriangle);
        }

        // Add new triangles from boundary edges to the point
        foreach (var edge in boundaryEdgesToAdd)
        {
            if (!IsConstrainedEdge(edge))
            {
                triangles.Add(new Triangle(edge.Start, edge.End, point));
            }
        }
    }

    private bool IsBoundaryTriangle(Triangle triangle)
    {
        // Check if the triangle shares any edge with the boundary
        return boundaryEdges.Contains(new Edge(triangle.A, triangle.B))
            || boundaryEdges.Contains(new Edge(triangle.B, triangle.C))
            || boundaryEdges.Contains(new Edge(triangle.C, triangle.A));
    }

    private bool IsConstrainedEdge(Edge edge)
    {
        // Check if the edge is part of your L-shaped boundary (you may hardcode this logic based on your boundary)
        // For example, you can check if the edge matches one of the boundary edges
        return boundaryEdges.Contains(edge);
    }

    private void AddEdge(HashSet<Edge> edges, Vector2 start, Vector2 end)
    {
        var edge = new Edge(start, end);

        // If the edge is a constrained edge (part of the boundary), do not allow flipping
        if (IsConstrainedEdge(edge))
        {
            // Simply add it, since it's part of the boundary
            if (!edges.Add(edge))
            {
                edges.Remove(edge); // Remove if it's already added (internal edge)
            }
        }
        else
        {
            // If it's not constrained, handle it normally
            if (!edges.Add(edge))
            {
                edges.Remove(edge); // Remove if it's already added (internal edge)
            }
        }
    }

    private void AddEdgeWithConstraints(HashSet<Edge> edges, Vector2 start, Vector2 end)
    {
        var edge = new Edge(start, end);
        if (!edges.Add(edge))
            edges.Remove(edge);
    }

    private List<Vector2> CreateSimulationBounds(Vector2 scalar)
    {
        List<Vector2> bounds = new List<Vector2>
        {
            new Vector2(1, 1),
            new Vector2(0.5f, 1),
            new Vector2(0, 1),
            new Vector2(-0.5f, 1),
            new Vector2(-1, 1),
            new Vector2(-1, 0.5f),
            new Vector2(-1, 0),
            new Vector2(-1, -0.5f),
            new Vector2(-1, -1),
            new Vector2(-0.5f, -1),
            new Vector2(0, -1),
            new Vector2(0.5f, -1),
            new Vector2(1, -1),
            new Vector2(1, -0.5f),
            new Vector2(1, 0),
            new Vector2(1, 0.5f),
        };

        // Scale bounds dynamically and add to points list for triangulation
        for (int i = 0; i < bounds.Count; i++)
        {
            bounds[i] *= scalar;
            points.Add(bounds[i]);

            // Store the edges of the boundary explicitly
            if (i < bounds.Count - 1)
            {
                boundaryEdges.Add(new Edge(bounds[i], bounds[i + 1]));
            }
            else
            {
                boundaryEdges.Add(new Edge(bounds[i], bounds[0])); // Close the boundary loop
            }
        }

        return bounds;
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && points != null)
        {
            Gizmos.color = Color.red;
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.2f);
            }

            // Draw the boundary explicitly
            Gizmos.color = Color.blue; // Or any color you prefer
            foreach (var edge in boundaryEdges)
            {
                Gizmos.DrawLine(
                    new Vector3(edge.Start.x, 0, edge.Start.y),
                    new Vector3(edge.End.x, 0, edge.End.y)
                );
            }
        }
    }
}
