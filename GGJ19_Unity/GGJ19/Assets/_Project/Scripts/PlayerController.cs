using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Block Selection")]
    public Block selectedBlock;
    public PlayBlock playBlock;

    [Header("Input")]
    public float joySpeed = 15.0f;
    public float offsetX = -2.5f;
    public float offsetY = -2.5f;

    private float posX = 0;
    private float posZ = 0;

    private Plane plane;

    private Camera cam;
    private RaycastHit hit;

    private void Awake()
    {
        cam = Camera.main;
        plane = new Plane(Vector3.up, 0);
    }

    void Start()
    {
        playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.GetRandomRoomType(), 0);
        playBlock.Populate();
    }

    void Update()
    {
        if (!cam || !playBlock)
            return;

        float placementX = posX;
        float placementZ = posZ;

        float mouseMag = Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
        if (mouseMag > Mathf.Epsilon)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            if(Physics.Raycast(ray, out hit, 100))
            {
                Vector3 hitPoint = hit.point;
                Debug.DrawRay(ray.origin, hitPoint - ray.origin, Color.green);

                posX = Mathf.Clamp(Mathf.Round(hitPoint.x - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
                posZ = Mathf.Clamp(Mathf.Round(hitPoint.z - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;

                placementX = posX;
                placementZ = posZ;
            }
            else
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        }
        else
        {
            float joyH = Input.GetAxis("Horizontal");
            float joyV = Input.GetAxis("Vertical");

            //Vector3 moveDirection = cam.transform.forward * joyV + cam.transform.right * joyH;

            posX += joyH * Time.deltaTime * joySpeed;
            posZ += joyV * Time.deltaTime * joySpeed;

            placementX = Mathf.Clamp(Mathf.Round(posX - 0.5f), 0.0f, GameManager.Instance.boardWidth - 1) + 0.5f + offsetX;
            placementZ = Mathf.Clamp(Mathf.Round(posZ - 0.5f), 0.0f, GameManager.Instance.boardHeight - 1) + 0.5f + offsetY;
        }

        // Move the playBlock to the target position
        playBlock.transform.position = new Vector3(placementX, Constants.boardPlayblockHeight, placementZ);

        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Jump"))
        {
            Vector3 placementPosition = new Vector3(placementX, Constants.boardPlayblockHeight, placementZ);
            playBlock.transform.position = placementPosition;

            Debug.DrawLine(placementPosition, placementPosition + Vector3.up, Color.blue);

            if(GameManager.Instance.PlacePlayBlock(playBlock))
            {
                // Get new block
                playBlock.SetData(GameManager.Instance.blockDatabase.GetRandomBlock(), Board.GetRandomRoomType(), 0);
                playBlock.Populate();
            }
            
        }
    }

}
