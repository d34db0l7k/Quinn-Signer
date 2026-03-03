using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Text content;

    [SerializeField] private float speed = 5f;

    [SerializeField] private Vector2 onScreenPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 offScreenPosition = new Vector2(2000f, 0f);

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

    public void ShowToast(string toastMessage, float duration)
    {
        content.text = toastMessage;
        gameObject.SetActive(true);
        if (notifyCoroutine != null) StopCoroutine(notifyCoroutine);
        notifyCoroutine = StartCoroutine(Notify(duration));
    }

    IEnumerator Notify(float duration)
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
