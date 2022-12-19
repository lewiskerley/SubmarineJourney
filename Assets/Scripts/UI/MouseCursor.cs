using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursor : MonoBehaviour
{
    public Camera mainCamera;
    private RectTransform cursor;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        cursor = GetComponent<RectTransform>();

        Cursor.visible = false;
    }

    void Update()
    {
        cursor.position = Input.mousePosition;
    }

    Plane xy = new Plane(Vector3.forward, Vector3.zero); //z = 0 plane!
    public Vector3 GetCursorWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        xy.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }
}
