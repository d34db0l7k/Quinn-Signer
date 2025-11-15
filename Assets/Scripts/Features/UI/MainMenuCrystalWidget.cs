using UnityEngine.Serialization;

namespace Features.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class MainMenuCrystalWidget : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text legacyText;
        [SerializeField] private RawImage crystalImage;
        [SerializeField] private string amountLabel;

        [Header("Pop Animation")]
        [SerializeField] private RectTransform popTarget;
        [SerializeField] private float popScale = 1.15f;
        [SerializeField] private float popTime = 0.12f;

        private int _lastShown = -1;

        private void OnEnable()
        {
            CrystalWallet.OnChanged += HandleWalletChanged;
            Refresh(true);
        }
        private void OnDisable()
        {
            CrystalWallet.OnChanged -= HandleWalletChanged;
        }

        private void HandleWalletChanged(int _) => Refresh(true);
        
        private void Refresh(bool force = false)
        {
            var count = CrystalWallet.Load();
            if (!force && count == _lastShown) return;

            _lastShown = count;

            var s = amountLabel + count;
            if (legacyText) legacyText.text = s;

            if (popTarget)
            {
                StopAllCoroutines();
                StartCoroutine(Pop());
            }
        }

        private System.Collections.IEnumerator Pop()
        {
            var baseScale = popTarget.localScale;
            var peak = baseScale * popScale;
            var t = 0f;
            while (t < popTime)
            {
                t += Time.unscaledDeltaTime;
                var k = Mathf.Sin((t / popTime) * Mathf.PI);
                popTarget.localScale = Vector3.Lerp(baseScale, peak, k);
                yield return null;
            }
            popTarget.localScale = baseScale;
        }
    }

}