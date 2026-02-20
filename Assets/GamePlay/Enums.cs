namespace GamePlay
{
    public static class SystemEnum
    {
        public enum eSellState
        {
            Empty,
            Filled
        }

        public enum eNodeState
        {
            CannotMove,
            Moveable,
            Drawable,
            Drawing
        }

        public enum eScenes
        {
            Main,
            InGame
        }
    }
}
