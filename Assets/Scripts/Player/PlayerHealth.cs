using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public float iFrame = 0.5f;

    public Slider healthBar;

    private float currentHealth;
    private float invincibleTimer;

    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private float flikerTimer;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;

            flikerTimer -= Time.deltaTime;

            if (flikerTimer <= 0f)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }

                flikerTimer = 0.1f;
            }
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }
    }

    public void TakeDamage(float damage, Vector2 hitDirection)
    {
        if (invincibleTimer > 0f)
        {
            return;
        }

        currentHealth -= damage;
        invincibleTimer = iFrame;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (playerMovement != null)
        {
            playerMovement.ApplyKnockBack(hitDirection);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died");
        
        if (GameEndUI.Instance != null)
        {
            GameEndUI.Instance.ShowGameOver();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    public void setMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
}