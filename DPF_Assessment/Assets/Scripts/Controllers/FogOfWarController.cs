using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarController : MonoBehaviour
{
    private FogOfWar fogOfWar;
    private GameController gameController;
    private Faction playerFaction;

    private void Awake()
    {
        fogOfWar = FindObjectOfType<FogOfWar>();
        gameController = FindObjectOfType<GameController>();
    }


    // Start is called before the first frame update
    void Start()
    {
        SetFogHeight(12);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateSelectableVisibility();

        if (playerFaction != null && fogOfWar != null)
        {
            fogOfWar.ProcessVisibility(playerFaction.Selectables());
        }
    }

    private void UpdateSelectableVisibility()
    {
        foreach (Selectable selectable in gameController.AllSelectablesButPlayers())
        {
            foreach (Selectable playersSelectable in gameController.GetPlayerFaction().Selectables())
            {
                float distance = Vector3.Distance(playersSelectable.transform.position, selectable.transform.position);
                if (distance < playersSelectable.SightDistance())
                {
                    selectable.Visable(true);
                }
                else
                {
                    selectable.Visable(false);
                }
            }
        }
    }

    public void SetPlayerFaction(Faction newFaction)
    {
        playerFaction = newFaction;
    }

    public void SetFogHeight(float newHeight)
    {
        fogOfWar.SetFogHeight(newHeight);
    }
}
