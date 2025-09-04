using Microsoft.Xna.Framework.Input;

namespace onboard.util.supervisorButton
{
    static class SupervisorButton
    {
        private static GamePadState player1_state;
        private static GamePadState player2_state;

        static SupervisorButton()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> True if the supervisor button keybind is pressed</returns>
        public static bool supervisorButtonPressed()
        {
            player1_state = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
            player2_state = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.Two);

            return false;
        }
    }
}