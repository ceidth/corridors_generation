using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIController : MonoBehaviour
{
    //suwaki
    [SerializeField] private Slider wSlider;
    [SerializeField] private Slider dSlider;

    //pole tekstowe koordynatow
    [SerializeField] private GameObject xInStart;
    [SerializeField] private GameObject zInStart;

    //MazeRenderer3D
    [SerializeField] private GameObject mazeRenderer;

    /*--------------*/

    //caly obiekt z parametrami
    [SerializeField] private GameObject paramsPanel;

    //wymiary
    [SerializeField] private GameObject size;

    //przerwa
    [SerializeField] private GameObject gap;

    //wybieranie koodynatow
    [SerializeField] private GameObject coordsFields;

    //punkty posrednie
    [SerializeField] private GameObject roomsPanel;

    //dlugosc odnog
    [SerializeField] private GameObject branchesPanel;

    //liczba sciezek
    [SerializeField] private GameObject pathsPanel;


    /*--------------*/

    public int roomsMainPath = 0;

    private int choice;

    public int width;
    public int depth;

    private int newWidth;
    private int newDepth;

    private int pathsNr;

    private List<GameObject> fields = new();

    private string[] opt = { "Anywhere", "MainPath", "Ends" };

    private List<GameObject> startFieldsList = new();
    private List<GameObject> finishFieldsList = new();

    private List<GameObject> coordsParent = new();


    // Start is called before the first frame update
    void Start()
    {
        newWidth = 0;
        newDepth = 0;

        paramsPanel.SetActive(false);
        roomsPanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        width = (int)wSlider.value;
        depth = (int)dSlider.value;

        if(newWidth != width || newDepth != depth)
        {
            foreach(GameObject f in fields)
            {
                CoordsValidate(f);
            }

            newWidth = width;
            newDepth = depth;
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

    }

    public void setUpParametersUI(int algorithm)
    {
        choice = algorithm;
        CleanInstatiated();
        
        var content = paramsPanel.transform.Find("Scroll View/Viewport/Content");
        paramsPanel.SetActive(true);

        UpdatePathsSlider();

        var stTmp = coordsFields.transform.Find("CoordElement").GameObject();

        RandomCoords(stTmp);
        fields.Add(stTmp);

        coordsParent.Clear();
        coordsParent.Add(coordsFields);

        string label;
        switch (algorithm)
        {
            case 1:
                label = "prim's algorithm";
                break;

            case 2:
                FinishFields(content);
                label = "kruskal's algorithm";
                break;

            case 3:
                FinishFields(content);
                label = "wilson's algorithm";
                break;

            case 4:
                label = "recursive backtracker";
                break;

            case 5:
                label = "hunt and kill algorithm";
                break;

            default:
                label = "algorithm";
                break;
        }

        paramsPanel.transform.Find("Scroll View/Viewport/Content/Algorithm").GameObject().GetComponent<TMP_Text>().text = label;

        GetAllCoords();
        UnifyButtonController();

    }

    private void FinishFields(Transform content)
    {
        GameObject finishFields = Instantiate(coordsFields, content);
        finishFields.transform.Find("Title").GameObject().GetComponent<TMP_Text>().text = "finish";

        var finTmp = finishFields.transform.Find("CoordElement").GameObject();

        RandomCoords(finTmp);
        finishFields.tag = "Gap";
        fields.Add(finTmp);

        foreach(Transform child in finishFields.transform)
        {
            if(child.CompareTag("StartElement"))
            {
                child.tag = "FinishElement";
            }
        }

        coordsParent.Add(finishFields);


    }


    public void generateClick()
    {
        foreach(var o in opt)
        {
            roomsPanel.transform.Find("Content/" + o).GameObject().GetComponent<Toggle>().isOn = false;
        }

        MazeGenerator3D.numberOfPaths = (int)pathsPanel.transform.Find("Slider").GameObject().GetComponent<Slider>().value;

        GetAllCoords();

        Transform finish = paramsPanel.transform.Find("Scroll View/Viewport/Content/CoordinatesFields(Clone)");

        MazeGenerator3D.start.Clear();
        MazeGenerator3D.finish.Clear();

        for(int i = 0; i < startFieldsList.Count; i++)
        {
            MazeGenerator3D.start.Add(new Position { X = int.Parse(startFieldsList[i].transform.Find("xInput").GameObject().GetComponent<TMP_InputField>().text), Y = 0, Z = int.Parse(startFieldsList[i].transform.Find("zInput").GameObject().GetComponent<TMP_InputField>().text) });
            
            if(finish != null)
            {
                MazeGenerator3D.finish.Add(new Position { X = int.Parse(finishFieldsList[i].transform.Find("xInput").GameObject().GetComponent<TMP_InputField>().text), Y = 0, Z = int.Parse(finishFieldsList[i].transform.Find("zInput").GameObject().GetComponent<TMP_InputField>().text) });
            }
        }

        mazeRenderer.GetComponent<MazeRenderer3D>().reGenerate(choice);
        RoomsUIController();
        BranchesUIController();
    }

    /*----ROOMS----*/

    private void RoomsUIController()
    {
        roomsPanel.SetActive(true);

        if(MazeSolver.ends.Count == 0)
        {
            roomsPanel.transform.Find("Content/" + opt[1]).GameObject().SetActive(false);
            roomsPanel.transform.Find("Content/" + opt[1] + "Slider").GameObject().SetActive(false);

            roomsPanel.transform.Find("Content/" + opt[2]).GameObject().SetActive(false);
            roomsPanel.transform.Find("Content/" + opt[2] + "Slider").GameObject().SetActive(false);
        }
        else
        {
            roomsPanel.transform.Find("Content/" + opt[1]).GameObject().SetActive(true);
            roomsPanel.transform.Find("Content/" + opt[1] + "Slider").GameObject().SetActive(true);

            roomsPanel.transform.Find("Content/" + opt[2]).GameObject().SetActive(true);
            roomsPanel.transform.Find("Content/" + opt[2] + "Slider").GameObject().SetActive(true);
        }

        //anywhere
        if (MazeSolver.visited.Count % 2 == 0)
        {
            roomsPanel.transform.Find("Content/" + opt[0] + "Slider").GameObject().GetComponent<Slider>().maxValue = (MazeSolver.visited.Count - 2) / 2;
        }
        else
        {
            roomsPanel.transform.Find("Content/" + opt[0] + "Slider").GameObject().GetComponent<Slider>().maxValue = (MazeSolver.visited.Count - 2) / 2;
        }

        //main path
        if (MazeSolver.solution.Count % 2 == 0)
        {
            roomsPanel.transform.Find("Content/" + opt[1] + "Slider").GameObject().GetComponent<Slider>().maxValue = (MazeSolver.solution.Count - 2) / 2;
        }
        else
        {
            roomsPanel.transform.Find("Content/" + opt[1] + "Slider").GameObject().GetComponent<Slider>().maxValue = (MazeSolver.solution.Count - 2) / 2;
        }

        //ends
        roomsPanel.transform.Find("Content/" + opt[2] + "Slider").GameObject().GetComponent<Slider>().maxValue = MazeSolver.ends.Count;


        UpdateRoomSliders();

    }

    public void UpdateRoomSliders()
    {
        foreach (var o in opt)
        {
            roomsPanel.transform.Find("Content/" + o + "Slider/Number").GameObject().GetComponent<TextMeshProUGUI>().text
                = roomsPanel.transform.Find("Content/" + o + "Slider").GameObject().GetComponent<Slider>().value.ToString();
        }
    }

    public void RoomsTogglesValidate()
    {
        if(roomsPanel.transform.Find("Content/Anywhere").GameObject().GetComponent<Toggle>().isOn)
        {
            roomsPanel.transform.Find("Content/MainPath").GameObject().GetComponent<Toggle>().isOn = false;
            roomsPanel.transform.Find("Content/Ends").GameObject().GetComponent<Toggle>().isOn = false;
        }
        else if(roomsPanel.transform.Find("Content/MainPath").GameObject().GetComponent<Toggle>().isOn || roomsPanel.transform.Find("Content/Ends").GameObject().GetComponent<Toggle>().isOn)
        {
            roomsPanel.transform.Find("Content/Anywhere").GameObject().GetComponent<Toggle>().isOn = false;
        }
    }

    public void GenerateRoomsClick()
    {
        bool[] choice = new bool[3];
        int[] number = new int[3];

        for(int i = 0; i < 3; i++)
        {
            choice[i] = roomsPanel.transform.Find("Content/" + opt[i]).GameObject().GetComponent<Toggle>().isOn;
            number[i] = (int)roomsPanel.transform.Find("Content/" + opt[i] + "Slider").GameObject().GetComponent<Slider>().value;
        }

        mazeRenderer.GetComponent<MazeRenderer3D>().GenerateRooms(choice, number);

    }

    /*----BRANCHES----*/
    private void BranchesUIController()
    {
        if(MazeSolver.ends.Count == 0)
        {
            branchesPanel.SetActive(false);
        }
        else
        {
            branchesPanel.transform.SetParent(paramsPanel.transform.Find("Scroll View/Viewport/Content"), false);
            branchesPanel.SetActive(true);
        }

        UpdateBranchesSlider();
    }

    public void UpdateBranchesSlider()
    {

        branchesPanel.transform.Find("Slider/Number").GameObject().GetComponent<TextMeshProUGUI>().text
            = branchesPanel.transform.Find("Slider").GameObject().GetComponent<Slider>().value.ToString();

    }

    public void BranchesOnClick()
    {
        mazeRenderer.GetComponent<MazeRenderer3D>().GenerateBranches((int)branchesPanel.transform.Find("Slider").GameObject().GetComponent<Slider>().value);
    }

    /*----PATHS----*/
    public void UpdatePathsSlider()
    {
        pathsNr = (int)pathsPanel.transform.Find("Slider").GameObject().GetComponent<Slider>().value;
        pathsPanel.transform.Find("Slider/Number").GameObject().GetComponent<TextMeshProUGUI>().text
            = pathsPanel.transform.Find("Slider").GameObject().GetComponent<Slider>().value.ToString();
    }

    private void CleanInstatiated()
    {
        fields.Clear();
        startFieldsList.Clear();
        finishFieldsList.Clear();

        var clones = GameObject.FindGameObjectsWithTag("Gap");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }
    }

    private void CleanInstatiatedInputs()
    {
        fields.Clear();
        startFieldsList.Clear();
        finishFieldsList.Clear();

        List<GameObject> clones = new List<GameObject>();
        clones.AddRange(GameObject.FindGameObjectsWithTag("StartElement"));
        clones.AddRange(GameObject.FindGameObjectsWithTag("FinishElement"));

        foreach (var clone in clones)
        {
            Destroy(clone);
        }
    }

    /*----COORDINATES----*/
    public void RandomCoords(GameObject coords)
    {
        var rng = new System.Random(/*seed*/);

        coords.transform.Find("xInput").GetComponent<TMP_InputField>().text = rng.Next(0, width).ToString();
        coords.transform.Find("zInput").GetComponent<TMP_InputField>().text = rng.Next(0, depth).ToString();
    }

    public void CoordsValidate(GameObject coords)
    {
        if (int.TryParse(coords.transform.Find("xInput").GetComponent<TMP_InputField>().text, out int valueX))
        {
            if (valueX < 0)
            {
                coords.transform.Find("xInput").GetComponent<TMP_InputField>().text = "0";
            }
            if (valueX >= width)
            {
                coords.transform.Find("xInput").GetComponent<TMP_InputField>().text = (width - 1).ToString();
            }

        }
        if (int.TryParse(coords.transform.Find("zInput").GetComponent<TMP_InputField>().text, out int valueZ))
        {
            if (valueZ < 0)
            {
                coords.transform.Find("zInput").GetComponent<TMP_InputField>().text = "0";
            }
            if (valueZ >= depth)
            {
                coords.transform.Find("zInput").GetComponent<TMP_InputField>().text = (depth - 1).ToString();
            }
        }
    }

    public void InstantiateFields()
    {
        startFieldsList.Clear();
        finishFieldsList.Clear();
        CleanInstatiatedInputs();

        var orig = coordsFields.transform.Find("CoordElement").GameObject();
        startFieldsList.Add(orig);
        for(int i = 1; i < pathsNr; i++)
        {
            var elem = Instantiate(orig, coordsFields.transform);
            elem.transform.Find("LabelPath").GameObject().GetComponent<TextMeshProUGUI>().text = "path " + (i + 1).ToString();
            elem.tag = "StartElement";
            RandomCoords(elem);
            fields.Add(elem);
            startFieldsList.Add(elem);
        }

        coordsFields.transform.Find("Buttons").SetAsLastSibling();

        Transform finish = paramsPanel.transform.Find("Scroll View/Viewport/Content/CoordinatesFields(Clone)");

        if (finish != null)
        {
            var origFin = finish.transform.Find("CoordElement").GameObject();
            finishFieldsList.Add(origFin);

            for (int i = 1; i < pathsNr; i++)
            {
                var elem = Instantiate(origFin, finish.transform);
                elem.transform.Find("LabelPath").GameObject().GetComponent<TextMeshProUGUI>().text = "path " + (i + 1).ToString();
                elem.tag = "FinishElement";
                RandomCoords(elem);
                fields.Add(elem);
                finishFieldsList.Add(elem);
            }

            finish.transform.Find("Buttons").SetAsLastSibling();
        }

        UnifyButtonController();
    }

    public void RandomAllCoords(GameObject CoordsParent)
    {
        RandomCoords(CoordsParent.transform.Find("CoordElement").GameObject());
        if(pathsNr > 1)
        {
            if(CoordsParent.transform.Find("Title").GameObject().GetComponent<TextMeshProUGUI>().text == "start")
            {
               foreach(var s in startFieldsList)
                {
                    RandomCoords(s);
                }
            }
            else if(CoordsParent.transform.Find("Title").GameObject().GetComponent<TextMeshProUGUI>().text == "finish")
            {
                foreach (var f in finishFieldsList)
                {
                    RandomCoords(f);
                }
            }
        }
        
    }

    private void GetAllCoords()
    {
        startFieldsList.Clear();
        startFieldsList.Add(coordsFields.transform.Find("CoordElement").GameObject());
        var stTmp = GameObject.FindGameObjectsWithTag("StartElement");
        startFieldsList.AddRange(stTmp);

        finishFieldsList.Clear();

        Transform finish = paramsPanel.transform.Find("Scroll View/Viewport/Content/CoordinatesFields(Clone)");
        if(finish != null)
        {
            finishFieldsList.Add(finish.Find("CoordElement").GameObject());
            foreach(Transform child in finish)
            {
                if(child.GameObject().CompareTag("FinishElement"))
                {
                    finishFieldsList.Add(child.GameObject());
                }
            }
        }

    }

    public void SameValues(GameObject CoordsParent)
    {

        GetAllCoords();


        if (CoordsParent.transform.Find("Title").GameObject().GetComponent<TextMeshProUGUI>().text == "start")
        {
            foreach (var s in startFieldsList)
            {
                s.transform.Find("xInput").GetComponent<TMP_InputField>().text = startFieldsList[0].transform.Find("xInput").GetComponent<TMP_InputField>().text;
                s.transform.Find("zInput").GetComponent<TMP_InputField>().text = startFieldsList[0].transform.Find("zInput").GetComponent<TMP_InputField>().text;
            }
        }
        else if (CoordsParent.transform.Find("Title").GameObject().GetComponent<TextMeshProUGUI>().text == "finish")
        {
            foreach (var f in finishFieldsList)
            {
                f.transform.Find("xInput").GetComponent<TMP_InputField>().text = finishFieldsList[0].transform.Find("xInput").GetComponent<TMP_InputField>().text;
                f.transform.Find("zInput").GetComponent<TMP_InputField>().text = finishFieldsList[0].transform.Find("zInput").GetComponent<TMP_InputField>().text;
            }
        }

            
    }

    private void UnifyButtonController()
    {

        foreach (var p in coordsParent)
        {
            var btn = p.transform.Find("Buttons/Unify").GameObject();
            btn.SetActive(true);

            if(pathsNr == 1)
            {
                btn.SetActive(false);
            }
            if(choice == 2)
            {
                btn.SetActive(false);
            }
        }

    }

}