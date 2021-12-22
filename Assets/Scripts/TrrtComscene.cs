using System.Numerics;
using System.Diagnostics;
using UnityEngine;
//using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;
using Object= UnityEngine.Object;
using Random=UnityEngine.Random;
using Transform=UnityEngine.Transform;



public class TrrtComscene : MonoBehaviour {
	public class Node {						//by default C# classes are of internal access modifier type and c# variales are of private type
		public Vector3 pos;

		public Vector3 parentPos;
		public int parentInd;
		public GameObject line = null;
		public float heightOffset = 0.5f; //lift up a little to prevent clipping

		//taken from DijkstraNode
		public int iGridX;//X Position in the Node Array
    	public int iGridY;//Y Position in the Node Array
    	[SerializeField] private float weight = int.MaxValue;
    	[SerializeField] private Transform parentNode = null;
    	[SerializeField] private List<Transform> neighbourNode;
    	[SerializeField] private bool walkable = true;

		public Node(Vector3 _pos, Vector3 _parentPos, int _parentInd, GameObject obj) {
			pos = _pos;
			parentPos = _parentPos;
			parentInd = _parentInd;
			
			//obj = (GameObject) Instantiate(Resources.Load("Waypoint"), pos, Quaternion.identity);
			
			if(parentInd >= 0) {
				line = (GameObject) Instantiate(linePrefab);
				LineRenderer lr = line.GetComponent<LineRenderer>();
				lr.SetPosition(0,pos + new Vector3(0f,heightOffset,0f));
				lr.SetPosition(1,parentPos + new Vector3(0f,heightOffset,0f));
			}
		}
		
		public void ConvertToPath() {
			GameObject.Destroy(line);
			line = (GameObject) Instantiate(pathPrefab);
			LineRenderer lr = line.GetComponent<LineRenderer>();
			lr.SetPosition(0,pos + new Vector3(0f,heightOffset,0f));
			lr.SetPosition(1,parentPos + new Vector3(0f,heightOffset,0f));
		}
	};
	
	
	public float stepSize;
	public Text coordText;
	public Text statusText;
	public Vector3 terrainSize; //remember, Y is height
	public float minX, maxX, minZ, maxZ, minHeight, maxHeight;
	public List<Node> nodes = new List<Node>();
	
	public bool solving = false;
	public int solvingSpeed = 1; //number of attempts to make per frame
	
	public float tx = 0f, tz = 0f; //target location to expand towards
	public bool needNewTarget = true; //keep track of whether our random sample is expired or still valid
	public int closestInd = 0; //the last node in the tree (the node before goal node)
	public int goalInd = 0; //when success is achieved, remember which node is close to goal
	public float extendAngle = 0f;
	public Transform startNode;
    public Transform endNode;
	public float temperature = 1e-6f;
	public const float temperatureAdjustFactor = 2.0f;
	public const float MIN_TEMPERATURE = 1e-15f;
	public int numTransitionFails = 0;
	public const int MAX_TRANSITION_FAILS = 20;
	public float endTime;  //to get the simulation time
    public float startTime;
	public float pGoToGoal = 0.1f;
	public const int MAX_NUM_NODES = 5000;
    public GameObject[] gameObjects;
	public GameObject csvObject;
	WriteToCSVFile writeToCsv;
	public float levelTimer;
	public bool updateTimer=true;
	public float pathCost=0;
	public List<Vector3> obstacleCoord;
	

	public static Object linePrefab;
	public static Object pathPrefab;
	public Vector3 ScaleofObs;
	public int k=0;
	
	// Use this for initialization
	void Start () {

	
		//writeToCsv = csvObject.GetComponent<WriteToCSVFile>();
		linePrefab = Resources.Load("LinePrefab");
		pathPrefab = Resources.Load("PathPrefab");	
	
		startTime = Time.realtimeSinceStartup;
	
		minX = 2;
		maxX = 400;
	
		minZ = 2;
	
		maxZ = 400;
		minHeight = 0;
		maxHeight = 400;
		Debug.Log("maxHeight" + maxHeight);
		stepSize = 10; //TODO experiment
		levelTimer=0.0f;

	}
	void OnMouseExit () {
		coordText.text = "";
	}
	
	
	public void BeginSolving(int speed, Transform startNode, Transform endNode) {
		solvingSpeed = speed;
		this.startNode=startNode;
		this.endNode=endNode;
		if(!solving) {
			solving = true;
			if(nodes.Count < 1) {
				//add initial node
				Node n = new Node(startNode.position, startNode.position, -1, gameObject);
				nodes.Add(n);
				
				Debug.Log("Added node " + nodes.Count + ": " + n.pos.x + ", " + n.pos.y + ", " + n.pos.z);
			}
		}
	}
	public void ContinueSolving()
	{
		while(solving) {
			if(nodes.Count < MAX_NUM_NODES) {
				//statusText.text = "Solving... (nodes="+nodes.Count+", temp=" + temperature.ToString("0.00E00") + ")";
				TRRTGrow();
			}
		}
	}

	
	
	void FoundGoal() {
		goalInd = closestInd; 
		solving = false;
		
		
		//trace path backwards to highlight navigation path
		int i = goalInd;
		Node n;
		
		pathCost = GetSegmentCost(nodes[goalInd].pos, endNode.position);
		
		while(i != 0) {
			n = nodes[i];
			
			Debug.Log("node co-ordinates of the green line " + nodes[i]);
			
			
			n.ConvertToPath();
			
			pathCost += GetSegmentCost(n.parentPos, n.pos);
			
			i = n.parentInd;
		
		}
		updateTimer = false;
		endTime = (Time.realtimeSinceStartup - startTime);
		Debug.Log("Solved! with " + nodes.Count + " nodes, cost=" + pathCost);
	//	writeToCsv.WriteCSV("TRRT", levelTimer, pathCost, nodes.Count, endTime);

	}
	
	float GetSegmentCost(Vector3 posA, Vector3 posB) {
		//float dCost = posB.y - posA.y; // this y co-ordinate was considered for terrain height
		float dx = posB.x - posA.x;
		float dz = posB.z - posA.z;
		
		float dist = Mathf.Sqrt(dx*dx + dz*dz);
		
		float cost = dist; //arbitrary, small distance component
		
		//if(dCost > 0) {
			//cost += dist*dCost;
		//}
		
		return cost;
	}
	
	
	void TRRTGrow ()
	{
		int numAttempts = 0;
		
		float dx, dz;
		
		Vector3 pos;
		Node n;
		
		float minDistSq;
		float distSq;
		
		bool goingToGoal;
		// Debug.Log ("check vertices" + newPos);

		while(numAttempts < solvingSpeed && nodes.Count < MAX_NUM_NODES) 
		{
			if(needNewTarget) {
				if(Random.value < pGoToGoal) {
					goingToGoal = true;
					tx = endNode.position.x;
					tz = endNode.position.z;
				} else {
					goingToGoal = false;
					tx = Random.Range(minX, maxX);
					tz = Random.Range(minZ, maxZ);
				}
				needNewTarget = false;
				
				//Debug.Log("New target: " + tx + ", " + tz);
				
				//Find which node is closest to (tx,tz)
				minDistSq = float.MaxValue;
				for(int i=0; i<nodes.Count; i++) {
					dx = tx - nodes[i].pos.x;
					dz = tz - nodes[i].pos.z;
					distSq = dx*dx + dz*dz;
					if(distSq < minDistSq) {
						closestInd = i;
						minDistSq = distSq;
					}
				}
				
				if(Mathf.Sqrt(minDistSq) <= stepSize) {
					//random sample is already "close enough" to tree to be considered reached
					if(goingToGoal) {
						FoundGoal();
						break;
					} else {
						needNewTarget = true;
						continue;
					}
				}
				//Debug.Log("obstacles" + obstacles[0]);
				//Debug.Log("closestInd: " + closestInd);
				
				dx = tx - nodes[closestInd].pos.x;
				dz = tz - nodes[closestInd].pos.z;
				
				extendAngle = Mathf.Atan2(dz, dx);
				
				//Debug.Log("dx dz a: " + dx + " " + dz + " " + extendAngle*180/Mathf.PI);
			}
			
			pos = new Vector3(nodes[closestInd].pos.x + stepSize*Mathf.Cos(extendAngle), 0f, nodes[closestInd].pos.z + stepSize*Mathf.Sin(extendAngle));
			pos.y = 400; //get y value from terrain
			
			if(TransitionTest(nodes[closestInd].pos, pos)) 
			{
			    // if (!(isNodeinsideObstacle(pos)))
				// {
					// if (!(isLineinsideObstacle(pos,nodes[closestInd].pos)))
					// {
					n = new Node(pos, nodes[closestInd].pos, closestInd, gameObject);
					nodes.Add(n);
					Debug.Log("Added node " + nodes.Count + ": " + n.pos.x + ", " + n.pos.y + ", " + n.pos.z);
					 if (updateTimer)
                		levelTimer += Time.deltaTime;
                		Debug.Log("levelTimer" + levelTimer);
				
		
					//Determine whether we are close enough to goal
					dx = endNode.position.x - n.pos.x;
					dz = endNode.position.z - n.pos.z;
					if(Mathf.Sqrt(dx*dx + dz*dz) <= stepSize) {
					//Reached the goal!
					FoundGoal();
					return;
					}
				
					//Determine whether we are close enough to target, or need to keep extending
					dx = tx - n.pos.x;
					dz = tz - n.pos.z;
					if(Mathf.Sqrt(dx*dx + dz*dz) <= stepSize) {
					//we've reached our target point, need a new target
					needNewTarget = true;
					} 
					else 
					{
					//keep extending from the latest node
					closestInd = nodes.Count - 1;
					}
					numAttempts++;
					// }
				// }
			} 
			
			else {
				//this extension is aborted due to transition test, need a new target
				//Debug.Log("Failed transition test");
				needNewTarget = true;
			}
		}
	}
	
	bool TransitionTest (Vector3 posA, Vector3 posB) {
		float dx = posB.x - posA.x;
		float dz = posB.z - posA.z;
		float dist = Mathf.Sqrt(dx*dx + dz*dz);
		
		float slope = (posB.y - posA.y) / dist;
		
		float pTransition; //transition probability, 0 to 1
		
		if(slope <= 0) 
		{
			//pTransition = 1.0f; //always go "downhill"
			pTransition = 0.5f; 
		} 
		else 
		{
			pTransition = Mathf.Exp(-slope/(temperature)); //FIXME
		}
		
		bool pass = Random.value < pTransition;
		
		if(!pass) 
		{
			if(numTransitionFails > MAX_TRANSITION_FAILS) 
			{
				//Heat the temperature up
				temperature = temperature * temperatureAdjustFactor;
				numTransitionFails = 0; //restart counter
			} 
			else 
			{
				numTransitionFails++;
			}
		} 
		else 
		{
			//Cool the temperature down
			if(temperature > MIN_TEMPERATURE) 
			{ //prevent slim chance of temp becoming 0
				temperature = temperature / temperatureAdjustFactor;
			}
			numTransitionFails = 0;	
		}
		
		return pass;
	}
}
