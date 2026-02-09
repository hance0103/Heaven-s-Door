namespace GamePlay.GridMap
{
    public class Grid
    {
        // 그리드의 가로 세로 셀 개수
        public int width;
        public int height;
        public eSystemEnum.eSellState[,] cells;

        public Grid(int w, int h)
        {
            width = w;
            height = h;
            cells = new eSystemEnum.eSellState[w, h];
            InitGrid();
        }

        private void InitGrid()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                    {
                        cells[i, j] = eSystemEnum.eSellState.Wall;
                    }
                    else
                    {
                        cells[i, j] = eSystemEnum.eSellState.Empty;
                    }
                }
            }
        }
    }
}
