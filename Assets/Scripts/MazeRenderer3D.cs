using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MazeRenderer3D : MonoBehaviour
{
    /*---PARAMETERS---*/
    [SerializeField]
    [Range(1, 30)]
    private int width = 5;

    [SerializeField]
    [Range(1, 30)]
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

    /*---UI---*/
    [SerializeField] private Slider wSlider = null;
    [SerializeField] private GameObject wText;

    [SerializeField] private Slider hSlider = null;
    [SerializeField] private GameObject hText;

    [SerializeField] private Slider dSlider = null;
    [SerializeField] private GameObject dText;

    [SerializeField] private GameObject input;


    private float move = 0.2f;
    private WallState[,,] maze;
    private int level;

    // Start is called before the first frame update
    void Start()
    {
        level = height;
        maze = MazeGenerator3D.Generate(width, height, depth, 0, 0);
        Draw(maze);
    }

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
        int seed;

        if(!string.IsNullOrEmpty(input.GetComponent<TMP_InputField>().text))
        {
            seed = Convert.ToInt32(input.GetComponent<TMP_InputField>().text);
        }
        else
        {
            var rng = new System.Random();
            seed = rng.Next();

        }
        Debug.Log("seed " + seed);
        reset();
        maze = MazeGenerator3D.Generate(width, height, depth, choice, seed);
        MazeSolver.Solve(maze, width, height, depth);
        Draw(maze);
    }
    

    private void reset()
    {
        var clones = GameObject.FindGameObjectsWithTag("wall");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }

    }


    private void Draw(WallState[,,] maze)
    {
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
                            pathPrefab.GetComponent<Renderer>().material = startMaterial;
                        }
                        else if(cell.HasFlag(WallState.SOLUTION))
                        {
                            pathPrefab.GetComponent<Renderer>().material = solutionMaterial;
                        }
                        else
                        {
                            pathPrefab.GetComponent<Renderer>().material = pathMaterial;
                        }

                        var path = Instantiate(pathPrefab, transform) as Transform;
                        path.position = position;

                        if(j == 0)
                        {
                            anchor.position = position;
                            //Debug.Log("Position " + position.x + ", " + position.y + ", " + position.z);
                        }


                        if (!cell.HasFlag(WallState.LEFT))
                        {
                            /*path.position += new Vector3(-move, 0, 0);
                            path.localScale = new Vector3(path.localScale.x + move, path.localScale.y, path.localScale.z);*/
                            var pathLeft = Instantiate(pathPrefab, transform) as Transform;
                            pathLeft.position = position + new Vector3(-move, 0, 0);
                        }

                        if (!cell.HasFlag(WallState.RIGHT))
                        {
                            /*path.position += new Vector3(move, 0, 0);
                            path.localScale = new Vector3(path.localScale.x + move, path.localScale.y, path.localScale.z);*/
                            var pathRight = Instantiate(pathPrefab, transform) as Transform;
                            pathRight.position = position + new Vector3(move, 0, 0);
                        }

                        if (!cell.HasFlag(WallState.FRONT))
                        {
                            /*path.position += new Vector3(0, 0, move);
                            path.localScale = new Vector3(path.localScale.x, path.localScale.y, path.localScale.z + move);*/
                            var pathFront = Instantiate(pathPrefab, transform) as Transform;
                            pathFront.position = position + new Vector3(0, 0, move);

                        }

                        if (!cell.HasFlag(WallState.BACK))
                        {
                            /*path.position += new Vector3(0, 0, -move);
                            path.localScale = new Vector3(path.localScale.x, path.localScale.y, path.localScale.z + move);*/
                            var pathBack = Instantiate(pathPrefab, transform) as Transform;
                            pathBack.position = position + new Vector3(0, 0, -move);
                        }

                        if(!cell.HasFlag(WallState.ABOVE))
                        {
                            var pathAbove = Instantiate(pathPrefab, transform) as Transform;
                            pathAbove.position = position + new Vector3(0, move, 0);
                        }

                        if (!cell.HasFlag(WallState.BELOW))
                        {
                            var pathBelow = Instantiate(pathPrefab, transform) as Transform;
                            pathBelow.position = position + new Vector3(0, -move, 0);
                        }
                    }

                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
