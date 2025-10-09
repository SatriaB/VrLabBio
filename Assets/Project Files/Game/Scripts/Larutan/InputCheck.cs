using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FatahDev;

public class InputCheck : MonoBehaviour
{
    [Header("References")]
    public TMP_InputField inputField;
    public TMP_InputField.ContentType contentType;
    public Button targetButton;
    public SimpleVRLPost post;

    void Start()
    {
        if (inputField == null || targetButton == null)
        {
            Debug.LogError("Assign inputField and targetButton in the inspector!");
            return;
        }

        // Initial check
        CheckInput(inputField.text);

        // Add listener to detect changes
        inputField.onValueChanged.AddListener((v) =>
        {
            post.result = inputField.text.ToString();
            CheckInput(v);
        });
        //targetButton.onClick.AddListener(() =>
        //{
        //    post.result = inputField.text.ToString();
        //});
    }

    void CheckInput(string value)
    {
        inputField.contentType = contentType;
        // Disable button if empty or only whitespace
        targetButton.interactable = !string.IsNullOrWhiteSpace(value);
    }

    void OnDestroy()
    {
        // Clean up listener
        inputField.onValueChanged.RemoveListener(CheckInput);
    }
}

