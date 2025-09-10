using Godot;

namespace onboard.util.permenentInput
{
    public class SupervisorButton
    {

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

            return player1_menuButtonDown && player2_menuButtonDown;
        }
    }
}