namespace GamePlay
{
    public static class SystemEnum
    {
        public enum eSellState
        {
            Empty,
            Filled,
            Wall
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
            Start,
            Choice,
            Ingame,
            Novel
        }

        public enum Character
        {
            None,
            Karen,

        }

        public enum Language
        {
            KOR,
            ENG
        }
    }
}
