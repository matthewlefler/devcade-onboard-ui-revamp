namespace onboard.util.permenentInput
{
    public class KeyLogger
    {
        public bool anythingPressed { get; private set; }

        public int numberOfGamepads { get; private set; }

        public int numberOfKeyboards { get; private set; }


        public KeyLogger()
        {
            anythingPressed = false;
        }

        public static void UpdateKeys()
        {
            // read from /dev/input
            // update connected keyboards and gamepads
            // update keyboard and gamepad state
        }
    }
}