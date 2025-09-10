namespace onboard.util.permenentInput
{
    struct Timeval {
        ulong       tv_sec;   /* Seconds */
        int  tv_usec;  /* Microseconds */
    };
    struct input_event {
        Timeval time;
        ushort type;
        ushort code;
        uint value;
    };

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
            // update number of connected keyboards and gamepads
            // update keyboard and gamepad state
        }
    }
}