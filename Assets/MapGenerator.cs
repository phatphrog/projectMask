using UnityEngine;
using System.Collections;
using System;

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

                if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
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
