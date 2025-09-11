using System.Collections.Generic;

namespace onboard.util.permenentInput
{
    public enum Button
    {
        A,
        B,
        X,
        Y,
        RB,
        LB,
        RIGHT,
        LEFT,
        UP,
        DOWN,
        BACK,
        GUIDE, // xbox logo button
        START,
    }

    public struct Axis
    {
        public double value;

        public Axis()
        {
            value = 0;
        }
    }

    public struct Joystick
    {
        public Axis x;
        public Axis y;
    }

    public struct xBoxGamePad
    {
        public HashSet<Button> buttonsPressed;

        public Axis leftTrigger;
        public Axis rightTrigger;

        public Joystick rightJoystick;
        public Joystick leftJoystick;

        public xBoxGamePad()
        {
            buttonsPressed = new HashSet<Button>();
        }


    }
}