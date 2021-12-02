namespace BadCode.Main
{
    public struct MatchCellComponent
    {
        public int x;
        public int y;
        public float pos_x;
        public float pos_y;
        public int lf_rt_dir; //-1 left 1 - rigth
        public int left_cell;
        public int right_cell;
        public int top_cell;
        public int bottom_cell;
        public int top_left_cell;
        public int top_right_cell;
        public int bottom_left_cell;
        public int bottom_right_cell;
    }
}