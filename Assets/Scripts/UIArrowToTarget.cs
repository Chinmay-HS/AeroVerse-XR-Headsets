using UnityEngine;

public class UIArrowToTarget : MonoBehaviour
{
    [Header("Target and Camera")]
    public Transform targetToPointTo;
    public Camera mainCamera;

    [Header("Optional")]
    public bool clampToScreenEdge = true;
    public float edgeBuffer = 50f; // pixels from edge

    private RectTransform arrowRect;

    void Start()
    {
        arrowRect = GetComponent<RectTransform>();
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (targetToPointTo == null || mainCamera == null)
            return;

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(targetToPointTo.position);

        // Check if behind camera
        if (screenPoint.z < 0)
        {
            screenPoint *= -1;
        }

        if (clampToScreenEdge)
        {
            screenPoint.x = Mathf.Clamp(screenPoint.x, edgeBuffer, Screen.width - edgeBuffer);
            screenPoint.y = Mathf.Clamp(screenPoint.y, edgeBuffer, Screen.height - edgeBuffer);
        }

        arrowRect.position = screenPoint;

        // Make the arrow point toward the model
        Vector3 dir = (targetToPointTo.position - mainCamera.transform.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0, 0, -angle);
    }
}