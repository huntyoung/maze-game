using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    class Cell
    {
        public bool inMaze { get; set; }
        public bool topWall { get; set; }
        public bool rightWall { get; set; }
        public bool bottomWall { get; set; }
        public bool leftWall { get; set; }
        public Cell topNeighbor { get; set; }
        public Cell rightNeighbor { get; set; }
        public Cell bottomNeighbor { get; set; }
        public Cell leftNeighbor { get; set; }
        public Cell parentCell { get; set; }
        public int row { get; set; }
        public int col { get; set; }

        public Cell()
        {
            inMaze = false;
            topWall = true;
            rightWall = true;
            bottomWall = true;
            leftWall = true;

            parentCell = null;

        }

        public void setNeighbors(Cell top, Cell bottom, Cell left, Cell right)
        {
            topNeighbor = top;
            bottomNeighbor = bottom;
            leftNeighbor = left;
            rightNeighbor = right;
        }
    }
}
