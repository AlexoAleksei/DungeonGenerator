using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dungeon;

public class StructurePlacer : MonoBehaviour
{
    [SerializeField]
    List<GameObject> structures;

    private V2LURD v2lurd;
    private V3LURD v3lurd;

    [SerializeField]
    GameObject cubePrefab;
    [SerializeField]
    Material blueMaterial;
    [SerializeField]
    Material greenMaterial;
    [SerializeField]
    Material redMaterial;
    [SerializeField]
    Material yellowMaterial;
    [SerializeField]
    Material violetMaterial;


    public void PlaceStructures(List<Room> rooms, List<GameObject> roomObj, List<Vector2Int> hallways)
    {
        v2lurd = new V2LURD();
        v3lurd = new V3LURD();

        PlaceDoors(rooms);
        PlaceRooms(rooms);
        PlaceHallways(hallways);
    }
    
    void PlaceHallways(List<Vector2Int> hallways)
    {
        foreach (var hallway in hallways)
        {
            PlaceCube(hallway, new Vector2Int(1, 1), blueMaterial);
        }
    }

    void PlaceDoors(List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            /*Debug.Log("Room Type ");
            Debug.Log(room.Type);
            Debug.Log("Doors count ");
            Debug.Log(room.doors.Count);*/
            room.RemoveDoorsRepeats();
            //Debug.Log("Doors count after");
            //Debug.Log(room.doors.Count);
            foreach (var door in room.doors)
            {
                /*GameObject go = Instantiate(cubePrefab, new Vector3(door.location.x, 0.5f,
                                                door.location.y), Quaternion.identity);
                go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                go.GetComponent<MeshRenderer>().material = greenMaterial;*/
                if (door.side == Vector2Int.left)
                {
                    //PlaceCube(door.location, new Vector2Int(1, 1), greenMaterial);
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x, 0.5f,
                                                door.location.y + 1.0f), Quaternion.Euler(v3lurd.directs[0]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
                else if (door.side == Vector2Int.up)
                {
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x + 1.0f, 0.5f,
                                                door.location.y + 1.0f), Quaternion.Euler(v3lurd.directs[1]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
                else if (door.side == Vector2Int.right)
                {
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x + 1.0f, 0.5f,
                                                door.location.y), Quaternion.Euler(v3lurd.directs[2]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
                else if (door.side == Vector2Int.down)
                {
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x, 0.5f,
                                                door.location.y), Quaternion.Euler(v3lurd.directs[3]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
            }
        }
    }

    void PlaceRooms(List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            switch (room.Type)
            {
                case RoomType.Entrance:
                    PlaceCube(room.bounds.position, room.bounds.size, greenMaterial);
                    break;
                case RoomType.Exit:
                    PlaceCube(room.bounds.position, room.bounds.size, violetMaterial);
                    break;
                case RoomType.Battle:
                    PlaceCube(room.bounds.position, room.bounds.size, redMaterial);
                    break;
                case RoomType.Decorative:
                    PlaceCube(room.bounds.position, room.bounds.size, yellowMaterial);
                    break;
                default:
                    PlaceCube(room.bounds.position, room.bounds.size, blueMaterial);
                    break;
            }
        }
    }

    void PlaceCube(Vector2Int location, Vector2Int size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }
}
