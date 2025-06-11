// DropdownController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown modelDropdown;
    [SerializeField] private Button spawnButton;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ModelSpawner modelSpawner;
    
    [Header("Model Names (Must Match ModelSpawner)")]
    [SerializeField] private string[] modelNames = new string[]
    {
        "Car", "House", "Tree", "Robot"
    };
    
    void Start()
    {
        SetupDropdownOptions();
        
        // Add listeners
        spawnButton.onClick.AddListener(OnSpawnButtonClicked);
        modelDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        
        Debug.Log("[DropdownController] Initialized with " + modelNames.Length + " models");
    }
    
    void SetupDropdownOptions()
    {
        modelDropdown.options.Clear();
        
        // Add default option
        modelDropdown.options.Add(new TMP_Dropdown.OptionData("-- Select Model --"));
        
        // Add model options
        foreach (string modelName in modelNames)
        {
            modelDropdown.options.Add(new TMP_Dropdown.OptionData(modelName));
            Debug.Log("[DropdownController] Added option: " + modelName);
        }
        
        modelDropdown.value = 0;
        modelDropdown.RefreshShownValue();
        spawnButton.interactable = false;
    }
    
    void OnDropdownValueChanged(int index)
    {
        bool validSelection = index > 0 && index <= modelNames.Length;
        spawnButton.interactable = validSelection;
        
        if (validSelection)
        {
            string selectedModel = modelNames[index - 1];
            Debug.Log("[DropdownController] Selected: " + selectedModel);
        }
        else
        {
            Debug.Log("[DropdownController] No valid model selected");
        }
    }
    
    void OnSpawnButtonClicked()
    {
        if (modelDropdown.value > 0 && modelDropdown.value <= modelNames.Length)
        {
            string selectedModel = modelNames[modelDropdown.value - 1];
            Debug.Log("[DropdownController] Attempting to spawn: " + selectedModel);
            
            if (modelSpawner != null)
            {
                modelSpawner.SpawnModel(selectedModel, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.LogError("[DropdownController] ModelSpawner is null!");
            }
        }
        else
        {
            Debug.LogWarning("[DropdownController] Invalid selection for spawning");
        }
    }
    
    // Debug method to check connections
    [ContextMenu("Debug Connections")]
    void DebugConnections()
    {
        Debug.Log("=== DROPDOWN CONTROLLER DEBUG ===");
        Debug.Log("Model Dropdown: " + (modelDropdown != null ? "Connected" : "NULL"));
        Debug.Log("Spawn Button: " + (spawnButton != null ? "Connected" : "NULL"));
        Debug.Log("Spawn Point: " + (spawnPoint != null ? "Connected" : "NULL"));
        Debug.Log("Model Spawner: " + (modelSpawner != null ? "Connected" : "NULL"));
        Debug.Log("Model Names Count: " + modelNames.Length);
        for (int i = 0; i < modelNames.Length; i++)
        {
            Debug.Log("  Model " + i + ": " + modelNames[i]);
        }
    }
}