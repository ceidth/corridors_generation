using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum WallState
{
    ABOVE = 1,
    BELOW = 2,
    LEFT = 4,
    RIGHT = 8,
    FRONT = 16,
    BACK = 32,

    FRONTIER = 64,
    VISITED = 128,
}

public struct Position
{
    public int X;
    public int Y;
    public int Z;
}

public struct Neighbour
{
    public Position Position;
    public WallState SharedWall;
}

public static class MazeGenerator3D
{

    public static WallState GetOppositeWall(WallState wall)
    {
        switch(wall)
        {
            case WallState.BELOW: return WallState.ABOVE;
            case WallState.ABOVE: return WallState.BELOW;
            case WallState.RIGHT: return WallState.LEFT;
            case WallState.LEFT: return WallState.RIGHT;
            case WallState.FRONT: return WallState.BACK;
            case WallState.BACK: return WallState.FRONT;
            default: return WallState.BACK;
        }
    }

    private static WallState[,,] PrimsAlgorithm(WallState[,,] maze, int width, int height, int depth, WallState initial)
    {
        //pozycja startowa na samej gorze kostki
        var rng = new System.Random(/*seed*/);
        var visited = new List<Position>();
        var frontier = new List<Neighbour>();
        var position = new Position { X = rng.Next(0, width), Y = height - 1, Z = rng.Next(0, depth) };

        //zdjemowana gorna sciana z pola, ustawiana flaga visited
        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        maze[position.X, position.Y, position.Z] &= ~WallState.ABOVE;
        visited.Add(position);
        Debug.Log("Start: " + position.X + ", " + position.Y + ", " + position.Z);

        frontier.AddRange(GetUnvisitedNeighbours(position, maze, width, height, depth));

        while(frontier.Count > 0)
        {
            for (int i = 0; i < frontier.Count; i++)
            {
                Debug.Log("Frontier " + i + ": " + frontier[i].Position.X + ", " + frontier[i].Position.Y + ", " + frontier[i].Position.Z);
            }

            var randIndex = rng.Next(0, frontier.Count);
            var randFront = frontier[randIndex].Position;

            Debug.Log("Chosen frontier: " + randFront.X + ", " + randFront.Y + ", " + randFront.Z);
            frontier.RemoveAt(randIndex);

            var visitedNeighbours = GetVisitedNeighbours(randFront, maze, width, height, depth);
            randIndex = rng.Next(0, visitedNeighbours.Count);
            var randomNeigh = visitedNeighbours[randIndex];
            var nPosition = randomNeigh.Position;
            
            if(nPosition.Y > randFront.Y)
            {
                Debug.Log("Clearing bc - visited Y: " + nPosition.Y + ", frontier Y: " + randFront.Y);
                frontier.Clear();
            }


            maze[randFront.X, randFront.Y, randFront.Z] &= ~randomNeigh.SharedWall;
            maze[nPosition.X, nPosition.Y, nPosition.Z] &= ~GetOppositeWall(randomNeigh.SharedWall);

            maze[randFront.X, randFront.Y, randFront.Z] |= WallState.VISITED;
            visited.Add(randFront);

            frontier.AddRange(GetUnvisitedNeighbours(randFront, maze, width, height, depth)); 
            
            if(randFront.Y == 0)
            {
                frontier.Clear();
            }

        }

        return maze;
    }

    private static WallState[,,] KruskalsAlgorithm(WallState[,,] maze, int width, int height, int depth, WallState initial)
    {


        return maze;
    }

    private static WallState[,,] WilsonsAlgorithm(WallState[,,] maze, int width, int height, int depth, WallState initial)
    {
        return maze;
    }

    public static WallState[,,] Generate(int width, int height, int depth, int choice)
    {
        WallState[,,] maze = new WallState[width, height, depth];
        WallState initial = WallState.ABOVE | WallState.BELOW | WallState.LEFT | WallState.RIGHT | WallState.FRONT | WallState.BACK;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    maze[i, j, k] = initial;
                }
            }
        }
            
        switch (choice)
        {
            case 1:
                return PrimsAlgorithm(maze, width, height, depth, initial);

            case 2:
                return KruskalsAlgorithm(maze, width, height, depth, initial);

            case 3:
                return WilsonsAlgorithm(maze, width, height, depth, initial);

            default:
                return maze;
        }
    }

    private static List<Neighbour> GetUnvisitedNeighbours(Position p, WallState[,,] maze, int width, int height, int depth)
    {
        var list = new List<Neighbour>();

        if(p.X > 0) //left
        {
            if (!maze[p.X - 1, p.Y, p.Z].HasFlag(WallState.VISITED) && !maze[p.X - 1, p.Y, p.Z].HasFlag(WallState.FRONTIER))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y,
                        Z = p.Z
                    },
                    SharedWall = WallState.LEFT
                });

                maze[p.X - 1, p.Y, p.Z] |= WallState.FRONTIER;
            }
        }

        if (p.X < width - 1) //right
        {
            if (!maze[p.X + 1, p.Y, p.Z].HasFlag(WallState.VISITED) && !maze[p.X + 1, p.Y, p.Z].HasFlag(WallState.FRONTIER))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y,
                        Z = p.Z
                    },
                    SharedWall = WallState.RIGHT
                });

                maze[p.X + 1, p.Y, p.Z] |= WallState.FRONTIER;
            }
        }

        if (p.Y > 0) //below
        {
            if (!maze[p.X, p.Y - 1, p.Z].HasFlag(WallState.VISITED) && !maze[p.X, p.Y - 1, p.Z].HasFlag(WallState.FRONTIER))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y - 1,
                        Z = p.Z
                    },
                    SharedWall = WallState.BELOW
                });

                maze[p.X, p.Y - 1, p.Z] |= WallState.FRONTIER;
            }
        }

        /*if (p.Y < height - 1) //above
        {
            if (!maze[p.X, p.Y + 1, p.Z].HasFlag(WallState.VISITED) && !maze[p.X, p.Y + 1, p.Z].HasFlag(WallState.FRONTIER))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1,
                        Z = p.Z
                    },
                    SharedWall = WallState.ABOVE
                });

                maze[p.X, p.Y + 1, p.Z] |= WallState.FRONTIER;
            }
        }*/

        if(p.Z > 0) //back
        {
            if (!maze[p.X, p.Y, p.Z - 1].HasFlag(WallState.VISITED) && !maze[p.X, p.Y, p.Z - 1].HasFlag(WallState.FRONTIER))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y,
                        Z = p.Z - 1
                    },
                    SharedWall = WallState.BACK
                });

                maze[p.X, p.Y, p.Z - 1] |= WallState.FRONTIER;
            }
        }


        if (p.Z < depth - 1) //front
        {
            if (!maze[p.X, p.Y, p.Z + 1].HasFlag(WallState.VISITED) && !maze[p.X, p.Y, p.Z + 1].HasFlag(WallState.FRONTIER))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y,
                        Z = p.Z + 1
                    },
                    SharedWall = WallState.FRONT
                });

                maze[p.X, p.Y, p.Z + 1] |= WallState.FRONTIER;
            }
        }

        return list;
    }

    private static List<Neighbour> GetVisitedNeighbours(Position p, WallState[,,] maze, int width, int height, int depth)
    {
        var list = new List<Neighbour>();

        if (p.X > 0) //left
        {
            if (maze[p.X - 1, p.Y, p.Z].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X - 1,
                        Y = p.Y,
                        Z = p.Z
                    },
                    SharedWall = WallState.LEFT
                });
            }
        }

        if (p.X < width - 1) //right
        {
            if (maze[p.X + 1, p.Y, p.Z].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X + 1,
                        Y = p.Y,
                        Z = p.Z
                    },
                    SharedWall = WallState.RIGHT
                });
            }
        }

        if (p.Y > 0) //below
        {
            if (maze[p.X, p.Y - 1, p.Z].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y - 1,
                        Z = p.Z
                    },
                    SharedWall = WallState.BELOW
                });
            }
        }

        if (p.Y < height - 1) //above
        {
            if (maze[p.X, p.Y + 1, p.Z].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y + 1,
                        Z = p.Z
                    },
                    SharedWall = WallState.ABOVE
                });
            }
        }

        if (p.Z > 0) //back
        {
            if (maze[p.X, p.Y, p.Z - 1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y,
                        Z = p.Z - 1
                    },
                    SharedWall = WallState.BACK
                });
            }
        }

        if (p.Z < depth - 1) //front
        {
            if (maze[p.X, p.Y, p.Z + 1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                {
                    Position = new Position
                    {
                        X = p.X,
                        Y = p.Y,
                        Z = p.Z + 1
                    },
                    SharedWall = WallState.FRONT
                });
            }
        }

        return list;
    }
}

