using UnityEngine;
using Features.Gameplay.Entities.Enemy;
using Features.Signing.Features.Signing;

public class SlowMotionHelper : MonoBehaviour
{
    [SerializeField] private float slowTimeScale = 0.2f;
    [SerializeField] private PreflightScrollUI trainingPanel;
    [SerializeField] private GameObject trainingPanelObject;
    private bool _isSlowed = false;

    public void ToggleSlowMotion()
    {
        if (_isSlowed)
        {
            Time.timeScale = 1f;
            _isSlowed = false;
            if (trainingPanelObject != null)
                trainingPanelObject.SetActive(false);
        }
        else
        {
            Time.timeScale = slowTimeScale;
            _isSlowed = true;
            PrioritizeActiveEnemyVideo();
            if (trainingPanelObject != null)
                trainingPanelObject.SetActive(true);
        }
    }

    private void PrioritizeActiveEnemyVideo()
    {
        var labels = FindObjectsByType<EnemyLabel>(FindObjectsSortMode.None);
        if (labels == null || labels.Length == 0) return;

        string activeWord = null;
        foreach (var label in labels)
        {
            if (label && !string.IsNullOrEmpty(label.targetWord))
            {
                activeWord = label.targetWord;
                break;
            }
        }

        if (string.IsNullOrEmpty(activeWord)) return;
        if (trainingPanel == null) return;

        trainingPanel.BuildListWithPriorityWord(activeWord);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}