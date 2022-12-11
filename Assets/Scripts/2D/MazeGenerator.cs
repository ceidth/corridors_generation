using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UIElements;
//using static UnityEditor.PlayerSettings;

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

public struct Edge2d
{
    public PositionOld Position { get; set; }
    public WallStateOld Wall; //LEFT & UP
    public Tree Set;
}

public struct Path2d
{
    public PositionOld Position;
    public WallStateOld Direction;
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
                //Debug.Log("Frontier " + i + ": " + frontier[i].Position.X + ", " + frontier[i].Position.Y);
            }
            //Debug.Log("Next iteration");
            var randIndex = rng.Next(0, frontier.Count);
            var randomFront = frontier[randIndex].Position;

            //Debug.Log("Chosen frontier: " + randomFront.X + ", " + randomFront.Y);
            frontier.RemoveAt(randIndex);

            if (maze[randomFront.X, randomFront.Y] == initial)
            {
                var visitedNeighbours = GetVisitedNeighbours(randomFront, maze, width, height);
                randIndex = rng.Next(0, visitedNeighbours.Count);
                var randomNeigh = visitedNeighbours[randIndex];
                var nPosition = randomNeigh.Position;

                //Debug.Log("Visited neighbour: " + nPosition.X + ", " + nPosition.Y);

                maze[randomFront.X, randomFront.Y] &= ~randomNeigh.SharedWall;
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeigh.SharedWall);

                maze[randomFront.X, randomFront.Y] |= WallStateOld.VISITED;
                visited.Add(randomFront);

                frontier.AddRange(GetUnvisitedNeighbours(randomFront, maze, width, height));
            }

        }

        return maze;

    }

    private static WallStateOld[,] KruskalsAlgorithm(WallStateOld[,] maze, int width, int height)
    {
        var rng = new System.Random(/*seed*/);
        var edges = new List<Edge2d>();

        Tree[,] sets = new Tree[width, height];
        for (int t = 0; t < width * height; t++) sets[t % width, t / height] = new Tree { };

        edges.AddRange(GetRandomizedEdges(width, height));

        while(edges.Count > 0) {
            var randIndex = rng.Next(0, edges.Count);
            var randEdge = edges[randIndex];
            edges.RemoveAt(randIndex);

            var neighbour = new PositionOld();
            if (randEdge.Wall == WallStateOld.LEFT)
            {
                neighbour.X = randEdge.Position.X - 1;
                neighbour.Y = randEdge.Position.Y;
            }

            if (randEdge.Wall == WallStateOld.UP)
            {
                neighbour.X = randEdge.Position.X;
                neighbour.Y = randEdge.Position.Y + 1;
            }

            Debug.Log("--------");
            Debug.Log(randEdge.Position.X + ", " + randEdge.Position.Y + " | " + randEdge.Wall);
            Debug.Log(neighbour.X + ", " + neighbour.Y);
           
            if (!sets[randEdge.Position.X, randEdge.Position.Y].isConnected(sets[neighbour.X, neighbour.Y]))
            {
                sets[randEdge.Position.X, randEdge.Position.Y].connect(sets[neighbour.X, neighbour.Y]);
                maze[randEdge.Position.X, randEdge.Position.Y] &= ~randEdge.Wall;
                maze[neighbour.X, neighbour.Y] &= ~GetOppositeWall(randEdge.Wall);
            }
        }
       

        return maze;
    }

    private static WallStateOld[,] WilsonsAlgorithm(WallStateOld[,] maze, int width, int height)
    {
        var rng = new System.Random(/*seed*/);
        var position = new PositionOld { X = rng.Next(0, width), Y = rng.Next(0, height) };

        maze[position.X, position.Y] |= WallStateOld.VISITED; //1000 1111
        //Debug.Log("Start: " + position.X + ", " + position.Y);
        var remaining = width * height - 1;

        while(remaining > 0)
        {
            remaining = remaining - Walk(rng, maze, width, height);
            //break;
            Debug.Log(remaining);

        }




        return maze;
    }

    private static WallStateOld[,] RecursiveBactracker(WallStateOld[,] maze, int width, int height)
    {
        var rng = new System.Random(/*seed*/);
        var positionStack = new Stack<PositionOld>();
        var position = new PositionOld { X = rng.Next(0, width), Y = rng.Next(0, height) };

        maze[position.X, position.Y] |= WallStateOld.VISITED;  // 1000 1111
        positionStack.Push(position);

        while (positionStack.Count > 0)
        {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if (neighbours.Count > 0)
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

    private static WallStateOld[,] HuntAndKill(WallStateOld[,] maze, int width, int height)
    {
        var rng = new System.Random(/*seed*/);
        var position = new PositionOld { X = rng.Next(0, width), Y = rng.Next(0, height) };

        maze[position.X, position.Y] |= WallStateOld.VISITED;

        while(position.X > -1)
        {
            WalkHAK(maze, position, width, height);
            position = HuntHAK(maze, width, height);
        }


        return maze;
    }

    private static void WalkHAK(WallStateOld[,] maze, PositionOld position, int width, int height)
    {
        maze[position.X, position.Y] |= WallStateOld.VISITED;
        NeighbourOld newPos = VisitRandomNeighbour(position, width, height, maze);

        if (newPos.SharedWall == WallStateOld.VISITED)
        {
            return;
        }

        maze[position.X, position.Y] |= WallStateOld.VISITED;
        maze[position.X, position.Y] &= ~newPos.SharedWall;
        maze[newPos.Position.X, newPos.Position.Y] &= ~GetOppositeWall(newPos.SharedWall);
        position = newPos.Position;

        while (newPos.SharedWall != WallStateOld.VISITED)
        {
            newPos = VisitRandomNeighbour(position, width, height, maze);
            maze[position.X, position.Y] |= WallStateOld.VISITED;

            if (newPos.SharedWall != WallStateOld.VISITED)
            {
              
                maze[position.X, position.Y] &= ~newPos.SharedWall;
                maze[newPos.Position.X, newPos.Position.Y] &= ~GetOppositeWall(newPos.SharedWall);
                position = newPos.Position;
            }
            
        }

        Debug.Log("Walk end " + newPos.Position.X + ", " + newPos.Position.Y);
        Debug.Log(newPos.SharedWall);
    }

    private static PositionOld HuntHAK(WallStateOld[,] maze, int width, int height)
    {
        List<NeighbourOld> neighbours = new List<NeighbourOld>();
        PositionOld pos = new PositionOld { X = -1, Y = -1 };

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                //Debug.Log("x: " + i + " y: " + j);
                if (!maze[i,j].HasFlag(WallStateOld.VISITED))
                {
                    pos.X = i;
                    pos.Y = j;
                    neighbours = GetVisitedNeighbours(pos, maze, width, height);

                    if(neighbours.Count > 0)
                    {
                        i = width; //hmmmmm?????????????
                        break;
                    }
                }
            }
        }

        /*Debug.Log("Current position " + pos.X + ", " + pos.Y);
        Debug.Log("Neighbours count " + neighbours.Count);
        Debug.Log("--------------");*/

        if (pos.X == -1)
        {
            return pos;
        }

        
        Shuffle(neighbours);

        Debug.Log("Current position " + pos.X + ", " + pos.Y);
        Debug.Log("Neighbour " + neighbours[0].Position.X + ", " + neighbours[0].Position.Y);
        Debug.Log("Neighbour wall " + neighbours[0].SharedWall);
        Debug.Log("--------------");

        maze[pos.X, pos.Y] &= ~neighbours[0].SharedWall;
        maze[neighbours[0].Position.X, neighbours[0].Position.Y] &= ~GetOppositeWall(neighbours[0].SharedWall);

        return pos;

    }


    private static NeighbourOld VisitRandomNeighbour(PositionOld position, int width, int height, WallStateOld[,] maze)
    {
        var dir = new List<WallStateOld> { WallStateOld.LEFT, WallStateOld.RIGHT, WallStateOld.UP, WallStateOld.DOWN };
        Shuffle(dir);

        foreach (WallStateOld d in dir)
        {
            if (d == WallStateOld.LEFT && position.X > 0 && !maze[position.X - 1, position.Y].HasFlag(WallStateOld.VISITED))
            {

                return new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = position.X - 1,
                        Y = position.Y
                    },
                    SharedWall = WallStateOld.LEFT
                };
            }

            if (d == WallStateOld.RIGHT && position.X < width - 1 && !maze[position.X + 1, position.Y].HasFlag(WallStateOld.VISITED))
            {
                return new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = position.X + 1,
                        Y = position.Y
                    },
                    SharedWall = WallStateOld.RIGHT
                };
            }

            if (d == WallStateOld.DOWN && position.Y > 0 && !maze[position.X, position.Y - 1].HasFlag(WallStateOld.VISITED))
            {
                return new NeighbourOld
                {
                    Position = new PositionOld
                    {
                        X = position.X,
                        Y = position.Y - 1
                    },
                    SharedWall = WallStateOld.DOWN
                };
            }

            if (d == WallStateOld.UP && position.Y < height - 1 && !maze[position.X, position.Y + 1].HasFlag(WallStateOld.VISITED))
            {
                return new NeighbourOld {
                    Position = new PositionOld
                    {
                        X = position.X,
                        Y = position.Y + 1
                    },
                    SharedWall = WallStateOld.UP
                };
            }
        }


        return new NeighbourOld
        {
            Position = position,
            SharedWall = WallStateOld.VISITED
        };
    }

    private static int Walk(System.Random rng, WallStateOld[,] maze, int width, int height)
    {

        //WallStateOld[,] visits = (WallStateOld[,])maze.Clone();
        WallStateOld[,] visits = new WallStateOld[width, height];
        //bool found = false;
        var startCell = new PositionOld();
        var randCell = new PositionOld();
        bool walking = true;

        var unvisited = new List<PositionOld>();

        /*while (!found)
        {
            randCell = new PositionOld { X = rng.Next(0, width), Y = rng.Next(0, height) };
            if(!maze[randCell.X, randCell.Y].HasFlag(WallStateOld.VISITED))
            {
                found = true;
            }
        }*/

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(!maze[i, j].HasFlag(WallStateOld.VISITED))
                {
                    unvisited.Add(new PositionOld { X = i, Y = j });
                }
            }
        }

        Shuffle(unvisited);

        randCell = unvisited[0];
        startCell = randCell;

       
        Debug.Log("start: " + randCell.X + ", " + randCell.Y + " | " + maze[randCell.X, randCell.Y]);

        while(walking)
        {
            walking = false;
            var neigh = GetValidNeighbour(randCell, visits, width, height);
            //Debug.Log(i + " : " + neigh.X + ", " + neigh.Y + " | " + maze[neigh.X, neigh.Y]);
           
            Debug.Log(randCell.X + ", " + randCell.Y + " | " + visits[randCell.X, randCell.Y]);

            if (!maze[neigh.X, neigh.Y].HasFlag(WallStateOld.VISITED))
            {
                randCell = neigh;
                walking = true;
            }

        }
        
        int steps = GetPath(startCell, visits, maze);

        Debug.Log("MAZE FINAL");

        for (int k = 0; k < width; k++)
        {
            for(int j = 0; j < height; j++)
            {
                Debug.Log(k + ", " + j + " | " + maze[k, j]);
            }
        }

        Debug.Log("PATH FINAL (VISITS)");

        for(int m = 0; m < width; m++)
        {
            for(int n = 0; n < height; n++)
            {
                Debug.Log(m + ", " + n + " | " + visits[m, n]);
            }
        }

        //otoz nie
        return steps;
    }

    private static int GetPath(PositionOld start, WallStateOld[,] visits, WallStateOld[,] maze) 
    {
        
        var cell = start;
        var nextCell = new PositionOld();
        int steps = 0;


        while (!maze[cell.X, cell.Y].HasFlag(WallStateOld.VISITED))
        {
            steps++;
            //maze[cell.X, cell.Y] |= WallStateOld.VISITED;
            Debug.Log("path: " + cell.X + ", " + cell.Y + " | " + maze[cell.X, cell.Y]);

            if (visits[cell.X, cell.Y].HasFlag(WallStateOld.LEFT))
            {
                maze[cell.X, cell.Y] &= ~WallStateOld.LEFT;
                nextCell = cell;
                nextCell.X = cell.X - 1;
                maze[nextCell.X, nextCell.Y] &= ~WallStateOld.RIGHT;

            }

            if(visits[cell.X, cell.Y].HasFlag(WallStateOld.RIGHT))
            {
                maze[cell.X, cell.Y] &= ~WallStateOld.RIGHT;
                nextCell = cell;
                nextCell.X = cell.X + 1;
                maze[nextCell.X, nextCell.Y] &= ~WallStateOld.LEFT;
            }

            if(visits[cell.X, cell.Y].HasFlag(WallStateOld.DOWN))
            {
                maze[cell.X, cell.Y] &= ~WallStateOld.DOWN;
                nextCell = cell;
                nextCell.Y = cell.Y - 1;
                maze[nextCell.X, nextCell.Y] &= ~WallStateOld.UP;
            }

            if (visits[cell.X, cell.Y].HasFlag(WallStateOld.UP))
            {
                maze[cell.X, cell.Y] &= ~WallStateOld.UP;
                nextCell = cell;
                nextCell.Y = cell.Y + 1;
                maze[nextCell.X, nextCell.Y] &= ~WallStateOld.DOWN;
            }

            maze[cell.X, cell.Y] |= WallStateOld.VISITED;
            cell = nextCell;

            
        }

        //steps++;

        return steps;
    }

    private static PositionOld GetValidNeighbour(PositionOld randCell, WallStateOld[,] visits, int width, int height)
    {
        var dir = new List<WallStateOld> { WallStateOld.LEFT, WallStateOld.RIGHT, WallStateOld.UP, WallStateOld.DOWN };
        Shuffle(dir);

        foreach(WallStateOld d in dir)
        {
            visits[randCell.X, randCell.Y] &= ~visits[randCell.X, randCell.Y];
            if (d == WallStateOld.LEFT && randCell.X > 0)
            {
                
                visits[randCell.X, randCell.Y] |= d;
                return new PositionOld
                {
                    X = randCell.X - 1,
                    Y = randCell.Y
                };
            }

            if (d == WallStateOld.RIGHT && randCell.X < width - 1)
            {
                visits[randCell.X, randCell.Y] |= d;
                return new PositionOld
                {
                    X = randCell.X + 1,
                    Y = randCell.Y
                };
            }

            if (d == WallStateOld.DOWN && randCell.Y > 0)
            {
                visits[randCell.X, randCell.Y] |= d;
                return new PositionOld
                {
                    X = randCell.X,
                    Y = randCell.Y - 1
                };
            }

            if (d == WallStateOld.UP && randCell.Y < height - 1)
            {
                visits[randCell.X, randCell.Y] |= d;
                return new PositionOld
                {
                    X = randCell.X,
                    Y = randCell.Y + 1
                };
            }
        }


        return randCell;
    }

    private static Edge2d GetNeighbourEdge(List<Edge2d> edges, Edge2d edge)
    {
        var neighbour = new Edge2d();
        if(edge.Wall == WallStateOld.LEFT)
        {
            neighbour = edges.Find(e => e.Position.X == edge.Position.X - 1);
        }

        if(edge.Wall == WallStateOld.UP)
        {
            neighbour = edges.Find(e => e.Position.Y == edge.Position.Y + 1);
        }

        return neighbour;
    }

    private static List<Edge2d> GetRandomizedEdges(int width, int height)
    {
        var edges = new List<Edge2d>();

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if(i != 0)
                {
                    edges.Add(new Edge2d
                    {
                        Position = new PositionOld
                        {
                            X = i,
                            Y = j
                        },
                        Wall = WallStateOld.LEFT,
                        Set = new Tree { parent = null }
                    });
                }
                
                if(j != height - 1)
                {
                    edges.Add(new Edge2d
                    {
                        Position = new PositionOld
                        {
                            X = i,
                            Y = j
                        },
                        Wall = WallStateOld.UP,
                        Set = new Tree { parent = null }
                    });
                }
            }
        }

        //randomize the edges (or not like that)
        //edges.Shuffle();

        return edges;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new System.Random(/*seed*/);
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


        return HuntAndKill(maze, width, height);
    }

}
