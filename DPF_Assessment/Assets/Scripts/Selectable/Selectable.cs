using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    SpriteRenderer selectionIndicator;

    protected void Awake()
    {
        selectionIndicator = GetComponentInChildren<SpriteRenderer>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        Selected(false);

        if (selectionIndicator == null) Debug.Log("Selection Indicator Missing");
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    public void Selected(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.enabled = isSelected;
        }
        
    }
}
