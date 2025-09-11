using System;
using System.Collections.Generic;

namespace onboard.util.permenentInput
{
    enum Key
    {
        A,          // alphabet keys
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,

        F1,         // "F keys" at the top of the keyboard
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,

        ESC,        // the Escape key

        ZERO,       // the numbers row near the top of the keyboard
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,

        MINUS, 	    // the (–) key on the numbers row of the keyboard
        EQUALS, 	// the (=) and (+) key on the numbers row of the keyboard
        TILDE, 		// the (`) and (~) key, on the numbers row of the keyboard
        BACKSPACE,
        TAB,
        LBRACKET,
        RBRACKET,
        BACKSLASH,
        CAPITAL, 	// the Caps Lock key
        SEMICOLON,
        APOSTROPHE,
        COMMA, 		// the comma (,) key
        PERIOD,
        SLASH,
        SPACE, 		// the space bar
        APPS, 		// the Application Menu key (if your keyboard has it, it would be right next to the right CTRL key)
        SYSRQ, 		// the print screen button
        SCROLL, 	// the Scroll Lock key
        PAUSE, 		// the Pause/Break button
        INSERT,
        DELETE,
        HOME,
        END,
        PAGEUP,
        PAGEDOWN,
        UPARROW, 	// UP 	      the up arrow button
        DOWNARROW, 	// DOWN       the down arrow button
        LEFTARROW, 	// LEFT       the left arrow button
        RIGHTARROW, // RIGHT 	  the right arrow button
        LSHIFT, 	// left shift key. When used as a Chord, turns into 'SHIFT'
        RSHIFT, 	// right shift key. When used as a Chord, turns into 'SHIFT'
        LALT, 		// left ALT key. When used as a Chord, turns into 'ALT'
        RALT, 		// right ALT key. When used as a Chord, turns into 'ALT'
        LCONTROL, 	// LCTRL 	  left CTRL key. When used as a Chord, turns into 'CTRL'
        RCONTROL,   // RCTRL 	  right CTRL key. When used as a Chord, turns into 'CTRL' 

        NUMPAD0, 	// the "0" key on the Number Pad
        NUMPAD1, 	// the "1" key on the Number Pad
        NUMPAD2, 	// the "2" key on the Number Pad
        NUMPAD3, 	// the "3" key on the Number Pad
        NUMPAD4, 	// the "4" key on the Number Pad
        NUMPAD5, 	// the "5" key on the Number Pad
        NUMPAD6, 	// the "6" key on the Number Pad
        NUMPAD7, 	// the "7" key on the Number Pad
        NUMPAD8, 	// the "8" key on the Number Pad
        NUMPAD9,    // the "9" key on the Number Pad

        NUMLOCK, 	// the Number Lock key on the Number Pad
        DIVIDE, 	// the (/) key on the Number Pad
        MULTIPLY, 	// the (*) key on the Number Pad
        SUBTRACT, 	// the (-) key on the Number Pad
        ADD, 	    // the (+) key on the Number Pad
        DECIMAL, 	// the (.) or period on the Number Pad
        NUMPADENTER,// the Enter key on the Number Pad     

        NONE,       // not a US-ENG keyboard key 
    }

    struct keyboardState
    {
        HashSet<Key> pressedKeys;

        public bool isKeyDown(Key key)
        {
            return pressedKeys.Contains(key);
        }

        public bool isKeyUp(Key key)
        {
            return !pressedKeys.Contains(key);
        }
    }
}

