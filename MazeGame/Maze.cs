using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{
    class Maze
    {
        private Cell[,] cells;
        private List<Cell> frontier;
        private Random rnd;
        public int size { get; set; }

        public Maze()
        {
            this.frontier = new List<Cell>();
            this.rnd = new Random();
        }

        public void printMaze()
        {
            for (int i = 0; i < size; i++)
            {
                Console.Write(" _");
            }
            Console.WriteLine();
            for (int i = 0; i < size; i++)
            {
                Console.Write("|");
                for (int j = 0; j < size; j++)
                {
                    if (i != size - 1 && cells[i + 1, j].topWall) Console.Write("_");
                    else if (i == size - 1) Console.Write("_");
                    else Console.Write(" ");
                    if (cells[i, j].rightWall) Console.Write("|");
                    else Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        public void generateMaze(int size)
        {
            this.size = size;
            this.cells = new Cell[size, size];

            // assign neighbors to all cells
            assignNeighbors();

            Cell lastAddedCell = cells[0, 0];
            lastAddedCell.inMaze = true;

            do
            {
                addFrontiers(lastAddedCell);
                lastAddedCell = addRandFrontierToMaze();
            } while (frontier.Count > 0);
        }

        private void assignNeighbors()
        {
            // initialize cells
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    cells[i, j] = new Cell();
                }
            }

            Cell top;
            Cell bottom;
            Cell left;
            Cell right;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == 0) top = null;
                    else top = cells[i - 1, j];

                    if (i == size - 1) bottom = null;
                    else bottom = cells[i + 1, j];

                    if (j == 0) left = null;
                    else left = cells[i, j - 1];

                    if (j == size - 1) right = null;
                    else right = cells[i, j + 1];

                    cells[i, j].setNeighbors(top, bottom, left, right);
                    cells[i, j].row = i;
                    cells[i, j].col = j;
                }
            }
        }

        private void addFrontiers(Cell lastAddedCell)
        {
            Cell top = lastAddedCell.topNeighbor;
            Cell bottom = lastAddedCell.bottomNeighbor;
            Cell left = lastAddedCell.leftNeighbor;
            Cell right = lastAddedCell.rightNeighbor;

            if (top != null && !top.inMaze && !frontier.Contains(top))
            {
                frontier.Add(top);
            }
            if (bottom != null && !bottom.inMaze && !frontier.Contains(bottom))
            {
                frontier.Add(bottom);
            }
            if (left != null && !left.inMaze && !frontier.Contains(left))
            {
                frontier.Add(left);
            }
            if (right != null && !right.inMaze && !frontier.Contains(right))
            {
                frontier.Add(right);
            }
        }

        private Cell addRandFrontierToMaze()
        {
            // Select random frontier
            Cell randFrontierCell = frontier[rnd.Next(frontier.Count)];
            List<Cell> connections = new List<Cell>();

            Cell top = randFrontierCell.topNeighbor;
            Cell bottom = randFrontierCell.bottomNeighbor;
            Cell left = randFrontierCell.leftNeighbor;
            Cell right = randFrontierCell.rightNeighbor;

            if (top != null && top.inMaze) connections.Add(top);
            if (bottom != null && bottom.inMaze) connections.Add(bottom);
            if (left != null && left.inMaze) connections.Add(left);
            if (right != null && right.inMaze) connections.Add(right);


            // Remove walls from a random maze connection
            Cell randConnection = connections[rnd.Next(connections.Count)];
            if (randConnection == top)
            {
                randFrontierCell.topWall = false;
                randConnection.bottomWall = false;
            }
            else if (randConnection == bottom)
            {
                randFrontierCell.bottomWall = false;
                randConnection.topWall = false;
            }
            else if (randConnection == left)
            {
                randFrontierCell.leftWall = false;
                randConnection.rightWall = false;
            }
            else if (randConnection == right)
            {
                randFrontierCell.rightWall = false;
                randConnection.leftWall = false;
            }

            // Add cell to maze and remove from frontier
            randFrontierCell.inMaze = true;
            frontier.Remove(randFrontierCell);

            return randFrontierCell;
        }

        public List<Cell> findShortestPath(Cell cell)
        {
            // initialize all parent cells to null
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    cells[i, j].parentCell = null;
                }
            }

            Queue<Cell> queue = new Queue<Cell>();
            List<Cell> visited = new List<Cell>();

            queue.Enqueue(cell);
            do
            {
                cell = queue.Dequeue();
                visited.Add(cell);
                if (cell.topNeighbor != null && cell.topWall == false && !visited.Contains(cell.topNeighbor))
                {
                    queue.Enqueue(cell.topNeighbor);
                    cell.topNeighbor.parentCell = cell;
                }
                if (cell.rightNeighbor != null && cell.rightWall == false && !visited.Contains(cell.rightNeighbor))
                {
                    queue.Enqueue(cell.rightNeighbor);
                    cell.rightNeighbor.parentCell = cell;
                }
                if (cell.bottomNeighbor != null && cell.bottomWall == false && !visited.Contains(cell.bottomNeighbor))
                {
                    queue.Enqueue(cell.bottomNeighbor);
                    cell.bottomNeighbor.parentCell = cell;
                }
                if (cell.leftNeighbor != null && cell.leftWall == false && !visited.Contains(cell.leftNeighbor))
                {
                    queue.Enqueue(cell.leftNeighbor);
                    cell.leftNeighbor.parentCell = cell;
                }
            } while (!visited.Contains(cells[size - 1, size - 1]));


            List<Cell> path = new List<Cell>();
            cell = cells[size - 1, size - 1];
            while (cell.parentCell != null)
            {
                path.Add(cell);
                cell = cell.parentCell;
            }

            return path;
        }

        public Cell[,] getCells()
        {
            return cells;
        }
    }
}
