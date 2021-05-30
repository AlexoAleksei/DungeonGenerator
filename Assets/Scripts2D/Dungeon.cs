using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon 
{
    public enum CellType //Для сетки генерации комнат
    {
        None,
        Room,
        Hallway
    }

    public enum RoomType //Типы комнат
    {
        Empty,
        Entrance,
        Exit,
        Battle,
        Decorative
    }

    public enum RoomSubType //Подтипы комнат
    { //При добавлении в префабах происходит сдвиг всех последующих подтипов, проверяйте это
        DefaultEntrance,
        DefaultExit,
        Arena,
        Treasury,
        Large,
        Lobby,
        Library
    }

    public enum ObjectCellType //Для сетки расстановки объектов
    {
        None, 
        Blocked, //Чтобы не использовалось расстановщиком (возле дверей например)
        Enemy,
        Interactive,
        Obstacle
    }

    public enum InteractiveObjPlaceType
    {
        Free,
        Central,
        NearWall,
        NoWall
    }

    public enum InteractiveObjType
    {
        None,
        Separate,
        Stash,
        Guard
    }

    //private Vector2Int[] V2LURD = { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down };
    //private Quaternion[] QLURD = { new Quaternion(0, 270, 0, 0), Quaternion.identity,
    //                               new Quaternion(0, 0, 90, 0), new Quaternion(0, 0, 180, 0) };

    public class V2LURD
    {
        public Vector2Int[] sides;

        public V2LURD()
        {
            sides = new Vector2Int[4] { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down };
        }
    }

    public class V3LURD
    {
        public Vector3[] directs;

        public V3LURD()
        {
            directs = new Vector3[4] { new Vector3(0, 180, 0), new Vector3(0, 270, 0),
                                       new Vector3(0, 0, 0), new Vector3(0, 90, 0) };
        }
    }

    public class DungeonInfo
    {
        public int entranceNum = 0;
        public int exitNum = 0;
        public int battleRoomNum = 0;
        public int decorRoomNum = 0;

        //Entrance subTypes number
        public int defaultEntranceNum = 0;

        //Exit subTypes number
        public int defaultExitNum = 0;

        //Battle room subTypes number
        public int ArenaNum = 0;
        public int TreasuryNum = 0;

        //Decorative room subTypes number
        public int LobbyNum = 0;

        public DungeonInfo()
        {

        }
    }

    public class InteractiveObjectList
    {
        public GameObject interactiveObj; //Образец объекта
        public List<Vector2Int> location; //Локации этих объектов
        public List<Vector3> side; //Направления зрения этих объектов
        int objNum;

        public int Count()
        {
            return objNum;
        }

        public InteractiveObjectList(GameObject gameObject, Vector2Int Location, Vector3 Side)
        {
            interactiveObj = gameObject;
            location = new List<Vector2Int>();
            location.Add(Location);
            side = new List<Vector3>();
            side.Add(Side);
            objNum = 1;
        }

        public void SetIntObj(int index, Vector2Int Location, Vector3 Side)
        {
            /*Debug.Log(interactiveObj.name);
            Debug.Log("location");
            Debug.Log(Location);
            Debug.Log(Side);*/
            location[index] = Location;
            side[index] = Side;
        }

        public void AddIntObj(Vector2Int Location, Vector3 Side)
        {
            location.Add(Location);
            side.Add(Side);
            objNum += 1;
        }

        public void DeleteIntObj(int index)
        {
            if (objNum > 0 && index >= 0 && index < objNum)
            {
                location.RemoveAt(index);
                side.RemoveAt(index);
                objNum -= 1;
            }
        }
    }

    public class Door
    {
        public Vector2Int location;
        public Vector2Int side; //left, up, right, down.

        public Door(Vector2Int Location, Vector2Int Side)
        {
            location = Location;
            side = Side;
        }
    }

    public class Room
    {
        public RectInt bounds;
        public RoomType Type;
        public RoomSubType subType;
        public List<Door> doors;
        public GameObject roomObj;
        public GameObject subTypeRoomObj;
        public Grid2D<ObjectCellType> grid;
        public List<InteractiveObjectList> iterObjList;

        public Room() { }

        public Room(Vector2Int location, Vector2Int size)
        {
            bounds = new RectInt(location, size);
            doors = new List<Door>();
            iterObjList = new List<InteractiveObjectList>();
        }

        public Room(Vector2Int location, Vector2Int size, RoomType type)
        {
            bounds = new RectInt(location, size);
            Type = type;
            doors = new List<Door>();
            iterObjList = new List<InteractiveObjectList>();
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }

        public void initGrid()
        {
            grid = new Grid2D<ObjectCellType>(new Vector2Int(bounds.size.x * 4 - 1, bounds.size.y * 4 - 1), Vector2Int.zero);
            DoorBlockedGridPosition();
           /* Debug.Log("x = ");
            Debug.Log(bounds.size.x);
            Debug.Log("y = ");
            Debug.Log(bounds.size.y);
            Debug.Log(new Vector2Int(bounds.size.x * 4 - 1, bounds.size.y * 4 - 1));
            Debug.Log("grid size = ");
            Debug.Log(grid.Size);*/
        }

        private void DoorBlockedGridPosition() //Блокирует позиции на сетке у дверей
        {
            foreach(var door in doors)
            {
                int posX = 0;
                int posY = 0;

                if (door.location.x == 0)
                {
                    posX = 0;
                }
                else posX = (door.location.x - bounds.position.x) * 4;
                if (door.location.y == 0)
                {
                    posY = 0;
                }
                else posY = (door.location.y - bounds.position.y) * 4;

               /* Debug.Log("bounds.position.x = ");
                Debug.Log(bounds.position.x);
                Debug.Log("bounds.position.y = ");
                Debug.Log(bounds.position.y);
                Debug.Log("door.location.x = ");
                Debug.Log(door.location.x);
                Debug.Log("door.location.y = ");
                Debug.Log(door.location.y);*/

                if (door.side == Vector2Int.left)
                {
                    posX += 0;
                    posY += 1;
                }
                else if (door.side == Vector2Int.up)
                {
                    posX += 1;
                    posY += 2;
                }
                else if (door.side == Vector2Int.right)
                {
                    posX += 2;
                    posY += 1;
                }
                else if (door.side == Vector2Int.down)
                {
                    posX += 1;
                    posY += 0;
                }
                /*Debug.Log("grid.Size = ");
                Debug.Log(grid.Size);
                Debug.Log("pos index is = ");
                Debug.Log(new Vector2Int(posX, posY));*/
                grid[new Vector2Int(posX, posY)] = ObjectCellType.Blocked;
            }
        }

        public void SetObjectGridPosition(Vector2Int location, int Space, ObjectCellType objectCellType)
        {
            grid[location.x, location.y] = objectCellType;

            if (Space <= 0)
            {
                return;
            }
            //Debug.Log("location");
            //Debug.Log(location);
            for (int i = 0; i < 2 * Space + 1; i++)
            {
                int posX = location.x + i - Space;
                for (int j = 0; j < 2 * Space + 1; j++)
                {
                    int posY = location.y + j - Space;
                    //Debug.Log("////////");
                    //Debug.Log(posX);
                    //Debug.Log(posY);
                    if (new Vector2Int(posX, posY) == location)
                    {
                        //Debug.Log("its ME");
                        continue;
                    }
                    if (posX >= 0 &&
                        posX < grid.Size.x &&
                        posY >= 0 &&
                        posY < grid.Size.y)
                    {
                        //Debug.Log("Blocked");
                        //
                        if (grid[posX, posY] == ObjectCellType.Interactive)
                        { //Если вдруг 
                            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA!");
                        }
                        grid[posX, posY] = ObjectCellType.Blocked;
                    }                  
                }
            }
        }

        public bool CheckObjSpace(Vector2Int location, int Space) //Проверка окружения объекта на сетке
        {
            if (Space <= 0)
            {
                return true;
            }
            //Debug.Log("location");
            //Debug.Log(location);
            for (int i = 0; i < 2 * Space + 1; i++)
            {
                int posX = location.x + i - Space;
                for (int j = 0; j < 2 * Space + 1; j++)
                {
                    int posY = location.y + j - Space;
                    //Debug.Log("////////");
                    //Debug.Log(posX);
                    //Debug.Log(posY);

                    if (new Vector2Int(posX, posY) == location)
                    {
                        //Debug.Log("its ME");
                        continue;
                    }
                    else if (posX >= 0 &&
                            posX < grid.Size.x && 
                            posY >= 0 &&
                            posY < grid.Size.y)
                    {
                        if (grid[posX, posY] != ObjectCellType.None &&
                            grid[posX, posY] != ObjectCellType.Blocked)
                        {
                            /*Debug.Log(posX);
                            Debug.Log(posY);*/
                            //Debug.Log(grid[posX, posY]);
                            return false;
                        }
                    }                   
                }
            }          
            return true;
        }

        public void ClearGrid()
        {
            Vector2Int pos = new Vector2Int(0, 0);
            for (int i = 0; i < grid.Size.x; i++)
            {
                pos.x = i;
                for (int j = 0; j < grid.Size.y; j++)
                {
                    pos.y = j;
                    grid[pos.x, pos.y] = ObjectCellType.None;
                }
            }
        }

        public void RemoveDoorsRepeats()
        {
            for (int i = 0; i < doors.Count; i++)
            {
                for (int j = i; j < doors.Count; j++)
                {
                    if (i != j && doors[i].location == doors[j].location && doors[i].side == doors[j].side)
                    {
                        doors.RemoveAt(j);
                        j -= 1;
                    }
                }
            }
        }

        public bool CheckDoors(Vector2Int location, Vector2Int side)
        {
            bool found = false;
            foreach (var door in doors)
            {
                if (door.location == location && door.side == side)
                {
                    found = true;
                    break;
                }                   
            }
            return found;
        }
    }

    public class Hallway
    {
        public RectInt bounds;
        public GameObject hallwayObj;

        public bool isLighted;

        public Hallway() { }

        public Hallway(Vector2Int location, Vector2Int size)
        {
            bounds = new RectInt(location, size);
            isLighted = false;
        }
    }

    public class HallwaySection
    {
        public List<Hallway> hallwayList;

        public HallwaySection()
        {
            hallwayList = new List<Hallway>();
        }
    }

    public class Reward
    {
        public int pointsAmount;
        public int healingPotionsAmount;

        public Reward() { }

        public Reward(int PointsAmount, int HealingPotionsAmount)
        {
            pointsAmount = PointsAmount;
            healingPotionsAmount = HealingPotionsAmount;
        } 
    }
}
