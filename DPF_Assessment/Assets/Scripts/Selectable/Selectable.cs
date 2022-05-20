using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    SpriteRenderer selectionIndicator;

    private void Awake()
    {
        selectionIndicator = GetComponentInChildren<SpriteRenderer>();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        Selected(false);
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    public void Selected(bool isSelected)
    {
        selectionIndicator.enabled = isSelected;
    }
}
