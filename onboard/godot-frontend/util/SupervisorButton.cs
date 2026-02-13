using Godot;

namespace onboard.util.supervisor_button
{
    static class SupervisorButton
    {
        public static bool anyButtonPressed { get { return Input.IsAnythingPressed(); } private set { } }

        public static bool isSupervisorButtonPressed()
        {
            bool player1_menu_pressed;
            bool player2_menu_pressed;

            // these are hard coded values that should be changed to something else
            player1_menu_pressed = Input.IsJoyButtonPressed(0, JoyButton.LeftStick);
            player2_menu_pressed = Input.IsJoyButtonPressed(1, JoyButton.LeftStick);

            return player1_menu_pressed && player2_menu_pressed;
        } 
    }
}