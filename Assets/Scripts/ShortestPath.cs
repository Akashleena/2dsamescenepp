using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortestPath : MonoBehaviour
{

    private GameObject[] nodes;
    WriteToCSVFile writeToCsv;
    public GameObject csvObject;
    private float endTime;
    private float startTime;
    public float levelTimer;
	public bool updateTimer=true;
    private void Awake()//When the program starts
    {
        writeToCsv = csvObject.GetComponent<WriteToCSVFile>();
        levelTimer=0.0f;
    }

    /// <summary>
    /// Finding the shortest path and return in a List
    /// </summary>
    /// <param name="start">The start point</param>
    /// <param name="end">The end point</param>
    /// <returns>A List of transform for the shortest path</returns>
    public List<Transform> findShortestPath(Transform start, Transform end)
    {
        startTime = Time.realtimeSinceStartup;

        nodes = GameObject.FindGameObjectsWithTag("Node");

        List<Transform> result = new List<Transform>();
        Transform node = DijkstrasAlgo(start, end);

        // While there's still previous node, we will continue.
        while (node != null)
        {
            result.Add(node);
            DijkstraNode currentNode = node.GetComponent<DijkstraNode>();
            node = currentNode.getParentNode();
        }

        result.Reverse();

        float totalCost = 0;
        // Reverse the list so that it will be from start to end.
        foreach(Transform nd in result) {
            totalCost += nd.GetComponent<DijkstraNode>().getWeight();
        }
        // totalCost *= 0.001f;
        updateTimer = false;
        writeToCsv.WriteCSV("Dijkstra", levelTimer, totalCost, result.Count, endTime);
        return result;
    }

    /// <summary>
    /// Dijkstra Algorithm to find the shortest path
    /// </summary>
    /// <param name="start">The start point</param>
    /// <param name="end">The end point</param>
    /// <returns>The end node</returns>
    private Transform DijkstrasAlgo(Transform start, Transform end)
    {
        // Nodes that are unexplored
        List<Transform> unexplored = new List<Transform>();

        // We add all the nodes we found into unexplored.
        foreach (GameObject obj in nodes)
        {
            DijkstraNode n = obj.GetComponent<DijkstraNode>();
            if (n.isWalkable())
            {
                n.resetNode();
                unexplored.Add(obj.transform);
            }
        }

        // Set the starting node weight to 0;
        DijkstraNode startNode = start.GetComponent<DijkstraNode>();
        startNode.setWeight(0);

        while (unexplored.Count > 0)
        {
            // Sort the explored by their weight in ascending order.
            unexplored.Sort((x, y) => x.GetComponent<DijkstraNode>().getWeight().CompareTo(y.GetComponent<DijkstraNode>().getWeight()));

            // Get the lowest weight in unexplored.
            Transform current = unexplored[0];

            // Note: This is used for games, as we just want to reduce compuation, better way will be implementing A*
            /*
            // If we reach the end node, we will stop.
            if(current == end)
            {   
                return end;
            }*/

            //Remove the node, since we are exploring it now.
            unexplored.Remove(current);

            DijkstraNode currentNode = current.GetComponent<DijkstraNode>();
            List<Transform> neighbours = currentNode.getNeighbourNode();
            foreach (Transform neighNode in neighbours)
            {
                DijkstraNode node = neighNode.GetComponent<DijkstraNode>();

                // We want to avoid those that had been explored and is not walkable.
                if (unexplored.Contains(neighNode) && node.isWalkable())
                {
                    // Get the distance of the object.
                    float distance = Vector3.Distance(neighNode.position, current.position);
                    distance = currentNode.getWeight() + distance;

                    // If the added distance is less than the current weight.
                    if (distance < node.getWeight())
                    {
                        // We update the new distance as weight and update the new path now.
                        node.setWeight(distance);
                        node.setParentNode(current);
                    }
                    if (updateTimer)
                		levelTimer += Time.deltaTime;
                		Debug.Log("levelTimer" + levelTimer);
                }
            }

        }

        endTime = (Time.realtimeSinceStartup - startTime);
        print("Compute time: " + endTime);

        print("Path completed!");

        return end;
    }

}
