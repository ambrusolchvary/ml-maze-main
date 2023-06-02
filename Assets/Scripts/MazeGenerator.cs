using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum WallState {
    // 0000 -> NO WALLS
    // 1111 -> LEFT,RIGHT,UP,DOWN
    LEFT = 1, // 0001
    RIGHT = 2, // 0010
    UP = 4, // 0100
    DOWN = 8, // 1000

    VISITED = 128, // 1000 0000
}

// Ezzel tartjuk szamon, hogy epp melyik koordinatan allunk a generalaskor
public struct Position {
    public int X;
    public int Y;
}

public struct Neighbour {
    public Position Position; // szomszed cella pozicioja
    public WallState SharedWall; // Az aktualis cella nezopontjabol, melyik iranyban levo fala szomszedos ezzel a szomszed cellaval
}

public static class MazeGenerator {

    private static WallState GetOppositeWall( WallState wall ) {
        switch(wall) {
            case WallState.RIGHT:
                return WallState.LEFT;
            case WallState.LEFT:
                return WallState.RIGHT;
            case WallState.UP:
                return WallState.DOWN;
            case WallState.DOWN:
                return WallState.UP;
            default:
                return WallState.LEFT;
        }
    }

    private static WallState[,] ApplyRecursiveBacktracker( WallState[,] maze, int width, int height ) {
        // here we make changes
        var rng = new System.Random(/*seed*/);
        var positionStack = new Stack<Position>();
        var position = new Position { X = rng.Next(0, width), Y = rng.Next(0, height) };

        maze[position.X, position.Y] |= WallState.VISITED;  // 1000 1111
        positionStack.Push(position);

        while(positionStack.Count > 0) // Addig iteralunk, amig ki nem urul a positionStack stack
        {
            var current = positionStack.Pop();
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if(neighbours.Count > 0) {
                positionStack.Push(current);

                var randIndex = rng.Next(0, neighbours.Count);
                var randomNeighbour = neighbours[randIndex];

                var nPosition = randomNeighbour.Position;
                maze[current.X, current.Y] &= ~randomNeighbour.SharedWall; // Itt tavolitjuk el az aktualis csomopont adott szomszedjaval kozos falat
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeighbour.SharedWall); // Itt ugyanazt a falat tavolitjuk el, csak a szomszed cella szemszogebol nezve
                maze[nPosition.X, nPosition.Y] |= WallState.VISITED;

                positionStack.Push(nPosition);
            }
        }

        return maze;
    }

    // Visszaadja az aktualis cellanak, a meg nem llatogatott szomszedait egz listaban
    private static List<Neighbour> GetUnvisitedNeighbours( Position p, WallState[,] maze, int width, int height ) {
        var list = new List<Neighbour>();

        if(p.X > 0) // left
        {
            if(!maze[p.X - 1, p.Y].HasFlag(WallState.VISITED)) {
                list.Add(new Neighbour {
                    Position = new Position {
                        X = p.X - 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.LEFT
                });
            }
        }

        if(p.Y > 0) // DOWN
        {
            if(!maze[p.X, p.Y - 1].HasFlag(WallState.VISITED)) {
                list.Add(new Neighbour {
                    Position = new Position {
                        X = p.X,
                        Y = p.Y - 1
                    },
                    SharedWall = WallState.DOWN
                });
            }
        }

        if(p.Y < height - 1) // UP
        {
            if(!maze[p.X, p.Y + 1].HasFlag(WallState.VISITED)) {
                list.Add(new Neighbour {
                    Position = new Position {
                        X = p.X,
                        Y = p.Y + 1
                    },
                    SharedWall = WallState.UP
                });
            }
        }

        if(p.X < width - 1) // RIGHT
        {
            if(!maze[p.X + 1, p.Y].HasFlag(WallState.VISITED)) {
                list.Add(new Neighbour {
                    Position = new Position {
                        X = p.X + 1,
                        Y = p.Y
                    },
                    SharedWall = WallState.RIGHT
                });
            }
        }

        return list;
    }

    public static WallState[,] Generate( int width, int height ) {
        WallState[,] maze = new WallState[width, height];
        WallState initial = WallState.RIGHT | WallState.LEFT | WallState.UP | WallState.DOWN;
        for(int i = 0;i < width;++i) {
            for(int j = 0;j < height;++j) {
                maze[i, j] = initial;  // 1111
            }
        }

        return ApplyRecursiveBacktracker(maze, width, height);
    }
}
