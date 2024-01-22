using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeSolver : MonoBehaviour
{

    public static List<Position> solution = new List<Position>();
    public static List<Position> visited = new List<Position>();
    public static List<Position> ends = new List<Position>();

    public static int[] branchCells;

    private static Position start;
    private static Position finish;

    public static void Solve(WallState[,,] maze, int width, int height, int depth, Position st, Position fin)
    {
        solution.Clear();
        ends.Clear();
        visited.Clear();
        //ResetRooms(maze, width, depth, height);

        GetVisited(maze, width, height, depth);
        GetDeadEndsAndSolution(maze, height);

        bool[,,] wasHere = new bool[width, height, depth];

        start = st;
        finish = fin;

        bool r = RecursiveSolver(start, maze, wasHere, width, height, depth);

    }

    private static bool RecursiveSolver(Position position, WallState[,,] maze, bool[,,] wasHere, int width, int height, int depth)
    {
         if(position.Equals(finish))
         {
            return true;
         }      

        if (wasHere[position.X, position.Y, position.Z] || !maze[position.X, position.Y, position.Z].HasFlag(WallState.VISITED))
        {
            return false;
        }

        wasHere[position.X, position.Y, position.Z] = true;

        //left
        if(position.X > 0 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.LEFT))
        {
            if(RecursiveSolver(new Position { X = position.X - 1, Y = position.Y, Z = position.Z}, maze, wasHere, width, height, depth))
            {
                maze[position.X, position.Y, position.Z] |= WallState.SOLUTION;
                return true;
            }
        }

        //right
        if(position.X < width - 1 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.RIGHT))
        {
            if (RecursiveSolver(new Position { X = position.X + 1, Y = position.Y, Z = position.Z }, maze, wasHere, width, height, depth))
            {
                maze[position.X, position.Y, position.Z] |= WallState.SOLUTION;
                return true;
            }
        }

        if (position.Y > 0 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.BELOW))
        {
            if (RecursiveSolver(new Position { X = position.X, Y = position.Y - 1, Z = position.Z }, maze, wasHere, width, height, depth))
            {
                maze[position.X, position.Y, position.Z] |= WallState.SOLUTION;
                return true;
            }
        }

        if (position.Y < height - 1 && maze[position.X, position.Y, position.Z].HasFlag(WallState.ABOVE))
        {
            if (RecursiveSolver(new Position { X = position.X, Y = position.Y + 1, Z = position.Z }, maze, wasHere, width, height, depth))
            {
                maze[position.X, position.Y, position.Z] |= WallState.SOLUTION;
                return true;
            }
        }

        if (position.Z > 0 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.BACK))
        {
            if (RecursiveSolver(new Position { X = position.X, Y = position.Y, Z = position.Z - 1 }, maze, wasHere, width, height, depth))
            {
                maze[position.X, position.Y, position.Z] |= WallState.SOLUTION;
                return true;
            }
        }

        if (position.Z < depth - 1 && !maze[position.X, position.Y, position.Z].HasFlag(WallState.FRONT))
        {
            if (RecursiveSolver(new Position { X = position.X, Y = position.Y, Z = position.Z + 1 }, maze, wasHere, width, height, depth))
            {
                maze[position.X, position.Y, position.Z] |= WallState.SOLUTION;
                return true;
            }
        }

        return false;

    }

    public static void GenerateRoomsOnMainPath(WallState[,,] maze, int width, int height, int depth, int number)
    {
        var tmp = new List<Position>(solution);

        var rng = new System.Random();
        int i = number;

        while(i >= 0)
        {
            var r = rng.Next(tmp.Count);
            var pos = tmp[r];
            tmp.RemoveAt(r);

            while(!CheckNeighbours(maze, pos, width, height, depth) && tmp.Count > 0)
            {
                r = rng.Next(tmp.Count);
                pos = tmp[r];
                tmp.RemoveAt(r);

            }

            if (tmp.Count == 0)
            {
                break;
            }

            maze[pos.X, pos.Y, pos.Z] |= WallState.ROOM;
            i--;

        }

    }

    public static void GenerateRoomsAnywhere(WallState[,,] maze, int width, int height, int depth, int number)
    {
        var tmp = new List<Position>(visited);

        var rng = new System.Random();
        int i = number;

        while (i >= 0)
        {
            var r = rng.Next(tmp.Count);
            var pos = tmp[r];
            tmp.RemoveAt(r);

            while (!CheckNeighbours(maze, pos, width, height, depth) && tmp.Count > 0)
            {
                r = rng.Next(tmp.Count);
                pos = tmp[r];
                tmp.RemoveAt(r);

            }

            if (tmp.Count == 0)
            {
                break;
            }

            maze[pos.X, pos.Y, pos.Z] |= WallState.ROOM;
            i--;
        }

    }

    public static void GenerateRoomsAtDeadEnds(WallState[,,] maze, int number)
    {
        var tmp = new List<Position>(ends);

        var rng = new System.Random();
        int i = number;

        while (i >= 0 && tmp.Count > 0)
        {
            var r = rng.Next(tmp.Count);
            var pos = tmp[r];
            tmp.RemoveAt(r);

            maze[pos.X, pos.Y, pos.Z] |= WallState.ROOM;
            i--;
        }

    }

    public static void GetDeadEndsAndSolution(WallState[,,] maze, int height)
    {
        solution.Clear();
        ends.Clear();
        int i;
        branchCells = new int[height];

        foreach (var v in visited)
        {
            i = 0;
            if (!maze[v.X, v.Y, v.Z].HasFlag(WallState.SOLUTION))
            {
                if(v.Y != 0 && v.Y != height - 1)
                {
                    branchCells[v.Y]++;

                    if(maze[v.X, v.Y, v.Z].HasFlag(WallState.LEFT))
                    {
                        i++;
                    }

                    if (maze[v.X, v.Y, v.Z].HasFlag(WallState.RIGHT))
                    {
                        i++;
                    }

                    if (maze[v.X, v.Y, v.Z].HasFlag(WallState.FRONT))
                    {
                        i++;
                    }

                    if (maze[v.X, v.Y, v.Z].HasFlag(WallState.BACK))
                    {
                        i++;
                    }

                    if (maze[v.X, v.Y, v.Z].HasFlag(WallState.ABOVE))
                    {
                        i++;
                    }

                    if (maze[v.X, v.Y, v.Z].HasFlag(WallState.BELOW))
                    {
                        i++;
                    }

                }
            }
            else
            {
                solution.Add(v);
            }

            if(i == 5)
            {
                ends.Add(v);
            }

        }
        
    }

    private static bool CheckNeighbours(WallState[,,] maze, Position pos, int width, int height, int depth)
    {
        if (pos.Y + 1 == height - 1 && maze[pos.X, pos.Y + 1, pos.Z].HasFlag(WallState.VISITED))
        {
            return false;
        }

        if(pos.Y - 1 == 0 && maze[pos.X, pos.Y - 1, pos.Z].HasFlag(WallState.VISITED))
        {
            return false;
        }

        if(pos.X > 0 && maze[pos.X - 1, pos.Y, pos.Z].HasFlag(WallState.ROOM) && !maze[pos.X, pos.Y, pos.Z].HasFlag(WallState.LEFT))
        {
            return false;
        }

        if (pos.X < width - 1 && maze[pos.X + 1, pos.Y, pos.Z].HasFlag(WallState.ROOM) && !maze[pos.X, pos.Y, pos.Z].HasFlag(WallState.RIGHT))
        {
            return false;
        }

        if (pos.Y > 0 && maze[pos.X, pos.Y - 1, pos.Z].HasFlag(WallState.ROOM) && !maze[pos.X, pos.Y, pos.Z].HasFlag(WallState.BELOW))
        {
            return false;
        }

        if (pos.Y < height - 1 && maze[pos.X, pos.Y + 1, pos.Z].HasFlag(WallState.ROOM) && !maze[pos.X, pos.Y, pos.Z].HasFlag(WallState.ABOVE))
        {
            return false;
        }

        if (pos.Z > 0 && maze[pos.X, pos.Y, pos.Z - 1].HasFlag(WallState.ROOM) && !maze[pos.X, pos.Y, pos.Z].HasFlag(WallState.BACK))
        {
            return false;
        }

        if (pos.Z < depth - 1 && maze[pos.X, pos.Y, pos.Z + 1].HasFlag(WallState.ROOM) && !maze[pos.X, pos.Y, pos.Z].HasFlag(WallState.FRONT))
        {
            return false;
        }

        return true;
    }

    private static void GetVisited(WallState[,,] maze, int width, int height, int depth) //to juz gdzies bylo na 10000000%
    {

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < depth; k++)
                {
                    if(maze[i, j, k].HasFlag(WallState.VISITED))
                    {
                        visited.Add(new Position { X = i, Y = j, Z = k });
                    }
                }
            }
        }

    }

    public static void ResetRooms(WallState[,,] maze, int width, int height, int depth)
    {
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    if (maze[i, j, k].HasFlag(WallState.ROOM))
                    {
                        maze[i, j, k] &= ~WallState.ROOM;
                    }

                    if (maze[i, j, k].HasFlag(WallState.FRONTIER))
                    {
                        maze[i, j, k] &= ~WallState.FRONTIER;
                    }
                }
                    
            } 
        } 
    
    }

    public static void ReCalculate(WallState[,,] maze, int width, int height, int depth)
    {
        visited.Clear();
        ends.Clear();

        GetVisited(maze, width, height, depth);
        GetDeadEndsAndSolution(maze, height);
    }

}
