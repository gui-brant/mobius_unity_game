using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        //splits all walls into 2 halves: cardinal, and diagonals
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalIsometricDirectionsList);
        var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalIsometricDirectionsList);
        CreateBasicWall(tilemapVisualizer, basicWallPositions, floorPositions);
        CreateCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions);
    }

    private static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2> cornerWallPositions, HashSet<Vector2> floorPositions)
    {
        // checks in all of the directions of eight directions in the list if there is a wall, if is, add 1, if isnt add 0 (all 8)
        foreach (var position in cornerWallPositions)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
    }
    // checks in all of the directions of four directions in the list if there is a wall, if is, add 1, if isnt add 0 (cardinal)
    private static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2> basicWallPositions, HashSet<Vector2> floorPositions)
    {
        foreach (var position in basicWallPositions)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalIsometricDirectionsList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
        }
    }
    // creates a HashSet of all walls
    private static HashSet<Vector2> FindWallsInDirections(HashSet<Vector2> floorPositions, List<Vector2> directionList)
    {
        
        HashSet<Vector2> wallPositions = new HashSet<Vector2>();
        //goes through all positions of walls
        foreach (var position in floorPositions)
        {
            // in each direction specified (like only diagonals, or only cardinal)
            foreach (var direction in directionList)
            {
                //find neightbour positions
                var neighbourPosition = position + direction;
                //if the neighbouring position DOESNT have a floor there, it must be a wall
                if (floorPositions.Contains(neighbourPosition) == false)
                    wallPositions.Add(neighbourPosition); // add the wall to the list
            }
        }
        return wallPositions;
    }

}
