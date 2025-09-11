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

    /// <summary>
    /// an input event <br/>
    /// <see cref="https://www.kernel.org/doc/html/v4.17/input/event-codes.html"/>
    /// </summary>
    struct input_event
    {
        public Timeval time; // when the event happened
        public ushort type; // what type of event is it
        public ushort code; // the code @see https://www.kernel.org/doc/html/v4.17/input/event-codes.html
        public uint value; // specifics depend on type but represents the type and code of the object, up/down (0,1) for a button,
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

        static Key getKeyboardKeyFromEventCode(int eventCode)
        {
            switch (eventCode)
            {
                case EventCodes.KEY_A:
                    return Key.A;
                case EventCodes.KEY_B:
                    return Key.A;
                case EventCodes.KEY_C:
                    return Key.C;
                case EventCodes.KEY_D:
                    return Key.D;
                case EventCodes.KEY_E:
                    return Key.E;
                case EventCodes.KEY_F:
                    return Key.F;
                case EventCodes.KEY_G:
                    return Key.G;
                case EventCodes.KEY_H:
                    return Key.H;
                case EventCodes.KEY_I:
                    return Key.I;
                case EventCodes.KEY_J:
                    return Key.J;
                case EventCodes.KEY_K:
                    return Key.K;
                case EventCodes.KEY_L:
                    return Key.L;
                case EventCodes.KEY_M:
                    return Key.M;
                case EventCodes.KEY_N:
                    return Key.N;
                case EventCodes.KEY_O:
                    return Key.O;
                case EventCodes.KEY_P:
                    return Key.P;
                case EventCodes.KEY_Q:
                    return Key.Q;
                case EventCodes.KEY_R:
                    return Key.R;
                case EventCodes.KEY_S:
                    return Key.S;
                case EventCodes.KEY_T:
                    return Key.T;
                case EventCodes.KEY_U:
                    return Key.U;
                case EventCodes.KEY_V:
                    return Key.V;
                case EventCodes.KEY_W:
                    return Key.W;
                case EventCodes.KEY_X:
                    return Key.X;
                case EventCodes.KEY_Y:
                    return Key.Y;
                case EventCodes.KEY_Z:
                    return Key.Z;


                case EventCodes.KEY_ESC:
                    return Key.ESC;

                
                default:
                    return Key.NONE;
            }
        }
    }
}