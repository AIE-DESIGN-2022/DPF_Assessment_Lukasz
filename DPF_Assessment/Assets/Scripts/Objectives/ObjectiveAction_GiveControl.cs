using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction_GiveControl : ObjectiveAction
{ 
    [SerializeField] List<Selectable> list = new List<Selectable>();

    public override void DoAction()
    {
        base.DoAction();
        TurnControl();
    }

    private void TurnControl()
    {

        foreach (Selectable selectable in list)
        {
            Faction oldFaction = selectable.Faction();

            gameController.GetPlayerFaction().TransferOwnership(selectable);
            selectable.FlashUntillSelected();
        }

        gameController.CameraController().PanCameraTo(list[0].transform.position);
    }
}
