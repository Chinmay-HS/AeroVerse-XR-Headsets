using UnityEngine;
using System.Collections.Generic;

public class AnnotationManager : MonoBehaviour
{
    public Camera vrCamera;
    public GameObject annotationPrefab;

    [System.Serializable]
    public class AnnotationEntry
    {
        public string partName;
        public string label;
    }

    public List<AnnotationEntry> annotationsToSpawn;

    private GameObject currentModel;
    private List<GameObject> spawnedAnnotations = new List<GameObject>();
    private bool annotationsVisible = false;

    public void SetCurrentModel(GameObject model)
    {
        // Clear existing annotations when switching models
        ClearAllAnnotations();
        
        currentModel = model;
        Debug.Log("[AnnotationManager] Current model set to: " + model.name);
    }

    public void ToggleAnnotations()
    {
        if (annotationsVisible)
        {
            HideAnnotations();
        }
        else
        {
            SpawnAnnotationsOnClick();
        }
    }

    public void SpawnAnnotationsOnClick()
    {
        if (currentModel == null)
        {
            Debug.LogWarning("[AnnotationManager] No model set. Cannot spawn annotations.");
            return;
        }

        // Clear existing annotations first to prevent duplicates
        ClearAllAnnotations();

        PartIdentifier[] parts = currentModel.GetComponentsInChildren<PartIdentifier>(true);
        HashSet<string> foundParts = new HashSet<string>();

        foreach (var entry in annotationsToSpawn)
        {
            bool matched = false;

            foreach (var part in parts)
            {
                if (part.partName == entry.partName)
                {
                    GameObject annotation = Instantiate(annotationPrefab);
                    annotation.name = $"Annotation_{entry.partName}";
                    annotation.transform.SetParent(null); // world-space

                    // Add to our tracking list
                    spawnedAnnotations.Add(annotation);

                    Annotation a = annotation.GetComponent<Annotation>();
                    a.targetPart = part.transform;
                    a.mainCamera = vrCamera;

                    var text = annotation.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null)
                        text.text = entry.label;

                    foundParts.Add(entry.partName);
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                Debug.LogWarning($"[AnnotationManager] Could not find part '{entry.partName}' in model '{currentModel.name}'!");
            }
        }

        annotationsVisible = true;
        Debug.Log($"[AnnotationManager] Annotation spawning complete. {foundParts.Count}/{annotationsToSpawn.Count} annotations spawned.");
    }

    public void HideAnnotations()
    {
        foreach (GameObject annotation in spawnedAnnotations)
        {
            if (annotation != null)
            {
                annotation.SetActive(false);
            }
        }
        annotationsVisible = false;
        Debug.Log("[AnnotationManager] Annotations hidden.");
    }

    public void ShowAnnotations()
    {
        foreach (GameObject annotation in spawnedAnnotations)
        {
            if (annotation != null)
            {
                annotation.SetActive(true);
            }
        }
        annotationsVisible = true;
        Debug.Log("[AnnotationManager] Annotations shown.");
    }

    public void ClearAllAnnotations()
    {
        foreach (GameObject annotation in spawnedAnnotations)
        {
            if (annotation != null)
            {
                DestroyImmediate(annotation);
            }
        }
        spawnedAnnotations.Clear();
        annotationsVisible = false;
        Debug.Log("[AnnotationManager] All annotations cleared.");
    }

    public void ClearAnnotationsByPartName(string partName)
    {
        for (int i = spawnedAnnotations.Count - 1; i >= 0; i--)
        {
            GameObject annotation = spawnedAnnotations[i];
            if (annotation != null && annotation.name == $"Annotation_{partName}")
            {
                DestroyImmediate(annotation);
                spawnedAnnotations.RemoveAt(i);
                Debug.Log($"[AnnotationManager] Cleared annotation for part: {partName}");
            }
        }
    }

    // Utility methods for checking state
    public bool AreAnnotationsVisible()
    {
        return annotationsVisible && spawnedAnnotations.Count > 0;
    }

    public int GetActiveAnnotationCount()
    {
        int count = 0;
        foreach (GameObject annotation in spawnedAnnotations)
        {
            if (annotation != null && annotation.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalAnnotationCount()
    {
        // Remove null references
        spawnedAnnotations.RemoveAll(annotation => annotation == null);
        return spawnedAnnotations.Count;
    }

    // Clean up when object is destroyed
    void OnDestroy()
    {
        ClearAllAnnotations();
    }

    // Optional: Clean up null references periodically
    void Update()
    {
        // Remove destroyed annotations from our list
        for (int i = spawnedAnnotations.Count - 1; i >= 0; i--)
        {
            if (spawnedAnnotations[i] == null)
            {
                spawnedAnnotations.RemoveAt(i);
            }
        }
    }
}