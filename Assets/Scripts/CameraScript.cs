using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public Camera cam;
    [SerializeField] private GameObject target;
    [SerializeField] public float distance = 10.0f;

    private Vector3 previousPosition;
    private Vector3 oldTarget = Vector3.zero;

    void Start()
    {
        //cam.transform.LookAt(target);
        //cam.transform.position = new Vector3(target.position.x, target.position.y, target.position.z - 10.0f);
        /*previousPosition = cam.transform.position;*/

    }

    void FixedUpdate ()
    {
        if (Input.GetMouseButtonDown(0)) //jedno klikniecie
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            //cam.transform.position = 
            //previousPosition = new Vector3(target.position.x, target.position.y, target.position.z - 10.0f);
        }
        else if (Input.GetMouseButton(0)) //przytrzymany przycisk
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            /*float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically

            cam.transform.position = target.position;

            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!

            cam.transform.Translate(new Vector3(0, 0, -distance));*/

            cam.transform.RotateAround(target.transform.position, new Vector3(0, 1, 0), -direction.x * 180);
            Debug.Log(target.transform.position);

            previousPosition = newPosition;

        }

        Vector3 pos = cam.transform.position;
        //pos.z += Input.mouseScrollDelta.y * Time.deltaTime * 100.0f;
        cam.transform.position = pos + (transform.forward * Input.mouseScrollDelta.y);

        //cam.transform.LookAt(target);
        /*cam.transform.position = new Vector3(distance + target.position.x, target.position.y, target.position.z);*/

        if(oldTarget != target.transform.position)
        {
            cam.transform.position = new Vector3(target.transform.position.x, 0, target.transform.position.z - 20);
            cam.transform.LookAt(target.transform);
            oldTarget = target.transform.position;
        }

    }
}
