// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Color playerColor; // The color used in the player's units/buildings selection & minimap indicators
    [SerializeField] private List<Selectable> currentSelection; // A list of the currently selected units/buildings/resources
    private int playerNumber = 1; // the player number, will be changed later when multiplayer is added
    private HUD_Manager HUD_Manager; // a referance to the HUD manager class
    private GameCameraController cameraController; // a referance to the player controller class
    private bool playerControlOnline = true; // true when player can issue orders and make selections
    private RectTransform selectionBox; // a referance to the selection box HUD element
    private Vector2 selectionPoint1; // the location on screen where the top left corner of the selection box is
    private Vector2 selectionPoint2; // the location on screen where the bottom right corner of the selection box is
    private LayerMask terrainMask; // contains only the terrain layer
    private LayerMask selectableMask; // contains only the terrain and selectable layers

    // Cursors
    private ECursorMode cursorMode = ECursorMode.Normal;
    private Texture2D cursor;
    private Texture2D cursorActive;
    private Texture2D cursorMove;
    private Texture2D cursorMoveActive;
    private Texture2D cursorAttack;
    private Texture2D cursorAttackActive;
    private Texture2D cursorWorker;
    private Texture2D cursorWorkerActive;
    private Texture2D cursorPatrol;
    private Texture2D cursorPatrolActive;

    private List<GameObject> selectionCircles = new List<GameObject>(); // a list of currently instantiated movement order indicators
    private GameObject selectionCirclePrefab; // the prefab used to instantiae a movement confirmation indicator
    private float selectionCircleRate = 0.05f; // how fast the movement order indicator shrinks
    private float selectionCircleTimer = 0; // a timer used to shrink all the movement confrimation indicators per rate
    private bool settingPatrolPoint=false; // true when setting a patrol point for units
    private bool mouseClickedRecently;
    private float doubleClickInterval = 0.25f;
    private float doubleClickTimer = 0;

    // The context the cursor (mouse pointer) can be in, based on what it is hovering over and what is selected
    public enum ECursorMode
    {
        Normal, // cursor when not in one of the other states
        Move, // when a movement order can be issued to units
        Attack, // when an attack order can be issued to units/towers
        Worker, // when worker can construct or gather resource under mouse
        Heal, // when healer can heal unit under mouse, currently unused
        Patrol // when setting a new patrol point
    }

    // Run while the game is loading
    private void Awake()
    {
        cameraController = FindObjectOfType<GameCameraController>();
        selectionBox = FindSelectionBox();
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
        selectableMask |= (1 << LayerMask.NameToLayer("Terrain"));
        selectableMask |= (1 << LayerMask.NameToLayer("Selectable"));
        LoadCursors();
        selectionCirclePrefab = (GameObject)Resources.Load("Prefabs/selectionCircle");
    }

    // Loads the cursor textures from the resources folder.
    private void LoadCursors()
    {
        Texture2D[] loadedCursors = Resources.LoadAll<Texture2D>("Cursors/");
        foreach (var loadedCursor in loadedCursors)
        {
            if (loadedCursor.name == "G_Cursor_Basic2") cursor = loadedCursor;
            if (loadedCursor.name == "Cursor_Basic2") cursorActive = loadedCursor;
            if (loadedCursor.name == "G_Cursor_Move1") cursorMove = loadedCursor;
            if (loadedCursor.name == "Cursor_Move1") cursorMoveActive = loadedCursor;
            if (loadedCursor.name == "G_Cursor_Attack") cursorAttack = loadedCursor;
            if (loadedCursor.name == "Cursor_Attack") cursorAttackActive = loadedCursor;
            if (loadedCursor.name == "G_Cursor_Production") cursorWorker = loadedCursor;
            if (loadedCursor.name == "Cursor_Production") cursorWorkerActive = loadedCursor;
            if (loadedCursor.name == "G_Cursor_Attack_G") cursorPatrol = loadedCursor;
            if (loadedCursor.name == "Cursor_Attack_G") cursorPatrolActive = loadedCursor;
        }
    }

    // Finds the selection box UI element and sets the class' referance to it
    private RectTransform FindSelectionBox()
    {
        RectTransform[] rectTrans = FindObjectsOfType<RectTransform>();
        foreach (RectTransform rectTran in rectTrans)
        {
            if (rectTran.name == "SelectionBox") return rectTran;
        }

        Debug.LogError(name + " unable to find SelectionBox.");
        return null;
    }

    // Start is called before the first frame update
    private void Start()
    {
        currentSelection = new List<Selectable>();
        selectionBox.gameObject.SetActive(false);


        SetCursor(ECursorMode.Normal);
    }

    /* Function to change the cursor based on context
     * isActive = the mouse button is down
     * There are two color variations, one for when mouse button is down
     * the other when it is not.
     */
    private void SetCursor(ECursorMode newCursorMode, bool isActive = false)
    {
        if(newCursorMode != cursorMode) cursorMode = newCursorMode;

        Vector2 cursorOffSet = new Vector2();

        switch (cursorMode)
        {
            case ECursorMode.Normal:
                cursorOffSet = new Vector2(cursor.width / 3, 0);
                if (isActive)
                {
                    if (cursorActive != null) Cursor.SetCursor(cursorActive, cursorOffSet, CursorMode.Auto);
                }
                else
                {
                    
                    if (cursor != null) Cursor.SetCursor(cursor, cursorOffSet, CursorMode.Auto);
                }
                break;

            case ECursorMode.Move:
                cursorOffSet = new Vector2(cursorMove.width / 2, cursorMove.height /2);
                if (isActive)
                {
                    if (cursorMoveActive != null) Cursor.SetCursor(cursorMoveActive, cursorOffSet, CursorMode.Auto);
                }
                else
                {

                    if (cursorMove != null) Cursor.SetCursor(cursorMove, cursorOffSet, CursorMode.Auto);
                }
                break;

            case ECursorMode.Attack:
                cursorOffSet = new Vector2(cursorAttack.width / 3, 0);
                if (isActive)
                {
                    if (cursorAttackActive != null) Cursor.SetCursor(cursorAttackActive, cursorOffSet, CursorMode.Auto);
                }
                else
                {

                    if (cursorAttack != null) Cursor.SetCursor(cursorAttack, cursorOffSet, CursorMode.Auto);
                }
                break;

            case ECursorMode.Worker:
                cursorOffSet = new Vector2(cursorWorker.width / 3, 0);
                if (isActive)
                {
                    if (cursorWorkerActive != null) Cursor.SetCursor(cursorWorkerActive, cursorOffSet, CursorMode.Auto);
                }
                else
                {

                    if (cursorWorker != null) Cursor.SetCursor(cursorWorker, cursorOffSet, CursorMode.Auto);
                }
                break;

            case ECursorMode.Patrol:
                cursorOffSet = new Vector2(cursorAttack.width / 3, 0);
                if (isActive)
                {
                    if (cursorPatrolActive != null) Cursor.SetCursor(cursorPatrolActive, cursorOffSet, CursorMode.Auto);
                }
                else
                {

                    if (cursorPatrol != null) Cursor.SetCursor(cursorPatrol, cursorOffSet, CursorMode.Auto);
                }
                break;

            default:
                Debug.LogError(name + " - Cursor Mode not found.");
                break;

        }

        
    }

    // Changes cursor when mouse button down
    // Active = any mouse button down
    private void SetCursorActive(bool isActive)
    {
        SetCursor(cursorMode, isActive);
    }

    // True if current selection has worker units
    private bool SelectionHasWorkers()
    {
        foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.UnitType() == Unit.EUnitType.Worker) return true;
        }

        return false;
    }

    // True if the current selection has units
    private bool SelectionHasUnits()
    {
        if (currentSelection.Count > 0)
        {
            foreach (Selectable selectable in currentSelection)
            {
                Unit unit = selectable.GetComponent<Unit>();
                if (unit != null) return true;
            }
        }
        return false;
    }

    // True if the current selection has a unit cabale of attacking other units
    private bool SelectionHasAttackers()
    {
        foreach (Selectable selectable in currentSelection)
        {
            Attacker attacker = selectable.GetComponent<Attacker>();
            if (attacker != null) return true;
        }

        return false;
    }

    // Update is called once per frame
    private void Update()
    {
        DoubleClickTimer();
        SelectionCircleLogic(); // selection circles are drawn as movement order confirmation indicators
        UpdateCursor(); // change cursor based on context the cursor is in
        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapePushed();

        if (!playerControlOnline) return; // all code below this line will not execute when the player's control is disabled

        if (Input.GetMouseButtonDown(0)) HandleLeftMouseDown();

        if (Input.GetMouseButton(0)) HandleLeftMouse();

        if (Input.GetMouseButtonUp(0)) HandleLeftMouseUp();

        if (Input.GetMouseButtonDown(1)) HandleRightClick();

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) SetCursorActive(true);
        else SetCursorActive(false);


        if (Input.GetKeyDown(KeyCode.Delete)) KillSelectedUnits();

    }

    private void DoubleClickTimer()
    {
        if (mouseClickedRecently)
        {
            doubleClickTimer += Time.deltaTime;

            if (doubleClickTimer > doubleClickInterval)
            {
                DoubleClickReset();
            }
        }
    }

    private void DoubleClickReset()
    {
        mouseClickedRecently = false;
        doubleClickTimer = 0;
    }

    /* Process for updating the cursor
     * Called every frame
     */
    private void UpdateCursor()
    {
        if (Input.GetMouseButton(0)) return;

        if (cameraController.MouseIsInPlayArea())
        {
            if (SelectionHasUnits())
            {
                if (settingPatrolPoint) SetCursor(ECursorMode.Patrol);
                else if (IsPlayersSelectable(SelectableUnderMouse())) SetCursor(ECursorMode.Normal);
                else if (SelectionHasAttackers() && IsEnemy(SelectableUnderMouse())) SetCursor(ECursorMode.Attack);
                else if (SelectionHasWorkers() && (IsCollectableResource(SelectableUnderMouse()) || IsPlayersInteractableBuilding(SelectableUnderMouse()))) SetCursor(ECursorMode.Worker);
                else SetCursor(ECursorMode.Move);
            }
            else SetCursor(ECursorMode.Normal);
        }
        else
        {
            SetCursor(ECursorMode.Normal);
        }
    }

    /* Process for when player presses Esc on the keyboard
     * Varies based on what is currently being displayed
     * Closes any HUD dialog being shown if any
     * Otherwise opens the pause menu
     */
    private void HandleEscapePushed()
    {
        GameController gameController = FindObjectOfType<GameController>();

        if (gameController != null)
        {
            if (gameController.PauseMenu.IsShowing) gameController.PauseMenu.ToggleShowing();
            else if (gameController.SettingsMenu.IsShowing) gameController.SettingsMenu.Show(false);
            else if (gameController.ObjectivesMenu.IsShowing) gameController.ObjectivesMenu.Show(false);
            else if (!gameController.PauseMenu.IsShowing) gameController.PauseMenu.ToggleShowing();
        }
    }

    /* Adds the passed in selectable to the current selection
     */
    public void AddToSelection(Selectable selectable)
    {
        if (selectable.GetComponent<Building>() != null && selectable.GetComponent<Building>().BuildState() == Building.EBuildState.Destroyed) return;
        if (SelectableInSelection(selectable)) return;

        selectable.Selected(true);
        currentSelection.Add(selectable);
        HUD_Manager.NewSelection(currentSelection);
    }

    /* Adds the passed in list of selectable to the current selection
     */
    public void AddToSelection(List<Selectable> selectables)
    {
        foreach (Selectable selectable in selectables)
        {
            if (!SelectableInSelection(selectable))
            {
                selectable.Selected(true);
                currentSelection.Add(selectable);
            }
        }
        HUD_Manager.NewSelection(currentSelection);
    }

    // Removes passed in selectable from the current selection
    public void RemoveFromSelection(Selectable selectable)
    {
        if (SelectableInSelection(selectable))
        {
            currentSelection.Remove(selectable);
            selectable.Selected(false);
        }
        HUD_Manager.NewSelection(currentSelection);
    }

    // Proccess for when the left mouse button is initially pressed down
    private void HandleLeftMouseDown()
    {
        if (cameraController.MouseIsInPlayArea())
        {
            selectionPoint1 = Input.mousePosition;
        }
    }

    // Process for when the Left mouse button is currently being held down
    private void HandleLeftMouse()
    {
        if (selectionPoint1 == new Vector2()) return;
        selectionPoint2 = Input.mousePosition;

        if (Vector3.Distance(selectionPoint1, selectionPoint2) > 2.0f && cameraController.MouseIsInPlayArea())
        {
            selectionBox.gameObject.SetActive(true);
            cameraController.allowMovement = false;

            float width = selectionPoint2.x - selectionPoint1.x;
            float height = selectionPoint2.y - selectionPoint1.y;

            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            selectionBox.anchoredPosition = selectionPoint1 + new Vector2(width/2 , height/2);
        }
        else if (selectionBox.gameObject.activeSelf && !cameraController.MouseIsInPlayArea())
        {
            EndOfSelectionBox();
        }
    }

    // Process for when the Left mouse button is relased after being pressed
    private void HandleLeftMouseUp()
    {

        if (selectionBox.gameObject.activeInHierarchy)
        {
            EndOfSelectionBox();
        }
        else if (cameraController.MouseIsInPlayArea())
        {
            if (mouseClickedRecently)
            {
                ClearSelection();

                Selectable selectableUnderMouse = SelectableUnderMouse();
                if (selectableUnderMouse)
                {
                    Unit unitUnderMouse = selectableUnderMouse.GetComponent<Unit>();

                    if (unitUnderMouse)
                    {
                        AddToSelection(AllUnitsOfTypeOnScreen(unitUnderMouse.UnitType()));
                        DoubleClickReset();
                    }
                }
            }
            else
            {
                mouseClickedRecently = true;

                // if either left shit or left ctrl isn't pressed; clear selection
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.RightControl)) ClearSelection();

                Selectable selectable = SelectableUnderMouse();
                if (selectable != null)
                {
                    if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && SelectableInSelection(selectable))
                    {
                        RemoveFromSelection(selectable);
                    }

                    else if (selectable.PlayerNumber() == playerNumber)
                    {
                        AddToSelection(selectable);
                    }

                    else if (currentSelection.Count == 0)
                    {
                        AddToSelection(selectable);
                    }
                }
            }
        }

        selectionPoint1 = new Vector2();
        selectionPoint2 = new Vector2();
    }

    // Process for when player finishes drawing a selection box
    private void EndOfSelectionBox()
    {
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) ClearSelection(); 

        AddToSelection(PlayersUnits(InSelectionBox()));

        selectionBox.gameObject.SetActive(false);
        cameraController.allowMovement = true;
    }

    private List<Selectable> AllUnitsOfTypeOnScreen(Unit.EUnitType unitType)
    {
        List<Selectable> list = new List<Selectable>();

        List<Unit> playerUnits = FindObjectOfType<GameController>().GetPlayerFaction().units;
        foreach (Unit unit in playerUnits)
        {
            Vector3 screenPos = cameraController.Camera().WorldToScreenPoint(unit.transform.position);
            if (screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
            { 
                if (unit.UnitType() == unitType) list.Add(unit);
            }
        }
        return list;
    }

    // True if passed in selectable is in the list of currently selected Selectables
    private bool SelectableInSelection(Selectable newSelectable)
    {
        if (currentSelection.Count > 0)
        {
            foreach (Selectable selectable in currentSelection)
            {
                if (selectable == newSelectable) return true;
            }
        }

        return false;
    }

    // Process for when the player has right-clicked on the mouse
    private void HandleRightClick()
    {
        if (!cameraController.MouseIsInPlayArea()) return;
        if (currentSelection.Count > 0)
        {
            RaycastHit hit = UnderMouse();
            Selectable hitSelectable = hit.transform.GetComponent<Selectable>();
            CollectableResource collectableResource;
            Building building;
            Unit unit;

            if (hitSelectable != null)
            {
                collectableResource = hitSelectable.GetComponent<CollectableResource>();
                building = hitSelectable.GetComponent<Building>();
                unit = hitSelectable.GetComponent<Unit>();

                if (collectableResource != null) GiveCollectResourceOrder(collectableResource);

                if (building != null) RightClickOnBuilding(building);

                if (unit != null) RightClickOnUnit(unit);
            }
            else
            {
                GiveMoveOrder(hit.point);
            }
        }
    }

    // Handles when the player the player right-clicks on the passed in building
    // Either to (re)construct a building if friendly
    // Or to attack it if it's an enemy's 
    private void RightClickOnBuilding(Building building)
    {
        // check if owned by player
        if (building.PlayerNumber() == playerNumber)
        {
            GiveUnitToBuildingOrder(building);
        }
        else
        {
            GiveAttackOrder(building);
        }
    }

    // Handles when the player right-clicks the passed in unit
    // Healing them if they are friendly, or attacking them if they are a enemy
    private void RightClickOnUnit(Unit unit)
    {
        // check if owned by player
        if (unit.PlayerNumber() == playerNumber)
        {
            GiveUnitHealOrder(unit);
        }
        else
        {
            GiveAttackOrder(unit);
        }    
    }

    // Returns a list of selectables that are under a drawn selection box
    private List<Selectable> InSelectionBox()
    {
        List<Selectable> list = new List<Selectable>();

        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        Selectable[] allSelectables = GameObject.FindObjectsOfType<Selectable>();
        foreach (Selectable selectable in allSelectables)
        {
            Vector3 screenPos = cameraController.Camera().WorldToScreenPoint(selectable.transform.position);
            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                list.Add(selectable);
            }
        }

        return list;
    }

    // Returns a list of the player's units/buildings from the list of passed in units/buildings/resources
    private List<Selectable> PlayersUnits(List<Selectable> selectables)
    {
        List<Selectable> list = new List<Selectable>();

        foreach (Selectable selectable in selectables)
        {
            if (selectable.PlayerNumber() == playerNumber)
            {
                Unit unit = selectable.GetComponent<Unit>();
                if (unit != null) list.Add(unit);
            }
        }

        return list;
    }

    // Issues an order to units to interact with the passed in building if possible
    private void GiveUnitToBuildingOrder(Building building)
    {
        bool orderGiven = false;

        if (building.IsResourceDropPoint())
        {
            foreach (Selectable selectable in currentSelection)
            {
                Unit unit = selectable.GetComponent<Unit>();
                if (unit != null && unit.IsCarryingResource())
                {
                    unit.SetResourceDropOffPoint(building);
                    orderGiven = true;
                }
            }
        }

        if (building.BuildState() == Building.EBuildState.Building || (building.BuildState() == Building.EBuildState.Complete && !building.GetComponent<Health>().IsFull()))
        {
            foreach (Selectable selectable in currentSelection)
            {
                BuildingConstructor constructor = selectable.GetComponent<BuildingConstructor>();
                if (constructor != null)
                {
                    constructor.SetBuildTarget(building);
                    orderGiven = true;
                }
            }
        }

        if (orderGiven) building.ConfirmOrder();
    }

    /* Returns a RaycastHit for what is under the mouse
     */
    private RaycastHit UnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 500, selectableMask);
        return hit;
    }

    /* Function used for testing
     * Gives print out what is under mouse, combine with a mouse down function to use
     */
    private void AnyUnderMouse()
    {
        LayerMask fogLayer = new LayerMask();
        fogLayer |= (1 << LayerMask.NameToLayer("FogOfWar"));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 500, fogLayer);

        print("hit point" + hit.point);
        print("name= " + hit.transform.gameObject.name);
        print("layer= " + hit.transform.gameObject.layer);
    }

    /* Returns a selectable under the mouse pointer or null if there is none
     */
    private Selectable SelectableUnderMouse()
    {
        RaycastHit hit = UnderMouse();
        if (hit.transform != null)
        {
            Selectable selectable = hit.transform.GetComponent<Selectable>();
            return selectable;
        }
        else return null;
    }

    // True if selectable passed in is an enemy to the player
    private bool IsEnemy(Selectable selectableUnderMouse)
    {
        if (selectableUnderMouse == null) return false;

        Unit unit = selectableUnderMouse.GetComponent<Unit>();
        if (unit != null)
        {
            if (unit.PlayerNumber() != playerNumber) return true;
        }
        else
        {
            Building building = selectableUnderMouse.GetComponent<Building>();
            if (building != null && building.PlayerNumber() != playerNumber) return true;
        }

        return false;
    }

    // True if selectable passed in is a collectable resource
    private bool IsCollectableResource(Selectable selectableUnderMouse)
    {
        if (selectableUnderMouse == null) return false;

        CollectableResource collectableResource = selectableUnderMouse.GetComponent<CollectableResource>();
        if (collectableResource != null) return true;

        return false;
    }

    // True if selectable passed in is player's building and needs to be repaired.
    private bool IsPlayersInteractableBuilding(Selectable selectableUnderMouse)
    {
        if (selectableUnderMouse == null) return false;

        Building building = selectableUnderMouse.GetComponent<Building>();

        if (building != null && building.PlayerNumber() == playerNumber)
        {
            if (building.BuildState() == Building.EBuildState.Building) return true;
            if (building.BuildState() == Building.EBuildState.Complete && !building.GetComponent<Health>().IsFull()) return true;
        }

        return false;
    }

    // True if selectable passed in is owned by the player, is part of players faction
    private bool IsPlayersSelectable(Selectable selectableUnderMouse)
    {
        if (selectableUnderMouse == null) return false;

        if (selectableUnderMouse.PlayerNumber() == playerNumber) return true;

        return false;
    }

    // Returns a Vector of the RaycastHit from the mouse to the ground
    public Vector3 LocationUnderMouse()
    {
        return UnderMouse().point;
    }

    // Returns a Vector of a location on the terrain under the mouse
    public Vector3 TerrainLocationUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 1000, terrainMask);
        return hit.point;
    }

    // Clears the currect selection
    public void ClearSelection()
    {
        if (currentSelection.Count <= 0) return;

        foreach (Selectable selectable in currentSelection)
        {
            selectable.Selected(false);
        }
        currentSelection.Clear();
        HUD_Manager.ClearSelection();

        SetCursor(ECursorMode.Normal);
    }

    // Issues a movement order to the passed in location to the currently selected Units
    private void GiveMoveOrder(Vector3 newLocation)
    {
        bool hasGivenOrder = false;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(newLocation, out hit, 2, NavMesh.AllAreas)) return;

        if (currentSelection.Count == 1)
        {
            Unit unit = currentSelection[0].GetComponent<Unit>();
            if (unit != null && unit.PlayerNumber() == playerNumber)
            {
                unit.MoveTo(hit.position);
                hasGivenOrder = true;
            }
        }
        else
        {
            MoveInFormation(newLocation);
            hasGivenOrder = true;
        }

        if (hasGivenOrder) SpawnSelectionCircle(newLocation);
    }

    
    // Issues an attack order on the passed in selectable to the currectly selected Units
    private void GiveAttackOrder(Selectable newTarget)
    {
        bool hasGivenOrder = false;

        foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.PlayerNumber() == playerNumber && unit.UnitType() != Unit.EUnitType.Worker)
            {
                unit.SetTarget(newTarget);
                hasGivenOrder = true;
            }
            else
            {
                Building building = selectable.GetComponent<Building>();
                if (building != null)
                {
                    Attacker attacker = building.GetComponent<Attacker>();
                    if (attacker != null)
                    {
                        attacker.SetTarget(newTarget);
                        hasGivenOrder = true;
                    }
                }
            }
        }

        if (hasGivenOrder) newTarget.ConfirmAttack();
    }

    // Issues a healing order to the healers in the currect selection targeting the passed in Unit
    private void GiveUnitHealOrder(Unit unit)
    {
        bool hasGivenOrder = false;

        foreach (Selectable selectable in currentSelection)
        {
            Healer selectedHealer = selectable.GetComponent<Healer>();
            if (selectedHealer != null)
            {
                selectedHealer.SetTargetHealUnit(unit);
                hasGivenOrder = true;
            }
        }

        if (hasGivenOrder) unit.ConfirmOrder();
    }

    // Issues a collect of the passed in resource order to the current selection 
    private void GiveCollectResourceOrder(CollectableResource newResource)
    {
        bool hasGivenOrder = false;

        foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.UnitType() == Unit.EUnitType.Worker)
            {
                unit.SetTarget(newResource);
                hasGivenOrder = true;
            }
        }

        if (hasGivenOrder) newResource.ConfirmOrder();
    }

    // Sets the class' reference to the HUD manager
    public void SetHUD_Manager(HUD_Manager newManager)
    {
        HUD_Manager = newManager;
    }

    // Enables/Disables the player's control of the game
    public void PlayerControl(bool online)
    {
        playerControlOnline = online;
    }

    // Issues an order to multiple selected units to move but not all to the same spot
    // but rather each unit next to the last unit
    private void MoveInFormation(Vector3 newLocation)
    {
        int numberOfRows = (currentSelection.Count / 5) + 1;
        float offSet = 1.0f;

        /*print(numberOfRows);*/

        for (int i = 0; i < numberOfRows; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int currentIndex = j + (i * 5);

                if (currentIndex < currentSelection.Count)
                {
                    Vector3 newDestination = new Vector3(newLocation.x + (offSet * j),
                                               newLocation.y,
                                               newLocation.z + (offSet * i));
                    currentSelection[currentIndex].GetComponent<Unit>().MoveTo(newDestination);
                }
            }
        }
    }

    // True if nothing is currently selected
    public bool NothingSelected() { return currentSelection.Count == 0; }

    // Issues a Seppuku order to currently selected units/buildings
    private void KillSelectedUnits()
    {
        if (currentSelection.Count > 0)
        {
            Selectable[] selectables = currentSelection.ToArray();
            foreach (Selectable selectable in selectables)
            {
                if (selectable != null && selectable.PlayerNumber() == playerNumber)
                {
                    Health health = selectable.GetComponent<Health>();
                    if(health != null && health.IsAlive())
                    {
                        selectable.GetComponent<Health>().Kill();
                        currentSelection.Remove(selectable);
                    }
                }
            }

            ClearSelection();
        }
    }

    // Called when a selected unit has died so it can be removed for current selection list
    public void SelectedUnitDied(Selectable selectable)
    {
        if (currentSelection.Contains(selectable))
        {
            currentSelection.Remove(selectable);
        }
        selectable.Selected(false);

        HUD_Manager.NewSelection(currentSelection);
    }

    // Spawns an indicator to the player at the passed in location where a movement order has been issued
    private void SpawnSelectionCircle(Vector3 location)
    {
        if (selectionCirclePrefab != null)
        {
            Vector3 offset = new Vector3(0, 0.05f, 0);
            Quaternion rotation = Quaternion.Euler(90 , 0, 0);
            GameObject selectionCircle = Instantiate(selectionCirclePrefab, location + offset, rotation);
            selectionCircles.Add(selectionCircle);
        }
    }

    // Processes the animation of Movement Order Indicators spawned by the previous function
    private void SelectionCircleLogic()
    {
        if (selectionCircles.Count > 0)
        {
            selectionCircleTimer += Time.deltaTime;

            if (selectionCircleTimer > selectionCircleRate)
            {
                selectionCircleTimer = 0;

                GameObject[] selectionCirclesArray = selectionCircles.ToArray();

                foreach (GameObject selectionCircle in selectionCirclesArray)
                {
                    selectionCircle.transform.localScale -= selectionCircle.transform.localScale * 0.1f;

                    if (selectionCircle.transform.localScale.x <= 0.1f)
                    {
                        selectionCircles.Remove(selectionCircle);
                        Destroy(selectionCircle);
                    }
                }
            }
        }
    }

    // Returns the player's color, used in selection indication circles and squares
    public Color PlayerColor()
    {
        return playerColor;
    }

    // Enables/Disables player control when setting new patrol point for units
    public void SettingPatrolPoint(bool newSetting)
    {
        settingPatrolPoint = newSetting;
        PlayerControl(!newSetting);
        cameraController.allowMovement = !newSetting;
    }
}
// Writen by Lukasz Dziedziczak