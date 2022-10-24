using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum WallStateOld
{
    // 0000 -> NO WALLS
    // 1111 -> LEFT, RIGHT, UP, DOWN
    LEFT = 1,   // 0001
    RIGHT = 2,  // 0010
    UP = 4,     // 0100
    DOWN = 8,   // 1000

    FRONTIER = 64, // 0100 0000
    VISITED = 128, // 1000 0000
}

public struct PositionOld
{
    public int X;
    public int Y;
}

public struct NeighbourOld
{
    public PositionOld Position;
    public WallStateOld SharedWall;
}

public static class MazeGenerator
{

    private static WallStateOld GetOppositeWall(WallStateOld wall)
    {
        switch(wall)
        {
            case WallStateOld.RIGHT: return WallStateOld.LEFT;
            case WallStateOld.LEFT: return WallStateOld.RIGHT;
            case WallStateOld.UP: return WallStateOld.DOWN;
            case WallStateOld.DOWN: return WallStateOld.UP;
            default: return WallStateOld.LEFT;
        }
    }

    //backtracking (irrelevant)
    private static WallStateOld[,] ApplyRecursiveBacktracker(WallStateOld[,] maze, int width, int height)
    {
        //here we make changes

        var rng = new System.Random(/*seed*/);
        var positionStack = new Stack<PositionOld>();
        var position = new PositionOld { X = rng.Next(0, width), Y = rng.Next(0, height) };

        maze[position.X, position.Y] |= WallStateOld.VISITED; //1000 1111
        positionStack.Push(position);

        while (positionStack.Count > 0)
        {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if(neighbours.Count > 0)
            {
                positionStack.Push(current);

                var randIndex = rng.Next(0, neighbours.Count);
                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.Position;
                maze[current.X, current.Y] &= ~randomNeighbour.SharedWall;
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeighbour.SharedWall);

                maze[nPosition.X, nPosition.Y] |= WallStateOld.VISITED;

                positionStack.Push(nPosition);
            }
        }

        return maze;
    }

    private static WallStateOld[,] PrimsAlgorithm(WallStateOld[,] maze, int width, int height)
    {

        WallStateOld initial = WallStateOld.RIGHT | WallStateOld.LEFT | WallStateOld.UP | WallStateOld.DOWN;
        //losowanie pozycji startowej
        var rng = new System.Random(/*seed*/);
        var visited = new List<PositionOld>();
        var frontier = new List<NeighbourOld>();
        var position = new PositionOld { X = rng.Next(0, width), Y = rng.Next(0, height) };

        maze[position.X, position.Y] |= WallStateOld.VISITED; //1000 1111
        visited.Add(position);
        Debug.Log("Start: " + position.X + ", " + position.Y);

        frontier.AddRange(GetUnvisitedNeighbours(position, maze, width, height));
        
        while(frontier.Count > 0)
        {
            for (int i = 0; i < frontier.Count; i++)
            {
                Debug.Log("Frontier " + i + ": " + frontier[i].Position.X + ", " + frontier[i].Position.Y);
            }
            Debug.Log("Next iteration");
            var randIndex = rng.Next(0, frontier.Count);
            var randomFront = frontier[randIndex].Position;

            Debug.Log("Chosen frontier: " + randomFront.X + ", " + randomFront.Y);
            frontier.RemoveAt(randIndex);

            if (maze[randomFront.X, randomFront.Y] == initial)
            {
                var visitedNeighbours = GetVisitedNeighbours(randomFront, maze, width, height);
                randIndex = rng.Next(0, visitedNeighbours.Count);
                var randomNeigh = visitedNeighbours[randIndex];
                var nPosition = randomNeigh.Position;

                Debug.Log("Visited neighbour: " + nPosition.X + ", " + nPosition.Y);

                maze[randomFront.X, randomFront.Y] &= ~randomNeigh.SharedWall;
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeigh.SharedWall);

                maze[randomFront.X, randomFront.Y] |= WallStateOld.VISITED;
                visited.Add(randomFront);

                frontier.AddRange(GetUnvisitedNeighbours(randomFront, maze, width, height));
            }

        }

        return maze;

    }


    private static List<NeighbourOld> GetUnvisitedNeighbours(PositionOld p, WallStateOld[,] maze, int width, int height)
    {
        var list = new List<NeighbourOld>();
        //WallStateOld initial = WallStateOld.RIGHT | WallStateOld.LEFT | WallStateOld.UP | WallStateOld.DOWN;

        if (p.X > 0) //left
        {
            if (!maze[p.X - 1, p.Y].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallStateOld.LEFT
                });
            }
        }

        if (p.Y > 0) //down
        {
            if (!maze[p.X, p.Y - 1].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallStateOld.DOWN
                });
            }
        }

        if (p.Y < height - 1) //up
        {
            if (!maze[p.X, p.Y + 1].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallStateOld.UP
                });
            }
        }

        if (p.X < width - 1) //right
        {
            if (!maze[p.X + 1, p.Y].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallStateOld.RIGHT
                });
            }
        }

        return list;
    }

    private static List<NeighbourOld> GetVisitedNeighbours(PositionOld p, WallStateOld[,] maze, int width, int height)
    {
        var list = new List<NeighbourOld>();

        if (p.X > 0) //left
        {
            if (maze[p.X - 1, p.Y].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallStateOld.LEFT
                });
            }
        }

        if (p.Y > 0) //down
        {
            if (maze[p.X, p.Y - 1].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallStateOld.DOWN
                });
            }
        }

        if (p.Y < height - 1) //up
        {
            if (maze[p.X, p.Y + 1].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallStateOld.UP
                });
            }
        }

        if (p.X < width - 1) //right
        {
            if (maze[p.X + 1, p.Y].HasFlag(WallStateOld.VISITED))
            {
                list.Add(new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallStateOld.RIGHT
                });
            }
        }

        return list;
    }

    public static WallStateOld[,] Generate(int width, int height)
    {
        WallStateOld[,] maze = new WallStateOld[width, height];
        WallStateOld initial = WallStateOld.RIGHT | WallStateOld.LEFT | WallStateOld.UP | WallStateOld.DOWN;

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                maze[i, j] = initial;
            }
        }


        return PrimsAlgorithm(maze, width, height);
        //return ApplyRecursiveBacktracker(maze, width, height);
    }
}
