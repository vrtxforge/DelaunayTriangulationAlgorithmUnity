using UnityEngine;

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
