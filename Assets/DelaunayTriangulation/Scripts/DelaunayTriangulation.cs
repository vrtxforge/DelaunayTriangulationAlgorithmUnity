using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    [Header("Main Simulation Parameter")]
    [Tooltip("Minimum bound for random point generation within the simulation area.")]
    public float minBound;

    [Tooltip("Maximum bound for random point generation within the simulation area.")]
    public float maxBound;

    [Tooltip(
        "Speed at which the simulation progresses, affecting delay between each point's insertion."
    )]
    public float simulationSpeed = 0.1f;

    [Space(10)]
    [Tooltip("Maximum number of random points generated within the simulation area.")]
    public int maxPointCount = 10;

    [Tooltip("Scale of the super triangle used to contain all points during triangulation.")]
    public float superTriangleScale = 3;

    private List<Vector2> points = new List<Vector2>();
    private List<Triangle> triangles = new List<Triangle>();
    private List<Triangle> finalTriangles = new List<Triangle>();
    private Triangle superTriangle;

    private float simulationScale;

    private void Start()
    {
        simulationScale = maxBound;

        // Generate super triangle
        superTriangle = GenerateSuperTriangle(superTriangleScale, simulationScale);

        // Generate bounds and add them as points for triangulation
        var bounds = CreateSimulationBounds(new Vector2(minBound, maxBound));

        // Generate random points within the bounds
        GeneratePoints();

        // Start coroutine for visualization
        StartCoroutine(TriangulateCoroutine());
    }

    private Triangle GenerateSuperTriangle(float superTriangleScale, float boundScale)
    {
        float direction = 1;
        float masterScale = boundScale * superTriangleScale;

        // Right bottom
        Vector2 a = new Vector2(-direction * masterScale, -masterScale / 2);

        // Left bottom
        Vector2 b = new Vector2(direction * masterScale, -masterScale / 2);

        // Top
        Vector2 c = new Vector2(0, masterScale);

        return new Triangle(a, b, c);
    }

    private IEnumerator TriangulateCoroutine()
    {
        // Add the super triangle initially to contain all points
        triangles.Add(superTriangle);

        // Insert each point one by one with a delay to visualize
        foreach (var point in points)
        {
            InsertPoint(point);

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

    private void InsertPoint(Vector2 point)
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

        // Create new triangles from the point to each boundary edge
        foreach (var edge in boundaryEdges)
        {
            triangles.Add(new Triangle(edge.Start, edge.End, point));
        }
    }

    private void AddEdge(HashSet<Edge> edges, Vector2 start, Vector2 end)
    {
        var edge = new Edge(start, end);

        // If the edge is already in the set, remove it (since it's internal)
        if (!edges.Add(edge))
        {
            edges.Remove(edge);
        }
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
        }

        return bounds;
    }

    private void GeneratePoints()
    {
        for (int i = 0; i < maxPointCount; i++)
        {
            float x = Random.Range(minBound, maxBound);
            float y = Random.Range(minBound, maxBound);

            Vector2 newPoint = new Vector2(x, y);
            points.Add(newPoint);
        }
    }

    private void DrawTriangles(List<Triangle> trianglesToDraw, Color color)
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

    private void Update()
    {
        if (finalTriangles.Count > 0)
        {
            DrawTriangles(finalTriangles, Color.red);
        }
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
        }
    }
}
