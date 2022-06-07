// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    public bool allowMovement = true;
    [SerializeField, Range(1f, 49f)] float boarderScreenPercentage = 5f;
    [SerializeField] float scrollSpeed = 1;
    [SerializeField, Range(0f, 1f)] float timeBeforeScrollStart;
    [SerializeField, Range(1f, 10f)] float cameraRotationSpeed = 2;

    [Header("Camera Setup")]
    [SerializeField, Range(1f, 90f)] float cameraAngle = 75f;
    [SerializeField] float distanceToGround = 10;
    [SerializeField, Range(10f, 100f)] float maxDistanceToGround = 100f;
    [SerializeField, Range(1f, 20f)] float minDistanceToGround = 5f;
    [SerializeField, Range(2f, 50f)] float angleChangeDistanceToGround = 10;

    [Header("UI")]
    [SerializeField] RectTransform topHUDSection;
    [SerializeField] RectTransform bottomHUDSection;

    [Header("Terrain")]
    [SerializeField] bool centerZero = false;

    private Camera _camera;
    private float currentDistanceToGround;
    private float currentCameraAngle;
    private float terrainX;
    private float terrainZ;
    private float timeInScrollSpace = 0;
    private float topScreenOffSet = 0;
    private float bottomScreenOffSet = 0;
    private float mousePosX;
    private GameController gameController;
    private LineRenderer minimapcameraLines;
    private LayerMask terrainMask;

    private Faction playerFaction;


    void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        gameController = FindObjectOfType<GameController>();
        minimapcameraLines = FindObjectOfType<LineRenderer>();
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
    }

    void Start()
    {
        currentDistanceToGround = distanceToGround;
        currentCameraAngle = cameraAngle;
        InitilizeTerrainData();
        ResetCameraPosition();

        if (Display.displays.Length > 1) Display.displays[0].Activate();
        if (topHUDSection != null) topScreenOffSet = topHUDSection.rect.height / Screen.height;
        if (bottomHUDSection != null) bottomScreenOffSet = bottomHUDSection.rect.height / Screen.height;

        if (_camera == null) Debug.LogError("Camera not found");

        DrawMinimapCameraLines();
    }

    private void InitilizeTerrainData()
    {
        Terrain terrain = FindObjectOfType<Terrain>();        

        terrainX = terrain.terrainData.size.x;
        terrainZ = terrain.terrainData.size.z;
    }

    void Update()
    {
        CameraMovement();
        CameraHeigh();
        PanByArrowKey();
        PanByRightClick();

        /*if (Input.GetMouseButtonDown(2)) allowMovement = false;
        if (Input.GetMouseButton(2)) CameraRotation();
        if (Input.GetMouseButtonUp(2)) allowMovement = true;*/
    }

    private void PanByRightClick()
    {
        if (Input.GetMouseButton(1) && gameController.PlayerController().NothingSelected())
        {
            float MouseX = Input.GetAxis("Mouse X") * -1;
            float MouseY = Input.GetAxis("Mouse Y") * -1;

            Vector3 step = new Vector3(MouseX, 0, MouseY);
            Vector3 newPosition = transform.position + step;
            if (step.magnitude > 0 && IsInsideTerrain(newPosition))
            {
                transform.position = newPosition;
                DrawMinimapCameraLines();
            }
        }
    }

    private void PanByArrowKey()
    {
        Vector3 step = new Vector3();
        if (Input.GetKey(KeyCode.DownArrow)) step.z -= scrollSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) step.z += scrollSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow)) step.x -= scrollSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) step.x += scrollSpeed * Time.deltaTime;

        Vector3 newPosition = transform.position + step * 5;

        if (step.magnitude > 0 && IsInsideTerrain(newPosition))
        {
            transform.position = newPosition;
            DrawMinimapCameraLines();
        }
    }

    private void CameraRotation()
    {
        if (!MouseIsInPlayArea()) return;
        float horizontal = Input.GetAxis("Mouse X");
        if (horizontal == 0) return;
        transform.RotateAround(transform.position, transform.TransformDirection(Vector3.up), horizontal * cameraRotationSpeed);
    }

    private void CameraHeigh()
    {
        if (!MouseIsInPlayArea()) return;

        Vector2 mouseScrollDelta = Input.mouseScrollDelta;

        if (mouseScrollDelta.magnitude > 0)
        {
            float newDistanceToGround = currentDistanceToGround + mouseScrollDelta.y * -1;
            if (newDistanceToGround > minDistanceToGround && newDistanceToGround < maxDistanceToGround)
            {
                currentDistanceToGround = newDistanceToGround;
                currentCameraAngle = NewCameraAngle(newDistanceToGround);
                ResetCameraPosition();
                DrawMinimapCameraLines();
            }
        }  
    }

    private void CameraMovement()
    {
        if (!allowMovement || !MouseIsInPlayArea()) return;

        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        float screenBoarder = boarderScreenPercentage / 100;
        Vector3 newPosition = new Vector3();
        bool notInTopOrBottom = false;
        bool notInLeftOrRight = false;

        if (mouseX >= 1 || mouseX <= 0 || mouseY >= 1 || mouseY <= 0) return;

        if (mouseX < screenBoarder)
        {
            float step = ((screenBoarder - mouseX) * 100) * scrollSpeed * Time.deltaTime;
            if (newPosition == new Vector3())
            {
                newPosition = transform.position;
            }
            newPosition.x -= step;
            timeInScrollSpace += Time.deltaTime;
        }
        else if (mouseX > 1 - screenBoarder)
        {
            float step = ((mouseX - (1 - screenBoarder)) * 100) * scrollSpeed * Time.deltaTime;
            if (newPosition == new Vector3())
            {
                newPosition = transform.position;
            }
            newPosition.x += step;
            timeInScrollSpace += Time.deltaTime;
        }
        else
        {
            notInLeftOrRight = true;
        }

        if (mouseY < screenBoarder + bottomScreenOffSet && mouseY > bottomScreenOffSet)
        {
            float step = (((screenBoarder + bottomScreenOffSet) - (mouseY)) * 100) * scrollSpeed * Time.deltaTime;
            if (newPosition == new Vector3())
            {
                newPosition = transform.position;
            }
            newPosition.z -= step;
            timeInScrollSpace += Time.deltaTime;
        }
        else if (mouseY > 1 - (screenBoarder + topScreenOffSet) && mouseY < 1 - topScreenOffSet)
        {
            float step = ((mouseY - (1 - (screenBoarder + topScreenOffSet))) * 100) * scrollSpeed * Time.deltaTime;
            if (newPosition == new Vector3())
            {
                newPosition = transform.position;
            }
            newPosition.z += step;
            timeInScrollSpace += Time.deltaTime;
        }
        else
        {
            notInTopOrBottom = true;
        }

        if (notInTopOrBottom && notInLeftOrRight) timeInScrollSpace = 0;
        if (newPosition != new Vector3() && IsInsideTerrain(newPosition) && timeInScrollSpace > timeBeforeScrollStart)
        {
            transform.position = newPosition;
            DrawMinimapCameraLines();
        }

    }

    private void ResetCameraPosition()
    {
        if (_camera == null) return;
        if (currentCameraAngle < 1 || currentCameraAngle > 90) return;
        if (currentDistanceToGround < minDistanceToGround || currentDistanceToGround > maxDistanceToGround) return;


        float height = currentDistanceToGround * Mathf.Sin(currentCameraAngle * Mathf.Deg2Rad);
        float distance = currentDistanceToGround * Mathf.Cos(currentCameraAngle * Mathf.Deg2Rad);

        Vector3 targetPosition = new Vector3(0.0f, height, distance * -1);
        float angleLerp = Mathf.Lerp(_camera.transform.localRotation.eulerAngles.x, currentCameraAngle, Time.deltaTime);

        /*_camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, targetPosition, Time.deltaTime);
        _camera.transform.localRotation = Quaternion.Euler(angleLerp, _camera.transform.localRotation.eulerAngles.y, _camera.transform.localRotation.eulerAngles.z);*/

        _camera.transform.localPosition = targetPosition;
        _camera.transform.localRotation = Quaternion.Euler(currentCameraAngle, _camera.transform.localRotation.eulerAngles.y, _camera.transform.localRotation.eulerAngles.z);
    }

    private float NewCameraAngle(float newDistanceToGround)
    {
        float newCameraAngle = cameraAngle;

        if (newDistanceToGround > minDistanceToGround && newDistanceToGround < angleChangeDistanceToGround)
        {
            float range = angleChangeDistanceToGround - minDistanceToGround;
            float distanceToGroundAsPercentage = (newDistanceToGround - minDistanceToGround) / range;
            newCameraAngle = (cameraAngle/100) * (distanceToGroundAsPercentage * 100);
        }
        //print(newCameraAngle);
        return newCameraAngle;
    }

    private bool IsInsideTerrain(Vector3 newPos)
    {
        bool xOk = false;
        bool zOk = false;

        if (centerZero)
        {
            if (newPos.x < terrainX / 2 && newPos.x > (terrainX / 2) * -1) xOk = true;
            if (newPos.z < terrainZ / 2 && newPos.z > (terrainZ / 2) * -1) zOk = true;
        }
        else
        {
            if (newPos.x > 0 && newPos.x < terrainX) xOk = true;
            if (newPos.z > 0 && newPos.z < terrainZ) zOk = true;
        }
        return xOk && zOk;
    }

    public bool IsInUIOffset()
    {
        float mouseY = Input.mousePosition.y / Screen.height;
        return mouseY < bottomScreenOffSet;
    }

    public bool MouseIsInPlayArea()
    {
        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        if (mouseX < 0 || mouseY < 0 || mouseX > 1 || mouseY > 1) return false;

        if (bottomScreenOffSet != 0 && mouseY < bottomScreenOffSet) return false;
        if (topScreenOffSet != 0 && mouseY > 1 - topScreenOffSet) return false;


        return true;
    }

    public Camera Camera() { return _camera; }

    private void DrawMinimapCameraLines()
    {
        List<Vector3> screenPositions = new List<Vector3>();
        screenPositions.Add(new Vector3(0, 0, 0));
        screenPositions.Add(new Vector3(Screen.width, 0, 0));
        screenPositions.Add(new Vector3(Screen.width, Screen.height, 0));
        screenPositions.Add(new Vector3(0, Screen.height, 0));

        List<Vector3> linePositions = new List<Vector3>();

        foreach (Vector3 screenPosition in screenPositions)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 500, terrainMask);
            Vector3 mapPos = new Vector3(hit.point.x, transform.position.y + 5, hit.point.z);
            linePositions.Add(mapPos);
        }

        

        if (minimapcameraLines != null)
        {
            for (int index = 0; index < linePositions.Count; index++)
            {
                minimapcameraLines.SetPosition(index, linePositions[index]);
            }

            float width = MapSize()/ 100 * 2;
            minimapcameraLines.startWidth = width;
            minimapcameraLines.endWidth = width;
        }
    }

    public float MapSize()
    {
        float mapSize = (terrainX + terrainZ) / 2;
        return mapSize;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
        DrawMinimapCameraLines();
    }

    public void SetPlayerFaction(Faction newFaction)
    {
        playerFaction = newFaction;
    }

    public List<Selectable> SelectablesOnScreen(List<Selectable> selectables)
    {
        List<Selectable> list = new List<Selectable>();

        foreach (Selectable selectable in selectables)
        {
            Vector3 screenPosition = _camera.WorldToScreenPoint(selectable.transform.position);

            if (screenPosition.x > 0 && screenPosition.x < Screen.width && screenPosition.y > 0 && screenPosition.y < Screen.height)
            {
                list.Add(selectable);
            }
        }


        return list;
    }

    public List<Selectable> PlayersSelectablesOnScreen()
    {
        List<Selectable> list = new List<Selectable>();

        if (playerFaction != null)
        {
            list = SelectablesOnScreen(playerFaction.Selectables());
        }

        return list;
    }

    public float CameraHeight()
    {
        return _camera.transform.position.y;
    }
}
// Writen by Lukasz Dziedziczak