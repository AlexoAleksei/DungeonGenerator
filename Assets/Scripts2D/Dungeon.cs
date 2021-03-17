using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon 
{
    public enum CellType
    {
        None,
        Room,
        Hallway
    }

    public enum RoomType
    {
        Empty,
        Entrance,
        Exit,
        Battle,
        Decorative
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
        public List<Door> doors;

        public Room() { }

        public Room(Vector2Int location, Vector2Int size)
        {
            bounds = new RectInt(location, size);
            Type = RoomType.Empty;
            doors = new List<Door>();
        }

        public Room(Vector2Int location, Vector2Int size, RoomType type)
        {
            bounds = new RectInt(location, size);
            Type = type;
            doors = new List<Door>();
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
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
}
