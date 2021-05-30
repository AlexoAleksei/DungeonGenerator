using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon;

public class EntranceRoomSub : MonoBehaviour
{
    public RoomSubType subType;

    public float maxPercentage;
    public Vector2Int minRoomSize;
    public Vector2Int maxRoomSize;

    public List<GameObject> interactiveObjects;
    public List<GameObject> obstacles;
}
