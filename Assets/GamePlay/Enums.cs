namespace GamePlay
{
    public static class SystemEnum
    {
        public enum eSellState
        {
            Wall,
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
    }
}
