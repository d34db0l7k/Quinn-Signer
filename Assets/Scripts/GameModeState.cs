namespace Features.Signing
{
    public static class GameModeState
    {
        public static bool HintTypingModeActive = false;

        public static void SetHintTypingMode(bool active)
        {
            HintTypingModeActive = active;
        }
    }
}