using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager instance;

    public int skillPoints = 100;

    public TMP_Text pointsText;
    public Character player;
    
    public Button damage1, damage2, damage3, damage4, damage5;
    public Button speed1, speed2, speed3, speed4, speed5;
    public Button health1, health2, health3, health4, health5;

    private bool d1, d2, d3, d4, d5;
    private bool s1, s2, s3, s4, s5;
    private bool h1, h2, h3, h4, h5;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
        RefreshButtons();
    }

    void UpdateUI()
    {
        if (pointsText != null)
            pointsText.text = "Points: " + skillPoints;
    }

    void EnsurePlayer()
    {
        if (player == null)
        {
            player = FindObjectOfType<Character>();
            ApplyAllUpgrades();
        }
    }

    public void AddPoints(int amount)
    {
        skillPoints += amount;
        UpdateUI();
    }

    void RefreshButtons()
    {
        SetButtonState(damage1, !d1);
        SetButtonState(damage2, d1 && !d2);
        SetButtonState(damage3, d2 && !d3);
        SetButtonState(damage4, d3 && !d4);
        SetButtonState(damage5, d4 && !d5);

        SetButtonState(speed1, !s1);
        SetButtonState(speed2, s1 && !s2);
        SetButtonState(speed3, s2 && !s3);
        SetButtonState(speed4, s3 && !s4);
        SetButtonState(speed5, s4 && !s5);

        SetButtonState(health1, !h1);
        SetButtonState(health2, h1 && !h2);
        SetButtonState(health3, h2 && !h3);
        SetButtonState(health4, h3 && !h4);
        SetButtonState(health5, h4 && !h5);
    }

    void SetButtonState(Button button, bool state)
    {
        if (button != null)
            button.interactable = state;
    }

    void ApplyAllUpgrades()
    {
        if (player == null) return;

        if (player is IAttacker attacker)
        {
            if (d1) attacker.ModifyAttackDamage(5);
            if (d2) attacker.ModifyAttackDamage(8);
            if (d3) attacker.ModifyAttackDamage(12);
            if (d4) attacker.ModifyAttackDamage(16);
            if (d5) attacker.ModifyAttackDamage(25);
        }

        if (s1) player.speed += 0.3f;
        if (s2) player.speed += 0.4f;
        if (s3) player.speed += 0.5f;
        if (s4) player.speed += 0.6f;
        if (s5) player.speed += 0.7f;

        if (h1) player.health += 20;
        if (h2) player.health += 30;
        if (h3) player.health += 40;
        if (h4) player.health += 50;
        if (h5) player.health += 75;
    }

    public void Damage1()
    {
        Buy(ref d1, 10);
    }

    public void Damage2()
    {
        Buy(ref d2, 30);
    }

    public void Damage3()
    {
        Buy(ref d3, 50);
    }

    public void Damage4()
    {
        Buy(ref d4, 70);
    }

    public void Damage5()
    {
        Buy(ref d5, 100);
    }

    public void Speed1()
    {
        Buy(ref s1, 10);
    }

    public void Speed2()
    {
        Buy(ref s2, 30);
    }

    public void Speed3()
    {
        Buy(ref s3, 50);
    }

    public void Speed4()
    {
        Buy(ref s4, 70);
    }

    public void Speed5()
    {
        Buy(ref s5, 100);
    }

    public void Health1()
    {
        Buy(ref h1, 10);
    }

    public void Health2()
    {
        Buy(ref h2, 30);
    }

    public void Health3()
    {
        Buy(ref h3, 50);
    }

    public void Health4()
    {
        Buy(ref h4, 70);
    }

    public void Health5()
    {
        Buy(ref h5, 100);
    }

    void Buy(ref bool flag, int cost)
    {
        EnsurePlayer();

        if (flag) return;
        if (skillPoints < cost) return;

        skillPoints -= cost;
        flag = true;

        ApplyAllUpgrades();
        RefreshButtons();
        UpdateUI();
    }
}