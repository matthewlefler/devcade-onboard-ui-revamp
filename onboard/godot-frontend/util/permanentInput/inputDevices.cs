using Microsoft.Xna.Framework.Input;

namespace onboard.util.permenentInput
{
    public static class KeyLogger
    {
        public static bool anyKeyPressed { get; private set; }

        public static KeyboardState keyboardState { get; private set; }
        public static Keys[] pressedKeyboardKeys { get; private set; }

        static KeyLogger()
        {
            anyKeyPressed = false;
        }

        public static void UpdateKeys()
        {
            
        }
    }
}