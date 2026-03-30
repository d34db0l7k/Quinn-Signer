using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SplashManager : MonoBehaviour
{
    [Header("Company Side")]
    public Image companyLogo;
    public Text presentsText;

    [Header("Game Side")]
    public Image gameLogo;
    public Text tapToPlayText;

    [Header("Effects")]
    public GameObject smokeEffect;

    [Header("Camera")]
    public UnityEngine.Camera splashCamera;
    public Color companyBackgroundColor = Color.white;
    public Color gameBackgroundColor = Color.white;

    [Header("Timing")]
    public float fadeSpeed = 1.5f;

    private bool canTap = false;

    void Start()
    {
        SetAlpha(companyLogo, 0);
        SetAlpha(presentsText, 0);
        SetAlpha(gameLogo, 0);
        SetAlpha(tapToPlayText, 0);

        if (smokeEffect != null)
            smokeEffect.SetActive(false);

        if (splashCamera != null)
            splashCamera.backgroundColor = companyBackgroundColor;

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (canTap && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            SceneManager.LoadScene("TitleScene");
    }

    IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(Fade(companyLogo, 0, 1));

        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(Fade(presentsText, 0, 1));

        yield return new WaitForSeconds(1.5f);

        // Trigger smoke and instantly hide both
        if (smokeEffect != null)
            smokeEffect.SetActive(true);

        SetAlpha(companyLogo, 0);
        SetAlpha(presentsText, 0);

        yield return new WaitForSeconds(1f); // let smoke play first

        // Fade background color
        if (splashCamera != null)
            yield return StartCoroutine(FadeBackgroundColor(companyBackgroundColor, gameBackgroundColor, 1.5f));

        yield return new WaitForSeconds(2f);

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

    IEnumerator FadeBackgroundColor(Color from, Color to, float duration)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            splashCamera.backgroundColor = Color.Lerp(from, to, t);
            yield return null;
        }
        splashCamera.backgroundColor = to;
    }

    void SetAlpha(Graphic element, float alpha)
    {
        Color c = element.color;
        c.a = alpha;
        element.color = c;
    }
}