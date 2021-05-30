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


    public void PlaceStructures(Grid2D<CellType> grid, Vector2Int fieldSize, List<Room> rooms, List<HallwaySection> hallwaySections)
    {
        v2lurd = new V2LURD();
        v3lurd = new V3LURD();

        PlaceDoors(rooms);
        PlaceRooms(grid, fieldSize, rooms);
        PlaceHallways(grid, fieldSize, hallwaySections);
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
                /*if (door.side == Vector2Int.left)
                {
                    //PlaceCube(door.location, new Vector2Int(1, 1), greenMaterial);
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x, 0.0f,
                                                door.location.y + 1.0f), Quaternion.Euler(v3lurd.directs[0]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
                else if (door.side == Vector2Int.up)
                {
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x + 1.0f, 0.0f,
                                                door.location.y + 1.0f), Quaternion.Euler(v3lurd.directs[1]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
                else if (door.side == Vector2Int.right)
                {
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x + 1.0f, 0.0f,
                                                door.location.y), Quaternion.Euler(v3lurd.directs[2]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }
                else if (door.side == Vector2Int.down)
                {
                    GameObject go = Instantiate(structures[3], new Vector3(door.location.x, 0.0f,
                                                door.location.y), Quaternion.Euler(v3lurd.directs[3]));
                    go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
                    go.GetComponent<MeshRenderer>().material = greenMaterial;
                }*/
                PlaceBorders(structures[3], door.location, door.side, room.roomObj); //Wall with Door
            }
        }
    }

    void PlaceRooms(Grid2D<CellType> grid, Vector2Int fieldSize, List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            foreach (var pos in room.bounds.allPositionsWithin) //Grid positions
            {
                PlaceFloor(pos, room.roomObj);
                PlaceCeiling(pos, room.roomObj);
                foreach (var side in v2lurd.sides)
                {
                    if ((pos + side).x < 0 || (pos + side).y < 0 ||
                        (pos + side).x > fieldSize.x || (pos + side).y > fieldSize.y)
                    {
                        PlaceBorders(structures[2], pos, side, room.roomObj); //Wall
                        continue;
                    }
                    switch (grid[pos + side])
                    {
                        case CellType.None:
                            PlaceBorders(structures[2], pos, side, room.roomObj); //Wall
                            break;
                        case CellType.Room:
                            break;
                        case CellType.Hallway:
                            if (!room.CheckDoors(pos, side))
                            {
                                PlaceBorders(structures[2], pos, side, room.roomObj); //Wall
                            }
                            break;
                    }
                }
            }
            /*switch (room.Type)
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
            }*/
        }
    }

    void PlaceHallways(Grid2D<CellType> grid, Vector2Int fieldSize, List<HallwaySection> hallwaySections)
    {
        foreach (var hallwaySection in hallwaySections)
        {
            foreach (var hallway in hallwaySection.hallwayList)
            {
                //PlaceCube(hallway, new Vector2Int(1, 1), blueMaterial);
                PlaceFloor(hallway.bounds.position, hallway.hallwayObj);
                PlaceCeiling(hallway.bounds.position, hallway.hallwayObj);
                foreach (var side in v2lurd.sides)
                {
                    if ((hallway.bounds.position + side).x < 0 || (hallway.bounds.position + side).y < 0 ||
                        (hallway.bounds.position + side).x >= fieldSize.x || (hallway.bounds.position + side).y >= fieldSize.y)
                    {
                        PlaceBorders(structures[4], hallway.bounds.position, side, hallway.hallwayObj);  //Hallway wall
                        continue;
                    }
                    switch (grid[hallway.bounds.position + side])
                    {
                        case CellType.None:
                            PlaceBorders(structures[4], hallway.bounds.position, side, hallway.hallwayObj); //Hallway wall
                            break;
                        case CellType.Room:
                            break;
                        case CellType.Hallway:
                            break;
                    }
                }
            }
        }
    }

    void PlaceFloor(Vector2Int location, GameObject parent)
    {
        GameObject go = Instantiate(structures[0], new Vector3(location.x, 0.0f, location.y), //Floor
                                    Quaternion.identity, parent.transform);
        go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        //go.GetComponent<MeshRenderer>().material = material;
    }

    void PlaceCeiling(Vector2Int location, GameObject parent)
    {
        GameObject go = Instantiate(structures[1], new Vector3(location.x, 0.0f, location.y), //Ceiling
                                    Quaternion.identity, parent.transform);
        go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        //go.GetComponent<MeshRenderer>().material = material;
    }

    /*void PlaceWall(Vector2Int location, Vector2Int side)
    {
        if (side == Vector2Int.left)
        {
            //PlaceCube(door.location, new Vector2Int(1, 1), greenMaterial);
            GameObject go = Instantiate(structures[2], new Vector3(location.x, 0.0f,
                                        location.y + 1.0f), Quaternion.Euler(v3lurd.directs[0]));
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
        else if (side == Vector2Int.up)
        {
            GameObject go = Instantiate(structures[2], new Vector3(location.x + 1.0f, 0.0f,
                                        location.y + 1.0f), Quaternion.Euler(v3lurd.directs[1]));
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
        else if (side == Vector2Int.right)
        {
            GameObject go = Instantiate(structures[2], new Vector3(location.x + 1.0f, 0.0f,
                                        location.y), Quaternion.Euler(v3lurd.directs[2]));
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
        else if (side == Vector2Int.down)
        {
            GameObject go = Instantiate(structures[2], new Vector3(location.x, 0.0f,
                                        location.y), Quaternion.Euler(v3lurd.directs[3]));
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
    }*/

    void PlaceBorders(GameObject prefab, Vector2Int location, Vector2Int side, GameObject parent)
    {
        if (side == Vector2Int.left)
        {
            //PlaceCube(door.location, new Vector2Int(1, 1), greenMaterial);
            GameObject go = Instantiate(prefab, new Vector3(location.x, 0.0f, location.y + 1.0f), 
                                        Quaternion.Euler(v3lurd.directs[0]), parent.transform);
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
        else if (side == Vector2Int.up)
        {
            GameObject go = Instantiate(prefab, new Vector3(location.x + 1.0f, 0.0f, location.y + 1.0f), 
                                        Quaternion.Euler(v3lurd.directs[1]), parent.transform);
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
        else if (side == Vector2Int.right)
        {
            GameObject go = Instantiate(prefab, new Vector3(location.x + 1.0f, 0.0f, location.y), 
                                        Quaternion.Euler(v3lurd.directs[2]), parent.transform);
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
        else if (side == Vector2Int.down)
        {
            GameObject go = Instantiate(prefab, new Vector3(location.x, 0.0f, location.y), 
                                        Quaternion.Euler(v3lurd.directs[3]), parent.transform);
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            //go.GetComponent<MeshRenderer>().material = greenMaterial;
        }
    }

    void PlaceCube(Vector2Int location, Vector2Int size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }
}
