using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DiamondSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float stepX = 0.9f;
    [SerializeField] private float stepY = 0.9f;

    private Vector2 top = new Vector2(-0.038f, 10.468f);
    private Vector2 right = new Vector2(8.901f, 1.466f);
    private Vector2 bottom = new Vector2(-0.038f, -7.554f);
    private Vector2 left = new Vector2(-9.030f, 1.445f);

    public void SpawnInDiamond()
    {
        if (prefab == null) { Debug.LogError("[DiamondSpawner] Prefab is NULL"); return; }

        float minX = left.x, maxX = right.x;
        float minY = bottom.y, maxY = top.y;

        for (float x = minX; x <= maxX; x += stepX)
        {
            for (float y = minY; y <= maxY; y += stepY)
            {
                if (IsInsideDiamond(new Vector2(x, y)))
                {
                    Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
                }
            }
        }
    }

    public void SpawnOneInDiamond()
    {
        if (prefab == null) { Debug.LogError("[DiamondSpawner] Prefab is NULL"); return; }

        float minX = left.x, maxX = right.x;
        float minY = bottom.y, maxY = top.y;

        List<Vector2> validPoints = new List<Vector2>();

        for (float x = minX; x <= maxX; x += stepX)
        {
            for (float y = minY; y <= maxY; y += stepY)
            {
                if (IsInsideDiamond(new Vector2(x, y)))
                    validPoints.Add(new Vector2(x, y));
            }
        }

        if (validPoints.Count == 0) return;

        Vector2 chosen = validPoints[Random.Range(0, validPoints.Count)];
        Instantiate(prefab, new Vector3(chosen.x, chosen.y, 0f), Quaternion.identity);
    }

    private bool IsInsideDiamond(Vector2 point)
    {
        return IsOnSameSide(point, top, right, bottom)
            && IsOnSameSide(point, right, bottom, left)
            && IsOnSameSide(point, bottom, left, top)
            && IsOnSameSide(point, left, top, right);
    }

    private bool IsOnSameSide(Vector2 point, Vector2 a, Vector2 b, Vector2 reference)
    {
        Vector2 edge = b - a;
        float crossPoint = edge.x * (point.y - a.y) - edge.y * (point.x - a.x);
        float crossReference = edge.x * (reference.y - a.y) - edge.y * (reference.x - a.x);
        return crossPoint * crossReference >= 0f;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DiamondSpawner))]
public class DiamondSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DiamondSpawner spawner = (DiamondSpawner)target;

        if (GUILayout.Button("Spawn In Diamond"))
            spawner.SpawnInDiamond();

        if (GUILayout.Button("Spawn One Random In Diamond"))
            spawner.SpawnOneInDiamond();
    }
}
#endif