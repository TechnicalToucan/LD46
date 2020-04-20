﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public delegate void HealthEvent(HealthComponent healthComponent, float health, float previousHealth);

    public List<ParticleSystem> ParticlesToStopOnDead = new List<ParticleSystem>();

    public bool DebugKill;

    public float maxHealth = 100;
    public float currentHealth = 100;

    [Range(0, 50)]
    public float HealthSubtractAmountPerInterval = 1;

    public float HealthSubtractInterval = 1;

    private float healthTimer;

    public event HealthEvent OnHealthDepleted;
    public event HealthEvent OnHealthRestored;
    public event HealthEvent OnHealthChanged;

    public bool MoveOffscreenOnDeath;

    public GameObject SpawnOnDeath = null;

    public void Offset(float offset, bool backupCall = false)
    {
        float previousHealth = currentHealth;

        currentHealth += offset;

        if (currentHealth <= 0)
        {
            currentHealth = 0.0f;            
        }

        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;            
        }

        if (currentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(this, currentHealth, previousHealth);
        }

        if (currentHealth <= 0 && (previousHealth > 0.0f || backupCall))
        {
            OnHealthDepleted?.Invoke(this, currentHealth, previousHealth);

            foreach (var ptfx in ParticlesToStopOnDead)
            {
                ptfx.Pause(true);
            }

            if (SpawnOnDeath)
            {
                GameObject.Instantiate(SpawnOnDeath, transform);
                SpawnOnDeath = null;
            }

            if (MoveOffscreenOnDeath)
            {
                var grid = GetComponent<GridActor>();
                var ptfx = GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in ptfx)
                {
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.Local;
                }


                if (grid)
                {
                    grid.TargetPosition = new Vector2Int(grid.TargetPosition.x, -4);
                    grid.MoveSpeed = 4;
                    grid.LockTargetPosition = true;
                    grid.SelfDestroyOnTargetReached = true;
                    grid.UseLinearMovementSpeed = true;
                }
            }
        }

        if (currentHealth == maxHealth && previousHealth < maxHealth)
        {
            OnHealthRestored?.Invoke(this, currentHealth, previousHealth);
        }
    }

    void Update()
    {
        if (HealthSubtractInterval > 0)
        {
            healthTimer += Time.deltaTime;

            if (healthTimer >= HealthSubtractInterval)
            {
                Offset(-HealthSubtractAmountPerInterval);
                healthTimer = 0;
            }
        }


        // Backup
        if (currentHealth <= 0 || DebugKill)
        {
            DebugKill = false;
            Offset(-100, true);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Obstacle")
        {
            var obstacleComp = other.GetComponent<ObstaclePicker>();
            if (obstacleComp)
            {
                Offset(-obstacleComp.GetCollisionDamage);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        
    }
}
