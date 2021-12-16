using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Debug = UnityEngine.Debug;
public class Pathfinding : MonoBehaviour {

    Grid GridReference;//For referencing the grid class
    WriteToCSVFile writeToCsv;
    public Transform StartPosition;//Starting position to pathfind from
    public Transform TargetPosition;//Starting position to pathfind to
    public GameObject csvObject;
    private Text statusText;
    private float endTime;
    private float startTime;

    
    private void Awake()//When the program starts
    {
        statusText = GameObject.Find("Status Text").GetComponent<Text>();
        GridReference = GetComponent<Grid>();//Get a reference to the game manager
        writeToCsv = csvObject.GetComponent<WriteToCSVFile>();
    }

        void Start() 
        {
            FindPath(StartPosition.position, TargetPosition.position);
            startTime = Time.realtimeSinceStartup;
        }
    
    // private void Update()//Every frame
    // {
    //     FindPath(StartPosition.position, TargetPosition.position);//Find a path to the goal
    // }

    void FindPath(Vector3 a_StartPos, Vector3 a_TargetPos)
    {
        Node StartNode = GridReference.NodeFromWorldPoint(a_StartPos);//Gets the node closest to the starting position
        Node TargetNode = GridReference.NodeFromWorldPoint(a_TargetPos);//Gets the node closest to the target position

        List<Node> OpenList = new List<Node>();//List of nodes for the open list
        HashSet<Node> ClosedList = new HashSet<Node>();//Hashset of nodes for the closed list

        OpenList.Add(StartNode);//Add the starting node to the open list to begin the program

        while(OpenList.Count > 0)//Whilst there is something in the open list
        {
            Node CurrentNode = OpenList[0];//Create a node and set it to the first item in the open list
            for(int i = 1; i < OpenList.Count; i++)//Loop through the open list starting from the second object
            {
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].ihCost < CurrentNode.ihCost)//If the f cost of that object is less than or equal to the f cost of the current node
                {
                    CurrentNode = OpenList[i];//Set the current node to that object
                }
            }
            OpenList.Remove(CurrentNode);//Remove that from the open list
            ClosedList.Add(CurrentNode);//And add it to the closed list

            if (CurrentNode == TargetNode)//If the current node is the same as the target node
            {
                GetFinalPath(StartNode, TargetNode);//Calculate the final path
                GridReference.updateTimer = false;
                Debug.Log("level timer" + GridReference.levelTimer);
                Debug.Log("update timer" + GridReference.updateTimer);
                endTime = (Time.realtimeSinceStartup - startTime);
                statusText.text = "Solved! with " + GridReference.totalNodes + " nodes, cost=" + GridReference.totalcost;
                writeToCsv.WriteCSV("A star", GridReference.levelTimer, GridReference.totalcost, GridReference.totalNodes, endTime);
            }

            foreach (Node NeighborNode in GridReference.GetNeighboringNodes(CurrentNode))//Loop through each neighbor of the current node
            {
                if (!NeighborNode.bIsWall || ClosedList.Contains(NeighborNode))//If the neighbor is a wall or has already been checked
                {
                    continue;//Skip it
                }
                int MoveCost = CurrentNode.igCost + GetManhattenDistance(CurrentNode, NeighborNode);//Get the F cost of that neighbor

                if (MoveCost < NeighborNode.igCost || !OpenList.Contains(NeighborNode))//If the f cost is greater than the g cost or it is not in the open list
                {
                    NeighborNode.igCost = MoveCost;//Set the g cost to the f cost
                    NeighborNode.ihCost = GetManhattenDistance(NeighborNode, TargetNode);//Set the h cost
                    NeighborNode.ParentNode = CurrentNode;//Set the parent of the node for retracing steps


                    if(!OpenList.Contains(NeighborNode))//If the neighbor is not in the openlist
                    {
                        OpenList.Add(NeighborNode);//Add it to the list
                    }
                }
                if (GridReference.updateTimer)
                GridReference.levelTimer += Time.deltaTime;
                Debug.Log("levelTimer" + GridReference.levelTimer);
            }

        }
    }



    void GetFinalPath(Node a_StartingNode, Node a_EndNode)
    {
        List<Node> FinalPath = new List<Node>();//List to hold the path sequentially 
        Node CurrentNode = a_EndNode;//Node to store the current node being checked

        while(CurrentNode != a_StartingNode)//While loop to work through each node going through the parents to the beginning of the path
        {
            FinalPath.Add(CurrentNode);//Add that node to the final path
            GridReference.totalNodes += 1;
            if (GridReference.updateTimer) //Try commenting this the cost value keeps on getting updated even if the goal node is reached
            GridReference.totalcost += GetTotalCost(CurrentNode, CurrentNode.ParentNode);
            CurrentNode = CurrentNode.ParentNode;//Move onto its parent node
            
        }
        
        FinalPath.Reverse();//Reverse the path to get the correct order

        GridReference.FinalPath = FinalPath;//Set the final path

    }
    float GetTotalCost(Node end, Node begin)
    {
        int dx = Mathf.Abs(end.iGridX - begin.iGridX);//x1-x2
        int dy = Mathf.Abs(end.iGridY - begin.iGridY);//y1-y2
        float dist = Mathf.Sqrt(dx*dx + dy*dy);
		float cost = dist;
        return cost;
    }
    int GetManhattenDistance(Node a_nodeA, Node a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.iGridX - a_nodeB.iGridX);//x1-x2
        int iy = Mathf.Abs(a_nodeA.iGridY - a_nodeB.iGridY);//y1-y2

        return ix + iy;//Return the sum
    }
}
