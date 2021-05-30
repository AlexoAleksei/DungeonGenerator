using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int guardListIndex;
    public string playerTag = "Player"; // тег игрока

    public Vector3 defaultPosition;
    NavMeshAgent agent;
    Animator animator;

    [SerializeField]
    float healthPoints;
    [SerializeField]
    float swordDamage;
    [SerializeField]
    float checkHitDistance; // Дистанция проверки возможности удара 
    [SerializeField]
    float hitDistance; // Дистанция удара (больше чем дистанция проверки)
    [SerializeField]
    float hitAngle; // Угол удара

    float slowdownTimer;
    float speed;

    bool isFighting = false;
    [SerializeField]
    bool isMovable = false; // Does an enemy has a NavMeshAgent?
    Transform target;

    private void Awake()
    {
        defaultPosition = transform.position;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (isMovable)
        {
            slowdownTimer = 0.0f;
            speed = agent.speed;
        }

        isFighting = false;
        target = null;
        enabled = false;
    }

    void OnEnable()
    {
        isFighting = false;
        target = null;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 2, Color.red);
        Debug.DrawLine(target.transform.position, transform.position, Color.green);

        if (gameObject.activeSelf && healthPoints > 0)
        { 
            if (slowdownTimer <= 0 && isMovable)
            {
                slowdownTimer = 0.0f;
                agent.speed = speed;
            }
            else
            {
                slowdownTimer -= Time.deltaTime;
            }

            if (isFighting && target && isMovable)
            {
                //agent.isStopped = false;
                agent.SetDestination(target.position);
                if (CheckPlayerHitDistance())
                {
                    agent.isStopped = true;
                    animator.Play("EnemySwordAttack");
                }
            }
            else if (!isFighting && isMovable)
            {
                agent.isStopped = false;
                agent.SetDestination(defaultPosition);
            }
            else if (!isFighting && isMovable && Vector3.Distance(transform.position, defaultPosition) <= 0.15)
            { //При возвращении в исходную позицию enabled = false;
                agent.isStopped = true;
                enabled = false;
            }
            else if (!isFighting && !isMovable)
            {
                enabled = false;
            }
        }
    }

    public void SwitchFightingState (bool fightingState)
    {
        isFighting = fightingState;
    }

    public void SetTarget(Transform Target)
    {
        target = Target;
        isFighting = true;
    }

    /*public void EnemySwordAttack()
    {
        RaycastHit hit;
        //Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, hitDistance))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.tag == playerTag)
            {
                hit.transform.GetComponent<PlayerController>().GetDamage(swordDamage);
            }
        }
        agent.isStopped = false;
    }*/

    public void EnemySwordAttack()
    {
        Vector3 targetDirection = target.transform.position - transform.position;

        Debug.Log(Vector3.Angle(transform.forward, targetDirection));
        Debug.Log(Vector3.Distance(transform.position, target.transform.position));

        if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance &&
            Vector3.Angle(transform.forward, targetDirection) <= hitAngle)
        {
            target.transform.GetComponent<PlayerController>().GetDamage(swordDamage);
        }
        agent.isStopped = false;
    }

    public bool CheckPlayerHitDistance()
    { //Проверка достижимости игрока
        RaycastHit hit;
        //Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, checkHitDistance))
        {
            if (hit.collider.tag == playerTag)
            {
                return true;
            }
        }
        return false;
    }

    public void GetDamage(float damage)
    {
        /*Debug.Log("GetDamage");
        Debug.Log("health " + healthPoints);
        Debug.Log("speed " + agent.speed);*/
        healthPoints -= damage;
        agent.speed = agent.speed / 3;
        /*Debug.Log("///////////////");
        Debug.Log(healthPoints);
        Debug.Log(agent.speed);*/
        if (healthPoints <= 0)
        {
            GetComponentInParent<BattleRoom>().DeleteEnemy(guardListIndex);
        }
    }
}