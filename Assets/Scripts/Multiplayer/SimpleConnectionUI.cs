using Unity.Netcode;
using UnityEngine;

namespace Multiplayer
{
    public class SimpleConnectionUI : MonoBehaviour
    {
        public void StartHost()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.StartHost();
        }

        public void StartClient()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.StartClient();
        }
    }
}