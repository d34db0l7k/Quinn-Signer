using UnityEngine;

namespace Core.Layout
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour {
        private Rect _last;
        private void OnEnable() => Apply();
        private void Update()
        {
            if (Screen.safeArea != _last) Apply();
        }
        private void Apply()
        {
            var rt=(RectTransform)transform; var sa=Screen.safeArea; _last=sa;
            Vector2 min=sa.position, max=sa.position+sa.size;
            min.x/=Screen.width; min.y/=Screen.height;
            max.x/=Screen.width; max.y/=Screen.height;
            rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=rt.offsetMax=Vector2.zero;
        }
    }
}

