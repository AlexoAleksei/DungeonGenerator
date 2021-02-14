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

public class Generator2D : MonoBehaviour {
    enum CellType {
        None,
        Room,
        Hallway
    }

    enum RoomType {
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
    Vector2Int size;
    [SerializeField]
    int maxRoomNum; //No less than 2
    [SerializeField]
    float battleRoomRatio; //From (0.0f to 1.0f)
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

    Random random;
    Grid2D<CellType> grid;
    List<Room> rooms;
    Delaunay2D delaunay;
    HashSet<Prim.Edge> selectedEdges;

    void Start() {
        Generate();
    }

    void Generate() {
        random = new Random(0); //If empty - all random, if number - seed
        grid = new Grid2D<CellType>(size, Vector2Int.zero);
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
    void GenerateRooms() {
        int[] roomTypeNum = new int[Enum.GetNames(typeof(RoomType)).Length - 1]; // Don't need "Empty" rooms
        roomTypeNum[0] = 1; //entranceRoomNumber
        roomTypeNum[1] = 1; //exitRoomNumber
        roomTypeNum[2] = (int)((maxRoomNum - roomTypeNum[0] - roomTypeNum[1]) * battleRoomRatio); //battleRoomNumber
        roomTypeNum[3] = maxRoomNum - roomTypeNum[0] - roomTypeNum[1] - roomTypeNum[2]; //decorativeRoomNumber

        Debug.Log(Enum.GetNames(typeof(RoomType)).Length);
        Debug.Log(roomTypeNum[0]);
        Debug.Log(roomTypeNum[1]);
        Debug.Log(roomTypeNum[2]);
        Debug.Log(roomTypeNum[3]);

       for (int i = 0; i < roomTypeNum.Length; i++) {
            RoomType type = RoomType.Empty;

            switch (i) {
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

            Vector2Int location = new Vector2Int(0, 0);

            Vector2Int roomSize = new Vector2Int(
                random.Next(roomMinSize.x, roomMaxSize.x),
                random.Next(roomMinSize.y, roomMaxSize.y)
            );

            Room newRoom = new Room(location, roomSize, type);
            rooms.Add(newRoom);
        }
    }

    void PlaceRooms() {
        for (int i = 0; i < roomCount; i++) {
            Vector2Int location = new Vector2Int(
                random.Next(0, size.x),
                random.Next(0, size.y)
            );

            Vector2Int roomSize = new Vector2Int(
                random.Next(1, roomMaxSize.x + 1),
                random.Next(1, roomMaxSize.y + 1)
            );

            bool add = true;
            Room newRoom = new Room(location, roomSize);
            Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            foreach (var room in rooms) {
                if (Room.Intersect(room, buffer)) {
                    add = false;
                    break;
                }
            }

            if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y) {
                add = false;
            }

            if (add) {
                rooms.Add(newRoom);
                PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                foreach (var pos in newRoom.bounds.allPositionsWithin) {
                    grid[pos] = CellType.Room;
                }
            }
        }
    }

    void Triangulate() {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var room in rooms) {
            vertices.Add(new Vertex<Room>((Vector2)room.bounds.position + ((Vector2)room.bounds.size) / 2, room));
        }

        delaunay = Delaunay2D.Triangulate(vertices);
    }

    void CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        List<Prim.Edge> mst = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(mst);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < 0.125) {
                selectedEdges.Add(edge);
            }
        }
    }

    void PathfindHallways() {
        DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.bounds.center;
            var endPosf = endRoom.bounds.center;
            var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
            var endPos = new Vector2Int((int)endPosf.x, (int)endPosf.y);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                var pathCost = new DungeonPathfinder2D.PathCost();
                
                pathCost.cost = Vector2Int.Distance(b.Position, endPos);    //heuristic

                if (grid[b.Position] == CellType.Room) {
                    pathCost.cost += 10;
                } else if (grid[b.Position] == CellType.None) {
                    pathCost.cost += 5;
                } else if (grid[b.Position] == CellType.Hallway) {
                    pathCost.cost += 1;
                }

                pathCost.traversable = true;

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;
                    }
                }

                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway) {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

    void PlaceCube(Vector2Int location, Vector2Int size, Material material) {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }

    void PlaceRoom(Vector2Int location, Vector2Int size) {
        PlaceCube(location, size, redMaterial);
    }

    void PlaceHallway(Vector2Int location) {
        PlaceCube(location, new Vector2Int(1, 1), blueMaterial);
    }
}
