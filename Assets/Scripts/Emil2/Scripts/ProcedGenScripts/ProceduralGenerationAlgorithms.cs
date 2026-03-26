using UnityEngine;
using System.Collections.Generic;

public static class ProceduralGenerationAlgorithms
{
    //Monte Carlo algorithm to create random walks which together create the floor of the map
    //if you dont know what Monte Carlo algorithm is dont worry about it
    public static HashSet<Vector2> SimpleRandomWalk(Vector2 startPosition, int walkLength)
    {
        
        HashSet<Vector2> path = new HashSet<Vector2>();
        path.Add(startPosition);

        var previousPosition = startPosition;

        for (int i = 0; i < walkLength; i++)
        {
            //Get new positions after each individual step
            var newPosition = previousPosition + Direction2D.GetRandomIsometricCardinalDirection();
            //addition to accumulate all of the locations
            path.Add(newPosition);
            previousPosition = newPosition;
        }
        //returns a set with all positions
        return path;
    }
}
//class of all sets of directions for isometric grid
public static class Direction2D
{
    // Now using Vector2 and 0.5f steps
    public static List<Vector2> cardinalIsometricDirectionsList = new List<Vector2>
    {
        new Vector2(0.5f, 0.5f),    //  up-right
        new Vector2(0.5f, -0.5f),   //  down-right
        new Vector2(-0.5f, 0.5f),   //  up-left
        new Vector2(-0.5f, -0.5f)   //  down-left
    };
    public static List<Vector2> diagonalIsometricDirectionsList = new List<Vector2>
    {
        new Vector2(0f, 1f),     // up
        new Vector2(1f, 0f),     // right
        new Vector2(0f, -1f),    // down
        new Vector2(-1f, 0f)     // left
        
    };
    public static List<Vector2> eightDirectionsList = new List<Vector2>
    {
        new Vector2(0f, 1f),         // up
        new Vector2(0.5f, 0.5f),     //  up-right
        new Vector2(1f, 0f),         // right
        new Vector2(0.5f, -0.5f),    //  right-down
        new Vector2(0f, -1f),        // down        
        new Vector2(-0.5f, -0.5f),   //  down-left
        new Vector2(-1f, 0f),        // left        
        new Vector2(-0.5f, 0.5f)     //  up-left
    };

    public static Vector2 GetRandomIsometricCardinalDirection()
    {
        return cardinalIsometricDirectionsList[Random.Range(0, cardinalIsometricDirectionsList.Count)];
    }
}