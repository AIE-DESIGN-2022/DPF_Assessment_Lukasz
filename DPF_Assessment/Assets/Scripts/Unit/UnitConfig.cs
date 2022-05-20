using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="RTS/Unit")]
public class UnitConfig : ScriptableObject
{
    [SerializeField] Unit unitPrefab;
}
