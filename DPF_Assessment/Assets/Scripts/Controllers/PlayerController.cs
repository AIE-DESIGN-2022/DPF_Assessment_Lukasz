// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Color playerColor;
    [SerializeField] private List<Selectable> currentSelection;
    private int playerNumber = 1;
    private HUD_Manager HUD_Manager;
    private GameCameraController cameraController;
    private bool playerControlOnline = true;
    private RectTransform selectionBox;
    private Vector2 selectionPoint1;
    private Vector2 selectionPoint2;
    private LayerMask terrainMask;

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

    private List<GameObject> selectionCircles = new List<GameObject>();
    private GameObject selectionCirclePrefab;
    private float selectionCircleRate = 0.05f;
    private float selectionCircleTimer = 0;

    public enum ECursorMode
    {
        Normal,
        Move,
        Attack,
        Worker,
        Heal
    }


    private void Awake()
    {
        cameraController = FindObjectOfType<GameCameraController>();
        selectionBox = FindSelectionBox();
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
        LoadCursors();
        selectionCirclePrefab = (GameObject)Resources.Load("Prefabs/selectionCircle");
    }

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
        }
    }

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

            default:
                Debug.LogError(name + " - Cursor Mode not found.");
                break;

        }

        
    }

    private void SetCursorActive(bool isActive)
    {
        SetCursor(cursorMode, isActive);
    }

    private bool SelectionHasWorkers()
    {
        foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.UnitType() == Unit.EUnitType.Worker) return true;
        }

        return false;
    }

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
        if (!playerControlOnline) return;

        UpdateCursor();

        if (Input.GetMouseButtonDown(0)) HandleLeftMouseDown();

        if (Input.GetMouseButton(0)) HandleLeftMouse();

        if (Input.GetMouseButtonUp(0)) HandleLeftMouseUp();

        if (Input.GetMouseButtonDown(1)) HandleRightClick();

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) SetCursorActive(true);
        else SetCursorActive(false);

        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapePushed();

        if (Input.GetKeyDown(KeyCode.Delete)) KillSelectedUnits();

        SelectionCircleLogic();
    }

    private void UpdateCursor()
    {
        if (Input.GetMouseButton(0)) return;

        if (cameraController.MouseIsInPlayArea())
        {
            if (SelectionHasUnits())
            {
                if (IsPlayersSelectable(SelectableUnderMouse())) SetCursor(ECursorMode.Normal);
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

    private void HandleEscapePushed()
    {
        FindObjectOfType<GameController>().PauseMenu().ToggleShowing();
    }

    private void HandleLeftMouseDown()
    {
        if (!cameraController.MouseIsInPlayArea()) return;

        selectionPoint1 = Input.mousePosition;
    }

    private void AddToSelection(Selectable selectable)
    {
        if (selectable.GetComponent<Building>() != null && selectable.GetComponent<Building>().BuildState() == Building.EBuildState.Destroyed) return;

        selectable.Selected(true);
        currentSelection.Add(selectable);
        HUD_Manager.NewSelection(currentSelection);
    }

    private void AddToSelection(List<Selectable> selectables)
    {
        foreach (Selectable selectable in selectables)
        {
            selectable.Selected(true);
            currentSelection.Add(selectable);
        }
        HUD_Manager.NewSelection(currentSelection);
    }

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
        else
        {
            selectionBox.gameObject.SetActive(false);
            cameraController.allowMovement = true;
        }

        /*if (selectionBox.gameObject.activeInHierarchy && !cameraController.MouseIsInPlayArea())
        {
            AddToSelection(PlayersUnits(InSelectionBox()));

            selectionBox.gameObject.SetActive(false);
            cameraController.allowMovement = true;
        }*/
    }

    private void HandleLeftMouseUp()
    {

        if (selectionBox.gameObject.activeInHierarchy && cameraController.MouseIsInPlayArea())
        {
            if (!Input.GetKey(KeyCode.LeftShift)) ClearSelection();

            AddToSelection(PlayersUnits(InSelectionBox()));

            selectionBox.gameObject.SetActive(false);
            cameraController.allowMovement = true;
        }
        else if (cameraController.MouseIsInPlayArea())
        {
            if (!Input.GetKey(KeyCode.LeftShift)) ClearSelection();

            Selectable selectable = SelectableUnderMouse();

            if (selectable != null && selectable.PlayerNumber() == playerNumber)
            {
                AddToSelection(selectable);
            }

            if (selectable != null && currentSelection.Count == 0)
            {
                AddToSelection(selectable);
            }
        }

        selectionPoint1 = new Vector2();
        selectionPoint2 = new Vector2();
    }

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

    private RaycastHit UnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        return hit;
    }

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

    private bool IsCollectableResource(Selectable selectableUnderMouse)
    {
        if (selectableUnderMouse == null) return false;

        CollectableResource collectableResource = selectableUnderMouse.GetComponent<CollectableResource>();
        if (collectableResource != null) return true;

        return false;
    }

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

    private bool IsPlayersSelectable(Selectable selectableUnderMouse)
    {
        if (selectableUnderMouse == null) return false;

        if (selectableUnderMouse.PlayerNumber() == playerNumber) return true;

        return false;
    }

    public Vector3 LocationUnderMouse()
    {
        return UnderMouse().point;
    }

    public Vector3 TerrainLocationUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 1000, terrainMask);
        return hit.point;
    }

    private void ClearSelection()
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

    private void GiveMoveOrder(Vector3 newLocation)
    {
        bool hasGivenOrder = false;

        if (currentSelection.Count == 1)
        {
            Unit unit = currentSelection[0].GetComponent<Unit>();
            if (unit != null && unit.PlayerNumber() == playerNumber)
            {
                unit.MoveTo(newLocation);
                hasGivenOrder = true;
            }
        }
        else
        {
            MoveInFormation(newLocation);
            hasGivenOrder = true;
        }

        if (hasGivenOrder) SpawnSelectionCircle(newLocation);

        /*foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.PlayerNumber() == playerNumber)
            {
                unit.MoveTo(newLocation);
            }
        }*/
    }

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

    public void SetHUD_Manager(HUD_Manager newManager)
    {
        HUD_Manager = newManager;
    }

    public void PlayerControl(bool online)
    {
        playerControlOnline = online;
    }

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

    public bool NothingSelected() { return currentSelection.Count == 0; }

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

    public void SelectedUnitDied(Selectable selectable)
    {
        if (currentSelection.Contains(selectable))
        {
            currentSelection.Remove(selectable);
        }
        selectable.Selected(false);

        HUD_Manager.NewSelection(currentSelection);
    }

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

    public Color PlayerColor()
    {
        return playerColor;
    }
}
// Writen by Lukasz Dziedziczak