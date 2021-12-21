using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Debug = UnityEngine.Debug;
public class PathfindingTRRT : MonoBehaviour 
{


class TrrtNode 
{
		public Vector3 pos;

		public Vector3 parentPos;
		public int parentInd;
		public GameObject line = null;
		public float heightOffset = 0.5f; //lift up a little to prevent clipping
		
		public TrrtNode(Vector3 _pos, Vector3 _parentPos, int _parentInd, GameObject obj) {
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
	
    public Grid GridReference;//For referencing the grid class
	public Node NodeforGrid; //For referencing node class of A star
    public Transform StartPosition;//Starting position to pathfind from
    public Transform TargetPosition;//Starting position to pathfind to
    
    private Text statusText;
    private float endTime;
    private float startTime;
    /* Trrt parameters */
    private float stepSize;
	private Text coordText;
	//private Text statusText;
	//private Vector3 terrainSize; //remember, Y is height
	private float minX, maxX, minZ, maxZ, minHeight, maxHeight;
	private List<TrrtNode> nodes = new List<TrrtNode>();
    private bool solving = false;
	private int solvingSpeed = 1; //number of attempts to make per frame
	private float tx = 0f, tz = 0f; //target location to expand towards
	private bool needNewTarget = true; //keep track of whether our random sample is expired or still valid
	private int closestInd = 0; //the last node in the tree (the node before goal node)
	private int goalInd = 0; //when success is achieved, remember which node is close to goal
	private float extendAngle = 0f;
	private float temperature = 1e-6f;
	private const float temperatureAdjustFactor = 2.0f;
	private const float MIN_TEMPERATURE = 1e-15f;
	private int numTransitionFails = 0;
	private const int MAX_TRANSITION_FAILS = 20;
	private float pGoToGoal = 0.1f;
    private const int MAX_NUM_NODES = 10000;
    public GameObject[] gameObjects;
	public GameObject csvObject;
	WriteToCSVFile writeToCsv;
	public float levelTimer;
	public bool updateTimer=true;

    private float pathCost=0;
	// public List<Vector3> obstacleCoord;
	// public List<List<Vector3>> obstaclesList = new List<List<Vector3>>();
	//public List<List> obstacles
	public static Object linePrefab;
	public static Object pathPrefab;
	public Vector3 ScaleofObs;
    public Vector3 start, goal;
	public int k=0;
    private void Awake()//When the program starts
    {
        statusText = GameObject.Find("Status Text").GetComponent<Text>();
        GridReference = GetComponent<Grid>();//Get a reference to the game manager
        writeToCsv = csvObject.GetComponent<WriteToCSVFile>();
    }

    void Start() 
    {
            Vector3 start = StartPosition.position;
            Vector3 goal = TargetPosition.position;
          //  FindPath(StartPosition.position, TargetPosition.position);
            startTime = Time.realtimeSinceStartup;
            writeToCsv = csvObject.GetComponent<WriteToCSVFile>();
		    linePrefab = Resources.Load("LinePrefab");
		    pathPrefab = Resources.Load("PathPrefab");	
            //terrainSize = Terrain.activeTer;
		    startTime = Time.realtimeSinceStartup;
		    //minX = -terrainSize.x/2;
		    minX = 2;
		    maxX = GridReference.iGridSizeX- 2;
		    //minZ = -terrainSize.z/2;
		    minZ = 2;
		    // maxZ = terrainSize.z/2;
		    maxZ = GridReference.iGridSizeY- 2;
		    minHeight = 0;
		    maxHeight = 600;
		    stepSize = 10; //TODO experiment
		    levelTimer=0.0f;
    }

    void Update ()
    {
		if(Input.GetKeyDown("s")) {
			BeginSolving(10);
		}
		if(solving)
        {
			if(nodes.Count < MAX_NUM_NODES) 
            {
				statusText.text = "Solving... (nodes="+nodes.Count+", temp=" + temperature.ToString("0.00E00") + ")";
				TRRTGrow();
			}
		}
	}
    
    void BeginSolving(int speed) {
		solvingSpeed = speed;
		if(!solving) {
			solving = true;
			if(nodes.Count < 1) {
				//add initial node
				TrrtNode n = new TrrtNode(start, start, -1, gameObject);
				nodes.Add(n);
				
				//Debug.Log("Added node " + nodes.Count + ": " + n.pos.x + ", " + n.pos.y + ", " + n.pos.z);
			}
		}
	}
    void FoundGoal() {
		goalInd = closestInd; 
		solving = false;
		
		
		//trace path backwards to highlight navigation path
		int i = goalInd;
		TrrtNode n;
		
		pathCost = GetSegmentCost(nodes[goalInd].pos, goal);
		
		while(i != 0) {
			n = nodes[i];
			
			Debug.Log("node co-ordinates of the green line " + nodes[i]);
			
			
			n.ConvertToPath();
			
			pathCost += GetSegmentCost(n.parentPos, n.pos);
			
			i = n.parentInd;
		
		}
		updateTimer = false;
		endTime = (Time.realtimeSinceStartup - startTime);
		statusText.text = "Solved! with " + nodes.Count + " nodes, cost=" + pathCost;
		writeToCsv.WriteCSV("TRRT", levelTimer, pathCost, nodes.Count, endTime);

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


    
    // private void Update()//Every frame
    // {
    //     FindPath(StartPosition.position, TargetPosition.position);//Find a path to the goal
    // }

    void TRRTGrow()
    {
        
        int numAttempts = 0;
		
		float dx, dz;
		
		Vector3 pos;
		TrrtNode n;
		
		float minDistSq;
		float distSq;
		
		bool goingToGoal;
		// Debug.Log ("check vertices" + newPos);

		while(numAttempts < solvingSpeed && nodes.Count < MAX_NUM_NODES) 
		{
			if(needNewTarget) {
				if(Random.value < pGoToGoal) {
					goingToGoal = true;
					tx = goal.x;
					tz = goal.z;
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
			pos.y = 600; //get y value from terrain
			
			if(TransitionTest(nodes[closestInd].pos, pos)) 
			{
			    
				// {
					// if (!(isLineinsideObstacle(pos,nodes[closestInd].pos)))
					// {
                 Node GridNodeArray=GridReference.NodeFromWorldPoint(pos); //to find the x and y position of the Node.cs Node Class Node array 
				 Debug.Log("gridnodearray" + GridNodeArray);
				// if(!(NodeforGrid.Node(pos, GridNodeArray[0], GridNodeArray[1])).bIsWall) //call the node constructor
				//{
					n = new TrrtNode(pos, nodes[closestInd].pos, closestInd, gameObject);
                    //if (!n.GridReference.NodeArray[index])//If the neighbor is a wall or has already been checked
                	//{
					nodes.Add(n);
					Debug.Log("Added node " + nodes.Count + ": " + n.pos.x + ", " + n.pos.y + ", " + n.pos.z);
					 if (updateTimer)
                		levelTimer += Time.deltaTime;
                		Debug.Log("levelTimer" + levelTimer);
				
		
					//Determine whether we are close enough to goal
					dx = goal.x - n.pos.x;
					dz = goal.z - n.pos.z;
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
				 	//}
					
				//} 
			}
			
			else {
				//this extension is aborted due to transition test, need a new target
				//Debug.Log("Failed transition test");
				needNewTarget = true;
			}
		}
	}
	
	bool TransitionTest (Vector3 posA, Vector3 posB) 
    {

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
        
     