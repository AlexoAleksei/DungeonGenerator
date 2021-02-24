/* Adapted from https://github.com/vazgriz/DungeonGenerator

Copyright (c) 2015-2019 Simon Zeni (simonzeni@gmail.com)


Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:


The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;

public class Generator2D : MonoBehaviour
{
    enum CellType
    {
        None,
        Room,
        Hallway
    }

    enum RoomType
    {
        Empty,
        Entrance,
        Exit,
        Battle,
        Decorative
    }

    /*class Room {
        public RectInt bounds;

        public Room(Vector2Int location, Vector2Int size) {
            bounds = new RectInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }
    }*/

    class Room
    {
        public RectInt bounds;
        public RoomType Type;

        public Room(Vector2Int location, Vector2Int size)
        {
            bounds = new RectInt(location, size);
            Type = RoomType.Empty;
        }

        public Room(Vector2Int location, Vector2Int size, RoomType type)
        {
            bounds = new RectInt(location, size);
            Type = type;
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
        }
    }

    [SerializeField]
    Vector2Int maxSize;
    [SerializeField]
    Vector2Int size;
    [SerializeField]
    int maxRoomNum; //No less than 3
    [SerializeField]
    float battleRoomRatio; //From (0.0f to 1.0f), 0.4 recommended
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector2Int roomMinSize; //No less than (1, 1)
    [SerializeField]
    Vector2Int roomMaxSize; //No less than (1, 1)
    [SerializeField]
    GameObject cubePrefab;
    [SerializeField]
    Material redMaterial;
    [SerializeField]
    Material blueMaterial;
    [SerializeField]
    Material yellowMaterial;
    [SerializeField]
    Material greenMaterial;
    [SerializeField]
    Material violetMaterial;

    Random random;
    Grid2D<CellType> grid;
    List<Room> rooms;
    Delaunay2D delaunay;
    HashSet<Prim.Edge> selectedEdges;


    void Start()
    {
        Generate();
    }

    void Generate()
    {
        random = new Random(); //If empty - all random, if number - seed
        grid = new Grid2D<CellType>(maxSize, Vector2Int.zero);
        rooms = new List<Room>();

        GenerateRooms();
        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
        //PlaceStructure(); //Размещает стены, пол, потолок, двери
        //PlaceGameObjects(); //Размещает объекты игрового процесса
        //PlaceObstacles(); //Размещает декоративные объекты
        //PlaceLighting(); //Размещает освещение
    }

    /* void GenerateRooms(int levelNum, PrevLevelInfo prevLevelInfo) {
        levelNum - влияет на мин/макс количество комнат,
                   количество комнат влияет на макс размер поля
                   количество комнат влияет на мин/макс размеры комнат
        prevLevelInfo - влияет на скорость роста сложности с возрастанием 
                        номера уровня (сложность комнат, количество наград, сила врагов)
    }*/
    void GenerateRooms()
    {
        int[] roomTypeNum = new int[Enum.GetNames(typeof(RoomType)).Length - 1]; // Don't need "Empty" rooms
        roomTypeNum[0] = 1; //entranceRoomNumber
        roomTypeNum[1] = 1; //exitRoomNumber
        roomTypeNum[2] = (int)((maxRoomNum - roomTypeNum[0] - roomTypeNum[1]) * battleRoomRatio); //battleRoomNumber
        roomTypeNum[3] = maxRoomNum - roomTypeNum[0] - roomTypeNum[1] - roomTypeNum[2]; //decorativeRoomNumber

        //Debug.Log(Enum.GetNames(typeof(RoomType)).Length);
        /*Debug.Log(roomTypeNum[0]);
        Debug.Log(roomTypeNum[1]);
        Debug.Log(roomTypeNum[2]);
        Debug.Log(roomTypeNum[3]);
        Debug.Log(roomTypeNum.Length);*/

        for (int i = 0; i < roomTypeNum.Length; i++)
        {
            for (int j = 0; j < roomTypeNum[i]; j++)
            {
                RoomType type = RoomType.Empty;
                Vector2Int location = new Vector2Int(-2, -2);
                Vector2Int roomSize = new Vector2Int(0, 0);

                switch (i)
                {
                    case 0:
                        type = RoomType.Entrance;
                        break;
                    case 1:
                        type = RoomType.Exit;
                        break;
                    case 2:
                        type = RoomType.Battle;
                        break;
                    case 3:
                        type = RoomType.Decorative;
                        break;
                    default:
                        type = RoomType.Empty;
                        break;
                }
                Room newRoom = new Room(location, roomSize, type);
                rooms.Add(newRoom);

                /*Debug.Log("index =");
                Debug.Log(i);
                Debug.Log("Type =");
                Debug.Log(rooms[i].Type);*/
            }
        }
    }

    void PlaceRooms()
    {
        List<Room> roomsCopy = new List<Room>(); //Creating a reserve copy
        roomsCopy.AddRange(rooms);
        Vector2Int entrance = new Vector2Int(0, 0);

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < roomCount; j++)
            {
                bool add = true;

                rooms[i].bounds.position = new Vector2Int(
                    random.Next(0, size.x),
                    random.Next(0, size.y)
                );
                if (rooms[i].Type == RoomType.Entrance)
                {
                    entrance = new Vector2Int(rooms[i].bounds.position.x, rooms[i].bounds.position.y);
                }
                else if (rooms[i].Type == RoomType.Exit)
                {
                    int range;
                    if (size.x >= size.y)
                    {
                        range = size.y / 2;
                    }
                    else
                    {
                        range = size.x / 2;
                    }
                    if (Vector2Int.Distance(rooms[i].bounds.position, entrance) <= range)
                    {
                        Debug.Log("Entrance and Exit are too close!");
                        //Debug.Log(Vector2Int.Distance(rooms[i].bounds.position, entrance));
                        //Debug.Log(range);
                        add = false;
                    }
                    Debug.Log("Range is fine!");
                    //Debug.Log(Vector2Int.Distance(rooms[i].bounds.position, entrance));
                    //Debug.Log(range);
                }

                switch (rooms[i].Type)
                {
                    case RoomType.Entrance:
                        rooms[i].bounds.size = new Vector2Int(2, 2);
                        break;
                    case RoomType.Exit:
                        rooms[i].bounds.size = new Vector2Int(2, 2);
                        break;
                    case RoomType.Battle:
                        int x = random.Next(roomMinSize.x + 1, roomMaxSize.x);
                        int y = random.Next(x - 1, x + 1);
                        rooms[i].bounds.size = new Vector2Int(x, y);
                        break;
                    case RoomType.Decorative:
                        x = random.Next(roomMinSize.x + 1, roomMaxSize.x - 1);
                        y = random.Next(x - 1, x + 1);
                        rooms[i].bounds.size = new Vector2Int(x, y);
                        break;
                    default:
                        rooms[i].bounds.size = new Vector2Int(roomMinSize.x, roomMinSize.y);
                        break;
                }

                /*Debug.Log(i);
                Debug.Log(rooms[i].bounds.position);
                Debug.Log(rooms[i].bounds.size);*/
               
                Room buffer = new Room(
                    rooms[i].bounds.position + new Vector2Int(-1, -1),
                    rooms[i].bounds.size + new Vector2Int(2, 2)
                );

                foreach (var room in rooms)
                {
                    if (Room.Intersect(room, buffer) && room != rooms[i])
                    {
                        //Debug.Log("Intersection!");
                        add = false;
                        break;
                    }
                }

                if (rooms[i].bounds.xMin < 0 || rooms[i].bounds.xMax >= size.x
                    || rooms[i].bounds.yMin < 0 || rooms[i].bounds.yMax >= size.y)
                {
                    //Debug.Log("OverField!");
                    add = false;
                }

                if (add)
                {                   
                    break;
                }

                if ((j == roomCount - 1) && !add) //Здесь комментарии не нужно удалять
                {
                    if (rooms[i].Type == RoomType.Entrance || rooms[i].Type == RoomType.Exit || rooms.Count <= 3)
                    {
                        Debug.Log("Retrying to place rooms!");
                        rooms.Clear();
                        rooms.AddRange(roomsCopy);
                        ResizeField();
                        i = -1;
                        break;
                    }                   
                    else
                    {
                        Debug.Log("Removing at");
                        Debug.Log(i);
                        Debug.Log("Type = ");
                        Debug.Log(rooms[i].Type);
                        rooms.RemoveAt(i);
                        i -= 1;
                    }
                }
            }
        }

        foreach (var room in rooms)
        {
            PlaceRoom(room.bounds.position, room.bounds.size, room.Type); //Models

            foreach (var pos in room.bounds.allPositionsWithin) //Grid positions
            {
                grid[pos] = CellType.Room;
            }
        }

        /*if (rooms.Count < 3) //Triangulation can't work with less than 3 rooms
        {
            Room newRoom = new Room(new Vector2Int(-1, -1), new Vector2Int(1, 1), RoomType.Empty);
            rooms.Add(newRoom);
        }*/

        Debug.Log("Rooms number = ");
        Debug.Log(rooms.Count);
        foreach (var room in rooms)
        {
            Debug.Log(room.Type);
        }
    }

    void Triangulate()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms)
        {
            vertices.Add(new Vertex<Room>((Vector2)room.bounds.position + ((Vector2)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay2D.Triangulate(vertices);
    }

    void CreateHallways()
    {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges)
        {
            if (random.NextDouble() < 0.125)
            {
                selectedEdges.Add(edge);
            }
        }
    }

    void PathfindHallways()
    {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();

                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (grid[b.Position] == CellType.Room)
                {
                    pathCost.cost += 10;
                }
                else if (grid[b.Position] == CellType.None)
                {
                    pathCost.cost += 5;
                }
                else if (grid[b.Position] == CellType.Hallway)
                {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.None)
                    {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0)
                    {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Hallway)
                    {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

    void ResizeField() //Increases the size of the field used, max is (maxSize.x, maxSize.y)
    {
        if (size.x >= maxSize.x || size.y >= maxSize.y)
        {
            Debug.Log("The max field size is exceeded!");
        }

        size.x += (int)(size.x * 0.1f) + 1;
        size.y += (int)(size.y * 0.1f) + 1;

        if (size.x >= maxSize.x)
        {
            size.x = maxSize.x;
        }
        if (size.y >= maxSize.y)
        {
            size.y = maxSize.y;
        }
    }

    /*void ClearField() //Clears the filed within ths size.x and size.y
    {
        Vector2Int pos = new Vector2Int(0, 0);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                pos = new Vector2Int(x, y);
                Debug.Log(pos);
                Debug.Log(grid[pos]);
                grid[pos] = CellType.None;
                Debug.Log(grid[pos]);
            }          
        }
        Debug.Log("Cleared!");
    }*/

    void PlaceCube(Vector2Int location, Vector2Int size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }

    void PlaceRoom(Vector2Int location, Vector2Int size, RoomType type)
    {
        switch (type)
        {
            case RoomType.Entrance:
                PlaceCube(location, size, greenMaterial);
                break;
            case RoomType.Exit:
                PlaceCube(location, size, violetMaterial);
                break;
            case RoomType.Battle:
                PlaceCube(location, size, redMaterial);
                break;
            case RoomType.Decorative:
                PlaceCube(location, size, yellowMaterial);
                break;
            default:
                PlaceCube(location, size, blueMaterial);
                break;
        }        
    }

    void PlaceHallway(Vector2Int location)
    {
        PlaceCube(location, new Vector2Int(1, 1), blueMaterial);
    }
}
