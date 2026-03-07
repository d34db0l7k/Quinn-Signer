using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text content;

    [SerializeField] private float speed = 5f;

    private Coroutine notifyCoroutine;

    // Ensure Toasts do not stack and interfere with each other
    public static Toast Instance { get; private set; }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowToast(string toastMessage, float duration, Vector2 onScreenPosition, Vector2 offScreenPosition)
    {
        content.text = toastMessage;
        gameObject.SetActive(true);
        if (notifyCoroutine != null) StopCoroutine(notifyCoroutine);
        notifyCoroutine = StartCoroutine(Notify(duration, onScreenPosition, offScreenPosition));
    }

    IEnumerator Notify(float duration, Vector2 onScreenPosition, Vector2 offScreenPosition)
    {
        rectTransform.anchoredPosition = offScreenPosition;
        while (Vector2.Distance(rectTransform.anchoredPosition, onScreenPosition) > 2f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, onScreenPosition, Time.deltaTime * speed);
            yield return null;
        }
        rectTransform.anchoredPosition = onScreenPosition;
        yield return new WaitForSeconds(duration);
        while (Vector2.Distance(rectTransform.anchoredPosition, offScreenPosition) > 2f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, offScreenPosition, Time.deltaTime * speed);
            yield return null;
        }
        rectTransform.anchoredPosition = offScreenPosition;
        gameObject.SetActive(false);
    } 
}
