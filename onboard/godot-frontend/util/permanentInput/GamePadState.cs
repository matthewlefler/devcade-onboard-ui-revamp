using System.Collections.Generic;

namespace onboard.util.permenentInput
{
    enum Button
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

    struct Axis
    {
        public double value;

        public Axis()
        {
            value = 0;
        }
    }

    struct Joystick
    {
        public Axis x;
        public Axis y;
    }

    struct xBoxGamePad
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