using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeSolver : MonoBehaviour
{
    
    public static void Solve(WallState[,,] maze, int width, int height, int depth)
    {
        Position position = new Position { X = 0, Y = height - 1, Z = 0};
        bool[,,] wasHere = new bool[width, height, depth];
        for(int i = 0; i < width; i++)
        {
            for(int k = 0; k < depth; k++)
            {
                if(maze[i, height - 1, k].HasFlag(WallState.VISITED))
                {
                    position = new Position { X = i, Y = height - 1, Z = k };
                }

                //wasHere[i, j, k] = false;
            }
            
        }


        bool r = RecursiveSolver(position, maze, wasHere, width, height, depth);
        //Debug.Log("rozwiazanie " + r);

    }

    private static bool RecursiveSolver(Position position, WallState[,,] maze, bool[,,] wasHere, int width, int height, int depth)
    {
        //znaleziono zejscie na dol
        if(position.Y == 0)
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
}
