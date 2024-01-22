using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;

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

    SOLUTION = 256,
    ROOM = 512,

    OLD_VISIT = 1024
}

public struct Position
{
    public int X;
    public int Y;
    public int Z;
}

/*--------PRIM'S & KRUSKAL'S & WILSON'S & BACKTRACKER & HUNT AND KILL ALGORITHM--------*/
public struct Neighbour
{
    public Position Position;
    public WallState SharedWall;
}

/*--------KRUSKAL'S ALGORITHM--------*/

public class Tree
{
    public Tree parent;

    public static Tree getRoot(Tree tree)
    {
        if (tree.parent == null)
        {
            return tree;
        }
        else
        {
            return getRoot(tree.parent);
        }
    }

    public Boolean isConnected(Tree tree)
    {
        if (getRoot(this) == getRoot(tree))
        {
            return true;
        }
        return false;
    }

    public void connect(Tree tree)
    {
        getRoot(tree).parent = this;
    }
}

public static class MazeGenerator3D
{
    private static List<Position> branches = new();

    public static int startX;
    public static int startZ;

    private static int oldX;
    private static int oldZ;

    public static int finishX = 0;
    public static int finishZ = 0;

    public static int numberOfPaths = 2;

    public static List<Position> start = new();
    public static List<Position> finish = new();
    public static WallState GetOppositeWall(WallState wall)
    {
        switch (wall)
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

    /*--------ALGORITHMS--------*/
    private static WallState[,,] Prims(WallState[,,] maze, int width, int height, int depth)
    { 
        var rng = new System.Random(/*seed*/);
        var frontier = new List<Neighbour>();
        var position = new Position { X = startX, Y = height - 1, Z = startZ };

        maze[position.X, position.Y, position.Z] |= WallState.VISITED;

        maze[position.X, position.Y, position.Z] &= ~WallState.BELOW;
        maze[position.X, position.Y - 1, position.Z] &= ~WallState.ABOVE;

        maze[position.X, position.Y - 1, position.Z] |= WallState.VISITED;

        position = new Position { X = position.X, Y = position.Y - 1, Z = position.Z };

        frontier.AddRange(GetUnvisitedNeighbours(position, maze, width, height, depth));

        while (frontier.Count > 0)
        {
            
            var randIndex = rng.Next(0, frontier.Count);
            var randFront = frontier[randIndex].Position;

            frontier.RemoveAt(randIndex);

            var visitedNeighbours = GetVisitedNeighbours(randFront, maze, width, height, depth);
            randIndex = rng.Next(0, visitedNeighbours.Count);
            var randomNeigh = visitedNeighbours[randIndex];
            var nPosition = randomNeigh.Position;


            if (nPosition.Y > randFront.Y)
            {
                frontier.Clear();
            }


            maze[randFront.X, randFront.Y, randFront.Z] &= ~randomNeigh.SharedWall;
            maze[nPosition.X, nPosition.Y, nPosition.Z] &= ~GetOppositeWall(randomNeigh.SharedWall);

            maze[randFront.X, randFront.Y, randFront.Z] |= WallState.VISITED;

            frontier.AddRange(GetUnvisitedNeighbours(randFront, maze, width, height, depth));

            if (randFront.Y == 0)
            {
                position = randFront;
                frontier.Clear();
            }

        }

        
        MazeSolver.Solve(maze, width, height, depth, new Position { X = startX, Y = height - 1, Z = startZ }, position);

        return maze;
    }

    public static WallState[,,] Kruskals(WallState[,,] maze, int width, int height, int depth)
    {
        var rng = new System.Random(/*seed*/);
        var edges = new List<Neighbour>();

        Tree[,,] sets = new Tree[width, height, depth];
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < depth; k++)
                {
                    sets[i, j, k] = new Tree { };
                }
            }
        }

        edges.AddRange(GetEdges(width, height, depth));

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (maze[i, j, k].HasFlag(WallState.OLD_VISIT))
                    {
                        if (!sets[i, j, k].isConnected(sets[oldX, height - 1, oldZ]))
                        {
                            sets[i, j, k].connect(sets[oldX, height - 1, oldZ]);
                        }

                        edges.RemoveAll(e => e.Position.Equals(new Position { X = i, Y = j, Z = k }));
                    }
                }
            }
        }

        var start = new Position { X = startX, Y = height - 1, Z = startZ };
        sets[start.X, start.Y, start.Z].connect(sets[start.X, start.Y - 1, start.Z]);
        maze[start.X, start.Y, start.Z] &= ~WallState.BELOW;
        maze[start.X, start.Y - 1, start.Z] &= ~WallState.ABOVE;

        maze[start.X, start.Y, start.Z] |= WallState.VISITED;
        maze[start.X, start.Y - 1, start.Z] |= WallState.VISITED;
        edges.RemoveAll(e => e.Position.Y == start.Y);

        var finish = new Position { X = finishX, Y = 1, Z = finishZ };
        sets[finish.X, finish.Y, finish.Z].connect(sets[finish.X, finish.Y - 1, finish.Z]);
        maze[finish.X, finish.Y, finish.Z] &= ~WallState.BELOW;
        maze[finish.X, finish.Y - 1, finish.Z] &= ~WallState.ABOVE;

        maze[finish.X, finish.Y, finish.Z] |= WallState.VISITED;
        maze[finish.X, finish.Y - 1, finish.Z] |= WallState.VISITED;
        edges.RemoveAll(e => e.Position.Y == finish.Y && e.SharedWall == WallState.BELOW);
        edges.RemoveAll(e => e.Position.Y == 0);

        while (edges.Count > 0)
        {
            var randIndex = rng.Next(0, edges.Count);
            var randEdge = edges[randIndex];
            edges.RemoveAt(randIndex);

            var neighbour = new Position();
            if(randEdge.SharedWall == WallState.LEFT)
            {
                neighbour.X = randEdge.Position.X - 1;
                neighbour.Y = randEdge.Position.Y;
                neighbour.Z = randEdge.Position.Z;
            }
            if (randEdge.SharedWall == WallState.BACK)
            {
                neighbour.X = randEdge.Position.X;
                neighbour.Y = randEdge.Position.Y;
                neighbour.Z = randEdge.Position.Z - 1;
            }
            if (randEdge.SharedWall == WallState.BELOW)
            {
                neighbour.X = randEdge.Position.X;
                neighbour.Y = randEdge.Position.Y - 1;
                neighbour.Z = randEdge.Position.Z;
            }

            if (!sets[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z].isConnected(sets[neighbour.X, neighbour.Y, neighbour.Z]))
            {

                sets[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z].connect(sets[neighbour.X, neighbour.Y, neighbour.Z]);
                maze[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z] &= ~randEdge.SharedWall;
                maze[neighbour.X, neighbour.Y, neighbour.Z] &= ~GetOppositeWall(randEdge.SharedWall);

                if(randEdge.Position.Y > neighbour.Y)
                {
                    edges.RemoveAll(e => e.Position.Y == randEdge.Position.Y && e.SharedWall == WallState.BELOW);
                }

                if (sets[start.X, start.Y, start.Z].isConnected(sets[finish.X, finish.Y, finish.Z]))
                {
                    edges.Clear();
                }

            }
        }

        MarkAsVisited(maze, sets, start, width, height, depth);

        MazeSolver.Solve(maze, width, height, depth, new Position { X = startX, Y = height - 1, Z = startZ }, new Position { X = finishX, Y = 0, Z = finishZ });

        return maze;
    }

    private static WallState[,,] Wilsons(WallState[,,] maze, int width, int height, int depth)
    {
        var rng = new System.Random(/*seed*/);
        var position = new Position { X = startX, Y = height - 1, Z = startZ };

        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        maze[position.X, position.Y, position.Z] &= ~WallState.BELOW;
        maze[position.X, position.Y - 1, position.Z] &= ~WallState.ABOVE;
        maze[position.X, position.Y - 1, position.Z] |= WallState.VISITED;

        var remaining = width * (height - 1) * depth - 1;
        int nHeight = height - 1;
        while(remaining > 0)
        {
            remaining = remaining - Walk(maze, width, nHeight, depth);
        }

        MazeSolver.Solve(maze, width, height, depth, new Position { X = startX, Y = height - 1, Z = startZ }, new Position { X = finishX, Y = 0, Z = finishZ });
        return maze;
    }

    private static WallState[,,] RecursiveBacktracker(WallState[,,] maze, int width, int height, int depth)
    {
        var rng = new System.Random(/*seed*/);
        var positionStack = new Stack<Position>();
        var position = new Position { X = startX, Y = height - 1, Z = startZ };

        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        maze[position.X, position.Y, position.Z] &= ~WallState.BELOW;
        maze[position.X, position.Y - 1, position.Z] &= ~WallState.ABOVE;
        maze[position.X, position.Y - 1, position.Z] |= WallState.VISITED;
        position = new Position { X = position.X, Y = position.Y - 1, Z = position.Z };
        positionStack.Push(position);

        while(positionStack.Count > 0)
        {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height, depth);

            if(neighbours.Count > 0)
            {
                positionStack.Push(current);

                var randIndex = rng.Next(0, neighbours.Count);
                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.Position;
                maze[current.X, current.Y, current.Z] &= ~randomNeighbour.SharedWall;
                maze[nPosition.X, nPosition.Y, nPosition.Z] &= ~GetOppositeWall(randomNeighbour.SharedWall);
                maze[nPosition.X, nPosition.Y, nPosition.Z] |= WallState.VISITED;

                positionStack.Push(nPosition);

                if (nPosition.Y == 0)
                {
                    position = nPosition;
                    positionStack.Clear();
                }
            }
        }

        MazeSolver.Solve(maze, width, height, depth, new Position { X = startX, Y = height - 1, Z = startZ }, position);
        return maze;
    }

    private static WallState[,,] HuntAndKill(WallState[,,] maze, int width, int height, int depth)
    {
        var rng = new System.Random(/*seed*/);
        var position = new Position { X = startX, Y = height - 1, Z = startZ };

        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        maze[position.X, position.Y, position.Z] &= ~WallState.BELOW;
        maze[position.X, position.Y - 1, position.Z] &= ~WallState.ABOVE;
        maze[position.X, position.Y - 1, position.Z] |= WallState.VISITED;
        position = new Position { X = position.X, Y = position.Y - 1, Z = position.Z };

        while (position.X > -1)
        {
            WalkHAK(maze, position, width, height, depth);
            position = HuntHAK(maze, width, height, depth);

            if(position.Y == 0)
            {
                maze[position.X, position.Y, position.Z] |= WallState.VISITED;
                break;
            }
        }
        MazeSolver.Solve(maze, width, height, depth, new Position { X = startX, Y = height - 1, Z = startZ }, position);
        return maze;
    }
    
    /*--------GENERATE--------*/
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
                return MultiplePathsPrim(maze, width, height, depth);

            case 2:
                return MultiplePathsKruskal(maze, width, height, depth);

            case 3:
                return MultiplePathsWilson(maze, width, height, depth);

            case 4:
                return MultiplePathsBacktracker(maze, width, height, depth);

            case 5:
                return MultiplePathsHAK(maze, width, height, depth);

            default:
                return maze;
        }
    }

    private static WallState[,,] MultiplePathsPrim(WallState[,,] maze, int width, int height, int depth)
    {
        int i = start.Count - 1;
        var rng = new System.Random();

        startX = start[0].X;
        startZ = start[0].Z;
        start.RemoveAt(0);

        Prims(maze, width, height, depth);
        
        while(i > 0)
        {
            MarkAs(maze, width, height, depth, true);
            MazeSolver.ResetRooms(maze, width, height, depth);

            startX = start[0].X;
            startZ = start[0].Z;
            

            Prims(maze, width, height, depth);

            if(IsConnected(maze, width, height, depth))
            {
                start.RemoveAt(0);
                i--;
            }
            
        }
        
        MarkAs(maze, width, height, depth, false);
        MazeSolver.ReCalculate(maze, width, height, depth);

        return maze;
    }

    private static WallState[,,] MultiplePathsKruskal(WallState[,,] maze, int width, int height, int depth)
    {
        int i = start.Count - 1;
        var rng = new System.Random();

        startX = start[0].X;
        startZ = start[0].Z;
        start.RemoveAt(0);

        oldX = startX;
        oldZ = startZ;

        finishX = finish[0].X;
        finishZ = finish[0].Z;
        finish.RemoveAt(0);

        Kruskals(maze, width, height, depth);

        while (i > 0)
        {
            MarkAs(maze, width, height, depth, true);
            MazeSolver.ResetRooms(maze, width, height, depth);

            startX = start[0].X;
            startZ = start[0].Z;

            finishX = finish[0].X;
            finishZ = finish[0].Z;
            
            Kruskals(maze, width, height, depth);

            if (IsConnected(maze, width, height, depth))
            {
                start.RemoveAt(0);
                finish.RemoveAt(0);
                i--;
            }

        }
        

        MarkAs(maze, width, height, depth, false);
        MazeSolver.ReCalculate(maze, width, height, depth);

        return maze;
    }

    private static WallState[,,] MultiplePathsWilson(WallState[,,] maze, int width, int height, int depth)
    {
        int i = start.Count - 1;
        var rng = new System.Random();

        startX = start[0].X;
        startZ = start[0].Z;
        start.RemoveAt(0);

        finishX = finish[0].X;
        finishZ = finish[0].Z;
        finish.RemoveAt(0);

        Wilsons(maze, width, height, depth);

        while (i > 0)
        {
            MarkAs(maze, width, height, depth, true);
            MazeSolver.ResetRooms(maze, width, height, depth);

            startX = start[0].X;
            startZ = start[0].Z;

            finishX = finish[0].X;
            finishZ = finish[0].Z;

            Wilsons(maze, width, height, depth);

            if (IsConnected(maze, width, height, depth))
            {
                start.RemoveAt(0);
                finish.RemoveAt(0);
                i--;
            }

        }

        MarkAs(maze, width, height, depth, false);
        MazeSolver.ReCalculate(maze, width, height, depth);

        return maze;
    }

    private static WallState[,,] MultiplePathsBacktracker(WallState[,,] maze, int width, int height, int depth)
    {
        int i = start.Count - 1;
        var rng = new System.Random();

        startX = start[0].X;
        startZ = start[0].Z;
        start.RemoveAt(0);

        RecursiveBacktracker(maze, width, height, depth);

        while (i > 0)
        {
            MarkAs(maze, width, height, depth, true);
            MazeSolver.ResetRooms(maze, width, height, depth);

            startX = start[0].X;
            startZ = start[0].Z;
            
            RecursiveBacktracker(maze, width, height, depth);

            if (IsConnected(maze, width, height, depth))
            {
                start.RemoveAt(0);
                i--;
            }

        }

        MarkAs(maze, width, height, depth, false);
        MazeSolver.ReCalculate(maze, width, height, depth);

        return maze;
    }

    private static WallState[,,] MultiplePathsHAK(WallState[,,] maze, int width, int height, int depth)
    {
        int i = start.Count - 1;
        var rng = new System.Random();

        startX = start[0].X;
        startZ = start[0].Z;
        start.RemoveAt(0);

        HuntAndKill(maze, width, height, depth);

        while (i > 0)
        {
            MarkAs(maze, width, height, depth, true);
            MazeSolver.ResetRooms(maze, width, height, depth);

            startX = start[0].X;
            startZ = start[0].Z;
            
            HuntAndKill(maze, width, height, depth);

            if (IsConnected(maze, width, height, depth))
            {
                start.RemoveAt(0);
                i--;
            }

        }

        MarkAs(maze, width, height, depth, false);
        MazeSolver.ReCalculate(maze, width, height, depth);

        return maze;
    }

    /*--------PRIM'S ALGORITHM (+ RECURSIVE BACKTRACKER)--------*/
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

        if (p.Z > 0) //back
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

    /*--------KRUSKAL'S ALGORITHM--------*/
    private static List<Neighbour> GetEdges(int width, int height, int depth)
    {
        var edges = new List<Neighbour>();

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                for(int k = 0; k < depth; ++k)
                {
                    if (i != 0)
                    {
                        edges.Add(new Neighbour
                        {
                            Position = new Position
                            {
                                X = i,
                                Y = j,
                                Z = k
                            },
                            SharedWall = WallState.LEFT
                        });
                    }

                    if (j != 0)
                    {
                        edges.Add(new Neighbour
                        {
                            Position = new Position
                            {
                                X = i,
                                Y = j,
                                Z = k
                            },
                            SharedWall = WallState.BELOW
                        });
                    }

                    if (k != 0)
                    {
                        edges.Add(new Neighbour
                        {
                            Position = new Position
                            {
                                X = i,
                                Y = j,
                                Z = k
                            },
                            SharedWall = WallState.BACK
                        });
                    }
                }
                
            }
        }

        return edges;
    }

    private static void MarkAsVisited(WallState[,,] maze, Tree[,,] sets, Position start, int width, int height, int depth)  
    {
        WallState initial = WallState.ABOVE | WallState.BELOW | WallState.LEFT | WallState.RIGHT | WallState.FRONT | WallState.BACK;

        for (int i = 0; i < width; ++i)
        {
            for(int j = 0; j < height; ++j)
            {
                for(int k = 0; k < depth; ++k)
                {
                    if (sets[i, j, k].isConnected(sets[start.X, start.Y, start.Z])) 
                    {
                        maze[i, j, k] |= WallState.VISITED;
                    }

                    else
                    {
                        maze[i, j, k] = initial;
                    }
                }
            }
        }
    }


    /*--------WILSON'S ALGORITHM--------*/

    private static int Walk(WallState[,,] maze, int width, int height, int depth)
    {
        WallState[,,] visits = new WallState[width, height, depth];
        var startCell = new Position();
        var randCell = new Position();
        bool walking = true;
        var unvisited = new List<Position>();
        var lowestY = new List<int>();
        int steps;

        for (int i = 0; i < width; i++)
        {
            for(int j = 1; j < height; j++)
            {
                for(int k = 0; k < depth; k++)
                {
                    if (!maze[i, j, k].HasFlag(WallState.VISITED))
                    {
                        unvisited.Add(new Position { X = i, Y = j, Z = k });
                    }
                    else
                    {
                        lowestY.Add(j);
                    }
                    
                }
            }
        }

        lowestY.Sort();
        Shuffle(unvisited);

        for(int u = 0; u < unvisited.Count; u++)
        {
            if (unvisited[u].Y <= lowestY[0])
            {
                randCell = unvisited[u];
                break;
            }
        }

        if(randCell.Y == 1)
        {
            randCell.X = finishX;
            randCell.Z = finishZ;
        }

        startCell = randCell;

        while (walking)
        {
            walking = false;
            var neigh = GetValidNeighbour(randCell, visits, width, lowestY[0] + 1, depth);

            if (!maze[neigh.X, neigh.Y, neigh.Z].HasFlag(WallState.VISITED))
            {
                randCell = neigh;
                walking = true;
            } 
        }

        steps = VisitPath(startCell, visits, maze);

        if (startCell.Y == 1)
        {
            steps = width * height * depth;

            maze[startCell.X, startCell.Y, startCell.Z] &= ~WallState.BELOW;
            maze[startCell.X, startCell.Y - 1, startCell.Z] &= ~WallState.ABOVE;

            maze[startCell.X, startCell.Y - 1, startCell.Z] |= WallState.VISITED;
        }

        return steps;
        
    }

    private static Position GetValidNeighbour(Position randCell, WallState[,,] visits, int width, int height, int depth)
    {
        var dir = new List<WallState> { WallState.LEFT, WallState.RIGHT, WallState.FRONT, WallState.BACK, WallState.ABOVE};
        Shuffle(dir);

        foreach(WallState d in dir)
        {
            //czyszczenie sciezki
            visits[randCell.X, randCell.Y, randCell.Z] &= ~visits[randCell.X, randCell.Y, randCell.Z];
            visits[randCell.X, randCell.Y, randCell.Z] |= d;

            if (d == WallState.LEFT && randCell.X > 0)
            {
                return new Position
                {
                    X = randCell.X - 1,
                    Y = randCell.Y,
                    Z = randCell.Z
                };
            }
            if (d == WallState.RIGHT && randCell.X < width - 1)
            {
                return new Position
                {
                    X = randCell.X + 1,
                    Y = randCell.Y,
                    Z = randCell.Z
                };
            }
            if (d == WallState.ABOVE && randCell.Y < height - 1)
            {
                return new Position
                {
                    X = randCell.X,
                    Y = randCell.Y + 1,
                    Z = randCell.Z
                };
            }
            if (d == WallState.FRONT && randCell.Z < depth - 1)
            {
                return new Position
                {
                    X = randCell.X,
                    Y = randCell.Y,
                    Z = randCell.Z + 1
                };
            }
            if (d == WallState.BACK && randCell.Z > 0)
            {
                return new Position
                {
                    X = randCell.X,
                    Y = randCell.Y,
                    Z = randCell.Z - 1
                };
            }
        }

        return randCell;
    }

    private static int VisitPath(Position start, WallState[,,] visits, WallState[,,] maze)
    {
        var cell = start;
        var nextCell = new Position();
        int steps = 0;

        while (!maze[cell.X, cell.Y, cell.Z].HasFlag(WallState.VISITED))
        {
            steps++;

            if (visits[cell.X, cell.Y, cell.Z].HasFlag(WallState.LEFT))
            {
                maze[cell.X, cell.Y, cell.Z] &= ~WallState.LEFT;
                nextCell = cell;
                nextCell.X = cell.X - 1;
                maze[nextCell.X, nextCell.Y, nextCell.Z] &= ~WallState.RIGHT;
            }
            if (visits[cell.X, cell.Y, cell.Z].HasFlag(WallState.RIGHT))
            {
                maze[cell.X, cell.Y, cell.Z] &= ~WallState.RIGHT;
                nextCell = cell;
                nextCell.X = cell.X + 1;
                maze[nextCell.X, nextCell.Y, nextCell.Z] &= ~WallState.LEFT;
            }
            if (visits[cell.X, cell.Y, cell.Z].HasFlag(WallState.ABOVE))
            {
                maze[cell.X, cell.Y, cell.Z] &= ~WallState.ABOVE;
                nextCell = cell;
                nextCell.Y = cell.Y + 1;
                maze[nextCell.X, nextCell.Y, nextCell.Z] &= ~WallState.BELOW;
            }
            /*if (visits[cell.X, cell.Y, cell.Z].HasFlag(WallState.BELOW))
            {
                maze[cell.X, cell.Y, cell.Z] &= ~WallState.BELOW;
                nextCell = cell;
                nextCell.Y = cell.Y - 1;
                maze[nextCell.X, nextCell.Y, nextCell.Z] &= ~WallState.ABOVE;
            }*/
            if (visits[cell.X, cell.Y, cell.Z].HasFlag(WallState.FRONT))
            {
                maze[cell.X, cell.Y, cell.Z] &= ~WallState.FRONT;
                nextCell = cell;
                nextCell.Z = cell.Z + 1;
                maze[nextCell.X, nextCell.Y, nextCell.Z] &= ~WallState.BACK;
            }
            if (visits[cell.X, cell.Y, cell.Z].HasFlag(WallState.BACK))
            {
                maze[cell.X, cell.Y, cell.Z] &= ~WallState.BACK;
                nextCell = cell;
                nextCell.Z = cell.Z - 1;
                maze[nextCell.X, nextCell.Y, nextCell.Z] &= ~WallState.FRONT;
            }

            maze[cell.X, cell.Y, cell.Z] |= WallState.VISITED;
            cell = nextCell;

        }
        return steps;
    }

    /*--------HUNT AND KILL ALGORITHM--------*/
    private static void WalkHAK(WallState[,,] maze, Position position, int width, int height, int depth) 
    {
        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        Neighbour newPos = VisitRandomNeighbour(position, width, height, depth, maze);

        if(newPos.SharedWall == WallState.VISITED)
        {
            return;
        }

        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        maze[position.X, position.Y, position.Z] &= ~newPos.SharedWall;
        maze[newPos.Position.X, newPos.Position.Y, newPos.Position.Z] &= ~GetOppositeWall(newPos.SharedWall);
        position = newPos.Position;

        while(newPos.SharedWall != WallState.VISITED)
        {
            newPos = VisitRandomNeighbour(position, width, height, depth, maze);
            maze[position.X, position.Y, position.Z] |= WallState.VISITED;

            if(newPos.SharedWall != WallState.VISITED)
            {
                maze[position.X, position.Y, position.Z] &= ~newPos.SharedWall;
                maze[newPos.Position.X, newPos.Position.Y, newPos.Position.Z] &= ~GetOppositeWall(newPos.SharedWall);
                position = newPos.Position;
            }
        }
    }

    private static Position HuntHAK(WallState[,,] maze, int width, int height, int depth)
    {
        List<Neighbour> neighbours = new List<Neighbour>();
        Position position = new Position { X = -1, Y = -1, Z = -1 };

        var rng = new System.Random(/*seed*/);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (!maze[i, j, k].HasFlag(WallState.VISITED))
                    {
                        position.X = i;
                        position.Y = j;
                        position.Z = k;

                        neighbours = GetVisitedNeighbours(position, maze, width, height, depth);

                        if (neighbours.Count > 0)
                        {
                            i = width;
                            j = height;
                            break;
                        }
                    }
                }
            }
        }

        if (position.X == -1)
        {
            return position;
        }

        Shuffle(neighbours);
        Neighbour neighbourPos = neighbours[0];

        maze[position.X, position.Y, position.Z] &= ~neighbourPos.SharedWall;
        maze[neighbourPos.Position.X, neighbourPos.Position.Y, neighbourPos.Position.Z] &= ~GetOppositeWall(neighbourPos.SharedWall);

        return position;

    }

    private static Neighbour VisitRandomNeighbour(Position position, int width, int height, int depth, WallState[,,] maze, bool plane = false) //& branches
    {
        List<WallState> dir;
        
        dir = new List<WallState> { WallState.LEFT, WallState.RIGHT, WallState.FRONT, WallState.BACK};
        
        
        Shuffle(dir);

        foreach (WallState d in dir)
        {
            if (d == WallState.LEFT && position.X > 0 && !maze[position.X - 1, position.Y, position.Z].HasFlag(WallState.VISITED) && !maze[position.X - 1, position.Y, position.Z].HasFlag(WallState.OLD_VISIT))
            {
                return new Neighbour
                {
                    Position = new Position
                    {
                        X = position.X - 1,
                        Y = position.Y,
                        Z = position.Z
                    },
                    SharedWall = WallState.LEFT
                };
            }
            if (d == WallState.RIGHT && position.X < width - 1 && !maze[position.X + 1, position.Y, position.Z].HasFlag(WallState.VISITED) && !maze[position.X + 1, position.Y, position.Z].HasFlag(WallState.OLD_VISIT))
            {
                return new Neighbour
                {
                    Position = new Position
                    {
                        X = position.X + 1,
                        Y = position.Y,
                        Z = position.Z
                    },
                    SharedWall = WallState.RIGHT
                };
            }
            if (d == WallState.FRONT && position.Z < depth - 1 && !maze[position.X, position.Y, position.Z + 1].HasFlag(WallState.VISITED) && !maze[position.X, position.Y, position.Z + 1].HasFlag(WallState.OLD_VISIT))
            {
                return new Neighbour
                {
                    Position = new Position
                    {
                        X = position.X,
                        Y = position.Y,
                        Z = position.Z + 1
                    },
                    SharedWall = WallState.FRONT
                };
            }
            if (d == WallState.BACK && position.Z > 0 && !maze[position.X, position.Y, position.Z - 1].HasFlag(WallState.VISITED) && !maze[position.X, position.Y, position.Z - 1].HasFlag(WallState.OLD_VISIT))
            {
                return new Neighbour
                {
                    Position = new Position
                    {
                        X = position.X,
                        Y = position.Y,
                        Z = position.Z - 1
                    },
                    SharedWall = WallState.BACK
                };
            }
        }

        return new Neighbour
        {
            Position = position,
            SharedWall = WallState.VISITED
        };
    }

    /*--------PARAMETERS--------*/
    //branches
    public static WallState[,,] BranchesLength(WallState[,,] maze, int width, int height, int depth, int length)
    {
        MazeSolver.ReCalculate(maze, width, height, depth);
        var startEnds = new List<Position>(MazeSolver.ends);

        HashSet<Position> toDelete = new HashSet<Position>();

        while (startEnds.Count > 0)
        {
            int i = length;

            var pos = startEnds[0];
            startEnds.RemoveAt(0);

            Neighbour newPos = VisitRandomNeighbour(pos, width, height, depth, maze, true);
            branches.Clear();

            bool[,,] wasHere = new bool[width, height, depth];
            bool r = RecursiveBranches(pos, maze, wasHere, width, height, depth);

            int placed = branches.Count;

            if (placed < length) // krotsze niz podana dlugosc
            {
                if (newPos.SharedWall != WallState.VISITED)
                {
                    maze[pos.X, pos.Y, pos.Z] &= ~newPos.SharedWall;
                    maze[newPos.Position.X, newPos.Position.Y, newPos.Position.Z] &= ~GetOppositeWall(newPos.SharedWall);
                    pos = newPos.Position;
                    maze[pos.X, pos.Y, pos.Z] |= WallState.VISITED;
                    i--;
                }

                i -= placed;

                while (newPos.SharedWall != WallState.VISITED && i > 0)
                {
                    newPos = VisitRandomNeighbour(pos, width, height, depth, maze);

                    if (newPos.SharedWall != WallState.VISITED)
                    {
                        maze[pos.X, pos.Y, pos.Z] &= ~newPos.SharedWall;
                        maze[newPos.Position.X, newPos.Position.Y, newPos.Position.Z] &= ~GetOppositeWall(newPos.SharedWall);
                        pos = newPos.Position;
                        maze[pos.X, pos.Y, pos.Z] |= WallState.VISITED;
                    }

                    i--;

                }
            }

            if (placed > length)
            {

                while (placed > length)
                {

                    var branch = branches.Last();
                    toDelete.Add(branch);
                    branches.RemoveAt(branches.Count - 1);
                    placed--;

                }

            }

        }

        foreach (var d in toDelete)
        {
            maze[d.X, d.Y, d.Z] &= ~WallState.VISITED;

            if(!maze[d.X, d.Y, d.Z].HasFlag(WallState.LEFT))
            {
                maze[d.X, d.Y, d.Z] |= WallState.LEFT;
                maze[d.X - 1, d.Y, d.Z] |= WallState.RIGHT;
            }

            if (!maze[d.X, d.Y, d.Z].HasFlag(WallState.RIGHT))
            {
                maze[d.X, d.Y, d.Z] |= WallState.RIGHT;
                maze[d.X + 1, d.Y, d.Z] |= WallState.LEFT;
            }

            if (!maze[d.X, d.Y, d.Z].HasFlag(WallState.FRONT))
            {
                maze[d.X, d.Y, d.Z] |= WallState.FRONT;
                maze[d.X, d.Y, d.Z + 1] |= WallState.BACK;
            }
            if (!maze[d.X, d.Y, d.Z].HasFlag(WallState.BACK))
            {
                maze[d.X, d.Y, d.Z] |= WallState.BACK;
                maze[d.X, d.Y, d.Z - 1] |= WallState.FRONT;
            }

        }

        MazeSolver.ResetRooms(maze, width, height, depth);
        MazeSolver.ReCalculate(maze, width, height, depth);

        return maze;
    }

    private static bool RecursiveBranches(Position position, WallState[,,] maze, bool[,,] wasHere, int width, int height, int depth)
    {
        if (maze[position.X, position.Y, position.Z].HasFlag(WallState.SOLUTION))
        {
            return true;
        }

        if (wasHere[position.X, position.Y, position.Z] || !maze[position.X, position.Y, position.Z].HasFlag(WallState.VISITED))
        {
            return false;
        }

        wasHere[position.X, position.Y, position.Z] = true;

        //left
        if (position.X > 0 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.LEFT))
        {
            if (RecursiveBranches(new Position { X = position.X - 1, Y = position.Y, Z = position.Z }, maze, wasHere, width, height, depth))
            {
                branches.Add(position);
                return true;
            }
        }

        //right
        if (position.X < width - 1 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.RIGHT))
        {
            if (RecursiveBranches(new Position { X = position.X + 1, Y = position.Y, Z = position.Z }, maze, wasHere, width, height, depth))
            {
                branches.Add(position);
                return true;
            }
        }

        if (position.Z > 0 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.BACK))
        {
            if (RecursiveBranches(new Position { X = position.X, Y = position.Y, Z = position.Z - 1 }, maze, wasHere, width, height, depth))
            {
                branches.Add(position);
                return true;
            }
        }

        if (position.Z < depth - 1 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.FRONT))
        {
            if (RecursiveBranches(new Position { X = position.X, Y = position.Y, Z = position.Z + 1 }, maze, wasHere, width, height, depth))
            {
                branches.Add(position);
                return true;
            }
        }

        return false;
    }

    //paths
    private static void MarkAs(WallState[,,] maze, int width, int height, int depth, bool old)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (maze[i, j, k].HasFlag(WallState.VISITED) && old)
                    {
                        maze[i, j, k] &= ~WallState.VISITED;
                        maze[i, j, k] |= WallState.OLD_VISIT;
                    }

                    if (maze[i, j, k].HasFlag(WallState.OLD_VISIT) && !old)
                    {
                        maze[i, j, k] &= ~WallState.OLD_VISIT;
                        maze[i, j, k] |= WallState.VISITED;
                    }
                }
            }
        }
    }

    private static bool IsConnected(WallState[,,] maze, int width, int height, int depth)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (maze[i, j, k].HasFlag(WallState.VISITED) && maze[i, j, k].HasFlag(WallState.OLD_VISIT))
                    {
                        return true;
                    }
                }
            }
        }

        //else reset
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (maze[i, j, k].HasFlag(WallState.VISITED))
                    {

                        maze[i, j, k] &= ~WallState.VISITED;

                        if (maze[i, j, k].HasFlag(WallState.SOLUTION))
                        {
                            maze[i, j, k] &= ~WallState.SOLUTION;
                        }

                        if (!maze[i, j, k].HasFlag(WallState.LEFT))
                        {
                            maze[i, j, k] |= WallState.LEFT;
                        }

                        if (!maze[i, j, k].HasFlag(WallState.RIGHT))
                        {
                            maze[i, j, k] |= WallState.RIGHT;
                        }

                        if (!maze[i, j, k].HasFlag(WallState.FRONT))
                        {
                            maze[i, j, k] |= WallState.FRONT;
                        }

                        if (!maze[i, j, k].HasFlag(WallState.BACK))
                        {
                            maze[i, j, k] |= WallState.BACK;
                        }

                        if (!maze[i, j, k].HasFlag(WallState.ABOVE))
                        {
                            maze[i, j, k] |= WallState.ABOVE;
                        }

                        if (!maze[i, j, k].HasFlag(WallState.BELOW))
                        {
                            maze[i, j, k] |= WallState.BELOW;
                        }
                    }



                }
            }
        }

        return false;
    }


    /*--------OTHER--------*/
    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}

