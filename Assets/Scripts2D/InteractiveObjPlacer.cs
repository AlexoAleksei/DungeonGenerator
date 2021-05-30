﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Dungeon;


public class InteractiveObjPlacer : MonoBehaviour
{
    [SerializeField]
    int intObjCount; //Число попыток постановки объекта

    //Entrance room subtypes
    public List<GameObject> EntranceRoomSubtypes;

    //Exit room subtypes
    public List<GameObject> ExitRoomSubtypes;

    //Battle room subtypes
    public List<GameObject> BattleRoomSubtypes;

    //Decorative room subtypes
    public List<GameObject> DecorRoomSubtypes;

    Random random;
    V3LURD v3lurd;

    public void PlaceInteractive(List<Room> rooms, List<HallwaySection> hallwaySections, DungeonInfo dungeonInfo)
    {
        random = new Random(); //If empty - all random, if number - seed
        v3lurd = new V3LURD();

        int[] subBattleCount = new int[BattleRoomSubtypes.Count]; //ассоциативный массив числа боевых комнат каждого подтипа, совпадает по индексам с BattleRoomSubTypes
        int[] subDecorCount = new int[DecorRoomSubtypes.Count]; //ассоциативный массив числа декоративных комнат каждого подтипа, совпадает по индексам с DecorRoomSubTypes

        foreach (var room in rooms) //Выбираем подтип комнаты нужного типа
        {
            switch (room.Type)
            {
                case RoomType.Entrance:
                    room.subType = RoomSubType.DefaultEntrance;
                    room.subTypeRoomObj = EntranceRoomSubtypes[0];
                    room.roomObj.GetComponent<EntranceRoom>().subType = RoomSubType.DefaultEntrance; //Сделать отдельный скрипт EntranceRoom для описания поведения
                    break;
                case RoomType.Exit:
                    room.subType = RoomSubType.DefaultExit;
                    room.subTypeRoomObj = ExitRoomSubtypes[0];
                    room.roomObj.GetComponent<ExitRoom>().subType = RoomSubType.DefaultExit; //Сделать отдельный скрипт ExitRoom для описания поведения
                    break;
                case RoomType.Battle:
                    RoomSubType sub_battle;
                    sub_battle = FindBattleLargestPossible(room, rooms, subBattleCount, dungeonInfo);

                    room.subType = sub_battle;
                    room.roomObj.GetComponent<BattleRoom>().subType = sub_battle;
                    /*Debug.Log("room.subType = ");
                    Debug.Log(room.subType);
                    Debug.Log("room.subTypeRoomObj.name = ");
                    Debug.Log(room.subTypeRoomObj.name);*/
                    break;
                case RoomType.Decorative:
                    RoomSubType sub_decor;
                    sub_decor = FindDecorLargestPossible(room, rooms, subDecorCount, dungeonInfo);

                    room.subType = sub_decor;
                    room.roomObj.GetComponent<DecorRoom>().subType = sub_decor;
                    /*Debug.Log("room.subType = ");
                    Debug.Log(room.subType);
                    Debug.Log("room.subTypeRoomObj.name = ");
                    Debug.Log(room.subTypeRoomObj.name);*/
                    /*int y = random.Next(0, DecorRoomSubtypes.Count);
                    room.subType = (RoomSubType)(y + (int)RoomSubType.Lobby);
                    room.roomObj.GetComponent<BattleRoom>().subType = (RoomSubType)(y + (int)RoomSubType.Lobby);*/
                    break;
            }
        }
        /*Debug.Log("Arena = ");
        Debug.Log(subBattleCount[0]);
        Debug.Log("Treasury = ");
        Debug.Log(subBattleCount[1]);
        Debug.Log("Large = ");
        Debug.Log(subBattleCount[2]);
        Debug.Log("Lobby = ");
        Debug.Log(subDecorCount[0]);
        Debug.Log("Library = ");
        Debug.Log(subDecorCount[1]);*/

        //Инициализируем интерактивные сетки комнат
        foreach (var room in rooms)
        {
            room.initGrid();
        }

        //Расставляем интерактивные объекты
        foreach (var room in rooms) 
        {
            List<GameObject> intObjList = new List<GameObject>(); //Список интерактивных объектов комнаты   
            //List<InteractiveObjectList> iterObjList = new List<InteractiveObjectList>();
            int intObjAmount = 0; //Число интерактивных объектов в комнате

            switch (room.Type)
            {
                case RoomType.Entrance:
                    intObjList = room.subTypeRoomObj.GetComponent<EntranceRoomSub>().interactiveObjects;
                    intObjAmount = room.bounds.size.x * room.bounds.size.y + 1;
                    break;
                case RoomType.Exit:
                    intObjList = room.subTypeRoomObj.GetComponent<ExitRoomSub>().interactiveObjects;
                    intObjAmount = room.bounds.size.x * room.bounds.size.y + 1;
                    break;
                case RoomType.Battle:
                    intObjList = room.subTypeRoomObj.GetComponent<BattleRoomSub>().interactiveObjects;
                    intObjAmount = room.bounds.size.x * room.bounds.size.y + 1;
                    break;
                case RoomType.Decorative:
                    intObjList = room.subTypeRoomObj.GetComponent<DecorRoomSub>().interactiveObjects;
                    intObjAmount = room.bounds.size.x * room.bounds.size.y + 1;
                    break;
            }
            /*GameObject go = Instantiate(roomsPrefabs.Find(x => x.name == roomType), 
                                        new Vector3(room.bounds.position.x + 0.5f * room.bounds.size.x, 0.5f, 
                                        room.bounds.position.y + 0.5f * room.bounds.size.y), Quaternion.identity);*/

            bool add = false; //Смогли ли добавить объект в виртуальный список
            float maxProbability = 0.0f;
            int indexInIntObjList = 0; //Если мы прошли и не добавили ни один объект - добавим объект с наибольшей вероятностью

            //Составляем виртуальные списки интерактивных объектов для каждой комнаты 
            for (int i = 0; i < intObjAmount; i++) 
            {
                if (intObjList.Count <= 0)
                {
                    break;
                }

                //GameObject maxProbIntObj;
                add = false;
                maxProbability = 0.0f;
                indexInIntObjList = 0;

                foreach (var intObj in intObjList) //Смотрим каждый вид объекта в порядке списка 
                {
                    InteractiveObject iterObj = intObj.GetComponent<InteractiveObject>();

                    if (room.iterObjList.Find(x => x.interactiveObj.name == intObj.name) != null)
                    { //Проверяем есть ли объект в виртуальном списке
                        if (room.iterObjList.Find(x => x.interactiveObj.name == intObj.name).Count() < iterObj.maxNum) 
                        { //Проверяем не превышено ли максимальное количество данного объекта в комнате
                            if ((((float)random.Next(0, 101) / 100.0f) <= iterObj.probability))
                            { //Проверяем вероятность
                                //Увеличили количество объектов данного типа в виртуальном списке
                                room.iterObjList.Find(x => x.interactiveObj.name == intObj.name).AddIntObj(Vector2Int.zero, Vector3.zero);
                                add = true;
                                break;
                            }
                            else if (iterObj.probability > maxProbability)
                            { //Проверяем максимальна ли его вероятность
                                maxProbability = iterObj.probability;
                                indexInIntObjList = intObjList.IndexOf(intObj);
                                //maxProbIntObj = intObj;
                            }
                        }
                        else //Переходим к следующему виду объекта
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (((float)random.Next(0, 101) / 100.0f) <= iterObj.probability)
                        { //Проверяем вероятность
                            //Добавили новый объект в виртуальный список
                            room.iterObjList.Add(new InteractiveObjectList(intObj, Vector2Int.zero, Vector3.zero));
                            //Debug.Log(intObj.name);
                            add = true;
                            break;
                        }
                        else if (iterObj.probability > maxProbability)
                        { //Проверяем максимальна ли его вероятность  
                            maxProbability = iterObj.probability;
                            indexInIntObjList = intObjList.IndexOf(intObj);
                            //maxProbIntObj = intObj;
                            /*Debug.Log("maxProbability = ");
                            Debug.Log(maxProbability);
                            Debug.Log("intObj = ");
                            Debug.Log(intObj);*/
                        }
                    }                   
                }

                if (!add && maxProbability > 0)
                {
                   /* Debug.Log(room.subType);
                    Debug.Log("intObjList.Count");
                    Debug.Log(intObjList.Count);
                    Debug.Log("indexInIntObjList");
                    Debug.Log(indexInIntObjList);
                    Debug.Log("iterObjList");
                    Debug.Log(iterObjList.Count);
                    if(iterObjList.Count > 0)
                    {
                        Debug.Log(iterObjList[0].interactiveObj.name);
                    }*/
                    //Debug.Log(intObjList[indexInIntObjList].name);
                    if (room.iterObjList.Find(x => x.interactiveObj.name == intObjList[indexInIntObjList].name) != null)
                    { //Если такой вид объекта есть - увеличим его количество
                        room.iterObjList.Find(x => x.interactiveObj.name == intObjList[indexInIntObjList].name).AddIntObj(Vector2Int.zero, Vector3.zero);
                        add = true;
                    }
                    else
                    { //Если нет - добавим в виртуальный список
                        room.iterObjList.Add(new InteractiveObjectList(intObjList[indexInIntObjList], Vector2Int.zero, Vector3.zero));
                        add = true;
                    }
                }
            }

            /*Debug.Log(room.subType);
            Debug.Log("x");
            Debug.Log(room.bounds.size.x);
            Debug.Log("y");
            Debug.Log(room.bounds.size.y);
            Debug.Log("objects");
            foreach (var intObj in iterObjList)
            {
                Debug.Log(intObj.interactiveObj.name);
                Debug.Log(intObj.location.Count);
            }*/                   

            bool placed = false;

            //Расставляем объекты виртуального списка на сетке
            foreach (var intObjType in room.iterObjList)
            //for (int i = 0; i < iterObjList.Count; i++) 
            { //Для каждого вида объекта
                InteractiveObject iterObj = intObjType.interactiveObj.GetComponent<InteractiveObject>();               

                for (int i = 0; i < intObjType.Count(); i++)
                { //Для каждого объекта данного вида
                    placed = false;
                    Vector2Int location = new Vector2Int(0, 0); //Позиция на интерактивной сетке комнаты

                    for (int j = 0; j < intObjCount; j++)
                    { //Попытка поставить
                        switch (iterObj.intObjPlaceType)
                        {
                            case InteractiveObjPlaceType.Free:
                                location.x = random.Next(0, room.grid.Size.x);
                                location.y = random.Next(0, room.grid.Size.y);
                                if (room.grid[location.x, location.y] == ObjectCellType.None && room.CheckObjSpace(location, iterObj.space))
                                { //Ставим координаты и ротацию
                                    intObjType.SetIntObj(i, location, intObjType.side[i]);
                                    room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                    placed = true;
                                }
                                //else continue;
                                break;
                            case InteractiveObjPlaceType.Central:
                                location.x = room.grid.Size.x / 2;
                                location.y = room.grid.Size.y / 2;
                                if (room.grid[location.x, location.y] == ObjectCellType.None && room.CheckObjSpace(location, iterObj.space))
                                { //Ставим координаты и ротацию
                                    intObjType.SetIntObj(i, location, intObjType.side[i]);
                                    room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                    placed = true;
                                }
                                else //Центр уже занят
                                {

                                    Debug.Log("Center Zaniat!!!");
                                    j = intObjCount;
                                    //continue;
                                }
                                break;
                            case InteractiveObjPlaceType.NearWall:
                                int sideX = random.Next(0, 4); // 0 - left 1 - up 2 - right 3 - down
                                location.x = random.Next(0, room.grid.Size.x);
                                location.y = random.Next(0, room.grid.Size.y);
                                switch (sideX)
                                {
                                    case 0:
                                        location.x = 0;
                                        break;
                                    case 1:
                                        location.x = room.grid.Size.x - 1;
                                        break;
                                    case 2:
                                        location.y = 0;
                                        break;
                                    case 3:
                                        location.y = room.grid.Size.y - 1;
                                        break;
                                }                             
                                if (room.grid[location.x, location.y] == ObjectCellType.None && room.CheckObjSpace(location, iterObj.space))
                                { //Ставим координаты и ротацию
                                    if(location.x == 0) //Левая стена
                                    {
                                        intObjType.SetIntObj(i, location, v3lurd.directs[2]);
                                        room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                        placed = true;
                                    }
                                    else if (location.x != 0) //Правая стена
                                    {
                                        intObjType.SetIntObj(i, location, v3lurd.directs[0]);
                                        room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                        placed = true;
                                    }
                                    else if (location.y == 0) //Задняя стена
                                    {
                                        intObjType.SetIntObj(i, location, v3lurd.directs[3]);
                                        room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                        placed = true;
                                    }
                                    else if (location.y != 0) //Передняя стена
                                    {
                                        intObjType.SetIntObj(i, location, v3lurd.directs[1]);
                                        room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                        placed = true;
                                    }
                                    else
                                    {
                                        intObjType.SetIntObj(i, location, v3lurd.directs[2]);
                                        room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                        placed = true;
                                    }
                                }
                                //else continue;
                                break;
                            case InteractiveObjPlaceType.NoWall:
                                location.x = random.Next(1, room.grid.Size.x - 1); //Кроме позиций у стен
                                location.y = random.Next(1, room.grid.Size.y - 1);
                                if (room.grid[location.x, location.y] == ObjectCellType.None && room.CheckObjSpace(location, iterObj.space))
                                { //Ставим координаты и ротацию
                                    intObjType.SetIntObj(i, location, intObjType.side[i]);
                                    room.SetObjectGridPosition(location, iterObj.space, ObjectCellType.Interactive);
                                    placed = true;
                                }
                                //else continue;
                                break;
                        }

                        if(placed)
                        {
                            break;
                        }

                        if (j >= intObjCount - 1 && !placed)
                        { //Удаляем если не получилось поставить
                            intObjType.DeleteIntObj(i);
                            i -= 1;

                            /*if(intObjType.Count() <= 0)
                            {
                                Debug.Log("REMOVEINTOBJ");
                                iterObjList.Remove(intObjType);
                            }*/
                        }
                    }
                }
            }

            //Спауним объекты из полученного списка на сцене
            /*foreach (var intObj in room.iterObjList)
            {
                for (int i = 0; i < intObj.Count(); i++)
                {
                    /*Debug.Log("intObj.location[i].x");
                    Debug.Log(intObj.location[i].x);
                    Debug.Log("intObj.location[i].y");
                    Debug.Log(intObj.location[i].y);
                    Debug.Log("room.bounds.position.x");
                    Debug.Log(room.bounds.position.x);
                    Debug.Log("room.bounds.position.x + intObj.location[i].x * 0.25f");
                    Debug.Log(room.bounds.position.x + intObj.location[i].x * 0.25f);
                    Debug.Log("room.bounds.position.y");
                    Debug.Log(room.bounds.position.y);
                    Debug.Log("room.bounds.position.y + intObj.location[i].y * 0.25f");
                    Debug.Log(room.bounds.position.y + intObj.location[i].y * 0.25f);
                    GameObject go = Instantiate(intObj.interactiveObj,
                                        new Vector3(room.bounds.position.x + 0.25f + intObj.location[i].x * 0.25f,
                                                    0.0f, room.bounds.position.y + 0.25f + intObj.location[i].y * 0.25f),
                                        Quaternion.Euler(intObj.side[i]), room.roomObj.transform); //side = v3lurd.directs[i]

                    InteractiveObject intObjInfo = go.GetComponent<InteractiveObject>();

                    switch (room.Type)
                    { //Добавляем в соотетствующий список объектов комнаты
                        case RoomType.Entrance:
                            break;
                        case RoomType.Exit:
                            break;
                        case RoomType.Battle:
                            if (intObjInfo.intObjType == InteractiveObjType.Guard)
                            {
                                room.roomObj.GetComponent<BattleRoom>().guards.Add(go.GetComponent<Enemy>());
                            }
                            else if (intObjInfo.intObjType == InteractiveObjType.Stash)
                            {
                                room.roomObj.GetComponent<BattleRoom>().stashs.Add(go);
                            }
                            else
                            {
                                room.roomObj.GetComponent<BattleRoom>().other.Add(go);
                            }
                            break;
                        case RoomType.Decorative:
                            if (intObjInfo.intObjType == InteractiveObjType.Stash)
                            {
                                room.roomObj.GetComponent<DecorRoom>().stashs.Add(go);
                            }
                            else
                            {
                                room.roomObj.GetComponent<DecorRoom>().other.Add(go);
                            }
                            break;
                    }
                    //go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);  
                }
            }*/
        }

        /*GameObject FindSubTypeRoomObj(Room room, )
        {
            List<GameObject> subTypeRoomsList = 
            foreach (var subTypeRoomObj in subTypeRoomsList)
            {
                if(room.subType == subTypeRoomObj.)
                {
                    return subTypeRoomObj;
                }
            }
        }*/
    }

    public void SpawnInteractiveObjects(List<Room> rooms)
    {
       foreach (var room in rooms)
        {
            //Спауним объекты из полученного списка на сцене
            foreach (var intObj in room.iterObjList)
            {
                for (int i = 0; i < intObj.Count(); i++)
                {
                    /*Debug.Log("intObj.location[i].x");
                    Debug.Log(intObj.location[i].x);
                    Debug.Log("intObj.location[i].y");
                    Debug.Log(intObj.location[i].y);
                    Debug.Log("room.bounds.position.x");
                    Debug.Log(room.bounds.position.x);
                    Debug.Log("room.bounds.position.x + intObj.location[i].x * 0.25f");
                    Debug.Log(room.bounds.position.x + intObj.location[i].x * 0.25f);
                    Debug.Log("room.bounds.position.y");
                    Debug.Log(room.bounds.position.y);
                    Debug.Log("room.bounds.position.y + intObj.location[i].y * 0.25f");
                    Debug.Log(room.bounds.position.y + intObj.location[i].y * 0.25f);*/
                    GameObject go = Instantiate(intObj.interactiveObj,
                                        new Vector3(room.bounds.position.x + 0.25f + intObj.location[i].x * 0.25f,
                                                    0.035f, room.bounds.position.y + 0.25f + intObj.location[i].y * 0.25f),
                                        Quaternion.Euler(intObj.side[i]), room.roomObj.transform); //side = v3lurd.directs[i]

                    InteractiveObject intObjInfo = go.GetComponent<InteractiveObject>();

                    switch (room.Type)
                    { //Добавляем в соотетствующий список объектов комнаты
                        case RoomType.Entrance:
                            break;
                        case RoomType.Exit:
                            break;
                        case RoomType.Battle:
                            if (intObjInfo.intObjType == InteractiveObjType.Guard)
                            {
                                room.roomObj.GetComponent<BattleRoom>().guards.Add(go.GetComponent<Enemy>());
                                go.GetComponent<Enemy>().guardListIndex = room.roomObj.GetComponent<BattleRoom>().guards.Count - 1;
                            }
                            else if (intObjInfo.intObjType == InteractiveObjType.Stash)
                            {
                                room.roomObj.GetComponent<BattleRoom>().stashs.Add(go.GetComponent<Chest>());
                            }
                            else
                            {
                                room.roomObj.GetComponent<BattleRoom>().other.Add(go);
                            }
                            break;
                        case RoomType.Decorative:
                            if (intObjInfo.intObjType == InteractiveObjType.Stash)
                            {
                                room.roomObj.GetComponent<DecorRoom>().stashs.Add(go);
                            }
                            else
                            {
                                room.roomObj.GetComponent<DecorRoom>().other.Add(go);
                            }
                            break;
                    }
                    //go.GetComponent<Transform>().localScale = new Vector3(1, 1, 1);  
                }
            }
        }
    }

    RoomSubType FindBattleLargestPossible(Room room, List<Room> rooms, int[] _sub_type_count, DungeonInfo dungeonInfo) 
    {
        Vector2Int maxSize = new Vector2Int(0, 0);
        int maxIndex = 0;
        RoomSubType _sub_type = RoomSubType.Arena;
        GameObject subTypeRoom = BattleRoomSubtypes[0]; //Arena

        foreach (var battleRoom in BattleRoomSubtypes) //Находим самую большую из подходящих
        {
            BattleRoomSub battleRoomSub = battleRoom.GetComponent<BattleRoomSub>();
            int battleRoomIndex = BattleRoomSubtypes.IndexOf(battleRoom); //Индекс из списка BattleRoomSubtypes
            if (battleRoomSub.maxPercentage >= (float)_sub_type_count[battleRoomIndex] / (float)dungeonInfo.battleRoomNum) //Проверка на превышение процента подтипа от общего
            {
                if (battleRoomSub.minRoomSize.x <= room.bounds.size.x &&
                    battleRoomSub.minRoomSize.y <= room.bounds.size.y &&
                    battleRoomSub.maxRoomSize.x >= room.bounds.size.x &&
                    battleRoomSub.maxRoomSize.y >= room.bounds.size.y &&
                    battleRoomSub.minRoomSize.x > maxSize.x &&
                    battleRoomSub.minRoomSize.y > maxSize.y)
                {
                    maxSize = battleRoomSub.minRoomSize;
                    _sub_type = battleRoomSub.subType;
                    maxIndex = battleRoomIndex;
                    subTypeRoom = battleRoom;
                }
            }
        }

        _sub_type_count[maxIndex] += 1;
        room.subTypeRoomObj = subTypeRoom;
        return _sub_type;
    }

    /*RoomSubType FindBattleLargestPossible(Room room, List<Room> rooms, DungeonInfo dungeonInfo) // Старый метод пищущий инфу в префабы
    {
        Vector2Int maxSize = new Vector2Int(0, 0);
        RoomSubType _sub_type = RoomSubType.Arena;
        int _sub_type_index = 0;

        foreach (var battleRoom in BattleRoomSubtypes) //Находим самую большую из подходящих
        {
            BattleRoomSub battleRoomSub = battleRoom.GetComponent<BattleRoomSub>();
            if (battleRoomSub.maxPercentage <= (float)battleRoomSub.Count / (float)dungeonInfo.battleRoomNum)
            {
                battleRoomSub.Limit = true;
                Debug.Log(battleRoomSub.subType);
                Debug.Log(battleRoomSub.Count);
            }
            else if (!battleRoomSub.Limit &&
                battleRoomSub.minRoomSize.x <= room.bounds.size.x &&
                battleRoomSub.minRoomSize.y <= room.bounds.size.y &&
                battleRoomSub.maxRoomSize.x >= room.bounds.size.x &&
                battleRoomSub.maxRoomSize.y >= room.bounds.size.y &&
                battleRoomSub.minRoomSize.x > maxSize.x &&
                battleRoomSub.minRoomSize.y > maxSize.y)
            {
                maxSize = battleRoomSub.minRoomSize;
                _sub_type = battleRoomSub.subType;
                _sub_type_index = BattleRoomSubtypes.IndexOf(battleRoom);
                //battleRoomSub.Count += 1;
                Debug.Log(maxSize);
                Debug.Log(_sub_type);
                Debug.Log(battleRoomSub.Count);
            }
        }

        BattleRoomSubtypes[_sub_type_index].GetComponent<BattleRoomSub>().Count += 1;
        return _sub_type;
    }*/

    RoomSubType FindDecorLargestPossible(Room room, List<Room> rooms, int[] _sub_type_count, DungeonInfo dungeonInfo) 
    {
        Vector2Int maxSize = new Vector2Int(0, 0);
        int maxIndex = 0;
        RoomSubType _sub_type = RoomSubType.Lobby;
        GameObject subTypeRoom = DecorRoomSubtypes[0]; //Lobby

        foreach (var decorRoom in DecorRoomSubtypes) //Находим самую большую из подходящих
        {
            DecorRoomSub decorRoomSub = decorRoom.GetComponent<DecorRoomSub>();
            int decorRoomIndex = DecorRoomSubtypes.IndexOf(decorRoom); //Индекс из списка BattleRoomSubtypes
            if (decorRoomSub.maxPercentage >= (float)_sub_type_count[decorRoomIndex] / (float)dungeonInfo.decorRoomNum) //Проверка на превышение процента подтипа от общего
            {
                if (decorRoomSub.minRoomSize.x <= room.bounds.size.x &&
                    decorRoomSub.minRoomSize.y <= room.bounds.size.y &&
                    decorRoomSub.maxRoomSize.x >= room.bounds.size.x &&
                    decorRoomSub.maxRoomSize.y >= room.bounds.size.y &&
                    decorRoomSub.minRoomSize.x > maxSize.x &&
                    decorRoomSub.minRoomSize.y > maxSize.y)
                {
                    maxSize = decorRoomSub.minRoomSize;
                    _sub_type = decorRoomSub.subType;
                    maxIndex = decorRoomIndex;
                    subTypeRoom = decorRoom;
                }
            }
        }

        _sub_type_count[maxIndex] += 1;
        room.subTypeRoomObj = subTypeRoom;
        return _sub_type;
    }
}