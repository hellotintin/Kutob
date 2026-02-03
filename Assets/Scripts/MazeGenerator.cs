using System;
using System.Collections.Generic;
using UnityEngine;
public class MazeGenerator : MonoBehaviour
{
    [Header("Grid Size (use odd numbers >= 11)")]
    public int rows = 15;
    public int cols = 15;

    [Header("Prefabs")]
    public GameObject wallPrefab;       
    public GameObject fakeWallPrefab;   
    public GameObject invisWallPrefab;  
    public GameObject exitPrefab;       

    [Header("Tuning")]
    [Range(0.05f, 0.40f)]
    public float fakeWallChance = 0.15f;  
    [Range(0.05f, 0.30f)]
    public float invisWallChance = 0.10f; 

    // 0 = wall, 1 = path
    private int[,] grid;

    public List<FakeWall> fakeWalls   = new List<FakeWall>();
    public List<InvisibleWall> invisWalls = new List<InvisibleWall>();
    public Vector3 startPosition { get; private set; }
    private GameObject mazeParent;

    public void Generate()
    {
        ClearMaze();                // destroy previous maze
        fakeWalls.Clear();
        invisWalls.Clear();

        EnsureOddSize();            // rows & cols must be odd
        grid = new int[rows, cols];

        CarvePassages();            // recursive backtracking
        PlaceWalls();               // spawn wall / fakeWall cubes
        PlaceInvisibleWalls();      // add hidden blockers on paths
        PlaceExit();                // find farthest dead end

        // player spaws 
        startPosition = GridToWorld(1, 1);
    }

    private void CarvePassages()
    {
        // fill grid with walls
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = 0;

        // start carving from (1,1)
        var visited = new bool[rows, cols];
        Carve(1, 1, visited);
    }

    private void Carve(int r, int c, bool[,] visited)
    {
        visited[r, c] = true;
        grid[r, c] = 1; // mark as path

        // shuffle directions so the maze is different every run
        var dirs = new List<(int dr, int dc)>
        {
            (0, 2), (0, -2), (2, 0), (-2, 0)
        };
        Shuffle(dirs);

        foreach (var (dr, dc) in dirs)
        {
            int nr = r + dr;
            int nc = c + dc;

            if (InBounds(nr, nc) && !visited[nr, nc])
            {
                grid[r + dr / 2, c + dc / 2] = 1;
                Carve(nr, nc, visited);
            }
        }
    }

    private void PlaceWalls()
    {
        mazeParent = new GameObject("Maze");

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == 0) // it's a wall cell
                {
                    // decide: real wall or fake wall?
                    // border walls are NEVER fake (para bounded ung maze jiro)
                    bool isBorder = (r == 0 || r == rows - 1 ||
                                     c == 0 || c == cols - 1);

                    if (!isBorder && UnityEngine.Random.value < fakeWallChance)
                    {
                        // fake wall 
                        var obj = Instantiate(fakeWallPrefab,
                                              GridToWorld(r, c),
                                              Quaternion.identity,
                                              mazeParent.transform);
                        obj.name = $"FakeWall_{r}_{c}";
                        fakeWalls.Add(obj.GetComponent<FakeWall>());
                    }
                    else
                    {
                        // real wall
                        var obj = Instantiate(wallPrefab,
                                              GridToWorld(r, c),
                                              Quaternion.identity,
                                              mazeParent.transform);
                        obj.name = $"Wall_{r}_{c}";
                    }
                }
            }
        }
    }

    private void PlaceInvisibleWalls()
    {
        // collect all path cells that are NOT the start (1,1) 
        var pathCells = new List<(int r, int c)>();
        for (int r = 1; r < rows - 1; r++)
            for (int c = 1; c < cols - 1; c++)
                if (grid[r, c] == 1 && !(r == 1 && c == 1))
                    pathCells.Add((r, c));

        Shuffle(pathCells);

        int count = Mathf.FloorToInt(pathCells.Count * invisWallChance);
        for (int i = 0; i < count && i < pathCells.Count; i++)
        {
            var (r, c) = pathCells[i];
            var obj = Instantiate(invisWallPrefab,
                                  GridToWorld(r, c),
                                  Quaternion.identity,
                                  mazeParent.transform);
            obj.name = $"InvisWall_{r}_{c}";
            invisWalls.Add(obj.GetComponent<InvisibleWall>());
        }
    }

    private void PlaceExit()
    {
        // bfs from (1.1) para ma find ung farthest reachable dead-end
        var dist    = new int[rows, cols];
        var visited = new bool[rows, cols];
        var queue   = new Queue<(int r, int c)>();

        queue.Enqueue((1, 1));
        visited[1, 1] = true;
        dist[1, 1]    = 0;

        int farthestR = 1, farthestC = 1, farthestDist = 0;

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();

            // dead end has 1 open neighbour (notstart)
            int openNeighbours = CountOpenNeighbours(r, c);
            if (openNeighbours == 1 && !(r == 1 && c == 1))
            {
                if (dist[r, c] > farthestDist)
                {
                    farthestDist = dist[r, c];
                    farthestR    = r;
                    farthestC    = c;
                }
            }

            foreach (var (dr, dc) in new[] { (0,1),(0,-1),(1,0),(-1,0) })
            {
                int nr = r + dr, nc = c + dc;
                if (InBounds(nr, nc) && !visited[nr, nc] && grid[nr, nc] == 1)
                {
                    visited[nr, nc] = true;
                    dist[nr, nc]    = dist[r, c] + 1;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        // spawn exit at the farthest chuchu
        var exitPos = GridToWorld(farthestR, farthestC);
        var exitObj = Instantiate(exitPrefab, exitPos, Quaternion.identity,
                                  mazeParent.transform);
        exitObj.name = "Exit";
    }

    private void EnsureOddSize()
    {
        if (rows % 2 == 0) rows++;
        if (cols % 2 == 0) cols++;
        rows = Mathf.Max(rows, 11);
        cols = Mathf.Max(cols, 11);
    }

    private bool InBounds(int r, int c)
        => r > 0 && r < rows - 1 && c > 0 && c < cols - 1;

    private int CountOpenNeighbours(int r, int c)
    {
        int count = 0;
        foreach (var (dr, dc) in new[] { (0,1),(0,-1),(1,0),(-1,0) })
        {
            int nr = r + dr, nc = c + dc;
            if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && grid[nr, nc] == 1)
                count++;
        }
        return count;
    }

    public static Vector3 GridToWorld(int row, int col)
    {
        return new Vector3(col, 0f, row);
    }
    private void ClearMaze()
    {
        if (mazeParent != null)
            Destroy(mazeParent);
    }
    private static void Shuffle<T>(List<T> list)
    {
        var rng = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
        int j = rng.Next(i + 1);
        (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
