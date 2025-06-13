using UnityEngine;
using UnityEngine.UI;

public class ModelScaleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private ModelSpawner modelSpawner;
    [SerializeField] private Text scaleText; // Optional UI display

    private void Start()
    {
        if (scaleSlider != null)
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);

        if (modelSpawner != null)
            modelSpawner.OnModelSpawnedWithScale += ResetSliderValue;

        // Optional: initialize display text
        if (scaleText != null && scaleSlider != null)
            scaleText.text = scaleSlider.value.ToString("F2");
    }

    private void OnScaleChanged(float newScale)
    {
        if (modelSpawner != null)
        {
            modelSpawner.SetCurrentModelScale(newScale);
        }

        if (scaleText != null)
        {
            scaleText.text = newScale.ToString("F2");
        }
    }

    private void ResetSliderValue(float scale)
    {
        // Prevent triggering OnScaleChanged again
        if (scaleSlider != null)
        {
            scaleSlider.SetValueWithoutNotify(scale);
        }

        if (scaleText != null)
        {
            scaleText.text = scale.ToString("F2");
        }
    }

    private void OnDestroy()
    {
        if (modelSpawner != null)
            modelSpawner.OnModelSpawnedWithScale -= ResetSliderValue;
    }
}