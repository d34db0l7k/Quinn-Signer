using Engine;
using Features.Signing;
using UnityEngine;
using UnityEngine.UI;

namespace Core.SceneManagement
{
    public class SceneBindings : MonoBehaviour
    {
        public WordBank wordBank;
        public SimpleExecutionEngine engine;
        public Text inferenceText;
        public Text condifenceScoreText;
        public Image background;
    }
}
