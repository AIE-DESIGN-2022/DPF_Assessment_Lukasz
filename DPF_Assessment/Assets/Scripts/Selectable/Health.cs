// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Tooltip("The maximum and starting health of this object.")]
    [SerializeField] private float health = 100;
    [SerializeField] private float currentHealth;
    private bool isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = health;
    }

    public void TakeDamage(float damageAmound)
    {
        if (!isAlive) return;

        if (currentHealth - damageAmound <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            currentHealth -= damageAmound;
        }
    }

    public void Heal(float healAmount)
    {
        if (!isAlive) return;

        if ( currentHealth + healAmount >= health)
        {
            currentHealth = health;
        }
        else
        {
            currentHealth += healAmount;
        }
    }

    private void Die()
    {
        isAlive = false;
        Unit unit = GetComponent<Unit>();
        if (unit != null) unit.ClearPreviousActions();

        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.SetTrigger("death");
        else
        {
            Building building = GetComponent<Building>();

            if (building != null)
            {
                building.SetBuildState(Building.EBuildState.Destroyed);

            }
        }


        if (GetComponent<Selectable>().IsSelected())
        {
            FindObjectOfType<PlayerController>().SelectedUnitDied(GetComponent<Selectable>());
        }
    }

    public float HealthPercentage()
    {
        return currentHealth / health;
    }

    public bool IsAlive() { return isAlive; }

    public bool IsFull() { return health == currentHealth; }

    public void NewBuilding() { currentHealth = 1; }

    public void Kill()
    {
        TakeDamage(currentHealth);
    }
}
// Writen by Lukasz Dziedziczak