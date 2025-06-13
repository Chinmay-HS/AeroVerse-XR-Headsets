using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Annotation : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform targetPart;
    public Camera mainCamera;
    public Vector3 offset = new Vector3(0, 0.3f, 0);

    [Header("Dynamic Sizing")]
    public float minScale = 0.3f;
    public float maxScale = 1.0f;
    public float scaleDistance = 10f; // Distance at which annotation reaches min scale
    public bool useModelBounds = true; // Scale based on target model size
    public float modelSizeMultiplier = 1.0f;

    [Header("Collision Avoidance")]
    public float labelPushDistance = 0.2f;
    public LayerMask obstructionMask;
    public float avoidanceRadius = 0.5f; // Radius to check for other annotations
    public float maxAvoidanceDistance = 2.0f;
    public int avoidanceSteps = 8; // Number of directions to try

    private LineRenderer lineRenderer;
    private Vector3 finalOffset;
    private static List<Annotation> allAnnotations = new List<Annotation>();
    private Bounds targetBounds;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.useWorldSpace = true;

        // Add to global list for collision detection
        allAnnotations.Add(this);

        // Calculate target bounds if using model-based scaling
        if (useModelBounds && targetPart != null)
        {
            CalculateTargetBounds();
        }
    }

    void OnDestroy()
    {
        allAnnotations.Remove(this);
    }

    void CalculateTargetBounds()
    {
        Renderer[] renderers = targetPart.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            targetBounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                targetBounds.Encapsulate(renderer.bounds);
            }
        }
        else
        {
            targetBounds = new Bounds(targetPart.position, Vector3.one);
        }
    }

    Vector3 FindAvoidancePosition(Vector3 desiredPosition)
    {
        Vector3 bestPosition = desiredPosition;
        float bestScore = float.MinValue;

        // Try different angles around the target
        for (int i = 0; i < avoidanceSteps; i++)
        {
            float angle = (360f / avoidanceSteps) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            
            // Try different distances
            for (float distance = 0.1f; distance <= maxAvoidanceDistance; distance += 0.2f)
            {
                Vector3 testPosition = targetPart.position + (direction * distance) + Vector3.up * offset.y;
                float score = EvaluatePosition(testPosition, desiredPosition);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = testPosition;
                }
            }
        }

        return bestPosition;
    }

    float EvaluatePosition(Vector3 position, Vector3 originalDesired)
    {
        float score = 0;

        // Prefer positions closer to original desired position
        float distanceToOriginal = Vector3.Distance(position, originalDesired);
        score -= distanceToOriginal * 2f;

        // Check distance to other annotations
        foreach (Annotation other in allAnnotations)
        {
            if (other == this || other.targetPart == null) continue;
            
            float distanceToOther = Vector3.Distance(position, other.transform.position);
            if (distanceToOther < avoidanceRadius)
            {
                score -= (avoidanceRadius - distanceToOther) * 10f; // Heavy penalty for overlap
            }
        }

        // Prefer positions visible to camera
        Vector3 directionToCamera = (mainCamera.transform.position - position).normalized;
        Ray ray = new Ray(position, directionToCamera);
        if (Physics.Raycast(ray, Vector3.Distance(position, mainCamera.transform.position), obstructionMask))
        {
            score -= 5f; // Penalty for obstruction
        }

        return score;
    }

    void Update()
    {
        if (targetPart == null || mainCamera == null) return;

        // Calculate dynamic scale
        float distanceToCamera = Vector3.Distance(mainCamera.transform.position, targetPart.position);
        float scaleFromDistance = Mathf.Lerp(maxScale, minScale, distanceToCamera / scaleDistance);
        
        float finalScale = scaleFromDistance;
        
        // Apply model size scaling if enabled
        if (useModelBounds)
        {
            float modelSize = Mathf.Max(targetBounds.size.x, targetBounds.size.y, targetBounds.size.z);
            float modelScale = Mathf.Clamp(modelSize * modelSizeMultiplier, 0.1f, 2.0f);
            finalScale *= modelScale;
        }
        
        finalScale = Mathf.Clamp(finalScale, minScale, maxScale);
        transform.localScale = Vector3.one * finalScale;

        // Calculate desired position
        Vector3 desiredPosition = targetPart.position + offset;
        
        // Find position that avoids other annotations
        Vector3 avoidedPosition = FindAvoidancePosition(desiredPosition);
        finalOffset = avoidedPosition - targetPart.position;

        // Additional obstruction check
        Vector3 directionToCamera = (mainCamera.transform.position - avoidedPosition).normalized;
        Ray ray = new Ray(targetPart.position, directionToCamera);
        if (Physics.Raycast(ray, out RaycastHit hit, Vector3.Distance(targetPart.position, mainCamera.transform.position), obstructionMask))
        {
            Vector3 right = Vector3.Cross(directionToCamera, Vector3.up).normalized;
            finalOffset += right * labelPushDistance;
        }

        Vector3 annotationPosition = targetPart.position + finalOffset;
        transform.position = annotationPosition;

        // Face the camera
        transform.LookAt(transform.position + mainCamera.transform.forward);

        // Update line with scaled width
        lineRenderer.startWidth = 0.005f * finalScale;
        lineRenderer.endWidth = 0.005f * finalScale;
        lineRenderer.SetPosition(0, targetPart.position);
        lineRenderer.SetPosition(1, annotationPosition);
    }

    void LateUpdate()
    {
        if (targetPart == null || mainCamera == null) return;

        transform.position = targetPart.position + finalOffset;
        transform.LookAt(mainCamera.transform);
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward);
    }

    // Helper method to manually trigger bounds recalculation
    public void RecalculateBounds()
    {
        if (useModelBounds && targetPart != null)
        {
            CalculateTargetBounds();
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (targetPart == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        
        if (useModelBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(targetBounds.center, targetBounds.size);
        }
    }
}



