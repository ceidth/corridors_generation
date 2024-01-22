using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class MazeRenderer3D : MonoBehaviour
{
    /*---PARAMETERS---*/
    [SerializeField]
    [Range(1, 30)]
    private int width = 5;

    [SerializeField]
    [Range(3, 30)]
    private int height = 5;

    [SerializeField]
    [Range(1, 30)]
    private int depth = 5;

    /*---RENDERING---*/

    [SerializeField]
    private Transform pathPrefab = null;

    [SerializeField]
    private Transform anchor = null;

    [SerializeField] private Material pathMaterial = null;
    [SerializeField] private Material startMaterial = null;
    [SerializeField] private Material solutionMaterial = null;
    [SerializeField] private Material roomMaterial = null;

    /*---UI---*/
    [SerializeField] private Slider wSlider = null;
    [SerializeField] private GameObject wText;

    [SerializeField] private Slider hSlider = null;
    [SerializeField] private GameObject hText;

    [SerializeField] private Slider dSlider = null;
    [SerializeField] private GameObject dText;

    /*---VARIABLES---*/
    [SerializeField] private GameObject uiController;

    private float move = 0.15f;
    private WallState[,,] maze;

    /*---COMBINING MESHES---*/
    private List<GameObject> gameObjectsToCombine = new();
    public GameObject[] meshColorObjects;
    private int c = 0;

    private void FixedUpdate()
    {
        wText.GetComponent<TextMeshProUGUI>().text = wSlider.value.ToString();
        hText.GetComponent<TextMeshProUGUI>().text = hSlider.value.ToString();
        dText.GetComponent<TextMeshProUGUI>().text = dSlider.value.ToString();

        width = (int)wSlider.value;
        height = (int)hSlider.value;
        depth = (int)dSlider.value;

    }

    public void reGenerate(int choice)
    {
        reset();
        maze = MazeGenerator3D.Generate(width, height, depth, choice);
        Draw(maze);
    }

    public void GenerateRooms(bool[] option, int[] number)
    {
        reset();
        MazeSolver.ResetRooms(maze, width, height, depth);

        if (option[0])
        {
            MazeSolver.GenerateRoomsAnywhere(maze, width, height, depth, number[0]);
        }

        if (option[2])
        {
            MazeSolver.GenerateRoomsAtDeadEnds(maze, number[2]);
        }

        if (option[1])
        {
            MazeSolver.GenerateRoomsOnMainPath(maze, width, height, depth, number[1]);
        }


        Draw(maze);
    }

    public void ResetRoomsOnClick()
    {
        MazeSolver.ResetRooms(maze, width, height, depth);
        Draw(maze);
    }

    public void GenerateBranches(int l)
    {
        MazeGenerator3D.BranchesLength(maze, width, height, depth, l);
        Draw(maze);
    }

    private void reset()
    {
        gameObjectsToCombine.Clear();
        for(int i = 0; i < meshColorObjects.Length; i++)
        {
            foreach(Transform child in meshColorObjects[i].transform)
            {
                Destroy(child.GameObject());
            }
        }
    }


    private void Draw(WallState[,,] maze)
    {

        var anchPos = new Vector3(0, 0, 0);

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                for (int k = 0; k < depth; ++k)
                {
                 
                    var cell = maze[i, j, k];
                    var position = new Vector3(-width / 2 + i, -height / 2 + j, -depth / 2 + k);


                    if(cell.HasFlag(WallState.VISITED))
                    {

                        if(j == 0 || j == height - 1)
                        {
                            anchPos += position;
                            c = 2;
                        }
                        else if (cell.HasFlag(WallState.ROOM))
                        {
                            c = 3;
                        }
                        else if(cell.HasFlag(WallState.SOLUTION))
                        {
                            c = 0;
                        }
                        else
                        {
                            c = 1;
                        }

                        var path = Instantiate(pathPrefab, transform) as Transform;
                        path.position = position;
                        path.SetParent(meshColorObjects[c].transform);
                        gameObjectsToCombine.Add(path.GameObject());


                        if (!cell.HasFlag(WallState.LEFT))
                        {
                            var pathLeft = Instantiate(pathPrefab, transform) as Transform;
                            pathLeft.position = position + new Vector3(-move, 0, 0);
                            pathLeft.SetParent(meshColorObjects[c].transform);
                            gameObjectsToCombine.Add(pathLeft.GameObject());
                        }

                        if (!cell.HasFlag(WallState.RIGHT))
                        {
                            var pathRight = Instantiate(pathPrefab, transform) as Transform;
                            pathRight.position = position + new Vector3(move, 0, 0);
                            pathRight.SetParent(meshColorObjects[c].transform);
                            gameObjectsToCombine.Add(pathRight.GameObject());
                        }

                        if (!cell.HasFlag(WallState.FRONT))
                        {
                            var pathFront = Instantiate(pathPrefab, transform) as Transform;
                            pathFront.position = position + new Vector3(0, 0, move);
                            pathFront.SetParent(meshColorObjects[c].transform);
                            gameObjectsToCombine.Add(pathFront.GameObject());

                        }

                        if (!cell.HasFlag(WallState.BACK))
                        {
                            var pathBack = Instantiate(pathPrefab, transform) as Transform;
                            pathBack.position = position + new Vector3(0, 0, -move);
                            pathBack.SetParent(meshColorObjects[c].transform);
                            gameObjectsToCombine.Add(pathBack.GameObject());
                        }

                        if(!cell.HasFlag(WallState.ABOVE))
                        {
                            var pathAbove = Instantiate(pathPrefab, transform) as Transform;
                            pathAbove.position = position + new Vector3(0, move, 0);
                            pathAbove.SetParent(meshColorObjects[c].transform);
                            gameObjectsToCombine.Add(pathAbove.GameObject());
                        }

                        if (!cell.HasFlag(WallState.BELOW))
                        {
                            var pathBelow = Instantiate(pathPrefab, transform) as Transform;
                            pathBelow.position = position + new Vector3(0, -move, 0);
                            pathBelow.SetParent(meshColorObjects[c].transform);
                            gameObjectsToCombine.Add(pathBelow.GameObject());
                        }
                    }

                }
            }
        }

        anchPos.y = 0.0f;
        anchor.position = anchPos / (2 * MazeGenerator3D.numberOfPaths);


        for(int o = 0; o < meshColorObjects.Length; o++)
        {
            if(meshColorObjects[o] != null)
            {
                CombineMeshes(meshColorObjects[o]);
            }
        }

    }

    public void ShowMainPath(bool show)
    {
        if(show)
        {
            StandardShaderUtils.ChangeRenderMode(pathMaterial, StandardShaderUtils.BlendMode.Fade);
        }
        else
        {
            StandardShaderUtils.ChangeRenderMode(pathMaterial, StandardShaderUtils.BlendMode.Opaque);
        }
    }

    private void CombineMeshes(GameObject obj)
    {
      
       Vector3 position = obj.transform.position;
       obj.transform.position = Vector3.zero;

       MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
       CombineInstance[] combine = new CombineInstance[meshFilters.Length];
       int i = 1;
       while (i < meshFilters.Length)
       {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
       }

       obj.transform.GetComponent<MeshFilter>().mesh = new Mesh();
       obj.transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
       obj.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
       obj.transform.gameObject.SetActive(true);

       obj.transform.position = position;
        
    }


}
