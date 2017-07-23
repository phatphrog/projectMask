using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter cave;

    List<Vector3> vertices;
    List<int> triangles;

    //given a certain vertex we need to be able to tell which triangles that vertex belongs to
    //so we are going to create a dictionary
    //takes in a key and a value
    //pass in an int for the key = vertexIndex
    //and a list of triangles
    //what we get out is a list of all the triangles that vertex is a part of
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();

    //list to store all of our outlines
    List<List<int>> outlines = new List<List<int>>();

    //make sure we don't check a vertex twice
    //much quicker to do 'contain' checks on a HashSet than a List thereby improving efficiency
    HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] map, float squareSize)
    {

        outlines.Clear();
        checkedVertices.Clear();
        triangleDictionary.Clear();
        
        //NB! testing code - this is for when we reset the map and a collider lready exists on the previous map
        if(walls.gameObject.GetComponent<MeshCollider>())
        {
            Destroy(walls.gameObject.GetComponent<MeshCollider>());
        }
        if (cave.gameObject.GetComponent<MeshCollider>())
        {
            Destroy(cave.gameObject.GetComponent<MeshCollider>());
        }



        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        //add a mesh collider to the landmass we have generated
        MeshCollider landCollider = cave.gameObject.AddComponent<MeshCollider>();
        landCollider.sharedMesh = mesh;

        CreateWallMesh();
        
    }

    void CreateWallMesh()
    {
        //calculate the moutlines to extrude walls!
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]);//left vertex 0
                wallVertices.Add(vertices[outline[i + 1]]);//right vertex 1
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);//bottom left vertex 2
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight);//bottom right vertex 3

                //winding wall triangles anti clockwise::

                //starting with left vertex
                wallTriangles.Add(startIndex + 0);
                //bottom left vertex
                wallTriangles.Add(startIndex + 2);
                //bottom right vertex
                wallTriangles.Add(startIndex + 3);

                //bottom right vertex
                wallTriangles.Add(startIndex + 3);
                //top right vertex
                wallTriangles.Add(startIndex + 1);
                //top left vertex
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        //add a meshcollider to our wall object
        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    } 

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point: all surrounding nodes are walls
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if(points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }

        if(points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }

        if(points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }

        if(points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }


    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        //store the triangle in the dictionary
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if(triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        } else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    //go through every single vertex in the mesh
    //check if its an outline
    //if it is an outline then follow that outline until it meets up with itself again and then add it to the outlines list
    //updates the list of outlines 
    //each oultline is made up of int verex indeces
    void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex ++)
        {
            if(!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if(newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);

                    //once we've gone around the the last index
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);

        //so we never check the same vertex again
        checkedVertices.Add(vertexIndex);

        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if(nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }

    }

    //given an outline vertex can find a connected vertex that forms an outline edge
    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for(int i=0; i <trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];
            //look at each of the 3 vertices in this triangle and compare if they are an outline edge
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                //make sure we dont check the same vertex twice
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                { 
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        //no outline edge found for the vertex
        return -1;
    }
   
    //if vertexA and vertexB only share one common triangle, then it is an outline edge
    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i <trianglesContainingVertexA.Count; i++)
        {
            if(trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if(sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }

        //if there is only one shared triangle it will return true
        //otherwise return false
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        //indexer allows us to access vertices in any triangle
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        //this allows us to access the vertices from our triangle like an array
        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }


    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];

            for (int x = 0; x < nodeCountX; x ++) {
                for (int y = 0; y < nodeCountY; y ++) {
                    Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
                    controlNodes[x,y] = new ControlNode(pos,map[x,y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX -1,nodeCountY -1];
            for (int x = 0; x < nodeCountX-1; x ++) {
                for (int y = 0; y < nodeCountY-1; y ++) {
                    squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
                }
            }

        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (!topLeft.active)
            {
                configuration += 8;
            }

            if (!topRight.active)
            {
                configuration += 4;
            }

            if(!bottomRight.active)
            {
                configuration += 2;
            }

            if (!bottomLeft.active)
            {
                configuration += 1;
            }
        }
    }

	public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;  
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);

        }
    }

    /*
void OnDrawGizmos()
{
    if (squareGrid != null)
    {

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
                Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * .4f);

                Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
                Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * .4f);

                Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
                Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * .4f);

                Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
                Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * .4f);

                Gizmos.color = Color.blue;


                Gizmos.DrawSphere(squareGrid.squares[x, y].centreTop.position, .15f);
                Gizmos.DrawSphere(squareGrid.squares[x, y].centreRight.position, .15f);
                Gizmos.DrawSphere(squareGrid.squares[x, y].centreBottom.position, .15f);
                Gizmos.DrawSphere(squareGrid.squares[x, y].centreLeft.position, .15f);
            }
        }
    }
}*/

}
