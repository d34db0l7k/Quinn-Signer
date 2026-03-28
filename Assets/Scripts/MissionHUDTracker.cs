using UnityEngine;
using UnityEngine.UI;
using Features.Gameplay.Entities.Player;
using Features.CameraManagement;

public class MissionHUDTracker : MonoBehaviour
{
    [System.Serializable]
    public class DialogueTrigger
    {
        public float triggerZ;
        [TextArea] public string dialogue;
    }

    [Header("Player")]
    public Transform player;
    public InfinitePlayerMovement playerMovement;
    public CameraFollow cameraFollow;

    [Header("HUD")]
    public Text dialogueText;
    public GameObject hudPanel;
    public Text continueText;

    [Header("Dialogue Triggers")]
    public DialogueTrigger[] triggers;

    private int _currentTrigger = -1;
    private bool _isReading = false;

    private void Start()
    {
        hudPanel.SetActive(false);
        if (continueText != null)
            continueText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_isReading)
        {
            if (Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                ResumeGame();
            }
            return;
        }

        for (int i = triggers.Length - 1; i >= 0; i--)
        {
            if (player.position.z >= triggers[i].triggerZ)
            {
                if (_currentTrigger != i)
                {
                    _currentTrigger = i;
                    ShowDialogue(triggers[i].dialogue);
                }
                break;
            }
        }
    }

    private void ShowDialogue(string text)
    {
        _isReading = true;
        hudPanel.SetActive(true);
        dialogueText.text = text;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cameraFollow != null)
            cameraFollow.enabled = false;

        if (continueText != null)
        {
            continueText.gameObject.SetActive(true);
            continueText.text = "Tap to continue...";
        }
    }

    private void ResumeGame()
    {
        _isReading = false;
        hudPanel.SetActive(false);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (cameraFollow != null)
            cameraFollow.enabled = true;
    }
}