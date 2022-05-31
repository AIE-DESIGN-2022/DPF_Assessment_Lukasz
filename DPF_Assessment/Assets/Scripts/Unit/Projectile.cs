// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _timeToLive = 5;

    private float _lifeTime = 0;
    private float _damage;
    private Selectable _owner;
    private bool _flyingInAir = true;
    private ParticleSystem fire;
    private ParticleSystem explosion;

    private void Awake()
    {
        FindParticleSystems();
    }

    private void FindParticleSystems()
    {
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in systems)
        {
            if (system.name == "FX_Fireworks_Yellow_Small") explosion = system;
            if (system.name == "FX_Fire") fire = system;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _lifeTime += Time.deltaTime;

        if (_lifeTime > _timeToLive)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (_flyingInAir) transform.position += transform.forward * Time.deltaTime * _speed;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!_flyingInAir) return;

        Selectable selectable = other.GetComponent<Selectable>();
        if (selectable != null && selectable != _owner && selectable.PlayerNumber() != _owner.PlayerNumber())
        {
            Hit(selectable);
        }


        /*Unit _unit = other.transform.GetComponent<Unit>();
        if (_unit != null && _unit != _owner && _unit.PlayerNumber() != _owner.PlayerNumber())
        {
            Hit(_unit);
        }
        else
        {
            Building _building = other.transform.GetComponent<Building>();
            if (_building != null)
            {
                Hit(_building);
            }
        }*/
    }

    private void Hit(Selectable _target)
    {
        _flyingInAir = false;
        _target.TakeDamage(_damage, _owner);
        //transform.parent = _target.transform;
        Destroy(gameObject, 3);

        if (fire != null && explosion != null)
        {
            fire.Stop();
            explosion.Play();
        }
    }

    public void Setup(Selectable _newOwner, float _newDamage)
    {
        _owner = _newOwner;
        _damage = _newDamage;
    }
}
// Writen by Lukasz Dziedziczak