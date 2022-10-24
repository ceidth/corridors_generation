using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public Camera cam;
    [SerializeField] public Transform target;
    [SerializeField] public float distance = 10.0f;

    private Vector3 previousPosition;

    void Start()
    {
        /*cam.transform.LookAt(target);
        cam.transform.position = new Vector3(distance + target.position.x, target.position.y, target.position.z);*/
        /*previousPosition = cam.transform.position;*/
    }

    void FixedUpdate ()
    {
        /*if(Input.GetMouseButtonDown(0)) //jedno klikniecie
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0)) //przytrzymany przycisk
        {
            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically

            cam.transform.position = target.position;

            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!

            cam.transform.Translate(new Vector3(0, 0, -distance));

            previousPosition = newPosition;
        }*/

        cam.transform.LookAt(target);
        cam.transform.position = new Vector3(distance + target.position.x, target.position.y, target.position.z);

    }
}
