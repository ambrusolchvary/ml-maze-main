using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

// [RequireComponent(typeof(MazeRenderer))]
public class SphereAgentLogic : Agent {

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

        // Move the target to a new spot
        Target.localPosition = new Vector3(Mathf.RoundToInt(Random.value * (mazeWidth - 1) - mazeWidth / 2),
                                           0.5f,
                                           Mathf.RoundToInt(Random.value * (mazeHeight - 1) - mazeHeight / 2));

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

    public float forceMultiplier = 2;
    public override void OnActionReceived( ActionBuffers actions ) {
        // Negative reward due to shorter time to the goal is the better, don't waste time
        AddReward(-0.005f);
        // Actions, size = 2
        int moveX = (int)actions.DiscreteActions[0];
        int moveZ = (int)actions.DiscreteActions[1];

        Vector3 moveDirection = Vector3.zero;

        if(moveX == 0)
            moveDirection += Vector3.right;
        else if(moveX == 1)
            moveDirection += Vector3.left;

        if(moveZ == 0)
            moveDirection += Vector3.forward;
        else if(moveZ == 1)
            moveDirection += Vector3.back;

        agentBody.AddForce(moveDirection.normalized * forceMultiplier);

        // Megkeressuk az aktualis cella kozeppontjanak X koordinatajat
        int X = Mathf.RoundToInt(this.transform.localPosition.x);
        int i = X + (mazeWidth / 2);
        // Megkeressuk az aktualis cella kozeppontjanak Z koordinatajat
        int Z = Mathf.RoundToInt(this.transform.localPosition.z);
        int j = Z + (mazeHeight / 2);
        Vector3 actualCell = new Vector3(X, 1, Z);

        maze[i, j].floor.GetComponent<Renderer>().material.color = Color.red;
        //Debug.Log(actualCell);
        //Debug.Log("Oszlop:" + i);
        //Debug.Log("Sor:" + j);
        // Debug.Log("WallState: " + MazeRenderer.maze[i, j]);
        //MazeRenderer.maze[i, j] |= WallState.UP | WallState.RIGHT | WallState.DOWN | WallState.LEFT | WallState.VISITED;
        //Debug.Log("Utana: " + MazeRenderer.maze[i, j]);




        WallState actualCellState = maze[i, j].wallState;
        if(WallIsTooClose(actualCellState)) {
            AddReward(-0.1f);
            //mazeRenderer.GetComponent<MazeRenderer>().Reset();
            //EndEpisode();
        }




        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if(distanceToTarget < 0.5f) {
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

        discreteActionsOut[0] = GetMoveAxis("Horizontal");
        discreteActionsOut[1] = GetMoveAxis("Vertical");
    }

    private int GetMoveAxis( string axisName ) {
        float input = Input.GetAxis(axisName);

        if(input > 0)
            return 0;
        else if(input < 0)
            return 1;
        else
            return -1;
    }


}
