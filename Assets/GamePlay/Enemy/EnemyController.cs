using System;
using System.Collections.Generic;
using GamePlay.GridMap;
using Managers;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 보스 중앙 노드 (위치)
    [SerializeField] private Vector2Int middleNode;
    // 중앙 기준으로 보스가 차지하는 노드 범위
    [SerializeField] private List<Vector2Int> bossRange;
    // 현재 보스가 차지하고 있는 노드들
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
        currentNode.Clear();
        currentNode.Add(middleNode);
        foreach (var range in bossRange)
        {
            currentNode.Add(middleNode + range);
        }
    }

    private void MoveToNode(Vector2Int node)
    {
        
    }
}
