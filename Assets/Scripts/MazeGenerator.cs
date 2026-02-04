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
    public GameObject Floor;
    public GameObject Roof;

    [Header("Tuning")]
    [Range(0.05f, 0.40f)]
    public float fakeWallChance = 0.15f;
    [Range(0.05f, 0.30f)]
    public float invisWallChance = 0.10f;

    private int[,] grid; // 0 = wall, 1 = path

    public List<FakeWall> fakeWalls = new List<FakeWall>();
    public List<InvisibleWall> invisWalls = new List<InvisibleWall>();

    public Vector3 startPosition { get; private set; }
    private GameObject mazeParent;

    public void Generate()
    {
        ClearMaze();
        fakeWalls.Clear();
        invisWalls.Clear();

        EnsureOddSize();
        grid = new int[rows, cols];

        mazeParent = new GameObject("Maze");

        CarvePassages();
        PlaceWalls();
        PlaceInvisibleWalls();
        PlaceExit();
        PlaceFloor();
        PlaceRoof();

        startPosition = GridToWorld(1, 1);
    }

    private void CarvePassages()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                grid[r, c] = 0;

        var visited = new bool[rows, cols];
        Carve(1, 1, visited);
    }

    private void Carve(int r, int c, bool[,] visited)
    {
        visited[r, c] = true;
        grid[r, c] = 1;

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
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == 0)
                {
                    bool isBorder =
                        r == 0 || r == rows - 1 ||
                        c == 0 || c == cols - 1;

                    if (!isBorder && UnityEngine.Random.value < fakeWallChance)
                    {
                        var obj = Instantiate(
                            fakeWallPrefab,
                            GridToWorld(r, c),
                            Quaternion.identity,
                            mazeParent.transform
                        );
                        obj.name = $"FakeWall_{r}_{c}";
                        fakeWalls.Add(obj.GetComponent<FakeWall>());
                    }
                    else
                    {
                        var obj = Instantiate(
                            wallPrefab,
                            GridToWorld(r, c),
                            Quaternion.identity,
                            mazeParent.transform
                        );
                        obj.name = $"Wall_{r}_{c}";
                    }
                }
            }
        }
    }

    private void PlaceFloor()
    {
        // Lower the floor below walls
        float floorYOffset = -1f; // Adjust this to sit below walls

        var floor = Instantiate(
            Floor,
            new Vector3(
                (cols - 1) / 2f,
                floorYOffset,
                (rows - 1) / 2f
            ),
            Quaternion.identity,
            mazeParent.transform
        );

        floor.name = "Floor";

        // Scale to cover the maze
        floor.transform.localScale = new Vector3(
            cols,
            1f,
            rows
        );
    }

    private void PlaceRoof()
    {
        // Place the roof above walls (walls are at Y=0, so roof sits higher)
        float roofYOffset = 1.7f; // Adjust this to sit above walls

        var roof = Instantiate(
            Roof,
            new Vector3(
                (cols - 1) / 2f,
                roofYOffset,
                (rows - 1) / 2f
            ),
            Quaternion.identity,
            mazeParent.transform
        );

        roof.name = "Roof";

        // Scale to cover the maze (same as floor)
        roof.transform.localScale = new Vector3(
            cols,
            1f,
            rows
        );
    }

    private void PlaceInvisibleWalls()
    {
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
            var obj = Instantiate(
                invisWallPrefab,
                GridToWorld(r, c),
                Quaternion.identity,
                mazeParent.transform
            );
            obj.name = $"InvisWall_{r}_{c}";
            invisWalls.Add(obj.GetComponent<InvisibleWall>());
        }
    }

    private void PlaceExit()
    {
        var dist = new int[rows, cols];
        var visited = new bool[rows, cols];
        var queue = new Queue<(int r, int c)>();

        queue.Enqueue((1, 1));
        visited[1, 1] = true;

        int farthestR = 1, farthestC = 1, farthestDist = 0;

        while (queue.Count > 0)
        {
            var (r, c) = queue.Dequeue();

            int openNeighbours = CountOpenNeighbours(r, c);
            if (openNeighbours == 1 && !(r == 1 && c == 1))
            {
                if (dist[r, c] > farthestDist)
                {
                    farthestDist = dist[r, c];
                    farthestR = r;
                    farthestC = c;
                }
            }

            foreach (var (dr, dc) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
            {
                int nr = r + dr;
                int nc = c + dc;

                if (InBounds(nr, nc) && !visited[nr, nc] && grid[nr, nc] == 1)
                {
                    visited[nr, nc] = true;
                    dist[nr, nc] = dist[r, c] + 1;
                    queue.Enqueue((nr, nc));
                }
            }
        }

        var exitObj = Instantiate(
            exitPrefab,
            GridToWorld(farthestR, farthestC),
            Quaternion.identity,
            mazeParent.transform
        );
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

        foreach (var (dr, dc) in new[] { (0, 1), (0, -1), (1, 0), (-1, 0) })
        {
            int nr = r + dr;
            int nc = c + dc;

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