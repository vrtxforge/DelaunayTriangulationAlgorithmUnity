using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangulationAlgorithm : MonoBehaviour
{
    [Header("Main Simulation Parameters")]
    public float minBound;
    public float maxBound;
    public float simulationSpeed = 0.1f;
    public int maxPointCount = 10;
    public float superTriangleScale = 3;

    protected List<Vector2> points = new List<Vector2>();
    protected List<Triangle> triangles = new List<Triangle>();
    protected List<Triangle> finalTriangles = new List<Triangle>();
    protected Triangle superTriangle;
    protected float simulationScale;

    public void InitiateVariables(bool autoGeneratePoints = false)
    {
        simulationScale = maxBound;
        superTriangle = GenerateSuperTriangle(superTriangleScale, simulationScale);
        if (autoGeneratePoints)
        {
            GeneratePoints();
        }
    }

    public virtual IEnumerator TriangulateCoroutine()
    {
        yield return null;
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
