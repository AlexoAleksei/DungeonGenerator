﻿/* Adapted from https://github.com/vazgriz/DungeonGenerator

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
using UnityEngine.AI;
using Random = System.Random;
using Graphs;
using Dungeon;

public class Generator2D : MonoBehaviour
{
    [SerializeField]
    Vector2Int maxSize;
    [SerializeField]
    Vector2Int fieldSize;
    [SerializeField]
    int maxRoomNum; //No less than 3
    [SerializeField]
    float battleRoomRatio; //From (0.0f to 1.0f), 0.4 recommended
    [SerializeField]
    int roomCount;
    [SerializeField]
    Vector2Int battleRoomMinSize; //No less than (1, 1)
    [SerializeField]
    Vector2Int battleRoomMaxSize; //No less than (1, 1)
    [SerializeField]
    Vector2Int decorRoomMinSize; //No less than (1, 1)
    [SerializeField]
    Vector2Int decorRoomMaxSize; //No less than (1, 1)
    [SerializeField]
    GameObject hallwayPrefab;
    [SerializeField]
    GameObject playerPrefab;
    [SerializeField]
    List<GameObject> roomsPrefabs;

    [SerializeField]
    NavMeshSurface navMeshSurface;

    Random random;
    Grid2D<CellType> grid;
    Delaunay2D delaunay;
    HashSet<Prim.Edge> selectedEdges;
    StructurePlacer structurePlacer;
    InteractiveObjPlacer interactivePlacer;
    DecorativeObjPlacer decorativePlacer;
    LightingPlacer lightingPlacer;
    List<Room> rooms;
    List<HallwaySection> hallwaySections;
    DungeonInfo dungeonInfo;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        navMeshSurface = GameObject.Find("NavMesh").GetComponent<NavMeshSurface>();
        //-35909437
        //int timeseed = Environment.TickCount;
        random = new Random(-35909437); //If empty - all random, if number - seed
        //Debug.Log("Seed is ");
        //Debug.Log(timeseed);
        grid = new Grid2D<CellType>(maxSize, Vector2Int.zero);
        rooms = new List<Room>();
        hallwaySections = new List<HallwaySection>();
        dungeonInfo = new DungeonInfo();
        structurePlacer = gameObject.GetComponent<StructurePlacer>();
        interactivePlacer = gameObject.GetComponent<InteractiveObjPlacer>();
        decorativePlacer = gameObject.GetComponent<DecorativeObjPlacer>();
        lightingPlacer = gameObject.GetComponent<LightingPlacer>();

        GenerateRooms(); //Создает список комнат типа Room 
        PlaceRooms(); 
        CreateRoomsObjects(); //Создает список GameObject с комнатами разных типов
        Triangulate();
        CreateHallways();
        PathfindHallways();
        CreateHallwaysObjects();
        PlaceStructures(); //Размещает стены, пол, потолок, двери
        PlaceInteractiveObjects(); //Размещает объекты игрового процесса
        PlaceObstacles(); //Размещает декоративные объекты
        BakeNavMesh();
        interactivePlacer.SpawnInteractiveObjects(rooms);
        PlaceLighting(); //Размещает освещение
        SpawnPlayer(); //Размещение игрока
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
                    random.Next(0, fieldSize.x), 
                    random.Next(0, fieldSize.y)
                );
                if (rooms[i].Type == RoomType.Entrance)
                {
                    entrance = new Vector2Int(rooms[i].bounds.position.x, rooms[i].bounds.position.y);
                }
                else if (rooms[i].Type == RoomType.Exit)
                {
                    int range;
                    if (fieldSize.x >= fieldSize.y)
                    {
                        range = fieldSize.y / 2;
                    }
                    else
                    {
                        range = fieldSize.x / 2;
                    }
                    if (Vector2Int.Distance(rooms[i].bounds.position, entrance) <= range)
                    {
                        //Debug.Log("Entrance and Exit are too close!");
                        //Debug.Log(Vector2Int.Distance(rooms[i].bounds.position, entrance));
                        //Debug.Log(range);
                        add = false;
                    }
                    //Debug.Log("Range is fine!");
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
                        int x = random.Next(battleRoomMinSize.x, battleRoomMaxSize.x + 1); // +1, так как не включает верхнюю границу
                        int y = random.Next(x - 1, x + 1);
                        if (y < battleRoomMinSize.y) y = battleRoomMinSize.y;
                        rooms[i].bounds.size = new Vector2Int(x, y);
                        break;
                    case RoomType.Decorative:
                        x = random.Next(decorRoomMinSize.x, decorRoomMaxSize.x + 1);
                        y = random.Next(x - 1, x + 1);
                        if (y < decorRoomMinSize.y) y = decorRoomMinSize.y;
                        rooms[i].bounds.size = new Vector2Int(x, y);
                        break;
                    default:
                        rooms[i].bounds.size = new Vector2Int(decorRoomMinSize.x, decorRoomMinSize.y);
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

                if (rooms[i].bounds.xMin < 0 || rooms[i].bounds.xMax >= fieldSize.x
                    || rooms[i].bounds.yMin < 0 || rooms[i].bounds.yMax >= fieldSize.y)
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
                        //Debug.Log("Retrying to place rooms!");
                        rooms.Clear();
                        rooms.AddRange(roomsCopy);
                        ResizeField();
                        i = -1;
                        break;
                    }                   
                    else
                    {
                        //Debug.Log("Removing at");
                        //Debug.Log(i);
                        //Debug.Log("Type = ");
                        //Debug.Log(rooms[i].Type);
                        rooms.RemoveAt(i);
                        i -= 1;
                    }
                }
            }
        }

        foreach (var room in rooms)
        {
            //PlaceRoom(room.bounds.position, room.bounds.size, room.Type); //Models

            foreach (var pos in room.bounds.allPositionsWithin) //Grid positions
            {
                grid[pos] = CellType.Room;
            }
        }

        Debug.Log("Rooms number = ");
        Debug.Log(rooms.Count);
        foreach (var room in rooms)
        {
            Debug.Log(room.Type);
        }
    }

    void CreateRoomsObjects()
    {
        foreach (var room in rooms)
        {
            int i;
            switch (room.Type) //Выбираем нужный префаб типа комнаты
            {
                case RoomType.Entrance:
                    i = (int)RoomType.Entrance - 1; // -1 - cause RoomType.Empty = 0
                    dungeonInfo.entranceNum += 1;
                    break;
                case RoomType.Exit:
                    i = (int)RoomType.Exit - 1;
                    dungeonInfo.exitNum += 1;
                    break;
                case RoomType.Battle:
                    i = (int)RoomType.Battle - 1;
                    dungeonInfo.battleRoomNum += 1;
                    break;
                case RoomType.Decorative:
                    i = (int)RoomType.Decorative - 1;
                    dungeonInfo.decorRoomNum += 1;
                    break;
                default:
                    i = (int)RoomType.Decorative - 1;
                    dungeonInfo.decorRoomNum += 1;
                    break;
            }
            /*GameObject go = Instantiate(roomsPrefabs.Find(x => x.name == roomType), 
                                        new Vector3(room.bounds.position.x + 0.5f * room.bounds.size.x, 0.5f, 
                                        room.bounds.position.y + 0.5f * room.bounds.size.y), Quaternion.identity);*/
            GameObject go = Instantiate(roomsPrefabs[i],
                                        new Vector3(room.bounds.position.x + 0.5f * room.bounds.size.x, 0.5f,
                                        room.bounds.position.y + 0.5f * room.bounds.size.y), Quaternion.identity);
            room.roomObj = go;
            //go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            go.GetComponent<BoxCollider>().size = new Vector3(room.bounds.size.x, 1, room.bounds.size.y) ;
        }

        Debug.Log("entrance = ");
        Debug.Log(dungeonInfo.entranceNum);
        Debug.Log("exit = ");
        Debug.Log(dungeonInfo.exitNum);
        Debug.Log("battle = ");
        Debug.Log(dungeonInfo.battleRoomNum);
        Debug.Log("decorative = ");
        Debug.Log(dungeonInfo.decorRoomNum);
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
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(fieldSize);

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
                HallwaySection hallwaySection = new HallwaySection();
                //hallwaySections.Add(hallwaySection);

                for (int i = 0; i < path.Count; i++)
                {
                    var current = path[i];

                    if (grid[current] == CellType.None)
                    {
                        grid[current] = CellType.Hallway;
                        Hallway newHallway = new Hallway(current, new Vector2Int(1, 1));
                        hallwaySection.hallwayList.Add(newHallway);
                    }

                    if (i > 0 && grid[current] == CellType.Room && grid[path[i - 1]] == CellType.Hallway)
                    {
                        AddDoorToRoom(current, path[i - 1], true);
                    }

                    if (i > 0 && grid[current] == CellType.Hallway && grid[path[i - 1]] == CellType.Room)
                    {
                        AddDoorToRoom(current, path[i - 1], false);
                    }

                    if (i > 0)
                    {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                if (hallwaySection.hallwayList.Count > 0)
                { //Если в данной секции не было добавлено ни одной новой ячейки коридора
                    hallwaySections.Add(hallwaySection);
                }

                /*if (hallwaySection.hallwayList.Count <= 0)
                { //Если в данной секции не было добавлено ни одной новой ячейки коридора
                    hallwaySections.RemoveAt(hallwaySections.Count - 1);
                }*/

                /*foreach (var pos in path)
                {
                    if (grid[pos] == CellType.Hallway)
                    {
                        PlaceHallway(pos);
                    }
                }*/
            }
        }
        /*foreach (var room in rooms)
        {
            Debug.Log("room is");
            Debug.Log(room.bounds.position);
            Debug.Log(room.bounds.center);
            foreach (var door in room.doors)
            {
                Debug.Log("door is");
                Debug.Log(door.location);
                Debug.Log(door.side);
            }
            Debug.Log("////////////////////");
        }*/
    }

    void AddDoorToRoom(Vector2Int current, Vector2Int prev, bool isRoomToHallway)
    {
        if (isRoomToHallway) //If the current cell is room and the previous is hallway
        {
            bool found = false;
            foreach (var room in rooms)
            {
                foreach (var pos in room.bounds.allPositionsWithin) //Grid positions
                {
                    if (current == pos)
                    {
                        room.doors.Add(new Door(current, new Vector2Int(prev.x - current.x, prev.y - current.y)));
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
        }

        else
        {
            bool found = false;
            foreach (var room in rooms)
            {
                foreach (var pos in room.bounds.allPositionsWithin) //Grid positions
                {
                    if (prev == pos)
                    {
                        room.doors.Add(new Door(prev, new Vector2Int(current.x - prev.x, current.y - prev.y)));
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
        }
    }

    void CreateHallwaysObjects()
    {
        /*foreach (var hallway in hallways)
        { //Дописать спаун префаба нужного типа комнат
            GameObject go = Instantiate(hallwayPrefab,
                                        new Vector3(hallway.bounds.position.x + 0.5f * hallway.bounds.size.x, 0.5f,
                                        hallway.bounds.position.y + 0.5f * hallway.bounds.size.y), Quaternion.identity);
            hallway.hallwayObj = go;
            go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
        }*/

        foreach (var hallwaySection in hallwaySections)
        { //Дописать спаун префаба нужного типа комнат
            foreach(var hallway in hallwaySection.hallwayList)
            {
                GameObject go = Instantiate(hallwayPrefab,
                                        new Vector3(hallway.bounds.position.x + 0.5f * hallway.bounds.size.x, 0.5f,
                                        hallway.bounds.position.y + 0.5f * hallway.bounds.size.y), Quaternion.identity);
                hallway.hallwayObj = go;
                go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);
            }
        }
    }

    void PlaceStructures()
    {
        structurePlacer.PlaceStructures(grid, fieldSize, rooms, hallwaySections);
    }

    void PlaceInteractiveObjects()
    {
        interactivePlacer.PlaceInteractive(rooms, hallwaySections, dungeonInfo);
    }

    void PlaceObstacles()
    {
        decorativePlacer.PlaceObstacles(rooms, hallwaySections, dungeonInfo);
    }

    void PlaceLighting()
    {
        lightingPlacer.PlaceLighting(rooms, hallwaySections);
    }

    void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

    void ResizeField() //Increases the size of the field used, max is (maxSize.x, maxSize.y)
    {
        if (fieldSize.x >= maxSize.x && fieldSize.y >= maxSize.y)
        {
            Debug.Log("The max field size is exceeded!");
        }

        fieldSize.x += (int)(fieldSize.x * 0.1f) + 1;
        fieldSize.y += (int)(fieldSize.y * 0.1f) + 1;

        if (fieldSize.x >= maxSize.x)
        {
            fieldSize.x = maxSize.x;
        }
        if (fieldSize.y >= maxSize.y)
        {
            fieldSize.y = maxSize.y;
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

    void SpawnPlayer()
    {
        GameObject player = GameObject.Find("Player");
        //Debug.Log(player.name);
        //player.GetComponent<PlayerController>().canvas.transform.position = rooms[0].roomObj.transform.position;
        player.GetComponent<PlayerController>().controller.enabled = false;
        player.transform.position = rooms[0].roomObj.transform.position;
        player.transform.rotation = Quaternion.identity;
        player.GetComponent<PlayerController>().controller.enabled = true;

        //GameObject go = Instantiate(playerPrefab, rooms[0].roomObj.transform.position, Quaternion.identity); //Spawn player in Entrance room
        //go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        //go.GetComponent<MeshRenderer>().material = material;
    }

    /*void RemoveHallwaysRepeats()
    {
        for (int i = 0; i < hallways.Count; i++)
        {
            for (int j = i; j < hallways.Count; j++)
           
                Debug.Log("iter num ");
                Debug.Log(j);
                Debug.Log(hallways[i]);
                Debug.Log(hallways[j]);
                if (i != j && hallways[i] == hallways[j])
                {
                    hallways.RemoveAt(j);
                    j -= 1;
                }
            }
        }
    }*/

    /*void PlaceCube(Vector2Int location, Vector2Int size, Material material)
    {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }

    void PlaceRoom(Vector2Int location, Vector2Int size, RoomType type)
    {
        GameObject go = Instantiate(roomPrefab, new Vector3(location.x + 0.5f * size.x, 0 + 0.5f, location.y + 0.5f * size.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
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
    }*/
}
