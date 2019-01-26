using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public GameObject block = null;
    public Camera cam = null;

    Plane plane;

    // Start is called before the first frame update
    void Start()
    {
        plane = new Plane(Vector3.up, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!cam || !block)
            return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);

            int board_width = 20;
            int board_height = 20;

            hitPoint.x = Mathf.Clamp( Mathf.Round(hitPoint.x - 0.5f), 0, board_width-1) + 0.5f;
            hitPoint.z = Mathf.Clamp( Mathf.Round(hitPoint.z - 0.5f), 0, board_height-1) + 0.5f;

            //Move your cube GameObject to the point where you clicked
            block.transform.position = hitPoint;
        }
    }
}
