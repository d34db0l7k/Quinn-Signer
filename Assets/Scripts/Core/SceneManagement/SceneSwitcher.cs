using System.Collections;
using Features.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    public class SceneSwitcher : MonoBehaviour
    {
        public static SceneSwitcher Instance {get; private set;}

        private void Awake()
        {
            // Ensure only one instance exists and persist across scenes
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SwitchSceneAfterDelay(int buildIdx, float delay)
        {
            StartCoroutine(LoadSceneAfterDelay(buildIdx, delay));
        }

        private static IEnumerator LoadSceneAfterDelay(int buildIdx, float delay)
        {
            yield return new WaitForSeconds(delay);
            CrystalWallet.Save(MasterInfo.CrystalCount);
            SceneManager.LoadScene(buildIdx);
        }
    }
}