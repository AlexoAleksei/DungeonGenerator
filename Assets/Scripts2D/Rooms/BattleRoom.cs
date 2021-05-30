using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon;

public class BattleRoom : MonoBehaviour
{
    public RoomSubType subType;

    public List<Enemy> guards; // Ссылки на компоненты "Enemy" всех дочерних объектов с InteractiveObjType = Guard
    public List<Chest> stashs; // Ссылки на все дочерние объекты с InteractiveObjType = Stash
    public List<GameObject> other;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach (var guard in guards)
            {
                guard.enabled = true;
                guard.SetTarget(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (var guard in guards)
            {
                guard.SwitchFightingState(false);
            }
        }
    }

    public void DeleteEnemy(int indexInGuardList)
    {
        if (guards[indexInGuardList] != null)
        {
            guards[indexInGuardList].gameObject.SetActive(false);

            bool isRoomClear = true;

            foreach (var guard in guards)
            {
                if (guard.isActiveAndEnabled)
                {
                    isRoomClear = false;
                    break;
                }
            }

            if (isRoomClear)
            {
                foreach (var stash in stashs)
                {
                    stash.isAvailable = true;
                }
            }
        }
    }
}
