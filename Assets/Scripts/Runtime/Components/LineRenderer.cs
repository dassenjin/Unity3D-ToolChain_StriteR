using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class FLineRenderer : ALineRendererBase
    {
        public Vector3[] m_LinePositions;
        
        private static int kInstanceID = 0;
        protected override string GetInstanceName() => $"Line - ({kInstanceID++})";

        //I should put all these stuffs into shaders ?
        protected override void PopulatePositions(Transform _transform, List<Vector3> _vertices, List<Vector3> _normals)
        {
            if (m_LinePositions == null) return;
            
            for (int i = 0; i < m_LinePositions.Length; i++)
            {
                _vertices.Add(m_LinePositions[i]);
                _normals.Add(Vector3.right);
            }
        }
    }

    public class LineRenderer : ARuntimeRendererMonoBehaviour<FLineRenderer>
    {

    }

}
