using System;
using UnityEngine;

namespace GamePlay.GridMap
{
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly Vector2Int A;
        public readonly Vector2Int B;

        public Edge(Vector2Int a, Vector2Int b)
        {
            if (a.x < b.x || (a.x == b.x && a.y <= b.y))
            {
                A = a;
                B = b;
            }
            else
            {
                A = b;
                B = a;
            }
        }

        public bool Equals(Edge other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is Edge other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(A, B);
    }
}