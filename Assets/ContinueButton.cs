using UnityEngine;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    private Button continueButton;

    private void Start()
    {
        continueButton = GetComponent<Button>();
        // Check if a saved game exists and set the button's visibility
        bool hasSaveData = PlayerPrefs.HasKey("CardMatch_Save");
        continueButton.interactable = hasSaveData;
    }
}