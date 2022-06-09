using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Combat Multiplier")]
public class CombatMultiplier : ScriptableObject
{
    [SerializeField] private Combatant[] combatants;

    [System.Serializable]
    public class Combatant
    {
        public Unit.EUnitType unitType;
        public float buildingAttackMultiplier = 1;
        public List<Oponent> oponents;
    }

    [System.Serializable]
    public class Oponent
    {
        public Unit.EUnitType unitType;
        public float attackMultiplier = 1;
    }

    private float AttackMultiplier(Unit.EUnitType attacker, Unit.EUnitType attackie)
    {
        float attackMultiplier = 1;

        foreach (Combatant combatant in combatants)
        {
            if (combatant.unitType == attacker)
            {
                foreach (Oponent oponent in combatant.oponents)
                {
                    if (oponent.unitType == attackie)
                    {
                        attackMultiplier *= oponent.attackMultiplier;
                    }
                }
            }
        }


        return attackMultiplier;
    }

    private float AttackMultiplier(Unit.EUnitType attacker, Building.EBuildingType attackie)
    {
        float attackMultiplier = 1;

        foreach (Combatant combatant in combatants)
        {
            if (combatant.unitType == attacker)
            {
                attackMultiplier = combatant.buildingAttackMultiplier;
            }
        }


        return attackMultiplier;
    }

    private float AttackMultiplier(Building.EBuildingType attacker, Unit.EUnitType attackie)
    {
        return 1;
    }

    private float AttackMultiplier(Building.EBuildingType attacker, Building.EBuildingType attackie)
    {
        return 1;
    }

    public float GetMultiplier(Selectable attacker, Selectable target)
    {
        Unit attackerUnit = attacker.GetComponent<Unit>();
        Building attackerBuilding = attacker.GetComponent<Building>();
        Unit targetUnit = target.GetComponent<Unit>();
        Building targetBuilding = target.GetComponent<Building>();

        if (attackerUnit != null)
        {
            if (targetUnit != null)
            {
                return AttackMultiplier(attackerUnit.UnitType(), targetUnit.UnitType());
            }
            else if (targetBuilding != null)
            {
                return AttackMultiplier(attackerUnit.UnitType(), targetBuilding.BuildingType());
            }
            else
            {
                Debug.LogError(name + "unknown multiplier combination");
                return 0;
            }
        }
        else if (attackerBuilding != null)
        {
            if (targetUnit != null)
            {
                return AttackMultiplier(attackerBuilding.BuildingType(), targetUnit.UnitType());
            }
            else if (targetBuilding != null)
            {
                return AttackMultiplier(attackerBuilding.BuildingType(), targetBuilding.BuildingType());
            }
            else
            {
                Debug.LogError(name + "unknown multiplier combination");
                return 0;
            }
        }
        else
        {
            Debug.LogError(name + "unknown multiplier combination");
            return 0;
        }
    }
}
