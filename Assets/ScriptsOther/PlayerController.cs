using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dungeon;

public class PlayerController : MonoBehaviour
{
    public float speed = 1.0f;
    public float jumpspeed = 2.0f;
    public float gravity = 9.81f;
    public int pointsAmount = 10;
    public float healthPoints;
    public float maxHealthPoints;
    public float swordDamage;
    public float hitDistance = 0.5f; // в приделах этой дистанции противника можно поражать
    public int healthPotionAmount = 0;
    public float healthPotionHeal = 0;

    private bool isFrozen = false;

    private Vector3 moveDir = Vector3.zero;
    public CharacterController controller;
    private CameraController cameraControllerX;
    private CameraController cameraControllerY;

    public KeyCode potionKey = KeyCode.R; //клавиша зелья
    public KeyCode characterKey = KeyCode.T; // клавиша окна персонажа

    Animator animator;
    PlayerSight playerSight;
    PlayerHeathBar healthBar;
    public GameObject canvas;
    Transform characterWindow;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        //controller = GetComponent<CharacterController>();
        cameraControllerX = GetComponent<CameraController>();
        cameraControllerY = transform.Find("Main Camera").GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        playerSight = transform.Find("Main Camera").GetComponent<PlayerSight>();
        healthBar = GetComponent<PlayerHeathBar>();
        healthBar.ChangePotionAmount(healthPotionAmount);
        canvas = GameObject.Find("Canvas");
        characterWindow = canvas.transform.Find("CharacterWindow");

        UpdateCharacterWindow();
        characterWindow.gameObject.SetActive(false);

        isFrozen = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && controller.isGrounded && !isFrozen)
        { //Left Mouse Button
            animator.Play("PlayerSwordAttack");
        }
        if (Input.GetKeyDown(KeyCode.R) && controller.isGrounded && !isFrozen)
        {
            if (healthPotionAmount > 0 && healthPoints < maxHealthPoints)
            {
                healthPoints += healthPotionHeal;
                healthPotionAmount -= 1;
                if (healthPoints >= maxHealthPoints)
                {
                    healthPoints = maxHealthPoints;
                }
                healthBar.ChangeHealth(healthPoints, maxHealthPoints);
                healthBar.ChangePotionAmount(healthPotionAmount);
            }
        }
        else if (Input.GetKeyDown(characterKey) && controller.isGrounded)
        {
            if (!characterWindow.gameObject.activeSelf)
            {
                isFrozen = true;

                cameraControllerX.enabled = false;
                cameraControllerY.enabled = false;

                characterWindow.gameObject.SetActive(true);
                UpdateCharacterWindow();

                Cursor.lockState = CursorLockMode.None;
            }  
            else
            {
                characterWindow.gameObject.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;

                cameraControllerX.enabled = true;
                cameraControllerY.enabled = true;

                isFrozen = false;
            }
        }

        healthBar.ChangeHealth(healthPoints, maxHealthPoints);
        healthBar.ChangePotionAmount(healthPotionAmount);
    }

    void FixedUpdate()
    {
        if(controller.isGrounded && !isFrozen)
        {
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDir = transform.TransformDirection(moveDir);
            moveDir *= speed;
        }
        if(Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && !isFrozen)
        {
            moveDir.y = jumpspeed;
        }
        moveDir.y -= gravity * Time.deltaTime;

        controller.Move(moveDir * Time.deltaTime);
    }

    void PlayerSwordAttack()
    {
        Debug.Log("Lexa Event");
        RaycastHit hit = playerSight.CheckPlayerAttack(hitDistance, swordDamage);
    }

    public void GetDamage(float damage)
    {
        healthPoints -= damage;
        healthBar.ChangeHealth(healthPoints, maxHealthPoints);
        if(healthPoints <= 0)
        {
            Debug.Log("Zdox!!!");
            gameObject.SetActive(false);
        }
    }

    public void AddReward(Reward reward)
    {
        pointsAmount += reward.pointsAmount;
        healthPotionAmount += reward.healingPotionsAmount;
    }

    public void UpdateCharacterWindow()
    {
        Debug.Log("Lexa");
        Debug.Log(characterWindow.name);
        characterWindow.Find("PointsValue").GetComponent<Text>().text = pointsAmount.ToString();
        characterWindow.Find("SwordDamageValue").GetComponent<Text>().text = swordDamage.ToString();
        characterWindow.Find("MaxHealthValue").GetComponent<Text>().text = maxHealthPoints.ToString();
        characterWindow.Find("MoveSpeedValue").GetComponent<Text>().text = speed.ToString();
        characterWindow.Find("PotionHealValue").GetComponent<Text>().text = healthPotionHeal.ToString();
    }

    public void IncreaseSwordDamage ()
    {
        Debug.Log(pointsAmount);
        Debug.Log("isFrozen" + isFrozen);
        if (pointsAmount > 0)
        {
            swordDamage += 5;
            pointsAmount -= 1;
            Debug.Log("SwordDamage");
            UpdateCharacterWindow();
        }
    }

    public void IncreaseMaxHeath()
    {
        if (pointsAmount > 0)
        {
            maxHealthPoints += 15;
            pointsAmount -= 1;
            Debug.Log("MaxHealth");
            UpdateCharacterWindow();
        }
    }

    public void IncreaseSpeed()
    {
        if (pointsAmount > 0)
        {
            speed += 0.05f;
            pointsAmount -= 1;
            Debug.Log("Speed");
            UpdateCharacterWindow();
        }
    }

    public void IncreaseHeal()
    {
        if (pointsAmount > 0)
        {
            healthPotionHeal += 10f;
            pointsAmount -= 1;
            Debug.Log("IncreaseHeal");
            UpdateCharacterWindow();
        }
    }
}
