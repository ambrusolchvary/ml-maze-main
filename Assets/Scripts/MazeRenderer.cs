using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public struct Cell {
    public Vector3 position;
    public Transform floorPrefab;
    public Transform floor;
    public WallState wallState;

    public Cell( Vector3 position, Transform floorPrefab, WallState wallState ) {
        this.position = position;
        this.floorPrefab = floorPrefab;
        this.floor = null;
        this.wallState = wallState;
    }
}

public class MazeRenderer : MonoBehaviour {

    [SerializeField]
    [Range(1, 50)]
    public int width = 10;

    [SerializeField]
    [Range(1, 50)]
    public int height = 10;

    [SerializeField]
    private float size = 1f; // a cella merete

    [SerializeField]
    private Transform wallPrefab = null;

    [SerializeField]
    private Transform floorPrefab = null;

    [SerializeField]
    private Cell[,] maze = null;

    public Cell[,] GetMaze() {
        return maze;
    }

    // Start is called before the first frame update
    void Start() {
        maze = new Cell[width, height];
        WallState[,] wallStates = MazeGenerator.Generate(width, height);
        Draw(wallStates);
    }


    private void Draw( WallState[,] wallStates ) {
        for(int i = 0;i < width;++i) {
            for(int j = 0;j < height;++j) {
                Vector3 cellPosition = new Vector3((-width / 2 + i)*size, 0, (-height / 2 + j)*size);
                maze[i, j] = new Cell(cellPosition, floorPrefab, wallStates[i, j]);
                maze[i, j].floor = Instantiate(maze[i, j].floorPrefab, maze[i, j].position, Quaternion.identity);
                maze[i, j].floor.localScale = new Vector3(0.1f * size, 1f, 0.1f * size);

                /* var floor = Instantiate(floorPrefab, transform) as Transform;
                  floor.position = maze[i, j].position;
                  floor.localScale = new Vector3(0.1f * size, 1f, 0.1f * size); */

                if(maze[i, j].wallState.HasFlag(WallState.UP)) {
                    var topWall = Instantiate(wallPrefab, transform) as Transform;
                    topWall.position = maze[i, j].position + new Vector3(0, 0, size / 2);
                    topWall.localScale = new Vector3(size, topWall.localScale.y, topWall.localScale.z);
                }

                if(maze[i, j].wallState.HasFlag(WallState.LEFT)) {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = maze[i, j].position + new Vector3(-size / 2, 0, 0);
                    leftWall.localScale = new Vector3(size, leftWall.localScale.y, leftWall.localScale.z);
                    leftWall.eulerAngles = new Vector3(0, 90, 0);
                }

                /* Ha minden cellanak kirajzoljuk csak a bal es a felso oldalon levo falat, akkor a negyszog alaku teruletunkben
                 minden cellanak ki lesz rajzolva mindegyik fala, kizarolag az a legalso sorban es a legjobboldali oszlopban levo cellaknak
                fognak hianyozni az also avagy a jobboldali fala, emiatt az also es jobb oldali falakat csak akkor rajzoltatjuk ki,
                hogyha az adott cella a jobboldali oszlopban vagy a legalso sorban szerepel a labirintusunkban.*/

                if(i == width - 1) // i = width-1 := utolso oszlopot reprezentalja
                {
                    if(maze[i, j].wallState.HasFlag(WallState.RIGHT)) {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = maze[i, j].position + new Vector3(+size / 2, 0, 0);
                        rightWall.localScale = new Vector3(size, rightWall.localScale.y, rightWall.localScale.z);
                        rightWall.eulerAngles = new Vector3(0, 90, 0);
                    }
                }

                if(j == 0) // j = 0 := elso sort reprezentalja
                {
                    if(maze[i, j].wallState.HasFlag(WallState.DOWN)) {
                        var bottomWall = Instantiate(wallPrefab, transform) as Transform;
                        bottomWall.position = maze[i, j].position + new Vector3(0, 0, -size / 2);
                        bottomWall.localScale = new Vector3(size, bottomWall.localScale.y, bottomWall.localScale.z);
                    }
                }
            }

        }

    }

    private void DestroyFloors() {
        for(int i = 0;i < width;i++) {
            for(int j = 0;j < height;j++) {
                if(maze[i, j].floor != null) {
                    Destroy(maze[i, j].floor.gameObject);
                }
            }
        }
    }

    public void Reset() {
        foreach(Transform child in transform) {
            Destroy(child.gameObject);
        }
        DestroyFloors();
        maze = new Cell[width, height];
        WallState[,] wallStates = MazeGenerator.Generate(width, height);
        Draw(wallStates);
    }

    // Update is called once per frame
    void Update() {

    }

    /*
    public void OnDestroy() {
        if(gameObject != null) {
            Destroy(gameObject);
        }
    }
    */
}
