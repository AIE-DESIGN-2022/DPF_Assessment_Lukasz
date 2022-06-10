using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveGetToLocation : Objective
{
    // Start is called before the first frame update
    private new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated) return;

        Selectable selectable = other.GetComponent<Selectable>();
        if (selectable != null && selectable.PlayerNumber() == gameController.GetPlayerFaction().PlayerNumber())
        {
            ObjectiveComplete();
        }
    }
}
