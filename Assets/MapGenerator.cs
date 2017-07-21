using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    void Start()
    {
        GenerateMap();  
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        //experiment with these values
        //different values for i will generate different cave shapes
        //e.g., change to different rule sets after a number of steps
        //then change back
        for (int i = 0; i < 6; i++)
        {
            SmoothMap();
        }

        ProcessMap(); //remove wall regions that are less than a certain number of tiles in size

        int borderSize = 2;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if(x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize )
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    // we are in our bordered area
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }
    
    //using a flood fill algorithm (GetRegionTiles) we remove all wall regions that are below a certain threshold in tile size
    //and we remove room regions that are below a certain threshold in size
    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1); //get wall regions
        int wallThresholdSize = 50;

        //remove the wallregions
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize) //remove wall regions that are made up of less tiles than the wallthreshold size
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }

        }

        List<List<Coord>> roomRegions = GetRegions(0); //get room regions
        int roomThresholdSize = 50;
        //add the rooms that do not get removed (are bigger than the threshold size)
        List<Room> survivingRooms = new List<Room>();

        //remove the wallregions
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize) //remove room regions that are made up of less tiles than the roomthreshold size
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            } else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }

        }

        survivingRooms.Sort();
        /*
        foreach(Room r in survivingRooms)
        {
            //print room sizes in decsending order
            print(r.roomSize);
        }*/

        //survivingRooms[0] is the largest (main) room
        survivingRooms[0].isMain = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessiblityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if(forceAccessiblityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        } else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if(!forceAccessiblityFromMainRoom) {
                possibleConnectionFound = false; // still in the process of considering if any of the other rooms are closer before we make the connections
                if(roomA.connectedRooms.Count > 0)
                {
                    //this room already has a connection
                    continue; //to skip to the next room
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }
               
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            //calling for each of the roomA's - only want to occur if we are not forcing accesibility from the main room
            if(possibleConnectionFound && !forceAccessiblityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        //create passage when we are forcing accesibility from the main room
        //because we have considered all rooms and found the closets passage
        if (possibleConnectionFound && forceAccessiblityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessiblityFromMainRoom)
        {
            //force the accesibility from main room
            //any rooms that are not connected to the main room will be forced to find a connection
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.blue, 100);
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        //convert a coordinate into actual world position
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    //given a certain tile type, return all the regions of that type of tile (wall or not) 
    //list of lists of coords (one list of coords = a region)
    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }


    //flood fill algorithm
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY]; //check if its a wall tile or not

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord (startX, startY));
        mapFlags[startX, startY] = 1; //we've looked at that tile so mark it as a 1

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue(); //return first item in the queue and remove it form the queue
            tiles.Add(tile);

            //check tiles around the tile at x and y
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {

                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if(IsInMapRange(x,y) && (y == tile.tileY || x == tile.tileX)) //add the first && so we don't check diagonal tiles
                    {
                        if(mapFlags[x,y] == 0 && map[x,y] == tileType)
                        {
                            mapFlags[x, y] = 1;//we've looked at this tile
                            queue.Enqueue(new Coord(x, y)); //move onto the next tile
                        }
                    }
                }
            }
        }

        return tiles;
    }

    //check fo a neighbouring tile (all 8 surrounding tiles)
   bool IsInMapRange(int x, int y) {
       return x >= 0 && x < width && y >= 0 && y < height; 
   }

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width-1 || y == 0 || y == height - 1)
                {
                    //create a wall if we are at an edge of the map
                    map[x, y] = 1;
                } else
                {
                    //othewise randomly fill spaces based on randomFillPercent
                    //1 = wall
                    //0 = open spaces
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
                
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);


                if (neighbourWallTiles > 4)
                {
                    //make it into a wall
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {

                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) 
    {
        int wallCount = 0;

        //loop on a 3 x 3 grid around a specific tile at gridX gridY
        //essentially looking at the neighbours around the specified tile in the map
        for (int neighbourX = gridX -1; neighbourX <= gridX + 1; neighbourX ++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {

                if(IsInMapRange(neighbourX, neighbourY)) //check for a neighbouring tile
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        //if its a wall tile (1) wallcount will increase
                        //if its 0 nothing happens (no wall).
                        wallCount += map[neighbourX, neighbourY];
                    }
                } else
                {
                    //we are at the edge of the map
                    //encourage wall growth at the edge of the map
                    wallCount++;
                }
               
            }
        }

        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class Room : IComparable <Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMain; //flag for the MAIN (biggest) room

        public Room()
        {
            //empty room
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();

            //go through all the tiles in our room
            //look each tile's neighbours and if any of the neighbours are wall tiles we will know that they are at the edge of the room
            //looking at a cross around the tile at x,y
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++ )
                {
                    for (int y = tile.tileY -1; y <= tile.tileY+1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if(!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach(Room connectedRoom in connectedRooms)
                {
                    //set rooms connected to main room as accessible
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            //update the accessibility of all the connected rooms when two rooms are connected
            if(roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    /*
    void OnDrawGizmos()
    {
      
        if(map != null)
        {
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
        
    }*/


}
