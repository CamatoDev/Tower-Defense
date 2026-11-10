using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float startSpeed = 10f;
    [HideInInspector]
    public float speed; 
    public float startHealth = 100f;
    private float health;
    public int gain = 500;

    public GameObject deathEffect;

    public Image healthbar;

    private bool isDead = false;

    // Référence au composant visuel pour le hit flash
    private EnemyVisuals visuals;

    public void Start()
    {
        speed = startSpeed;
        health = startHealth;
        // GetComponent retourne null si EnemyVisuals n'est pas attaché — pas de crash
        visuals = GetComponent<EnemyVisuals>();
    }

    public void TakeDammage(float amount)
    {
        health -= amount;

        healthbar.fillAmount = health / startHealth;

        // Feedback visuel : flash blanc bref sur l'impact
        if (visuals != null)
        {
            visuals.FlashHit();
        }

        if(health <= 0 && !isDead)
        {
            Die();
        }
    }

    public void slow(float amount)
    {
        speed = startSpeed * (1f - amount);
    }

    private void Die()
    {
        isDead = true;

        PlayerStats.money += gain;

        GameObject deathParticules = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(deathParticules, 2f);

        WaveSpawner.EnemiesAlive --;

        Destroy(gameObject);
    }
    
}
