using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct GPolygon:IShape
    {
        public float3[] positions;
        [NonSerialized] public float3 center;
        public GPolygon(params float3[] _positions)
        {
            positions = _positions;
            center = _positions.Average();
        }

        public float3 GetSupportPoint(float3 _direction)=>positions.MaxElement(_p => math.dot(_direction, _p));
        public float3 Center => center;

        public void DrawGizmos() => UGizmos.DrawLinesConcat(positions);
        public static readonly GPolygon kDefault = new GPolygon(kfloat3.forward,kfloat3.right,kfloat3.back,kfloat3.left);
    }

    [Serializable]
    public struct G2Polygon : I2Shape , IEnumerable<float2>
    {
        public float2[] positions;
        [NonSerialized] public float2 center;
        public G2Polygon(IEnumerable<float2> _positions) : this(_positions.ToArray()) { }
        public G2Polygon(params float2[] _positions)
        {
            positions = _positions;
            center = _positions.Average();
        }


        public float2 GetSupportPoint(float2 _direction)=>positions.MaxElement(_p => math.dot(_direction, _p));
        public float2 Center => center;
        public static readonly G2Polygon kDefault = new G2Polygon(kfloat2.up,kfloat2.right,kfloat2.down,kfloat2.left);
        public float2 this[int _value] => positions[_value];
        public int Count => positions.Length;
        public IEnumerator<float2> GetEnumerator()
        {
            foreach (var point in positions)
                yield return point;
        }
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }
}