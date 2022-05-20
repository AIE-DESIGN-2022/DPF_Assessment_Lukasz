using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Selectable
{
    public Movement _movement;

    private new void Awake()
    {
        base.Awake();
        _movement = GetComponent<Movement>();
    }


}
