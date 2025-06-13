using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIButtonLogger : MonoBehaviour
{
    [Header("Assign all your buttons here")]
    [SerializeField] private List<Button> buttons;

    private void Awake()
    {
        foreach (Button btn in buttons)
        {
            if (btn != null)
            {
                string btnName = btn.name; // Capture name for closure
                btn.onClick.AddListener(() => OnButtonClicked(btnName));
            }
        }
    }

    private void OnButtonClicked(string buttonName)
    {
        Debug.Log($"Button clicked: {buttonName}");
    }
}
