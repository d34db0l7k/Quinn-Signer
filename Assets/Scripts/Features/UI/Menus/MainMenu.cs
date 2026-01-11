using UnityEngine;
using UnityEngine.SceneManagement;

namespace Features.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        public void Play()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void Quit()
        {
            Application.Quit();
            Debug.Log("Player Has Quit The Game");
        }
    }
}
