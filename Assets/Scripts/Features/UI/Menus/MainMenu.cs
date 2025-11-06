using UnityEngine;
using UnityEngine.SceneManagement;

namespace Features.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        // Load Play Scene
        public void Play()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        // Quit the Game
        public void Quit()
        {
            Application.Quit();
            Debug.Log("Player Has Quit The Game");
        }
    }
}
