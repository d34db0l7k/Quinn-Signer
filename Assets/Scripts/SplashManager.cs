using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SplashManager : MonoBehaviour
{
    [Header("Company Side")]
    public Text companyLogo;
    public Text presentsText;

    [Header("Game Side")]
    public Text gameLogo;
    public Text tapToPlayText;

    [Header("Timing")]
    public float fadeSpeed = 1.5f;
    public float holdDuration = 2f;

    private bool canTap = false;

    void Start()
    {
        SetAlpha(companyLogo, 0);
        SetAlpha(presentsText, 0);
        SetAlpha(gameLogo, 0);
        SetAlpha(tapToPlayText, 0);

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (canTap && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            SceneManager.LoadScene("TitleScene");
    }

    IEnumerator PlaySequence()
    {
        yield return StartCoroutine(Fade(companyLogo, 0, 1));

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(Fade(presentsText, 0, 1));

        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(FadeTogether(
            new Graphic[] { companyLogo, presentsText }, 1, 0));

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(Fade(gameLogo, 0, 1));
        yield return StartCoroutine(Fade(tapToPlayText, 0, 1));

        canTap = true;
    }

    IEnumerator Fade(Graphic element, float from, float to)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;
            SetAlpha(element, Mathf.Lerp(from, to, t));
            yield return null;
        }
        SetAlpha(element, to);
    }

    IEnumerator FadeTogether(Graphic[] elements, float from, float to)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * fadeSpeed;
            foreach (var el in elements)
                SetAlpha(el, Mathf.Lerp(from, to, t));
            yield return null;
        }
        foreach (var el in elements)
            SetAlpha(el, to);
    }

    void SetAlpha(Graphic element, float alpha)
    {
        Color c = element.color;
        c.a = alpha;
        element.color = c;
    }
}