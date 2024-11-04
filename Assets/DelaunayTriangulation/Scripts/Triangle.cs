using UnityEngine;

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
