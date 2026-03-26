using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    public event Action<IReadOnlyCollection<Vector2>> DungeonGenerated;
    public IReadOnlyCollection<Vector2> LastGeneratedFloorPositions { get; private set; } = Array.Empty<Vector2>();

    // The file to access all of the methods
    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    // Initial Position
    [SerializeField]
    protected Vector2 startPosition = Vector2.zero;
    //method to gen the dungeon
    public void GenerateDungeon()
    {
        //clear the map
        tilemapVisualizer.Clear();
        //create a new one
        HashSet<Vector2> floorPositions = RunProceduralGeneration();
        LastGeneratedFloorPositions = floorPositions;
        DungeonGenerated?.Invoke(LastGeneratedFloorPositions);
    }
    //requires te actual method to create it
    protected abstract HashSet<Vector2> RunProceduralGeneration();
}
