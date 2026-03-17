using UnityEngine;

public class SlowMotionHelper : MonoBehaviour
{
    [SerializeField] private float slowTimeScale = 0.2f;
    private bool isSlowed = false;

    public void ToggleSlowMotion()
    {
        if (isSlowed)
        {
            Time.timeScale = 1f;
            isSlowed = false;
        }
        else
        {
            Time.timeScale = slowTimeScale;
            isSlowed = true;
        }
    }

    
}