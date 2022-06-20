using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarController : MonoBehaviour
{
    private FogOfWar fogOfWar;
    private GameController gameController;
    private Faction playerFaction;

    private float taskTimer = Mathf.Infinity;

    private void Awake()
    {
        fogOfWar = FindObjectOfType<FogOfWar>();
        gameController = FindObjectOfType<GameController>();
    }


    // Start is called before the first frame update
    void Start()
    {
        //SetFogHeight(12);
    }

    // Update is called once per frame
    void Update()
    {
        if (fogOfWar == null) return;
        taskTimer += Time.deltaTime;

        if (fogOfWar.IsReady && taskTimer > fogOfWar.LastTaskDuration * 1.1f)
        {
            taskTimer = 0;
            fogOfWar.ProcessVisibilityAsync(playerFaction.Selectables());
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
        if (fogOfWar != null) fogOfWar.SetFogHeight(newHeight);
    }
}
