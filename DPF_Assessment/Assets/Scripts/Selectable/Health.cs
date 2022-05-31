// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Tooltip("The maximum and starting health of this object.")]
    [SerializeField] private float _health = 100;
    [SerializeField] private float _currentHealth;
    private bool _isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = _health;
    }

    public void TakeDamage(float _damageAmound)
    {
        if (!_isAlive) return;

        if (_currentHealth - _damageAmound <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        else
        {
            _currentHealth -= _damageAmound;
        }
    }

    public void Heal(float _healAmount)
    {
        if (!_isAlive) return;

        if ( _currentHealth + _healAmount >= _health)
        {
            _currentHealth = _health;
        }
        else
        {
            _currentHealth += _healAmount;
        }
    }

    private void Die()
    {
        _isAlive = false;
        Animator _animator = GetComponent<Animator>();
        if (_animator != null) _animator.SetTrigger("death");
        else
        {
            Building building = GetComponent<Building>();

            if (building != null)
            {
                building.SetBuildState(Building.EBuildState.Destroyed);

            }
        }
    }

    public float HealthPercentage()
    {
        return _currentHealth / _health;
    }

    public bool IsAlive() { return _isAlive; }

    public bool IsFull() { return _health == _currentHealth; }

    public void NewBuilding() { _currentHealth = 1; }
}
// Writen by Lukasz Dziedziczak