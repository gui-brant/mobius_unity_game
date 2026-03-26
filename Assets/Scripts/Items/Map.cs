using UnityEngine;

public class Map : ObjectiveItem
{
    [SerializeField] private string mapObjectiveId = "map_tick";

    protected override string ObjectiveId => mapObjectiveId;
}
