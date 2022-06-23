// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float timeToLive = 5;

    private float lifeTime = 0;
    private float damage;
    private Selectable owner;
    private bool flyingInAir = true;
    private ParticleSystem fire;
    private ParticleSystem explosion;
    private CombatMultiplier combatMultiplier;

    private void Awake()
    {
        FindParticleSystems();
        combatMultiplier = (CombatMultiplier)Resources.Load<CombatMultiplier>("CombatMultiplier");
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
        if (combatMultiplier == null) Debug.LogError(name + " cannot find CombatMultiplier");
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime += Time.deltaTime;

        if (lifeTime > timeToLive)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (flyingInAir) transform.position += transform.forward * Time.deltaTime * speed;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!flyingInAir) return;

        Selectable selectable = other.GetComponent<Selectable>();
        if (selectable != null && selectable.IsAlive() && selectable != owner && selectable.PlayerNumber() != owner.PlayerNumber())
        {
            Hit(selectable);
        }
    }

    private void Hit(Selectable target)
    {
        if (target == null) return;
        flyingInAir = false;
        float damageMultiplier = combatMultiplier.GetMultiplier(owner, target);
        //print(name + " giving damage=" + damage + " multiplier=" + damageMultiplier + " total=" + damage * damageMultiplier);
        target.TakeDamage(damage * damageMultiplier, owner);
        transform.parent = target.transform;
        Destroy(gameObject, 3);

        if (fire != null && explosion != null)
        {
            fire.Stop();
            explosion.Play();
        }
    }

    public void Setup(Selectable newOwner, float newDamage)
    {
        owner = newOwner;
        damage = newDamage;
    }
}
// Writen by Lukasz Dziedziczak