using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Godot;

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
    struct InputEvent
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
        public Dictionary<string, xBoxGamePad> gamepads;


        public int numberOfKeyboards { get; private set; }
        public Dictionary<string, keyboardState> keyboards;

        private DirectoryInfo eventDirectory = new DirectoryInfo("/dev/input");

        private const int sizeOfInputEvent = 24;

        public KeyLogger()
        {
            anythingPressed = false;

            gamepads = new Dictionary<string, xBoxGamePad>();
            keyboards = new Dictionary<string, keyboardState>();
        }

        public void UpdateKeys()
        {
            // read from /dev/input/eventX
            FileInfo[] files = eventDirectory.GetFiles("*.txt");

            anythingPressed = false;
            
            foreach (FileInfo file in files)
            {
                Span<byte> fileBytes = File.ReadAllBytes(file.FullName);

                string fileName = file.Name;

                // update number of connected keyboards and gamepads

                // update keyboard and gamepad state
                // translate from bytes to struct
                if (fileBytes.Length < 0)
                {
                    //failure
                    continue;
                }

                if (fileBytes.Length % sizeOfInputEvent != 0)
                {
                    //failure
                    continue;
                }

                Span<InputEvent> input_Events = MemoryMarshal.Cast<byte, InputEvent>(fileBytes);

                for (int i = 0; i < input_Events.Length; i++)
                {
                    anythingPressed = true;
                    if (input_Events[i].type == EventTypes.EV_KEY)
                    {
                        Key key = getKeyboardKeyFromEventCode(input_Events[i].code);
                        if (!keyboards.ContainsKey(fileName))
                        {
                            keyboards.Add(fileName, new keyboardState());
                        }

                        switch (input_Events[i].value)
                        {
                            case 1:
                                keyboards[fileName].pressedKeys.Add(key);
                                break;

                            case 0:
                                keyboards[fileName].pressedKeys.Remove(key);
                                break;

                            default:
                                keyboards[fileName].pressedKeys.Remove(key);
                                break;
                        }
                    }

                }

            }




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

                case EventCodes.KEY_F1:
                    return Key.F1;
                case EventCodes.KEY_F2:
                    return Key.F2;
                case EventCodes.KEY_F3:
                    return Key.F3;
                case EventCodes.KEY_F4:
                    return Key.F4;
                case EventCodes.KEY_F5:
                    return Key.F5;
                case EventCodes.KEY_F6:
                    return Key.F6;
                case EventCodes.KEY_F7:
                    return Key.F7;
                case EventCodes.KEY_F8:
                    return Key.F8;
                case EventCodes.KEY_F9:
                    return Key.F9;
                case EventCodes.KEY_F10:
                    return Key.F10;
                case EventCodes.KEY_F11:
                    return Key.F11;
                case EventCodes.KEY_F12:
                    return Key.F12;
 
                case EventCodes.KEY_ESC:
                    return Key.ESC;

                case EventCodes.KEY_0:
                    return Key.ZERO;
                case EventCodes.KEY_1:
                    return Key.ONE;
                case EventCodes.KEY_2:
                    return Key.TWO;
                case EventCodes.KEY_3:
                    return Key.THREE;
                case EventCodes.KEY_4:
                    return Key.FOUR;
                case EventCodes.KEY_5:
                    return Key.FIVE;
                case EventCodes.KEY_6:
                    return Key.SIX;
                case EventCodes.KEY_7:
                    return Key.SEVEN;
                case EventCodes.KEY_8:
                    return Key.EIGHT;
                case EventCodes.KEY_9:
                    return Key.NINE;

                case EventCodes.KEY_MINUS:
                    return Key.MINUS;
                case EventCodes.KEY_EQUAL:
                    return Key.EQUALS;
                case EventCodes.KEY_GRAVE: // this thing: `
                    return Key.TILDE;
                case EventCodes.KEY_BACKSPACE:
                    return Key.BACKSPACE;
                case EventCodes.KEY_TAB:
                    return Key.TAB;
                case EventCodes.KEY_LEFTBRACE:
                    return Key.LBRACKET;
                case EventCodes.KEY_RIGHTBRACE:
                    return Key.RBRACKET;
                case EventCodes.KEY_BACKSLASH:
                    return Key.BACKSLASH;
                case EventCodes.KEY_CAPSLOCK:
                    return Key.CAPSLOCK;
                case EventCodes.KEY_SEMICOLON:
                    return Key.SEMICOLON;
                case EventCodes.KEY_APOSTROPHE:
                    return Key.APOSTROPHE;
                case EventCodes.KEY_COMMA:
                    return Key.COMMA;
                case EventCodes.KEY_DOT:
                    return Key.PERIOD;
                case EventCodes.KEY_SLASH:
                    return Key.SLASH;
                case EventCodes.KEY_SPACE:
                    return Key.SPACE;
                case EventCodes.KEY_APPSELECT:
                    return Key.APPS;
                case EventCodes.KEY_SYSRQ:
                    return Key.SYSRQ;
                case EventCodes.KEY_SCROLLLOCK:
                    return Key.SCROLLLOCK;
                case EventCodes.KEY_PAUSE:
                    return Key.PAUSE;
                case EventCodes.KEY_INSERT:
                    return Key.INSERT;
                case EventCodes.KEY_DELETE:
                    return Key.DELETE;
                case EventCodes.KEY_HOME:
                    return Key.HOME;
                case EventCodes.KEY_END:
                    return Key.END;
                case EventCodes.KEY_PAGEUP:
                    return Key.PAGEUP;
                case EventCodes.KEY_PAGEDOWN:
                    return Key.PAGEDOWN;
                case EventCodes.KEY_UP:
                    return Key.UPARROW;
                case EventCodes.KEY_DOWN:
                    return Key.DOWNARROW;
                case EventCodes.KEY_LEFT:
                    return Key.LEFTARROW;
                case EventCodes.KEY_RIGHT:
                    return Key.RIGHTARROW;
                case EventCodes.KEY_LEFTSHIFT:
                    return Key.LSHIFT;
                case EventCodes.KEY_RIGHTSHIFT:
                    return Key.RSHIFT;
                case EventCodes.KEY_LEFTALT:
                    return Key.LALT;
                case EventCodes.KEY_RIGHTALT:
                    return Key.RALT;
                case EventCodes.KEY_LEFTCTRL:
                    return Key.LCONTROL;
                case EventCodes.KEY_RIGHTCTRL:
                    return Key.RCONTROL;

                case EventCodes.KEY_KP0:
                    return Key.NUMPAD0;
                case EventCodes.KEY_KP1:
                    return Key.NUMPAD1;
                case EventCodes.KEY_KP2:
                    return Key.NUMPAD2;
                case EventCodes.KEY_KP3:
                    return Key.NUMPAD3;
                case EventCodes.KEY_KP4:
                    return Key.NUMPAD4;
                case EventCodes.KEY_KP5:
                    return Key.NUMPAD5;
                case EventCodes.KEY_KP6:
                    return Key.NUMPAD6;
                case EventCodes.KEY_KP7:
                    return Key.NUMPAD7;
                case EventCodes.KEY_KP8:
                    return Key.NUMPAD8;
                case EventCodes.KEY_KP9:
                    return Key.NUMPAD9;

                case EventCodes.KEY_NUMLOCK:
                    return Key.NUMLOCK;
                case EventCodes.KEY_KPSLASH:    
                    return Key.DIVIDE;
                case EventCodes.KEY_KPASTERISK:    
                    return Key.MULTIPLY;
                case EventCodes.KEY_KPMINUS:    
                    return Key.SUBTRACT;
                case EventCodes.KEY_KPPLUS:    
                    return Key.ADD;
                case EventCodes.KEY_KPDOT:    
                    return Key.DECIMAL;
                case EventCodes.KEY_KPENTER:    
                    return Key.NUMPADENTER;
                default:
                    return Key.NONE;
            }
        }
    }
}