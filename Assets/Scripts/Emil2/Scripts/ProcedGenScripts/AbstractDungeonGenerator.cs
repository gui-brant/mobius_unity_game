using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
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
        RunProceduralGeneration();
    }
    //requires te actual method to create it
    protected abstract void RunProceduralGeneration();
}