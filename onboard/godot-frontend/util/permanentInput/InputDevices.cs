using System;
using System.IO;
using System.Runtime.InteropServices;

namespace onboard.util.permenentInput
{
    struct Timeval
    {
        public uint tv_sec;   /* Seconds */
        public int tv_usec;  /* Microseconds */
    };

    struct input_event
    {
        public Timeval time; // when the event happened
        public ushort type; // what type
        public ushort code; // the code @see https://www.kernel.org/doc/html/v4.17/input/event-codes.html
        public uint value; // specifics depend on type but represents the value of the object, up/down for a button, angle for a joystick
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
            // read from /dev/input/eventX

            Span<byte> fileBytes = File.ReadAllBytes("/dev/input/event0");

            // update number of connected keyboards and gamepads

            // update keyboard and gamepad state
            // translate from bytes to struct
            if (fileBytes.Length < 0)
            {
                //failure
            }

            Span<input_event> input_Events = MemoryMarshal.Cast<byte, input_event>(fileBytes);
            
        }
    }
}