using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSight : MonoBehaviour
{
    public float checkDistance = 5; // в приделах этой дистанции дверь будет доступна
    public string doorTag = "Door"; // тег двери
    public string enemyTag = "Enemy"; // тег противника
    public string stashTag = "Stash";
    public KeyCode key = KeyCode.F; // клавиша действия
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out hit, checkDistance))
            {
                //Debug.Log(hit.collider.name);
                if (hit.collider.tag == doorTag)
                {
                    hit.transform.GetComponent<DoorController>().enabled = true;
                    hit.transform.GetComponent<DoorController>().Invert(transform);
                }
                else if (hit.collider.tag == stashTag)
                {
                    hit.transform.GetComponent<Chest>().enabled = true;
                    hit.transform.GetComponent<Chest>().GetReward(gameObject.GetComponentInParent<PlayerController>());
                }
            }
        }
    }

    public RaycastHit CheckPlayerAttack(float hitDistance, float swordDamage)
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out hit, hitDistance))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.tag == enemyTag)
            {
                hit.transform.GetComponent<Enemy>().GetDamage(swordDamage);
            }
        }
        return hit;
    }
}
