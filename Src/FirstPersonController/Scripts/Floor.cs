using UnityEngine;
using TMPro;

public class Floor : MonoBehaviour
{
    public static Floor Instance { get; private set; }

    [SerializeField] private Transform spawn;
    [SerializeField] private TextMeshProUGUI displayText;

    public Transform Spawn => spawn;
    public TextMeshProUGUI DisplayText => displayText;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowText(string message)
    {
        if (displayText != null)
            displayText.text = message;
    }

    public void ClearText()
    {
        if (displayText != null)
            displayText.text = string.Empty;
    }
}