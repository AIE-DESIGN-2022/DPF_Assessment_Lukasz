using UnityEngine;
using UnityEngine.UI;

/* This is a class for an individual selectable showing in the bottom info bar
 * when multiple things have been selected.
 * Shows icon of the selectable and it's health.
 * When clicked it will clear the selection and only select the clicked selectable.
 * When clicked and control is held selectable is removed from current selection.
 */
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

    // Called when first instantiated to setup this UI element
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

    // Called to update the health bar, when selected unit is damaged or is healed.
    public void UpdateHealthBar()
    {
        if (health != null)
        {
            healthForeground.transform.localScale = new Vector3(health.HealthPercentage(), 1, 1);
        }
    }

    // Returns the selectable represented by this UI element.
    public Selectable Selectable { get { return selected; } }


    // When this UI element is clicked
    private void OnClick()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            playerController.RemoveFromSelection(selected);
        }
        else
        {
            playerController.ClearSelection();
            playerController.AddToSelection(selected);
        }
    }
    
}
