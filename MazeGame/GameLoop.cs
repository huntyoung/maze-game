using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MazeGame
{
    public class GameLoop : Game
    {
        private GraphicsDeviceManager m_graphics;
        private SpriteBatch m_spriteBatch;
        private Maze m_maze;

        private Texture2D m_background;
        private Texture2D m_line;
        private Texture2D llama;
        private Texture2D taco;
        private Texture2D star;
        private Texture2D poop;

        private SpriteFont font;

        private Cell[,] cells;
        private Cell llamaCell;
        private List<Cell> visitedCells = new List<Cell>();
        private List<Cell> originalShortestPath = new List<Cell>();
        private List<Cell> shortestPath = new List<Cell>();
        private List<int>[] highScores = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
        private bool breadcrumbs = false;
        private bool path = false;
        private bool hint = false;
        private bool showHighScores = false;
        private bool showCredits = false;


        private float[] llamaPosition = new float[] { 0, 0 };

        private int mazeWidthInPixels = 600;
        private int mazeWidthInCells;
        private int lineThickness = 3;
        private bool mazeDrawn = false;
        private bool newMaze = true;
        private bool mazeFinished = false;
        private bool moved;
        private float llamaScale;
        private int cellSize;
        private bool keyDown = false;
        private int score = 0;
        private int time = 0;
        private float elapsedTime = 0;

        public GameLoop()
        {
            m_graphics = new GraphicsDeviceManager(this);
            m_maze = new Maze();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            m_graphics.PreferredBackBufferWidth = 900;
            m_graphics.PreferredBackBufferHeight = 650;

            m_graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            m_spriteBatch = new SpriteBatch(GraphicsDevice);

            m_background = Content.Load<Texture2D>("images/background");
            llama = Content.Load<Texture2D>("images/llama");
            taco = Content.Load<Texture2D>("images/taco");
            star = Content.Load<Texture2D>("images/star");
            poop = Content.Load<Texture2D>("images/poop");
            font = Content.Load<SpriteFont>("fonts/base");


            // creates a 1*1 "rectangle" which you can then resize to form a rectangle/line shape
            m_line = new Texture2D(GraphicsDevice, 1, 1);
            m_line.SetData(new Color[] { Color.Black });
        }

        protected override void Update(GameTime gameTime)
        {
            processInput();

            if (moved)
            {
                shortestPath = m_maze.findShortestPath(llamaCell);

                // calculate score every movement
                if (originalShortestPath.Contains(llamaCell) && !visitedCells.Contains(llamaCell))
                    score += 5;
                else if (!originalShortestPath.Contains(llamaCell) && shortestPath.Count > 0
                    && originalShortestPath.Contains(shortestPath[shortestPath.Count - 1]) && !visitedCells.Contains(llamaCell))
                    score -= 1;
                else if (visitedCells.Contains(llamaCell)) { }
                else
                    score -= 2;

                moved = false;
            }

            if (newMaze)
            {
                time = 0;
                elapsedTime = 0;

                cells = m_maze.getCells();
                mazeWidthInCells = m_maze.size;
                cellSize = mazeWidthInPixels / mazeWidthInCells;
                llamaPosition[0] = cellSize / 3.5f;
                llamaPosition[1] = cellSize / 5f;
                llamaCell = cells[0, 0];
                visitedCells.Clear();

                originalShortestPath = m_maze.findShortestPath(cells[0, 0]);
                shortestPath = originalShortestPath;
                path = false;
                breadcrumbs = false;
                score = 0;

                newMaze = false;
                mazeFinished = false;
            }

            if (llamaCell == cells[mazeWidthInCells - 1, mazeWidthInCells - 1] && !mazeFinished)
            {
                mazeFinished = true;

                // add to high scores table
                int mazeSizeIdx = (mazeWidthInCells / 5) - 1;

                if (highScores[mazeSizeIdx].Count > 0)
                {
                    for (int i=0; i<highScores[mazeSizeIdx].Count; i++)
                    {
                        if(highScores[mazeSizeIdx][i] < score)
                        {
                            highScores[mazeSizeIdx].Insert(i, score);
                            break;
                        }
                        else if (i == highScores[mazeSizeIdx].Count - 1)
                        {
                            highScores[mazeSizeIdx].Add(score);
                            break;
                        }
                    } 
                }
                else
                {
                    highScores[mazeSizeIdx].Add(score);
                }

            }

            if (!mazeFinished)
            {
                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (elapsedTime >= 1f)
                {
                    time++;
                    float carryOverTime = elapsedTime - 1f;
                    elapsedTime = carryOverTime;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            m_spriteBatch.Begin();

            int backImageSize = mazeWidthInPixels / 2;
            m_spriteBatch.Draw(m_background, new Rectangle(0, 0, backImageSize, backImageSize), Color.White);
            m_spriteBatch.Draw(m_background, new Rectangle(backImageSize, 0, backImageSize, backImageSize), Color.White);
            m_spriteBatch.Draw(m_background, new Rectangle(0, backImageSize, backImageSize, backImageSize), Color.White);
            m_spriteBatch.Draw(m_background, new Rectangle(backImageSize, backImageSize, backImageSize, backImageSize), Color.White);


            drawMaze();

            if (breadcrumbs)
            {
                for (int i = 0; i < visitedCells.Count; i++)
                {
                    Cell cell = visitedCells[i];
                    m_spriteBatch.Draw(poop, new Rectangle((cellSize * (cell.col)) + (int)(cellSize / 6f), cellSize * (cell.row) + (int)(cellSize / 6f),
                                (int)(cellSize / 1.5), (int)(cellSize / 1.5)), Color.White);
                }
            }

            if (path)
            {
                for (int i = 0; i < shortestPath.Count; i++)
                {
                    Cell cell = shortestPath[i];
                    m_spriteBatch.Draw(star, new Rectangle((cellSize * (cell.col)) + (int)(cellSize / 6f), cellSize * (cell.row) + (int)(cellSize / 6f),
                        (int)(cellSize / 1.5), (int)(cellSize / 1.5)), Color.White);
                }
            }

            if (hint)
            {
                if (shortestPath.Count > 0)
                {
                    Cell cell = shortestPath[shortestPath.Count - 1];
                    m_spriteBatch.Draw(star, new Rectangle((cellSize * (cell.col)) + (int)(cellSize / 6f), cellSize * (cell.row) + (int)(cellSize / 6f),
                        (int)(cellSize / 1.5), (int)(cellSize / 1.5)), Color.White);
                }
            }

            if (showHighScores)
            {
                m_spriteBatch.DrawString(font,
                    $"High scores for {mazeWidthInCells} x {mazeWidthInCells}", new Vector2(615, 200), Color.Black);
                int mazeSizeIdx = (mazeWidthInCells / 5) - 1;
                for (int i=0; i<highScores[mazeSizeIdx].Count; i++)
                {
                    m_spriteBatch.DrawString(font,
                        $"Score: {highScores[mazeSizeIdx][i]}", new Vector2(630, 220 + (i * 20)), Color.Black);
                }
            }

            if (showCredits)
            {
                m_spriteBatch.DrawString(font,
                    $"Credits go to none other " +
                    $"{System.Environment.NewLine}than the great HUNTER JASON YOUNG", new Vector2(615, 560), Color.Black);
            }

            if (!showHighScores)
            {
                m_spriteBatch.DrawString(font,
                    $"Controls: " +
                    $"{System.Environment.NewLine}  F1: 5x5                   F2: 10x10" +
                    $"{System.Environment.NewLine}  F3: 15x15               F4: 20x20" +
                    $"{System.Environment.NewLine}  F5: High Scores    F6: Credits" +
                    $"{System.Environment.NewLine}    B: Breadcrumbs    P: Path" +
                    $"{System.Environment.NewLine}    H: Hint", new Vector2(615, 200), Color.Black);
            }


            m_spriteBatch.Draw(taco, new Rectangle((cellSize * (mazeWidthInCells - 1)) + (int)(cellSize / 12f), cellSize * (mazeWidthInCells - 1) + (int)(cellSize / 5f), 
                (int)(cellSize / 1.1), (int)(cellSize / 1.5)), Color.White);
            m_spriteBatch.Draw(llama, new Vector2(llamaPosition[0], llamaPosition[1]), null, Color.White,
                0f, Vector2.Zero, llamaScale, SpriteEffects.None, 0f);


            m_spriteBatch.DrawString(font, $"Score: {score}", new Vector2(710, 125), Color.Black);
            m_spriteBatch.DrawString(font, $"Time: {time}", new Vector2(710, 150), Color.Black);


            if (mazeFinished)
            {
                m_spriteBatch.DrawString(font, 
                    $"Congratulations! You finished a {mazeWidthInCells} x {mazeWidthInCells}" +
                    $"{System.Environment.NewLine}maze in {time} seconds with a score of {score}"
                    , new Vector2(615, 50), Color.Black);
            }

            m_spriteBatch.End();

            base.Draw(gameTime);
        }

        private void drawMaze()
        {
            for (int i = 0; i < mazeWidthInCells; i++)
            {
                for (int j = 0; j < mazeWidthInCells; j++)
                {
                    if (cells[i, j].leftWall)
                    {
                        // vertical
                        m_spriteBatch.Draw(m_line, new Rectangle(j * cellSize, i * cellSize, lineThickness, cellSize+lineThickness), Color.White);
                    }
                    if (cells[i, j].topWall)
                    {
                        // horizontal
                        m_spriteBatch.Draw(m_line, new Rectangle(j * cellSize, i * cellSize, cellSize+lineThickness, lineThickness), Color.White);
                    }
                    if (j == mazeWidthInCells - 1)
                    {
                        // vertical right edge
                        m_spriteBatch.Draw(m_line, new Rectangle((j+1) * cellSize, i * cellSize, lineThickness, cellSize+lineThickness), Color.White);
                    }
                    if (i == mazeWidthInCells - 1)
                    {
                        // horizontal bottom edge
                        m_spriteBatch.Draw(m_line, new Rectangle(j * cellSize, (i + 1) * cellSize, cellSize+lineThickness, lineThickness), Color.White);
                    }
                }
            }

            mazeDrawn = true;
        }

        private void processInput()
        {
            KeyboardState state = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || state.IsKeyDown(Keys.Escape))
                Exit();

            if (!keyDown)
            {
                if (mazeDrawn && !mazeFinished)
                {
                    if (state.IsKeyDown(Keys.B))
                        breadcrumbs = !breadcrumbs;
                    if (state.IsKeyDown(Keys.H))
                        hint = !hint;
                    if (state.IsKeyDown(Keys.P))
                    {
                        path = !path;
                        shortestPath = m_maze.findShortestPath(llamaCell);
                    }
                    if (state.IsKeyDown(Keys.W) && llamaCell.topWall == false)
                    {
                        if (!visitedCells.Contains(llamaCell))
                            visitedCells.Add(llamaCell);
                        llamaPosition[1] -= cellSize;
                        llamaCell = llamaCell.topNeighbor;

                        moved = true;
                    }
                    else if (state.IsKeyDown(Keys.A) && llamaCell.leftWall == false)
                    {
                        if (!visitedCells.Contains(llamaCell))
                            visitedCells.Add(llamaCell);
                        llamaPosition[0] -= cellSize;
                        llamaCell = llamaCell.leftNeighbor;

                        moved = true;
                    }
                    else if (state.IsKeyDown(Keys.S) && llamaCell.bottomWall == false)
                    {
                        if (!visitedCells.Contains(llamaCell))
                            visitedCells.Add(llamaCell);
                        llamaPosition[1] += cellSize;
                        llamaCell = llamaCell.bottomNeighbor;

                        moved = true;
                    }
                    else if (state.IsKeyDown(Keys.D) && llamaCell.rightWall == false)
                    {
                        if (!visitedCells.Contains(llamaCell))
                            visitedCells.Add(llamaCell);
                        llamaPosition[0] += cellSize;
                        llamaCell = llamaCell.rightNeighbor;

                        moved = true;
                    }
                }

                if (state.IsKeyDown(Keys.F1) || !mazeDrawn)
                {
                    m_maze.generateMaze(5);
                    llamaScale = 0.1f;
                    newMaze = true;
                }
                else if (state.IsKeyDown(Keys.F2))
                {
                    m_maze.generateMaze(10);
                    llamaScale = 0.05f;
                    newMaze = true;
                }
                else if (state.IsKeyDown(Keys.F3))
                {
                    m_maze.generateMaze(15);
                    llamaScale = 0.035f;
                    newMaze = true;
                }
                else if (state.IsKeyDown(Keys.F4))
                {
                    m_maze.generateMaze(20);
                    llamaScale = 0.025f;
                    newMaze = true;
                }
                else if (state.IsKeyDown(Keys.F5))
                {
                    showHighScores = !showHighScores;
                }
                else if (state.IsKeyDown(Keys.F6))
                {
                    showCredits = !showCredits;
                }
            }

            // limits to only one key press per press
            if (state.GetPressedKeys().Length == 0)
                keyDown = false;
            else
                keyDown = true;
        }
    }
}
