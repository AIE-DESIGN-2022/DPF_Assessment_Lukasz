// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private List<Selectable> currentSelection;
    private int playerNumber = 1;
    private HUD_Manager HUD_Manager;
    private GameCameraController cameraController;
    private bool playerControlOnline = true;
    private RectTransform selectionBox;
    private Vector2 selectionPoint1;
    private Vector2 selectionPoint2;
    private LayerMask terrainMask;


    private void Awake()
    {
        cameraController = FindObjectOfType<GameCameraController>();
        selectionBox = FindSelectionBox();
        terrainMask |= (1 << LayerMask.NameToLayer("Terrain"));
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
    }

    // Update is called once per frame
    private void Update()
    {
        if (!playerControlOnline) return;

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();

        if (Input.GetMouseButton(0)) HandleLeftMouseDown();

        if (Input.GetMouseButtonUp(0)) HandleLeftMouseUp();

        if (Input.GetMouseButtonDown(1)) HandleRightClick();

        if (Input.GetKeyDown(KeyCode.Escape)) HandleEscapePushed();
    }

    private void HandleEscapePushed()
    {
        FindObjectOfType<GameController>().PauseMenu().ToggleShowing();
    }

    private void HandleLeftClick()
    {
        if (!cameraController.MouseIsInPlayArea()) return;
        if (!Input.GetKey(KeyCode.LeftShift)) ClearSelection();

        RaycastHit hit = UnderMouse();
        Selectable selectable = hit.transform.GetComponent<Selectable>();
        if (selectable != null && selectable.PlayerNumber() == playerNumber)
        {
            AddToSelection(selectable);
        }

        if (selectable != null && currentSelection.Count == 0)
        {
            AddToSelection(selectable);
        }

        if (selectable == null) // if no selectable object was hit with mouse click
        {
            selectionPoint1 = Input.mousePosition;
        }
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

    private void HandleLeftMouseDown()
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
    }

    private void HandleLeftMouseUp()
    {
        if (selectionBox.gameObject.activeInHierarchy && cameraController.MouseIsInPlayArea())
        {
            AddToSelection(PlayersUnits(InSelectionBox()));

            selectionBox.gameObject.SetActive(false);
            cameraController.allowMovement = true;
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
        if (building.IsResourceDropPoint())
        {
            foreach (Selectable selectable in currentSelection)
            {
                Unit unit = selectable.GetComponent<Unit>();
                if (unit != null && unit.IsCarryingResource())
                {
                    unit.SetResourceDropOffPoint(building);
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
                }
            }
        }
    }

    private RaycastHit UnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        return hit;
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
    }

    private void GiveMoveOrder(Vector3 newLocation)
    {
        if (currentSelection.Count == 1)
        {
            Unit unit = currentSelection[0].GetComponent<Unit>();
            if (unit != null && unit.PlayerNumber() == playerNumber)
            {
                unit.MoveTo(newLocation);
            }
        }
        else
        {
            MoveInFormation(newLocation);
        }


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
        foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.PlayerNumber() == playerNumber && unit.UnitType() != Unit.EUnitType.Worker)
            {
                unit.SetTarget(newTarget);
            }
            else
            {
                Building building = selectable.GetComponent<Building>();
                if (building != null)
                {

                }
            }
        }
    }

    private void GiveUnitHealOrder(Unit unit)
    {
        foreach (Selectable selectable in currentSelection)
        {
            Healer selectedHealer = selectable.GetComponent<Healer>();
            if (selectedHealer != null)
            {
                selectedHealer.SetTargetHealUnit(unit);
            }
        }    
    }

    private void GiveCollectResourceOrder(CollectableResource newResource)
    {
        foreach (Selectable selectable in currentSelection)
        {
            Unit unit = selectable.GetComponent<Unit>();
            if (unit != null && unit.UnitType() == Unit.EUnitType.Worker)
            {
                unit.SetTarget(newResource);
            }
        }
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
}
// Writen by Lukasz Dziedziczak