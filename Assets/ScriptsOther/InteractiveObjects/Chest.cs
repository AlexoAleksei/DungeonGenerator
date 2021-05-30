using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon;

public class Chest : MonoBehaviour
{
    public int cost;

    public bool isAvailable;

    int pointsAmount;
    int healingPotionsAmount;

    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        isAvailable = false;

        pointsAmount = cost;
        healingPotionsAmount = cost / 5;
        if (healingPotionsAmount < 1)
        {
            healingPotionsAmount = 1;
        }
        enabled = false;
    }

    void Update()
    {
        
    }

    public void GetReward(PlayerController playerController)
    {
        if (isAvailable)
        {
            if (animator)
            {
                animator.Play("ChestOpen");
            }
            playerController.AddReward(new Reward(pointsAmount, healingPotionsAmount));

            isAvailable = false;
            enabled = false;
        }
        enabled = false;
    }
}
