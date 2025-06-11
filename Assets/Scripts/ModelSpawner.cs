// ModelSpawner.cs
using UnityEngine;
using System.Collections.Generic;

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
    
    [Header("Spawn Settings")]
    [SerializeField] private bool destroyPreviousModel = true;
    [SerializeField] private bool enableModelInteraction = true;
    [SerializeField] private LayerMask spawnLayer = 0;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private GameObject currentSpawnedModel;
    private Dictionary<string, ModelData> modelDictionary;
    
    void Start()
    {
        InitializeModelDictionary();
        DebugAvailableModels();
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
            Debug.Log("[ModelSpawner] Available models:");
            foreach (var key in modelDictionary.Keys)
            {
                Debug.Log("  - " + key);
            }
            return;
        }
        
        // Clear previous model
        if (destroyPreviousModel && currentSpawnedModel != null)
        {
            Debug.Log("[ModelSpawner] Destroying previous model: " + currentSpawnedModel.name);
            DestroyImmediate(currentSpawnedModel);
        }
        
        ModelData modelData = modelDictionary[modelName];
        
        // Calculate spawn transform
        Vector3 finalPosition = position + modelData.spawnOffset;
        Quaternion finalRotation = rotation * Quaternion.Euler(modelData.spawnRotation);
        
        // Spawn model
        currentSpawnedModel = Instantiate(modelData.prefab, finalPosition, finalRotation);
        currentSpawnedModel.name = "Spawned_" + modelName;
        
        // Apply scale
        if (modelData.spawnScale != 1f)
        {
            currentSpawnedModel.transform.localScale = Vector3.one * modelData.spawnScale;
        }
        
        // Set layer
        if (spawnLayer.value != 0)
        {
            SetLayerRecursively(currentSpawnedModel, LayerMaskToLayer(spawnLayer));
        }
        
        // Add VR interaction
        if (enableModelInteraction)
        {
            AddVRInteraction(currentSpawnedModel);
        }
        
        Debug.Log("[ModelSpawner] Successfully spawned '" + modelName + "' at " + finalPosition);
    }
    
    void AddVRInteraction(GameObject model)
    {
        // Add Rigidbody if missing
        if (model.GetComponent<Rigidbody>() == null)
        {
            var rb = model.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
        }
        
        // Add Collider if missing
        if (model.GetComponent<Collider>() == null)
        {
            model.AddComponent<BoxCollider>();
        }
        
        // Add XR Grab Interactable (uncomment if using XR Toolkit)
        /*
        if (model.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>() == null)
        {
            model.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
        }
        */
        
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
    
    // Debug methods
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
    
    [ContextMenu("List Dictionary Contents")]
    void ListDictionaryContents()
    {
        if (modelDictionary == null)
        {
            Debug.Log("Dictionary is null!");
            return;
        }
        
        Debug.Log("=== DICTIONARY CONTENTS ===");
        foreach (var kvp in modelDictionary)
        {
            Debug.Log("Key: '" + kvp.Key + "' -> Prefab: " + kvp.Value.prefab.name);
        }
    }
}