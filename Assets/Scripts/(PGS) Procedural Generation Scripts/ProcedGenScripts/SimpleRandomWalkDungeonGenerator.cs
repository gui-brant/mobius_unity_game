using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{
    //Default variables
    //These are the specifications of ow to create the dungeon
    [SerializeField]
    public int iterations = 10;
    [SerializeField]
    public int walkLength = 10;
    //AKA: can you start from a previous walk(true) -> or do you have to start at origin (0,0) -> (false)?
    [SerializeField]
    public bool startRandomlyEachIteration = true;

    protected override HashSet<Vector2> RunProceduralGeneration() 
    {
        //create a map of all floors through random walks
        HashSet<Vector2> floorPositions = RunRandomWalk();
        tilemapVisualizer.Clear();
        
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        
        WallGenerator.CreateWalls(floorPositions,tilemapVisualizer);
        return floorPositions;
    }

    private HashSet<Vector2> RunRandomWalk()
    {
        var currentPosition = startPosition;
        HashSet<Vector2> floorPositions = new HashSet<Vector2>();
        for (int i = 0; i < iterations; i++) 
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition,walkLength);
            floorPositions.UnionWith(path);
            if (startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            
        }
        return floorPositions;
    }

}
 
