using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

// [RequireComponent(typeof(MazeRenderer))]
public class SphereAgentLogic : Agent {

    private Vector3 previousCell;
    private Vector3 targetCell;

    private Cell[,] maze = null;
    private int mazeWidth = 10;
    private int mazeHeight = 10;

    private MazeRenderer mazeRenderer = null;
    //MazeRenderer rendererComponent = null;
    Rigidbody agentBody;

    // Start is called before the first frame update
    void Start() {
        agentBody = GetComponent<Rigidbody>();
    }

    //[SerializeField]
    //private Transform wallPrefab = null;

    public Transform Target;
    public override void OnEpisodeBegin() {
        // Debug.Log(maze.ToString());

        /*
        // If the Agent fell, zero its momentum
        if(this.transform.localPosition.y < -10) {
            this.agentBody.angularVelocity = Vector3.zero;
            this.agentBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        */

        // Move the agent to a new spot

        mazeRenderer = FindObjectOfType<MazeRenderer>();
        //MazeRenderer rendererComponent = mazeRenderer.GetComponent<MazeRenderer>();
        mazeWidth = mazeRenderer.width;
        mazeHeight = mazeRenderer.height;
        maze = mazeRenderer.GetMaze(); // Ez lehet, hogy nem is fog kelleni vagy nem itt fog

        this.transform.localPosition = new Vector3(Mathf.RoundToInt(Random.value * (mazeWidth - 1) - mazeWidth / 2),
                                           0.5f,
                                           Mathf.RoundToInt(Random.value * (mazeHeight - 1) - mazeHeight / 2));

         
        // Megkeressuk az aktualis cella kozeppontjanak X koordinatajat
        int X = Mathf.RoundToInt(this.transform.localPosition.x);
        // Megkeressuk az aktualis cella kozeppontjanak Z koordinatajat
        int Z = Mathf.RoundToInt(this.transform.localPosition.z);
        this.previousCell = new Vector3(X, 1, Z);

        
        // Move the target to a new spot
        Target.localPosition = new Vector3(Mathf.RoundToInt(Random.value * (mazeWidth - 1) - mazeWidth / 2),
                                           0.5f,
                                           Mathf.RoundToInt(Random.value * (mazeHeight - 1) - mazeHeight / 2));
        // Megkeressuk az aktualis cella kozeppontjanak X koordinatajat
        int x = Mathf.RoundToInt(Target.localPosition.x);
        // Megkeressuk az aktualis cella kozeppontjanak Z koordinatajat
        int z = Mathf.RoundToInt(Target.localPosition.z);
        this.targetCell = new Vector3(x, 1, z);


        this.agentBody.angularVelocity = Vector3.zero;
        this.agentBody.velocity = Vector3.zero;
    }

    public override void CollectObservations( VectorSensor sensor ) {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(agentBody.velocity.x);
        sensor.AddObservation(agentBody.velocity.z);

        //Ide meg hozza kell adnod azt is h az agens.poz - wall.pos meg meg tobb egyebet is


    }

    private bool WallIsTooClose( WallState walls ) {
        bool tooClose = false;
        if(walls.HasFlag(WallState.UP)) {
            Vector3 topWall = this.transform.localPosition;
            topWall.z = Mathf.RoundToInt(topWall.z) + 0.5f;
            tooClose = Vector3.Distance(this.transform.localPosition, topWall) < 0.31f;
        }
        if(walls.HasFlag(WallState.LEFT) && !tooClose) {
            Vector3 leftWall = this.transform.localPosition;
            leftWall.x = Mathf.RoundToInt(leftWall.x) - 0.5f;
            tooClose = Vector3.Distance(this.transform.localPosition, leftWall) < 0.31f;
        }
        if(walls.HasFlag(WallState.RIGHT) && !tooClose) {
            Vector3 rightWall = this.transform.localPosition;
            rightWall.x = Mathf.RoundToInt(rightWall.x) + 0.5f;
            tooClose = Vector3.Distance(this.transform.localPosition, rightWall) < 0.31f;
        }
        if(walls.HasFlag(WallState.DOWN) && !tooClose) {
            /*  var bottomWall = Instantiate(wallPrefab, transform) as Transform;
              tooClose = Vector3.Distance(this.transform.localPosition, bottomWall.localPosition) < 0.1f; */
            Vector3 bottomWall = this.transform.localPosition;
            bottomWall.z = Mathf.RoundToInt(bottomWall.z) - 0.5f;
            tooClose = Vector3.Distance(this.transform.localPosition, bottomWall) < 0.31f;
        }
        return tooClose;
    }

    private bool ActualCellIsDeadEnd( WallState cellWalls ) {
        if(cellWalls.HasFlag(WallState.UP) && cellWalls.HasFlag(WallState.RIGHT) && cellWalls.HasFlag(WallState.DOWN)
            || cellWalls.HasFlag(WallState.RIGHT) && cellWalls.HasFlag(WallState.DOWN) && cellWalls.HasFlag(WallState.LEFT)
            || cellWalls.HasFlag(WallState.DOWN) && cellWalls.HasFlag(WallState.LEFT) && cellWalls.HasFlag(WallState.UP)
            || cellWalls.HasFlag(WallState.LEFT) && cellWalls.HasFlag(WallState.UP) && cellWalls.HasFlag(WallState.RIGHT)) 
        {
            return true;
        }
        return false;
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived( ActionBuffers actions ) {
        // Negative reward due to shorter time to the goal is the better, don't waste time
        AddReward(-0.005f);
        // Actions, size = 2
        int dirNum = (int)actions.DiscreteActions[0];
        Debug.Log(actions.DiscreteActions[0]);
        Vector3 moveDirection = Vector3.zero;
        Vector3 aimedPos = Vector3.zero;
        switch(dirNum) {
            case 1:
                moveDirection = new Vector3(0, 0, 0.1f);
                break;
            case 2:
                moveDirection = new Vector3(0.1f, 0, 0);
                break;
            case 3:
                moveDirection = new Vector3(0, 0, -0.1f);
                break;
            case 4:
                moveDirection = new Vector3(-0.1f, 0, 0);
                break;
            default:
                break;
        }
        aimedPos = this.transform.localPosition + moveDirection;

        //agentBody.MovePosition(moveDirection.normalized);
        RaycastHit hitWall;
        if(Physics.Raycast(this.transform.position, moveDirection, out hitWall, 0.5f)) {
            if(hitWall.collider.CompareTag("mazeWall")) {
                // Ha falba akar lepni, akkor helyben marad es negativ jutalmat kap
                AddReward(-0.1f);
            } else {
                this.transform.localPosition = aimedPos;
            }
        } else {
            this.transform.localPosition = aimedPos;
        }

        // Megkeressuk az aktualis cella kozeppontjanak X koordinatajat
        int X = Mathf.RoundToInt(this.transform.localPosition.x);
        int i = X + (mazeWidth / 2);
        // Megkeressuk az aktualis cella kozeppontjanak Z koordinatajat
        int Z = Mathf.RoundToInt(this.transform.localPosition.z);
        int j = Z + (mazeHeight / 2);
        Vector3 actualCell = new Vector3(X, 1, Z);

        if(actualCell == previousCell) {
            AddReward(-0.005f);
        }

        previousCell = actualCell;

        if(maze[i, j].floor.GetComponent<Renderer>().material.color != Color.red){
            AddReward(0.005f);
        }

        maze[i, j].floor.GetComponent<Renderer>().material.color = Color.red;
        //Debug.Log(actualCell);
        //Debug.Log("Oszlop:" + i);
        //Debug.Log("Sor:" + j);
        // Debug.Log("WallState: " + MazeRenderer.maze[i, j]);
        //MazeRenderer.maze[i, j] |= WallState.UP | WallState.RIGHT | WallState.DOWN | WallState.LEFT | WallState.VISITED;
        //Debug.Log("Utana: " + MazeRenderer.maze[i, j]);




        WallState actualCellState = maze[i, j].wallState;
        if(ActualCellIsDeadEnd(actualCellState))
            AddReward(-0.1f);

        if(WallIsTooClose(actualCellState)) {
            AddReward(-0.0001f);
            //mazeRenderer.GetComponent<MazeRenderer>().Reset();
            //EndEpisode();
        }

        float maxRaycastDistance = 10f;
        if((actualCell-targetCell).magnitude < (previousCell - targetCell).magnitude) {
            RaycastHit hit;
            if(Physics.Raycast(this.transform.localPosition, targetCell - transform.position, out hit, maxRaycastDistance)) {
                if(hit.collider.CompareTag("target")) {
                    // Ha latja a targetet es arra is lepett magas jutalom (mar fel siker)
                    AddReward(0.5f);
                } else {
                    // Ha nem latja a sugarakkal a target-et (akkor is kis jutalom, hogy minel kozelebb akarjon menni, de ne akadjon azert be emiatt)
                    if(!Physics.Raycast(this.transform.localPosition, moveDirection, 0.75f)) {
                        AddReward(0.2f);
                    } else if(Physics.Raycast(this.transform.localPosition, moveDirection, out hit, 10f)) {
                        if(hit.collider.CompareTag("cellFloor")) {
                            // Ha latja a targetet es arra is lepett magas jutalom (mar fel siker)
                            AddReward(0.2f);
                        }
                    }
                }
            }
        }

        if(StepCount == MaxStep) {
            AddReward(-0.5f);
            mazeRenderer.GetComponent<MazeRenderer>().Reset();
            EndEpisode();
        }


        // Reached target
        if(actualCell == this.targetCell) {
            AddReward(1.0f);
            mazeRenderer.GetComponent<MazeRenderer>().Reset();
            EndEpisode();
        }

        // Fell off platform
        else if(this.transform.localPosition.y < 0) {
            EndEpisode();
        }
    }


    public override void Heuristic( in ActionBuffers actionsOut ) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if(Input.GetKey(KeyCode.UpArrow)) {
            discreteActionsOut[0] = 1;
        }
        if(Input.GetKey(KeyCode.RightArrow)) {
            discreteActionsOut[0] = 2;
        }
        if(Input.GetKey(KeyCode.DownArrow)) {
            discreteActionsOut[0] = 3;
        }
        if(Input.GetKey(KeyCode.LeftArrow)) {
            discreteActionsOut[0] = 4;
        }
    }


}
