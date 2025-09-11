using Godot;

namespace onboard.util.permenentInput
{
    public class SupervisorButton
    {
        KeyLogger keyLogger = new KeyLogger();
        public SupervisorButton()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> True if the supervisor button keybind is pressed</returns>
        public bool supervisorButtonPressed()
        {
            keyLogger.UpdateKeys();

            GD.Print(keyLogger.keyboards.GetEnumerator().Current.ToString());
            
            bool player1_menuButtonDown = false;
            bool player2_menuButtonDown = false;

            return player1_menuButtonDown && player2_menuButtonDown;
        }
    }
}