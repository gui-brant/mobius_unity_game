using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//access the custom editor for all children of AbstractDungeonGenerator
[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class RandomDungeonGeneratorEditor : Editor
{
    AbstractDungeonGenerator generator;
    // when script is first loaded, before the game starts
    public void Awake()
    {
        //
        generator = (AbstractDungeonGenerator)target; // target is the object in unity when it is selected in Hierarchy

    }
    //in the inspector
    public override void OnInspectorGUI()
    {   
        base.OnInspectorGUI();
        if(GUILayout.Button("Create Dungeon")/*create button */) // which if presed
        {
            generator.GenerateDungeon(); // runs GenerateDungeon in all of the generators
        }
    }
}
