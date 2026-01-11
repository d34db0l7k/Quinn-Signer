using UnityEngine;
using UnityEngine.EventSystems;

namespace Features.Signing
{
    public class HoldToSignButtonRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Place Signer here!")]
        public Signer signer;

        public void OnPointerDown(PointerEventData eventData) { signer?.BeginMobileSign(); }
        public void OnPointerUp(PointerEventData eventData)   { signer?.EndMobileSign();   }
    }
}
