using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_InfoElement : MonoBehaviour
{
    private RawImage icon;
    private Image healthForeground;
    private Button button;

    private Selectable selected;
    private Health health;
    private PlayerController playerController;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        icon = GetComponentInChildren<RawImage>();
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.name == "HealthForeground") healthForeground = image;
        }

        playerController = FindObjectOfType<PlayerController>();
    }


    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set(Selectable selectable)
    {
        selected = selectable;
        Unit unit = selectable.GetComponent<Unit>();
        Building building = selectable.GetComponent<Building>();

        if (unit != null)
        {
            icon.texture = unit.Faction.Config().Icon(unit.UnitType());
        }

        if (building != null)
        {
            icon.texture = building.Faction.Config().Icon(building.BuildingType());
        }

        health = selected.GetComponent<Health>();
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (health != null)
        {
            healthForeground.transform.localScale = new Vector3(health.HealthPercentage(), 1, 1);
        }
    }

    public Selectable Selectable { get { return selected; } }

    private void OnClick()
    {
        /*List<Selectable> list = new List<Selectable>();
        list.Add(selected);*/

        playerController.ClearSelection();
        playerController.AddToSelection(selected);
    }
    
}
