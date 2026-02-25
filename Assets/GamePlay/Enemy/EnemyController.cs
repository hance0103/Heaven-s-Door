using System;
using System.Collections.Generic;
using GamePlay.GridMap;
using Managers;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 보스 중앙 노드 (위치)
    [SerializeField] private Vector2Int currentNode;
    public Vector2Int CurrentNode => currentNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = GridMath.NodeToWorld(
            currentNode.x, 
            currentNode.y, 
            GameManager.Instance.gridManager.Origin, 
            GameManager.Instance.gridManager.CellWorldSize);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void MoveToNode(Vector2Int node)
    {
        
    }
}
