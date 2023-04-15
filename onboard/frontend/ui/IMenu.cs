using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace onboard.ui; 

public interface IMenu {
    public void Initialize();
    
    public void LoadContent(ContentManager contentManager);
    
    public void Update(GameTime gameTime);
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    
    public void Unload();
}