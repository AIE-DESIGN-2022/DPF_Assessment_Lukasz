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

    void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
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
    }

    private void InitilizeTerrainData()
    {
        Terrain terrain;
        var getTerrain = GameObject.Find("Terrain");
        terrain = getTerrain.GetComponent<Terrain>();

        terrainX = terrain.terrainData.size.x;
        terrainZ = terrain.terrainData.size.z;
    }

    void Update()
    {
        CameraMovement();
        CameraHeigh();

        if (Input.GetMouseButtonDown(2)) allowMovement = false;
        if (Input.GetMouseButton(2)) CameraRotation();
        if (Input.GetMouseButtonUp(2)) allowMovement = true;
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
            if (newPos.z > 0 && newPos.x < terrainZ) zOk = true;
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
}
// Writen by Lukasz Dziedziczak