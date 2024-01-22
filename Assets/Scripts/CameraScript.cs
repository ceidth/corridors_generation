using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public Camera cam;
    [SerializeField] private GameObject target;
    [SerializeField] public float distance = 10.0f;

    private Vector3 previousPosition;
    private Vector3 oldTarget = Vector3.zero;

    void FixedUpdate ()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0)) //jedno klikniecie
            {
                previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0)) //przytrzymany przycisk
            {
                Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
                Vector3 direction = previousPosition - newPosition;

                cam.transform.RotateAround(target.transform.position, new Vector3(0, 1, 0), -direction.x * 180);

                previousPosition = newPosition;

            }

            Vector3 pos = cam.transform.position;
            cam.transform.position = pos + (transform.forward * Input.mouseScrollDelta.y);
        }

        if(oldTarget != target.transform.position)
        {
            cam.transform.position = new Vector3(target.transform.position.x, 0, target.transform.position.z - 20);
            cam.transform.LookAt(target.transform);
            oldTarget = target.transform.position;
        }

    }
}
