using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon;

public class DecorativeObject : MonoBehaviour
{
    public InteractiveObjPlaceType intObjPlaceType;
    public int maxNum = 0;
    public float probability;

    public Vector2Int size;
    public int space = 0; //Отсуп от других объектов при расстановке
}
