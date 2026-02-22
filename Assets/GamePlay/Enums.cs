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
            Novel,
            None,
        }

        public enum Character
        {
            None,
            Karen,

        }

        public enum NovelScriptType
        {
            None,
            before,
            after,
            heaven,
            hell
        }
        public enum Language
        {
            KOR,
            ENG
        }

        public enum Judge
        {
            Before,
            Heaven,
            Hell
        }
    }
}
