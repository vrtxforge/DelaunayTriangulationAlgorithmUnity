using System.Collections.Generic;
using UnityEngine;

public class PlacePointsOnMouseClick : MonoBehaviour
{
    public List<Vector3> points;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlacePoint();
        }
        if (Input.GetMouseButtonDown(1))
        {
            ClearPoints();
        }
    }

    private void PlacePoint()
    {
        Vector3 mousePosition = Input.mousePosition;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        float distance;

        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);

            worldPosition.y = 0;

            Debug.Log("Clicked Position: " + worldPosition);
            points.Add(worldPosition);
            Debug.Log(points.Count);
        }
    }

    private void ClearPoints()
    {
        points.Clear();
        Debug.Log(points.Count);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && points != null)
        {
            Gizmos.color = Color.red;

            foreach (Vector3 point in points)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
        }
    }
}
