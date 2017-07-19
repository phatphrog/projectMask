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

        //remove the wallregions
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize) //remove room regions that are made up of less tiles than the roomthreshold size
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }

        }
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
                    //1 = wall
                    //0 = open space
                    map[x, y] = 1;
                }
                map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
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
