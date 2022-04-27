using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    public GameObject hexPrefab;

    public int gridWidth = 11;
    public int gridHeight = 11;

    float hexWidth = 0.9f;
    float hexHeight = 0.8f;
    public float gap = 0.0f;

    ArenaNode[,] arenaNodes;
    public GameObject bulbasaur;
    List<Pokemon> pokemonsA;
    List<Pokemon> pokemonsB;

    public List<poke> pokesA;
    public List<poke> pokesB;


    Vector3 startPos;

    public enum Team
    {
        A, B
    };

    public enum poke
    {
        Bulbasaur, Charmander, Squirtle
    };


    void Start()
    {
        arenaNodes = new ArenaNode[gridWidth, gridHeight];
        AddGap();
        CalcStartPos();
        CreateGrid();
        SpawnPokemon();
        InitiateBattle();
    }

    void AddGap()
    {
        hexWidth += hexWidth * gap;
        hexHeight += hexHeight * gap;
    }

    void CalcStartPos()
    {
        float offset = 0;
        if (gridHeight / 2 % 2 != 0)
            offset = hexWidth / 2;

        float x = -hexWidth * (gridWidth / 2) - offset;
        float y = hexHeight * 0.75f * (gridHeight / 2);

        startPos = new Vector3(x, y, 0);
    }

    Vector3 CalcWorldPos(Vector2 gridPos)
    {
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;

        float x = startPos.x + gridPos.x * hexWidth + offset;
        float y = startPos.z - gridPos.y * hexHeight * 0.75f;

        return new Vector3(x, y, 0);
    }

    void CreateGrid()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {

                GameObject hex = Instantiate(hexPrefab);
                Vector2 gridPos = new Vector2(x, y);
                hex.transform.position = CalcWorldPos(gridPos);
                hex.transform.parent = this.transform;
                ArenaNode anode = hex.GetComponent<ArenaNode>();
                arenaNodes[x, y] = anode;
                anode.x = x;
                anode.y = y;
            }
        }
    }

    void SpawnPokemon()
    {
        pokemonsA = new List<Pokemon>();
        pokemonsB = new List<Pokemon>();
        
        foreach(poke p in pokesA)
        {
            if(p == poke.Bulbasaur)
            {
                GameObject poke = Instantiate(bulbasaur);
                Pokemon pp = poke.GetComponent<Pokemon>();
                pokemonsA.Add(pp);
                pp.myNode = randomValidNode(0, gridWidth, 0, 2);
                pp.transform.position = pp.myNode.transform.position;
                pp.myTeam = Team.A;
                pp.myArena = this;
            }
        }
        foreach (poke p in pokesB)
        {
            if (p == poke.Bulbasaur)
            {
                GameObject poke = Instantiate(bulbasaur);
                Pokemon pp = poke.GetComponent<Pokemon>();
                pokemonsB.Add(pp);
                pp.myNode = randomValidNode(0, gridWidth, 5, 7);
                pp.transform.position = pp.myNode.transform.position;
                pp.myTeam = Team.B;
                pp.myArena = this;

            }
        }

        
        
    }

    void InitiateBattle()
    {
        foreach(Pokemon p in pokemonsA)
        {
            p.battle();
        }
        foreach (Pokemon p in pokemonsB)
        {
            //p.battle();
        }
    }

    ArenaNode randomValidNode(int x1, int x2, int y1, int y2)
    {
        int i = Random.Range(x1, x2);
        int j = Random.Range(y1, y2);
        return arenaNodes[i, j].occupied ? randomValidNode(x1, x2, y1, y2) : arenaNodes[i, j];
    }

    //Method to select the target ArenaNode to get to for the pokemons range
    public ArenaNode moveInRange(ArenaNode myNode, ArenaNode neighbourNode,Pokemon target, int range)
    {
        //Debug.Log("moveInRange: " + myNode.x + "," + myNode.y + "  ->   " + neighbourNode.x + "," + neighbourNode.y + " ::" + range);
        if(distance_BFS(myNode, target.myNode) <= range)
        {
            //Debug.Log("Quack");
            return myNode;
        }
        return next_node_BFS(myNode, neighbourNode);
    }

    //Select Valid Target
    public Pokemon getClosestTarget(Team team, ArenaNode myNode)
    {
        //Debug.Log("Getting closest target");
        if(team == Team.A)
        {
            int minDistance = 1000;
            int index = 0;
            int step = -1;
            foreach (Pokemon p in pokemonsB)
            {
                step++;
                int distance = distance_BFS(myNode, p.myNode);
                if (distance <= minDistance)
                {
                    minDistance = distance;
                    index = step;
                }
            }

            return pokemonsB[index];
        }
        if(team == Team.B)
        {
            int minDistance = 1000;
            int index = 0;
            int step = -1;
            foreach (Pokemon p in pokemonsA)
            {
                step++;
                int distance = distance_BFS(myNode, p.myNode);
                if (distance <= minDistance)
                {
                    minDistance = distance;
                    index = step;
                }
            }

            return pokemonsA[index];
        }

        return null;

    }

    //BFS algorithm to give next arena node to move to, called every update to ensure pokemon move to eachother not their old spots
    ArenaNode next_node_BFS(ArenaNode currNode, ArenaNode targetNode)
    {
        
        bool[,] visited = new bool[gridWidth, gridHeight];
        
        List<ArenaNode> path = new List<ArenaNode>();
        Queue<List<ArenaNode>> myq = new Queue<List<ArenaNode>>();
        path.Add(currNode);
        myq.Enqueue(path);

        while(myq.Count > 0)
        {
            List<ArenaNode> currPath = myq.Dequeue();
            ArenaNode at = currPath[currPath.Count - 1];
           
            if(visited[at.x, at.y])
            {
                continue;
            }
            visited[at.x, at.y] = true;

            if(at == targetNode)
            {
                
                return currPath[1];
            }

            //left
            if(validCords(at.x - 1, at.y))
            {

                if(!visited[at.x - 1, at.y])
                {
                    if(!arenaNodes[at.x - 1, at.y].occupied)
                    {
                        List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                        newPath.Add(arenaNodes[at.x - 1, at.y]);
                        myq.Enqueue(newPath);
                    }
                }
                
            }

            //right
            if (validCords(at.x + 1, at.y))
            {
                if (!visited[at.x + 1, at.y])
                {
                    if (!arenaNodes[at.x + 1, at.y].occupied)
                    {
                        List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                        newPath.Add(arenaNodes[at.x + 1, at.y]);
                        myq.Enqueue(newPath);
                    }
                }
                
            }
            //topLeft
            if (validCords(topLeft(at.x, at.y), at.y - 1))
            {
                if (!visited[topLeft(at.x, at.y), at.y - 1])
                {
                    if (!arenaNodes[topLeft(at.x, at.y), at.y - 1].occupied)
                    {
                        List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                        newPath.Add(arenaNodes[topLeft(at.x, at.y), at.y - 1]);
                        myq.Enqueue(newPath);
                    }
                }
                
            }
            //topRight
            if (validCords(topRight(at.x, at.y), at.y - 1))
            {
                if (!visited[topRight(at.x, at.y), at.y - 1])
                {
                    if (!arenaNodes[topRight(at.x, at.y), at.y - 1].occupied)
                    {
                        List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                        newPath.Add(arenaNodes[topRight(at.x, at.y), at.y - 1]);
                        myq.Enqueue(newPath);
                    }
                }
                
            }
            //botLeft
            if (validCords(topLeft(at.x, at.y), at.y + 1))
            {
                if (!visited[topLeft(at.x, at.y), at.y + 1])
                {
                    if (!arenaNodes[topLeft(at.x, at.y), at.y + 1].occupied)
                    {
                        List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                        newPath.Add(arenaNodes[topLeft(at.x, at.y), at.y + 1]);
                        myq.Enqueue(newPath);
                    }
                }
                
            }
            //botRight
            if (validCords(topRight(at.x, at.y), at.y + 1))
            {
                if (!visited[topRight(at.x, at.y), at.y + 1])
                {
                    if (!arenaNodes[topRight(at.x, at.y), at.y + 1].occupied)
                    {
                        List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                        newPath.Add(arenaNodes[topRight(at.x, at.y), at.y + 1]);
                        myq.Enqueue(newPath);
                    }
                }
                
            }


        }

        return currNode;

    }

    public int distance_BFS(ArenaNode currNode, ArenaNode targetNode)
    {

        bool[,] visited = new bool[gridWidth, gridHeight];

        List<ArenaNode> path = new List<ArenaNode>();
        
        Queue<List<ArenaNode>> myq = new Queue<List<ArenaNode>>();
        path.Add(currNode);
        myq.Enqueue(path);

        while (myq.Count > 0)
        {
            List<ArenaNode> currPath = myq.Dequeue();
            ArenaNode at = currPath[currPath.Count - 1];
            

            if (visited[at.x, at.y])
            {
                continue;
            }
            //Debug.Log("At: " + at.x + "," + at.y);
            visited[at.x, at.y] = true;

            if (at.x == targetNode.x && at.y == targetNode.y)
            {
                //Debug.Log("Distance: " + (currPath.Count - 1));
                return currPath.Count - 1;
            }

            //left
            if (validCords(at.x - 1, at.y))
            {

                if (!visited[at.x - 1, at.y])
                {
                    List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                    newPath.Add(arenaNodes[at.x - 1, at.y]);
                    myq.Enqueue(newPath);
                }

            }

            //right
            if (validCords(at.x + 1, at.y))
            {
                if (!visited[at.x + 1, at.y])
                {
                    List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                    newPath.Add(arenaNodes[at.x + 1, at.y]);
                    myq.Enqueue(newPath);
                }

            }
            //topLeft
            if (validCords(topLeft(at.x, at.y), at.y - 1))
            {
                if (!visited[topLeft(at.x, at.y), at.y - 1])
                {
                    List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                    newPath.Add(arenaNodes[topLeft(at.x, at.y), at.y - 1]);
                    myq.Enqueue(newPath);
                }

            }
            //topRight
            if (validCords(topRight(at.x, at.y), at.y - 1))
            {
                if (!visited[topRight(at.x, at.y), at.y - 1])
                {
                    List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                    newPath.Add(arenaNodes[topRight(at.x, at.y), at.y - 1]);
                    myq.Enqueue(newPath);
                }

            }
            //botLeft
            if (validCords(topLeft(at.x, at.y), at.y + 1))
            {
                if (!visited[topLeft(at.x, at.y), at.y + 1])
                {
                    List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                    newPath.Add(arenaNodes[topLeft(at.x, at.y), at.y + 1]);
                    myq.Enqueue(newPath);
                }

            }
            //botRight
            if (validCords(topRight(at.x, at.y), at.y + 1))
            {
                if (!visited[topRight(at.x, at.y), at.y + 1])
                {
                    List<ArenaNode> newPath = new List<ArenaNode>(currPath);
                    newPath.Add(arenaNodes[topRight(at.x, at.y), at.y + 1]);
                    myq.Enqueue(newPath);
                }

            }


        }

        //Debug.Log("Distance0");
        return 0;
    }

    public ArenaNode getClosestEmptyNeighbour(ArenaNode myNode, Pokemon target)
    {
        ArenaNode minNode = target.myNode;
        int min = 100;

        int x = target.myNode.x;
        int y = target.myNode.y;

        //left
        if (validCords(x - 1, y))
        {

            if (!arenaNodes[x - 1, y].occupied)
            {
                int d = distance_BFS(myNode, arenaNodes[x - 1, y]);
                if(d < min)
                {
                    min = d;
                    minNode = arenaNodes[x - 1, y];
                }
            }

        }

        //right
        if (validCords(x + 1, y))
        {

            if (!arenaNodes[x + 1, y].occupied)
            {
                int d = distance_BFS(myNode, arenaNodes[x + 1, y]);
                if (d < min)
                {
                    min = d;
                    minNode = arenaNodes[x + 1, y];
                }
            }
        }
        //topLeft
        if (validCords(topLeft(x, y), y - 1))
        {
            if (!arenaNodes[topLeft(x, y), y - 1].occupied)
            {
                int d = distance_BFS(myNode, arenaNodes[topLeft(x, y), y - 1]);
                if (d < min)
                {
                    min = d;
                    minNode = arenaNodes[topLeft(x, y), y - 1];
                }
            }

        }
        //topRight
        if (validCords(topRight(x, y), y - 1))
        {
            if (!arenaNodes[topRight(x, y), y - 1].occupied)
            {
                int d = distance_BFS(myNode, arenaNodes[topRight(x, y), y - 1]);
                if (d < min)
                {
                    min = d;
                    minNode = arenaNodes[topRight(x, y), y - 1];
                }
            }

        }
        //botLeft
        if (validCords(topLeft(x, y), y + 1))
        {
            if (!arenaNodes[topLeft(x, y), y + 1].occupied)
            {
                int d = distance_BFS(myNode, arenaNodes[topLeft(x, y), y + 1]);
                if (d < min)
                {
                    min = d;
                    minNode = arenaNodes[topLeft(x, y), y + 1];
                }
            }

        }
        //botRight
        if (validCords(topRight(x, y), y + 1))
        {
            if (!arenaNodes[topRight(x, y), y + 1].occupied)
            {
                int d = distance_BFS(myNode, arenaNodes[topRight(x, y), y + 1]);
                if (d < min)
                {
                    min = d;
                    minNode = arenaNodes[topRight(x, y), y + 1];
                }
            }

        }

        return minNode;
    }
    //Extra Functions for Search
    bool validCords(int x, int y)
    {
        return (x >= 0) && (x < gridWidth) && (y >= 0) && (y < gridHeight);
    }

    int topRight(int x, int y)
    {
        if(y%2 == 0)
        {
            return x;
        }
        return x + 1;
    }

    int topLeft(int x, int y)
    {
        if (y % 2 == 0)
        {
            return x - 1;
        }
        return x;
    }

    bool sameNode(ArenaNode a, ArenaNode b)
    {
        return (a.x == b.x) && (a.y == b.y);
    }

}
