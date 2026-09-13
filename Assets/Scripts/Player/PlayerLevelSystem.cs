using UnityEngine;
using System;

public class PlayerLevelSystem : MonoBehaviour
{
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel;

    public static event Action OnLevelUp;

    private void Start()
    {
        CalculateExpRequirement();
    }

    public void AddExp(int amount)
    {
        currentExp += amount;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            CalculateExpRequirement();

            Debug.Log($"Leveled Up! Current Level: {currentLevel}");
            OnLevelUp?.Invoke();
        }
    }

    private void CalculateExpRequirement()
    {
        // Smooth Level Curve: Exponential under Level 10, linear curve late game
        if (currentLevel <= 10)
        {
            expToNextLevel = Mathf.RoundToInt(10f * Mathf.Pow(1.35f, currentLevel - 1));
        }
        else
        {
            // Level 10+ scales steadily by +75 EXP per level to prevent endless level popups
            expToNextLevel = 200 + ((currentLevel - 10) * 75);
        }
    }
}