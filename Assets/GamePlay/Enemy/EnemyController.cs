using System;
using System.Collections.Generic;
using GamePlay.GridMap;
using Managers;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] private Vector2Int middleNode;
    [SerializeField] private List<Vector2Int> bossRange;
    [SerializeField] private List<Vector2Int> currentNode;
    public List<Vector2Int> CurrentNode => currentNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = GridMath.NodeToWorld(
            middleNode.x, 
            middleNode.y, 
            GameManager.Instance.gridManager.Origin, 
            GameManager.Instance.gridManager.CellWorldSize);
        
        ComputeBossNodes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ComputeBossNodes()
    {
        currentNode.Add(middleNode);
        foreach (var range in bossRange)
        {
            currentNode.Add(middleNode + range);
        }
    }

}
