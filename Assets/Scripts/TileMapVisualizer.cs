using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    // all of the tile sprites
    [SerializeField]
    private Tilemap  floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile;
    [SerializeField]
    private TileBase wallUp, wallRight, wallDown, wallLeft;
    [SerializeField]
    private TileBase wallUR, wallDR, wallDL, wallUL, WallEdgeCase, wallLeftHalfEdgeCase, wallRightHalfEdgeCase, columnINSIDER;
    
    //paints floor
    public void PaintFloorTiles(IEnumerable<Vector2> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }
    //paints all tiles on a specified tilemap in positions Enum
    private void PaintTiles(IEnumerable positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, (Vector2)position);
        }


}
    //paints any single tile with te sprite tat is specified in the SerializeFields and assigned to the tile argument
    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2 position)
    {
        //converts the tile position from world coordinates to tilemap coords
        var tilePosition = tilemap.WorldToCell((Vector3)position);
        //SETS tile on the tilemap
        tilemap.SetTile(tilePosition, tile);
    }
    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
    
    // Main four directions
    //position -> every single position that as been filtered for being a corner from the (floor list + cardinal direction)
    //binaryType -> neigbouring tiles of te corner (think 8 tiles around it) that have been converted into an 8 bit string of yes (1's) and no (0's).
    //starting at up and going in clockwise direction
    internal void PaintSingleBasicWall(Vector2 position, string binaryType)
    {
        
        int typeASInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        //diagonals    
        if (WallTypesHelper.wallDR.Contains(typeASInt)){tile = wallDR;}
        else if (WallTypesHelper.wallUL.Contains(typeASInt)){tile = wallUL;}
        else if (WallTypesHelper.wallUR.Contains(typeASInt)){tile = wallUR;}
        else if (WallTypesHelper.wallDL.Contains(typeASInt)){tile = wallDL;}
        //edge case
        else if (WallTypesHelper.WallEdgeCase.Contains(typeASInt)){tile = WallEdgeCase;}
        

        if (tile != null){PaintSingleTile(wallTilemap, tile, position);}
    }
    //paints all of the positions that has been identified as the corners around the floor
    internal void PaintSingleCornerWall(Vector2 position, string binaryType)
    {
        int typeASInt = Convert.ToInt32(binaryType, 2);
        //null untill assigned
        TileBase tile = null;
        //Main 4 directions
        if (WallTypesHelper.wallUp.Contains(typeASInt))                          { tile = wallUp; }//up
        else if (WallTypesHelper.wallRight.Contains(typeASInt))                  { tile = wallRight; }//right
        else if (WallTypesHelper.wallDown.Contains(typeASInt))                   {tile = wallDown;}//down
        else if (WallTypesHelper.wallLeft.Contains(typeASInt))                   {tile = wallLeft;}//
        //edge cases (please dont ask)
        else if (WallTypesHelper.WallEdgeCaseEightDirections.Contains(typeASInt)){tile = WallEdgeCase;}//edge case 
        else if (WallTypesHelper.wallLeftHalfEdgeCase.Contains(typeASInt))       {tile = wallLeftHalfEdgeCase;}// left half
        else if (WallTypesHelper.wallRightHalfEdgeCase.Contains(typeASInt))      {tile = wallRightHalfEdgeCase;}// right half
        // decorations
        else if (WallTypesHelper.WallInsider.Contains(typeASInt))                {tile = columnINSIDER;}// the column
        //painter
        if (tile != null)PaintSingleTile(wallTilemap, tile, position);
    }
}
