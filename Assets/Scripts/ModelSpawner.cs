using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Playables;
using Unity.VisualScripting;

[System.Serializable]
public class ModelData
{
    [Header("Model Configuration")]
    public string modelName;
    public GameObject prefab;

    [Header("Spawn Adjustments")]
    public Vector3 spawnOffset = Vector3.zero;
    public Vector3 spawnRotation = Vector3.zero;
    public float spawnScale = 1f;
}

public class ModelSpawner : MonoBehaviour
{
    [Header("Available Models")]
    [SerializeField] private ModelData[] availableModels;

    [Header("Animation Buttons")]
    [SerializeField] private Button explodeButton;
    [SerializeField] private Button assembleButton;
    [Header("Timelines for Jet Engine")]
    [SerializeField] private PlayableAsset normalTimeline;
    [SerializeField] private PlayableAsset reversedTimeline;
    private bool stateManager;
    

    [Header("Spawn Settings")]
    [SerializeField] private bool destroyPreviousModel = true;
    [SerializeField] private bool enableModelInteraction = true;
    [SerializeField] private LayerMask spawnLayer = 0;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private GameObject currentSpawnedModel;
    private Dictionary<string, ModelData> modelDictionary;
    private Vector3 originalModelScale = Vector3.one;

    private AnnotationManager annotationManager;
    private Animator animator;
    private PlayableDirector director;

    // Event to inform listeners (like slider UI)
    public System.Action<float> OnModelSpawnedWithScale;

    void Start()
    {
        InitializeModelDictionary();
        DebugAvailableModels();

        annotationManager = FindObjectOfType<AnnotationManager>();
        if (annotationManager == null)
        {

            Debug.LogWarning("[ModelSpawner] AnnotationManager not found in scene!");
        }
        explodeButton.onClick.AddListener(explode);
        assembleButton.onClick.AddListener(assemble);
    }

    void InitializeModelDictionary()
    {
        modelDictionary = new Dictionary<string, ModelData>();

        foreach (var model in availableModels)
        {
            if (string.IsNullOrEmpty(model.modelName))
            {
                Debug.LogError("[ModelSpawner] Model with empty name found!");
                continue;
            }

            if (model.prefab == null)
            {
                Debug.LogError("[ModelSpawner] Model '" + model.modelName + "' has no prefab assigned!");
                continue;
            }

            if (modelDictionary.ContainsKey(model.modelName))
            {
                Debug.LogError("[ModelSpawner] Duplicate model name: " + model.modelName);
                continue;
            }

            modelDictionary[model.modelName] = model;

            if (enableDebugLogs)
                Debug.Log("[ModelSpawner] Registered model: " + model.modelName);
        }
    }

    public void SpawnModel(string modelName, Vector3 position, Quaternion rotation)
    {
        Debug.Log("[ModelSpawner] Spawn request for: '" + modelName + "'");

        if (string.IsNullOrEmpty(modelName))
        {
            Debug.LogError("[ModelSpawner] Model name is null or empty!");
            return;
        }

        if (!modelDictionary.ContainsKey(modelName))
        {
            Debug.LogError("[ModelSpawner] Model '" + modelName + "' not found in dictionary!");    
            return;
        }

        if (destroyPreviousModel && currentSpawnedModel != null)
        {
            Debug.Log("[ModelSpawner] Destroying previous model: " + currentSpawnedModel.name);
            DestroyImmediate(currentSpawnedModel);
        }

        ModelData modelData = modelDictionary[modelName];

        Vector3 finalPosition = position + modelData.spawnOffset;
        Quaternion finalRotation = rotation * Quaternion.Euler(modelData.spawnRotation);

        currentSpawnedModel = Instantiate(modelData.prefab, finalPosition, finalRotation);
        currentSpawnedModel.name = "Spawned_" + modelName;

        // Save original prefab scale
        originalModelScale = modelData.prefab.transform.localScale;

        // Apply combined scale
        currentSpawnedModel.transform.localScale = originalModelScale * modelData.spawnScale;

        if (spawnLayer.value != 0)
        {
            SetLayerRecursively(currentSpawnedModel, LayerMaskToLayer(spawnLayer));
        }

        if (enableModelInteraction)
        {
            AddVRInteraction(currentSpawnedModel);
        }

        Debug.Log("[ModelSpawner] Successfully spawned '" + modelName + "' at " + finalPosition);

        OnModelSpawnedWithScale?.Invoke(modelData.spawnScale);

        if (annotationManager != null)
        {
            annotationManager.SetCurrentModel(currentSpawnedModel);
            Debug.Log("[ModelSpawner] Set current model for annotation: " + currentSpawnedModel.name);
        }
        else
        {
            Debug.LogWarning("[ModelSpawner] No AnnotationManager found in scene!");
        }
        stateManager = false;
        if (currentSpawnedModel.name == "Spawned_Jet")
        {
            director = currentSpawnedModel.GetComponent<PlayableDirector>();
            animator = currentSpawnedModel.GetComponent<Animator>();

            if (director == null)
            {
                Debug.LogWarning("[ModelSpawner] No Playable Director found on spawned model!");

            }

            if (animator == null)
            {
                Debug.LogWarning("[ModelSpawner] No AnimationController found on spawned model!");
            }
        }
        else
        {
            animator = currentSpawnedModel.GetComponent<Animator>();
            if (animator == null)
            {

                Debug.LogWarning("[ModelSpawner] No AnimationController found on spawned model!");
            }
        }
    }

    public void SetCurrentModelScale(float scale)
    {
        if (currentSpawnedModel != null)
        {
            currentSpawnedModel.transform.localScale = originalModelScale * scale;
            Debug.Log("[ModelSpawner] Updated model scale to: " + scale);
        }
    }

    void AddVRInteraction(GameObject model)
    {
        Rigidbody rb = model.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = model.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        if (model.GetComponent<Collider>() == null)
        {
            model.AddComponent<BoxCollider>();
        }

        Debug.Log("[ModelSpawner] Added VR interaction to: " + model.name);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    int LayerMaskToLayer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 1)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber;
    }

    public void ClearSpawnedModel()
    {
        if (currentSpawnedModel != null)
        {
            Debug.Log("[ModelSpawner] Clearing spawned model: " + currentSpawnedModel.name);
            DestroyImmediate(currentSpawnedModel);
            currentSpawnedModel = null;
        }
    }

    [ContextMenu("Debug Available Models")]
    void DebugAvailableModels()
    {
        Debug.Log("=== MODEL SPAWNER DEBUG ===");
        Debug.Log("Total models configured: " + availableModels.Length);
        Debug.Log("Models in dictionary: " + (modelDictionary != null ? modelDictionary.Count : 0));

        for (int i = 0; i < availableModels.Length; i++)
        {
            var model = availableModels[i];
            Debug.Log("Model " + i + ":");
            Debug.Log("  Name: '" + model.modelName + "'");
            Debug.Log("  Prefab: " + (model.prefab != null ? model.prefab.name : "NULL"));
            Debug.Log("  Offset: " + model.spawnOffset);
            Debug.Log("  Rotation: " + model.spawnRotation);
            Debug.Log("  Scale: " + model.spawnScale);
        }
    }
    
    private void explode()
    {
        Debug.Log(currentSpawnedModel.name);
        if(currentSpawnedModel == null)
        {
            Debug.Log("No Model selected");
            return;
        }
        if(stateManager == false)
        {
            if (currentSpawnedModel.name == "Spawned_Jet")
            {
                if (director.playableAsset != normalTimeline)
                {
                    director.playableAsset = normalTimeline;
                    director.Play();
                    stateManager = true;
                    return;
                }
                Debug.Log("if entered");
                director.Play();
                stateManager = true;
                return;
            }

            animator.Play("ExplodedState");
            stateManager = true;
        }
        
    }
    
    private void assemble()
    {
        Debug.Log(currentSpawnedModel.name);
        if (currentSpawnedModel == null)
        {
            Debug.Log("No Model selected");
            return;
        }
        if (stateManager == true)
        {
            if (currentSpawnedModel.name == "Spawned_Jet")
            {
                Debug.Log("if Entered");
                if (director.playableAsset != reversedTimeline)
                {
                    director.playableAsset = reversedTimeline;
                    director.Play();
                    stateManager = false;
                    return;
                }
                director.Play();
                stateManager = false;
                return;
            }

            animator.Play("AssembledState");
            stateManager = false;
        }
        
    }
}
