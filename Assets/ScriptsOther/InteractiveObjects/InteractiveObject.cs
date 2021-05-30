using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon;

public class InteractiveObject : MonoBehaviour
{
    public InteractiveObjPlaceType intObjPlaceType;
    public InteractiveObjType intObjType;
    public int maxNum = 0;
    public float probability;

    public Vector2Int size;
    public int space = 0; //Отсуп от других объектов при расстановке
}
