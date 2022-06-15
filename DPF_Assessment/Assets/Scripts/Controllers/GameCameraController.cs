// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCameraController : MonoBehaviour
{
    [Header("Camera Movement")]
    [Tooltip("True when camera movement is allowed.")]
    public bool allowMovement = true;
    [Tooltip("Percentage of the screen when mouse is inside scrolling will begin.")]
    [SerializeField, Range(1f, 49f)] float boarderScreenPercentage = 5f;
    [Tooltip("How fast the camera will plan accross the map. Also changeable by user in settings.")]
    [SerializeField] float scrollSpeed = 1;
    [Tooltip("How long in seconds before scrolling/paning starts when mouse pointer is in boarder.")]
    [SerializeField, Range(0f, 1f)] float timeBeforeScrollStart;
    [Tooltip("Speed of how fast Camera can rotate around Camera Controller object.")]
    [SerializeField, Range(1f, 10f)] float cameraRotationSpeed = 2;

    [Header("Camera Setup")]
    [Tooltip("At what angle is the camera looking at the map.")]
    [SerializeField, Range(1f, 90f)] float cameraAngle = 75f;
    [Tooltip("How far away is the camera away from the map.")]
    [SerializeField] float distanceToGround = 10;
    [Tooltip("The maximum distance the camera can be away from the ground.")]
    [SerializeField, Range(10f, 100f)] float maxDistanceToGround = 100f;
    [Tooltip("The minimum distance the camera can be away from the ground.")]
    [SerializeField, Range(1f, 20f)] float minDistanceToGround = 5f;
    [Tooltip("The distance to ground the angle begins to change.")]
    [SerializeField, Range(2f, 50f)] float angleChangeDistanceToGround = 10;

    [Header("UI")]
    [Tooltip("The object which holds all the top HUD section.")]
    [SerializeField] RectTransform topHUDSection;
    [Tooltip("The object which holds all the bottom HUD section.")]
    [SerializeField] RectTransform bottomHUDSection;

    [Header("Terrain")]
    [Tooltip("True if the center of the terrain is X=0 and Z=0, otherwise zero is bottom left of the terrain.")]
    [SerializeField] bool centerZero = false;

    private Camera _camera; // Referance to the main camera.
    private float currentDistanceToGround; // The current Distance to Ground, this will change during gameplay.
    private float currentCameraAngle; // The current Camera Angle to Ground, this will change during gameplay.
    private float terrainX; // The length of the terrain.
    private float terrainZ; // The width of the terrain.
    private float timeInScrollSpace = 0; // A timer to keep track of how long the mouse pointer has been in scroll space of the screen.
    private float topScreenOffSet = 0; // How big the top HUD section is.
    private float bottomScreenOffSet = 0; // How big the bottom HUD section is.
    private GameController gameController; // A referance to the Game Controller script.
    private LineRenderer minimapcameraLines; // A referance to the Lines that are rendered on the minimap to show where the camera is looking at.
    private LayerMask terrainMask; // A Layer Mask with only the terrain layer referanced.
    private LayerMask fogMask; // A Layer Mask with only the Fog Of War layer referanced.
    private Faction playerFaction; // A referance to the player's faction
    private FogOfWarController fogOfWarController; // A referance to the FogOfWar controller object.
    private bool autoPan; // True when the camera is being panned by script.
    private Vector3 autoPanLocation; // The location of where a script is automatically panning the camera to.
    private float autoPanSpeed; // How fast a script is automatically panning the camera.

    // Run at the start of the game before "Start" is run
    void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        gameController = FindObjectOfType<GameController>();
        minimapcameraLines = FindObjectOfType<LineRenderer>();
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
        fogMask |= (1 << LayerMask.NameToLayer("FogOfWar"));
        fogOfWarController = FindObjectOfType<FogOfWarController>();
    }

    // Rum at the start of the game
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

    // Finds the size of the terrain
    private void InitilizeTerrainData()
    {
        Terrain terrain = FindObjectOfType<Terrain>();        

        terrainX = terrain.terrainData.size.x;
        terrainZ = terrain.terrainData.size.z;
    }

    // Run at every frame
    void Update()
    {
        CameraMovement();
        CameraHeighUpdate();
        PanByArrowKey();
        PanByRightClick();
        AutoPan();

        /*if (Input.GetMouseButtonDown(2)) allowMovement = false;
        if (Input.GetMouseButton(2)) CameraRotation();
        if (Input.GetMouseButtonUp(2)) allowMovement = true;*/
    }

    // Function to facilitate paning by script, called at every frame.
    private void AutoPan()
    {
        if (autoPan)
        {
            transform.position = Vector3.Lerp(transform.position, autoPanLocation, autoPanSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, autoPanLocation) < 3)
            {
                //transform.position = autoPanLocation;
                autoPan = false;
                allowMovement = true;
            }
        }
    }

    /* Facilitates Panning when right mouse button is held down
     * and no units are selected. 
     * Called at every frame.
     */
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

    /* Facilitates paning by using arrow keys on keyboard
     * Called at every frame.
     */
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

    /* Facilitates ratating camera by holding middle mouse button.
     * Called at every frame.
     * Not currently implemented due to issues.
     */
    private void CameraRotation()
    {
        if (!MouseIsInPlayArea()) return;
        float horizontal = Input.GetAxis("Mouse X");
        if (horizontal == 0) return;
        transform.RotateAround(transform.position, transform.TransformDirection(Vector3.up), horizontal * cameraRotationSpeed);
    }

    /* Facilitates adjusting distance to ground by mouse wheel scroll
     * Calleds at every frame.
     * 
     */
    private void CameraHeighUpdate()
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
                //fogOfWarController.SetFogHeight(CameraHeight() / 3 * 2);
            }
        }  
    }

    /* Facilitaes camera scrolling/paning when mouse is at the edge of the game screen
     * Called at every frame.
     * 
     */
    private void CameraMovement()
    {
        if (!allowMovement || !MouseIsInPlayArea())
        {
            timeInScrollSpace = 0;
            return;
        }

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

    /* Sets the camera's position in relation to it's parent object which 
     * represents the center of the screen where the camera is pointing at on the map.
     * 
     * 
     */
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

    /* Returns the camera angle based on the distance to ground.
     * This facilitates camera rotation to 0 degrees as the camera approches 0 height.
     * Currently clamped at 45 degrees as there is no way to hide what is under the fog of war.
     */
    private float NewCameraAngle(float newDistanceToGround)
    {
        float newCameraAngle = cameraAngle;

        if (newDistanceToGround > minDistanceToGround && newDistanceToGround < angleChangeDistanceToGround)
        {
            float range = angleChangeDistanceToGround - minDistanceToGround;
            float distanceToGroundAsPercentage = (newDistanceToGround - minDistanceToGround) / range;
            newCameraAngle = (cameraAngle/100) * (distanceToGroundAsPercentage * 100);
        }

        if (newCameraAngle > 45)
        {
            return newCameraAngle;
        }
        else return 45;
        
    }

    /* True when the position is inside the terrain.
     * This is to prevent scrolling beyond the edge of the map.
     */
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

    /* True when the mouse is in the game's play area
     * False when mouse is over the HUD or UI object.
     */
    public bool MouseIsInPlayArea()
    {
        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        if (mouseX < 0 || mouseY < 0 || mouseX > 1 || mouseY > 1) return false;

        if (bottomScreenOffSet != 0 && mouseY < bottomScreenOffSet) return false;
        if (topScreenOffSet != 0 && mouseY > 1 - topScreenOffSet) return false;


        return true;
    }

    // Returns a referance to the main camera
    public Camera Camera() { return _camera; }

    /* Function which draws the lines on the minimap showing
     * where the camera is looking at.
     */
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
            Physics.Raycast(ray, out hit, 500, fogMask);
            Vector3 mapPos = new Vector3(hit.point.x, 25, hit.point.z);
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

    // Returns the lenth or width (the map is always square anyway) of the map.
    public float MapSize()
    {
        float mapSize = (terrainX + terrainZ) / 2;
        return mapSize;
    }

    // Function to immidiatly move to point camera at a specific position.
    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
        DrawMinimapCameraLines();
    }

    // Sets the Game-player's faction
    public void SetPlayerFaction(Faction newFaction)
    {
        playerFaction = newFaction;
    }

    /* Returns a list of selectables which are currently visible on the screen
     * from the list of selectables provided.
     */
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

    /* Returns a list of selectables owned by the player which are
     * currently visable on the screen.
     */
    public List<Selectable> PlayersSelectablesOnScreen()
    {
        List<Selectable> list = new List<Selectable>();

        if (playerFaction != null)
        {
            list = SelectablesOnScreen(playerFaction.Selectables());
        }

        return list;
    }

    // Returns the main camera's height
    public float CameraHeight()
    {
        return _camera.transform.position.y;
    }

    // Returns the current scroll speed
    public float ScrollSpeed { get { return scrollSpeed; } }

    // Sets a new scroll speed
    public void SetScrollSpeed(float newScrollSpeed)
    {
        scrollSpeed = newScrollSpeed;
    }

    // Function used to beging automated paning by script.
    public void PanCameraTo(Vector3 newLocation, float panSpeed = 1)
    {
        autoPan = true;
        autoPanLocation = new Vector3(newLocation.x, transform.position.y, newLocation.z);
        autoPanSpeed = panSpeed;
        allowMovement = false;
    }
}
// Writen by Lukasz Dziedziczak