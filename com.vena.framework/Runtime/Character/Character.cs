namespace Vena.Framework
{
    public class Character : Actor
    {
    }

    public partial class GameWorld
    {
        private Character _character;
        
        public static Character character
        {
            get => _game?._character;
            
            set
            {
                if(null == _game) return;
                
                _game._character = value;
            }
        }
    }
}