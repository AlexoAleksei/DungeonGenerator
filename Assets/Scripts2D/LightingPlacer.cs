using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Dungeon;

public class LightingPlacer : MonoBehaviour
{
    [SerializeField]
    GameObject LightPrefab;

    [SerializeField]
    float rarity; // Число которое влияет на частоту появления источников света 0.0f - 1.0f
                  // При rarity == 0 в каждой секции будет только одна лампа
    [SerializeField]
    float darknessProbability; // Вероятность того, что секция коридора не будет освещена

    Random random;

    public void PlaceLighting(List<Room> rooms, List<HallwaySection> hallwaySections)
    {
        random = new Random();

        PlaceRoomsLighting(rooms);
        PlaceHallwaysLighting(hallwaySections);
    }

    void PlaceRoomsLighting(List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            if (room.bounds.size.x < 4 && room.bounds.size.y < 4)
            { // В комнате где обе стены < 4 лампа ровно по центру
                GameObject go = Instantiate(LightPrefab, new Vector3(room.bounds.position.x + room.bounds.size.x / 2.0f, 0.8f, 
                                                                    room.bounds.position.y + room.bounds.size.y / 2.0f),
                                                                    Quaternion.Euler(new Vector3(90, 0, 0)), room.roomObj.transform);
            }
            else
            {
                for(int i = 1; i < room.bounds.size.x; i++)
                {
                    for (int j = 1; j < room.bounds.size.y; j++)
                    {
                        if (i % 2 == 0 && j % 2 != 0)
                        { // Каждая четная i и нечетная j
                            GameObject go = Instantiate(LightPrefab, new Vector3(room.bounds.position.x + i, 0.8f, room.bounds.position.y + j),
                                           Quaternion.Euler(new Vector3(90, 0, 0)), room.roomObj.transform);
                            //go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                            //go.GetComponent<Transform>().localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                        }
                    }
                }
            }
        }
    }

    void PlaceHallwaysLighting(List<HallwaySection> hallwaySections)
    {
        foreach (var hallwaySection in hallwaySections)
        {
            int lightNum = (int)(hallwaySection.hallwayList.Count * rarity);
            if (lightNum <= 0 || darknessProbability >= 1.0f)
            {
                if (darknessProbability <= (float)random.Next(0, 101) / 100.0f)
                {
                    lightNum = 1;
                }
                else
                { //В данной секции коридора не будет света
                    continue;
                }
            }
            int distance = (int)(hallwaySection.hallwayList.Count / lightNum);

            List<int> hallwayIndex = new List<int>();
            int lowerIndex = 0;
            int topIndex = distance; //Его мы не влючаем при выборе коридора

            for (int i = 0; i < lightNum; i++)
            {
                hallwayIndex.Add(random.Next(lowerIndex, topIndex));
                lowerIndex = topIndex;
                topIndex += distance;
            }

            foreach(var index in hallwayIndex)
            {
                if (hallwaySection.hallwayList[index].isLighted)
                {
                    continue;
                }
                Transform hallwayTransform = hallwaySection.hallwayList[index].hallwayObj.transform;
                GameObject go = Instantiate(LightPrefab, new Vector3(hallwayTransform.position.x, 0.8f, hallwayTransform.position.z),
                                            Quaternion.Euler(new Vector3(90, 0, 0)), hallwayTransform);
                //go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            }

            /*foreach (var hallway in hallwaySection.hallwayList)
            {
                Transform hallwayTransform = hallway.hallwayObj.transform;
                GameObject go = Instantiate(LightPrefab, new Vector3(hallwayTransform.position.x, 0.0f, hallwayTransform.position.z),
                                            Quaternion.identity, hallwayTransform);
                //go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            }*/
        }
    }
}
