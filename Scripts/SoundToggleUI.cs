using UnityEngine;
using UnityEngine.UI;

public class SoundToggleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Sprite soundOnIcon;
    [SerializeField] private Sprite soundOffIcon;

    private Button button;
    private Image buttonImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnButtonPressed);
    }

    private void OnEnable()
    {
        // Subscribe to the event so we are notified of changes
        AudioManager.OnMuteStateChanged += HandleMuteStateChanged;
        // Set the initial state when this object becomes active
        HandleMuteStateChanged(PlayerPrefs.GetInt("IsMuted", 0) == 1);
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        AudioManager.OnMuteStateChanged -= HandleMuteStateChanged;
    }

    private void HandleMuteStateChanged(bool isMuted)
    {
        buttonImage.sprite = isMuted ? soundOffIcon : soundOnIcon;
    }

    private void OnButtonPressed()
    {
        AudioManager.Instance.ToggleSound();
    }
}