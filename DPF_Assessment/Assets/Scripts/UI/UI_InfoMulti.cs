using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_InfoMulti : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private RectTransform multiBox;
    private UI_InfoElement infoElementPrefab;
    private List<UI_InfoElement> elementList = new List<UI_InfoElement>();

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        infoElementPrefab = (UI_InfoElement)Resources.Load<UI_InfoElement>("HUD_Prefabs/UI_InfoElement");
    }

    // Start is called before the first frame update
    void Start()
    {
        Show(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(bool isShowing)
    {
        if (canvas != null)
        {
            canvas.enabled = isShowing;
        }
    }

    public void NewSelection(List<Selectable> selection)
    {
        Show(true);

        foreach (Selectable selectable in selection)
        {
            UI_InfoElement infoElement = Instantiate(infoElementPrefab, multiBox.transform);
            infoElement.Set(selectable);
            elementList.Add(infoElement);

        }

    }

    public void ClearSelection()
    {
        Show(false);
        if (elementList.Count > 0)
        {
            foreach (UI_InfoElement element in elementList)
            {
                Destroy(element.gameObject);
            }
            elementList.Clear();
        }
    }

    public void UpdateHealthBar(Selectable selectable)
    {
        if (elementList.Count > 0)
        {
            foreach (UI_InfoElement element in elementList)
            {
                if (selectable == element.Selectable)
                {
                    element.UpdateHealthBar();
                }
            }
        }
    }
}
