using onboard.devcade;
using System.Collections.Generic;

namespace GodotFrontend;

interface GuiInterface
{
    /// <summary>
    /// updates/sets the list of games in the GUI,
    /// also should save a reference to the GuiManager as it is useful
    /// in calling functions related to the GuiManager, such as launchGame() and killGame()
    /// </summary>
    /// <param name="gameTitles"></param>
    /// <param name="model"></param>
    public void setGameList(List<DevcadeGame> gameTitles, GuiManager model);

    /// <summary>
    /// update the list of tags 
    /// </summary>
    /// <param name="tags"> the new tag list </param>
    public void setTagList(List<Tag> tags);

    /// <summary>
    /// sets the current tag,
    /// the GUI should then hide all games without this tag
    /// </summary>
    /// <param name="tag"></param>
    public void setTag(Tag tag);
}