using Godot;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace onboard.util.permenentInput
{
    public class SupervisorButton 
    {
        private static GamePadState player1_state;
        private static GamePadState player2_state;

        private static Keys[] player1_menuKeys = {
            Keys.Escape,
        };
        private static Keys[] player2_menuKeys = {
            Keys.Escape,
        };

        private static Buttons[] player1_menuButtons = {
            Buttons.Start,
        };
        private static Buttons[] player2_menuButtons = {
            Buttons.Start,
        };

        public SupervisorButton()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> True if the supervisor button keybind is pressed</returns>
        public bool supervisorButtonPressed()
        {
            bool player1_menuButtonDown = false;
            bool player2_menuButtonDown = false;

            // keyboard
            KeyboardState keyboardState = Keyboard.GetState();
            foreach (Keys menuKey in player1_menuKeys)
            {
                if (keyboardState.IsKeyDown(menuKey))
                {
                    player1_menuButtonDown = true;
                    break;
                }
            }

            foreach (Keys menuKey in player2_menuKeys)
            {
                if (keyboardState.IsKeyDown(menuKey))
                {
                    player2_menuButtonDown = true;
                    break;
                }
            }


            player1_state = GamePad.GetState(PlayerIndex.One);
            player2_state = GamePad.GetState(PlayerIndex.Two);

            // gamepad
            foreach (Buttons button in player1_menuButtons)
            {
                if (player1_state.IsButtonDown(button))
                {
                    player1_menuButtonDown = true;
                    break;
                }
            }

            foreach (Buttons button in player2_menuButtons)
            {
                if (player2_state.IsButtonDown(button))
                {
                    player2_menuButtonDown = true;
                    break;
                }
            }

            GD.Print(player1_state.Buttons.Start.ToString());
            GD.Print(player2_state.Buttons.Start.ToString());

            GD.Print(keyboardState.GetPressedKeys().ToString());

            GD.Print(keyboardState.GetPressedKeys().ToString());

            GD.Print(player1_menuButtonDown + " | " + player2_menuButtonDown);
            
            return player1_menuButtonDown && player2_menuButtonDown;
        }
    }
}