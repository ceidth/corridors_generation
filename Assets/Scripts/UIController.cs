using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    //przycisk do generowania
    //[SerializeField] private Button generate;

    /*--------------*/

    private int choice;

    public int width;
    public int depth;

    private int newWidth;
    private int newDepth;

    // Start is called before the first frame update
    void Start()
    {
        newWidth = 0;
        newDepth = 0;

        paramsPanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        width = (int)wSlider.value;
        depth = (int)dSlider.value;

        if(newWidth != width || newDepth != depth)
        {
            newWidth = width;
            newDepth = depth;
        }

        //coordsValidate();
        
    }

    public void randomCoords(GameObject coords)
    {
        var rng = new System.Random(/*seed*/);

        coords.transform.Find("xInput").GetComponent<TMP_InputField>().text = rng.Next(0, width).ToString();
        coords.transform.Find("zInput").GetComponent<TMP_InputField>().text = rng.Next(0, depth).ToString();
    }

    public void coordsValidate(GameObject coords)
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

    public void setUpParametersUI(int algorithm)
    {
        choice = algorithm;
        cleanInstatiated();

        var content = paramsPanel.transform.Find("Scroll View/Viewport/Content");
        paramsPanel.SetActive(true);

        //algorithmName();

        size.transform.SetParent(content, false);
        size.SetActive(true);

        var gap1 = Instantiate(gap, content);
        gap1.transform.SetSiblingIndex(3);

        coordsFields.transform.SetParent(content, false);
        randomCoords(coordsFields);

        string label;
        switch (algorithm)
        {
            case 1:
                prims();
                label = "prim's algorithm";
                break;

            case 2:
                kruskals(content);
                label = "kruskal's algorithm";
                break;

            case 3:
                wilsons();
                label = "wilson's algorithm";
                break;

            case 4:
                recursive();
                label = "recursive backtracker";
                break;

            case 5:
                handk();
                label = "hunt and kill algorithm";
                break;

            default:
                label = "algorithm";
                break;
        }

        paramsPanel.transform.Find("Scroll View/Viewport/Content/Algorithm").GameObject().GetComponent<TMP_Text>().text = label;

    }

    private void prims()
    {

    }

    private void kruskals(Transform content)
    {
        var gap2 = Instantiate(gap, content);
        gap2.transform.SetSiblingIndex(5);

        GameObject finishFields = Instantiate(coordsFields, content);
        finishFields.transform.Find("Title").GameObject().GetComponent<TMP_Text>().text = "finish field";
        randomCoords(finishFields);
        finishFields.tag = "Gap";

    }

    private void wilsons()
    {

    }

    private void recursive()
    {

    }

    private void handk()
    {

    }

    public void generateClick()
    {
        MazeGenerator3D.startX = int.Parse(xInStart.GetComponent<TMP_InputField>().text);
        MazeGenerator3D.startZ = int.Parse(zInStart.GetComponent<TMP_InputField>().text);

        Transform finish = paramsPanel.transform.Find("Scroll View/Viewport/Content/CoordinatesFields(Clone)");
        if (finish != null)
        {
            MazeGenerator3D.finishX = int.Parse(finish.Find("xInput").GameObject().GetComponent<TMP_InputField>().text);
            MazeGenerator3D.finishZ = int.Parse(finish.Find("zInput").GameObject().GetComponent<TMP_InputField>().text);
        }

        mazeRenderer.GetComponent<MazeRenderer3D>().reGenerate(choice);
    }

    private void cleanInstatiated()
    {
        var clones = GameObject.FindGameObjectsWithTag("Gap");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }
    }



}
