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

    SOLUTION = 256
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
    public static int startX;
    public static int startZ;

    public static int finishX = 0;
    public static int finishZ = 0;

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

    private static WallState[,,] Prims(WallState[,,] maze, int width, int height, int depth, int seed)
    {
        //pozycja startowa na samej gorze kostki
        var rng = new System.Random(seed);
        var visited = new List<Position>();
        var frontier = new List<Neighbour>();
        var position = new Position { X = startX, Y = height - 1, Z = startZ };

        //zdjemowana gorna sciana z pola, ustawiana flaga visited
        maze[position.X, position.Y, position.Z] |= WallState.VISITED;

        maze[position.X, position.Y, position.Z] &= ~WallState.BELOW;
        maze[position.X, position.Y - 1, position.Z] &= ~WallState.ABOVE;

        maze[position.X, position.Y - 1, position.Z] |= WallState.VISITED;

        position = new Position { X = position.X, Y = position.Y - 1, Z = position.Z };
        visited.Add(position);
        //Debug.Log("Start: " + position.X + ", " + position.Y + ", " + position.Z);

        frontier.AddRange(GetUnvisitedNeighbours(position, maze, width, height, depth));

        while (frontier.Count > 0)
        {
            
            var randIndex = rng.Next(0, frontier.Count);
            var randFront = frontier[randIndex].Position;

            //Debug.Log("Chosen frontier: " + randFront.X + ", " + randFront.Y + ", " + randFront.Z);
            frontier.RemoveAt(randIndex);

            var visitedNeighbours = GetVisitedNeighbours(randFront, maze, width, height, depth);
            randIndex = rng.Next(0, visitedNeighbours.Count);
            var randomNeigh = visitedNeighbours[randIndex];
            var nPosition = randomNeigh.Position;

            if (nPosition.Y > randFront.Y)
            {
                //Debug.Log("Clearing bc - visited Y: " + nPosition.Y + ", frontier Y: " + randFront.Y);
                frontier.Clear();
            }


            maze[randFront.X, randFront.Y, randFront.Z] &= ~randomNeigh.SharedWall;
            maze[nPosition.X, nPosition.Y, nPosition.Z] &= ~GetOppositeWall(randomNeigh.SharedWall);

            maze[randFront.X, randFront.Y, randFront.Z] |= WallState.VISITED;
            visited.Add(randFront);

            frontier.AddRange(GetUnvisitedNeighbours(randFront, maze, width, height, depth));

            if (randFront.Y == 0)
            {
                frontier.Clear();
            }

        }

        return maze;
    }

    public static WallState[,,] Kruskals(WallState[,,] maze, int width, int height, int depth, int seed)
    {
        var rng = new System.Random(seed);
        var edges = new List<Neighbour>();

        //dodanie ka¿dej komórki do setu/drzewa
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
        //Debug.Log("Initial: " + edges.Count);

        //losowy poczatek //tu usunac h-1
        var start = new Position { X = startX, Y = height - 1, Z = startZ };
        /*edges.RemoveAll(e => e.Position.X != start.X && e.Position.Z != start.Z && e.Position.Y == height - 1);
        Debug.Log("Start: " + edges.Count);*/
        sets[start.X, start.Y, start.Z].connect(sets[start.X, start.Y - 1, start.Z]);
        maze[start.X, start.Y, start.Z] &= ~WallState.BELOW;
        maze[start.X, start.Y - 1, start.Z] &= ~WallState.ABOVE;

        maze[start.X, start.Y, start.Z] |= WallState.VISITED;
        maze[start.X, start.Y - 1, start.Z] |= WallState.VISITED;
        edges.RemoveAll(e => e.Position.Y == start.Y);

        //losowy koniec //tu usunac 0
        var finish = new Position { X = finishX, Y = 1, Z = finishZ };
        /*edges.RemoveAll(e => e.Position.X != finish.X && e.Position.Z != finish.Z && e.Position.Y == 1 && e.Wall == WallState.BELOW);
        Debug.Log("Finish: " + edges.Count);*/
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

            //znajdz komorke ktora wspoldzieli krawedz
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

            /*Debug.Log("--------");
            Debug.Log(randEdge.Position.X + ", " + randEdge.Position.Y + ", " + randEdge.Position.Z + " | " + randEdge.Wall);
            Debug.Log(neighbour.X + ", " + neighbour.Y + ", " + neighbour.Z);*/


            if (!sets[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z].isConnected(sets[neighbour.X, neighbour.Y, neighbour.Z]))
            {

                sets[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z].connect(sets[neighbour.X, neighbour.Y, neighbour.Z]);
                maze[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z] &= ~randEdge.SharedWall;
                maze[neighbour.X, neighbour.Y, neighbour.Z] &= ~GetOppositeWall(randEdge.SharedWall);

               /* maze[randEdge.Position.X, randEdge.Position.Y, randEdge.Position.Z] |= WallState.VISITED;
                maze[neighbour.X, neighbour.Y, neighbour.Z] |= WallState.VISITED;*/

                if(randEdge.Position.Y > neighbour.Y)
                {
                    Debug.Log("Zejscie w dol, usuwane elementy w rzedzie " + randEdge.Position.Y);
                    edges.RemoveAll(e => e.Position.Y == randEdge.Position.Y && e.SharedWall == WallState.BELOW);
                }

                if (sets[start.X, start.Y, start.Z].isConnected(sets[finish.X, finish.Y, finish.Z]))
                {
                    edges.Clear();
                }

            }
        }

        MarkAsVisited(maze, sets, start, width, height, depth);

        return maze;
    }

    private static WallState[,,] Wilsons(WallState[,,] maze, int width, int height, int depth, int seed)
    {
        var rng = new System.Random(seed);
        var position = new Position { X = startX, Y = height - 1, Z = startZ };

        maze[position.X, position.Y, position.Z] |= WallState.VISITED;
        maze[position.X, position.Y, position.Z] &= ~WallState.BELOW;
        maze[position.X, position.Y - 1, position.Z] &= ~WallState.ABOVE;
        maze[position.X, position.Y - 1, position.Z] |= WallState.VISITED;

        var remaining = width * (height - 1) * depth - 1;
        //Debug.Log("przed " + (height - 1));
        int nHeight = height - 1;
        while(remaining > 0)
        {
            remaining = remaining - Walk(maze, width, nHeight, depth, seed);
            //Debug.Log("po " + height);
        }
        
        return maze;
    }

    private static WallState[,,] RecursiveBacktracker(WallState[,,] maze, int width, int height, int depth, int seed)
    {
        var rng = new System.Random(seed);
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
                    positionStack.Clear();
                }
            }
        }

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

        return maze;
    }
    
    /*--------GENERATE--------*/
    public static WallState[,,] Generate(int width, int height, int depth, int choice, int seed)
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
                return Prims(maze, width, height, depth, seed);

            case 2:
                return Kruskals(maze, width, height, depth, seed);

            case 3:
                return Wilsons(maze, width, height, depth, seed);

            case 4:
                return RecursiveBacktracker(maze, width, height, depth, seed);

            case 5:
                return HuntAndKill(maze, width, height, depth);

            default:
                return maze;
        }
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
        for(int i = 0; i < width; ++i)
        {
            for(int j = 0; j < height; ++j)
            {
                for(int k = 0; k < depth; ++k)
                {
                    if (sets[i, j, k].isConnected(sets[start.X, start.Y, start.Z])) 
                    {
                        maze[i, j, k] |= WallState.VISITED;
                    }
                }
            }
        }
    }

    /*--------WILSON'S ALGORITHM--------*/

    private static int Walk(WallState[,,] maze, int width, int height, int depth, int seed)
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

        Debug.Log(randCell.Y);
        startCell = randCell;

        while (walking)
        {
            walking = false;
            var neigh = GetValidNeighbour(randCell, visits, width, lowestY[0] + 1, depth, seed);

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

    private static Position GetValidNeighbour(Position randCell, WallState[,,] visits, int width, int height, int depth, int seed)
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
            /*if (d == WallState.BELOW && randCell.Y > 0)
            {
                return new Position
                {
                    X = randCell.X,
                    Y = randCell.Y - 1,
                    Z = randCell.Z
                };
            }*/
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

        //int i = rng.Next(0, width/2)
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


    private static Neighbour VisitRandomNeighbour(Position position, int width, int height, int depth, WallState[,,] maze)
    {
        var dir = new List<WallState> { WallState.LEFT, WallState.RIGHT, WallState.FRONT, WallState.BACK, WallState.ABOVE };
        Shuffle(dir);

        foreach (WallState d in dir)
        {
            if (d == WallState.LEFT && position.X > 0 && !maze[position.X - 1, position.Y, position.Z].HasFlag(WallState.VISITED))
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
            if (d == WallState.RIGHT && position.X < width - 1 && !maze[position.X + 1, position.Y, position.Z].HasFlag(WallState.VISITED))
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
            /*if (d == WallState.ABOVE && position.Y < height - 1 && !maze[position.X, position.Y + 1, position.Z].HasFlag(WallState.VISITED))
            {
                return new Neighbour
                {
                    Position = new Position
                    {
                        X = position.X,
                        Y = position.Y + 1,
                        Z = position.Z
                    },
                    SharedWall = WallState.ABOVE
                };
            }*/
            if (d == WallState.BELOW && position.Y > 0 && !maze[position.X, position.Y - 1, position.Z].HasFlag(WallState.VISITED))
            {
                return new Neighbour
                {
                    Position = new Position
                    {
                        X = position.X,
                        Y = position.Y - 1,
                        Z = position.Z
                    },
                    SharedWall = WallState.BELOW
                };
            }
            if (d == WallState.FRONT && position.Z < depth - 1 && !maze[position.X, position.Y, position.Z + 1].HasFlag(WallState.VISITED))
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
            if (d == WallState.BACK && position.Z > 0 && !maze[position.X, position.Y, position.Z - 1].HasFlag(WallState.VISITED))
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

