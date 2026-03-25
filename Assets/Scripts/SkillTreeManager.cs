using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    public int skillPoints = 100;

    public TMP_Text pointsText;
    public Character player;
    
    public Button damage1, damage2, damage3;
    public Button speed1, speed2, speed3;
    public Button health1, health2, health3;

    void Start()
    {
        UpdateUI();
        
        SetButtonState(damage2, false);
        SetButtonState(damage3, false);

        SetButtonState(speed2, false);
        SetButtonState(speed3, false);

        SetButtonState(health2, false);
        SetButtonState(health3, false);
    }

    void UpdateUI()
    {
        pointsText.text = "Points: " + skillPoints;
    }
    
    void SetButtonState(Button button, bool state)
    {
        if (button != null)
        {
            button.interactable = state;
        }
        else
        {
            Debug.LogWarning("Button reference missing!");
        }
    }

    public void Damage1()
    {
        Debug.Log("Damage1 clicked");

        if (skillPoints < 10) return;

        skillPoints -= 10;
        if (player is IAttacker attacker)
        {
            attacker.ModifyAttackDamage(5);
        }

        SetButtonState(damage1, false);
        SetButtonState(damage2, true);

        UpdateUI();
    }

    public void Damage2()
    {
        Debug.Log("Damage2 clicked");

        if (skillPoints < 30) return;

        skillPoints -= 30;
        if (player is IAttacker attacker)
        {
            attacker.ModifyAttackDamage(10);
        }

        SetButtonState(damage2, false);
        SetButtonState(damage3, true);

        UpdateUI();
    }

    public void Damage3()
    {
        Debug.Log("Damage3 clicked");

        if (skillPoints < 50) return;

        skillPoints -= 50;
        if (player is IAttacker attacker)
        {
            attacker.ModifyAttackDamage(20);
        }

        SetButtonState(damage3, false);

        UpdateUI();
    }

    public void Speed1()
    {
        Debug.Log("Speed1 clicked");

        if (skillPoints < 10) return;

        skillPoints -= 10;
        player.speed += 1f;

        SetButtonState(speed1, false);
        SetButtonState(speed2, true);

        UpdateUI();
    }

    public void Speed2()
    {
        Debug.Log("Speed2 clicked");

        if (skillPoints < 30) return;

        skillPoints -= 30;
        player.speed += 2f;

        SetButtonState(speed2, false);
        SetButtonState(speed3, true);

        UpdateUI();
    }

    public void Speed3()
    {
        Debug.Log("Speed3 clicked");

        if (skillPoints < 50) return;

        skillPoints -= 50;
        player.speed += 3f;

        SetButtonState(speed3, false);

        UpdateUI();
    }

    public void Health1()
    {
        Debug.Log("Health1 clicked");

        if (skillPoints < 10) return;

        skillPoints -= 10;
        player.health += 20;

        SetButtonState(health1, false);
        SetButtonState(health2, true);

        UpdateUI();
    }

    public void Health2()
    {
        Debug.Log("Health2 clicked");

        if (skillPoints < 30) return;

        skillPoints -= 30;
        player.health += 40;

        SetButtonState(health2, false);
        SetButtonState(health3, true);

        UpdateUI();
    }

    public void Health3()
    {
        Debug.Log("Health3 clicked");

        if (skillPoints < 50) return;

        skillPoints -= 50;
        player.health += 80;

        SetButtonState(health3, false);

        UpdateUI();
    }
}
