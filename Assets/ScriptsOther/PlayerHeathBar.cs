using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeathBar : MonoBehaviour
{
    private PlayerController playerController;

    public Image bar;
    public float fill;

    public Text PotionAmount;
    public int potionAmount;

    void Start()
    {
        //bar = GameObject.Find("Canvas").transform.Find("HealthBar").transform.Find("bar").GetComponent<Image>();
        //PotionAmount = GameObject.Find("Canvas").transform.Find("Potions").transform.Find("PotionAmount").GetComponent<Text>();

        playerController = gameObject.GetComponent<PlayerController>();
        fill = 1.0f; // Full

        PotionAmount.text = playerController.healthPotionAmount.ToString();
        potionAmount = playerController.healthPotionAmount;
    }

    public void ChangeHealth(float health, float maxHealth)
    {
        fill = health / maxHealth;
        bar.fillAmount = fill;
    }

    public void ChangePotionAmount(int amount)
    {
        potionAmount = amount;
        PotionAmount.text = amount.ToString();
    }
}
