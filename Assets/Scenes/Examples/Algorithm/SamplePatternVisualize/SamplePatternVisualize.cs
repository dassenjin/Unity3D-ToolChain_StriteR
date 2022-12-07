using System;
using UnityEngine;
using Unity.Mathematics;
using Rendering.Pipeline;

namespace ExampleScenes.Algorithm.SamplePatternVisualize
{
    public enum ESamplePattern
    {
        Grid,
        Stratified,
        Halton,
        HammersLey,
    }
    
    public class SamplePatternVisualize : MonoBehaviour
    {
        public ESamplePattern patternType = ESamplePattern.Grid;
        public int patternWidth=4,patternHeight=4;

        float2[] patterns;

        private void OnValidate()
        {
            switch (patternType)
            {
                case ESamplePattern.Grid:
                    patterns=SamplePattern2D.Grid(patternWidth,patternHeight);
                    break;
                case ESamplePattern.Stratified:
                    patterns=SamplePattern2D.Stratified(patternWidth,patternHeight,true);
                    break;
                case ESamplePattern.Halton:
                    patterns = SamplePattern2D.Halton((uint)(patternWidth * patternHeight));
                    break;
                case ESamplePattern.HammersLey:
                    patterns = SamplePattern2D.Hammersley((uint)(patternWidth * patternHeight));
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            int size = patterns.Length;
            
            for (int i = -5; i < 5; i++)
            {
                for (int j = -5; j < 5; j++)
                {
                    Gizmos.color = Color.white;
                    Gizmos.matrix = Matrix4x4.Translate(new Vector3(i+.5f,0,j+.5f));
                    Gizmos.DrawWireCube(Vector3.zero,Vector3.one.SetY(0f));
                    Gizmos.color = Color.red;
                    for (int k = 0; k < size; k++)
                    {
                        var pattern = patterns[k];
                        Gizmos.DrawSphere(new Vector3(pattern.x,0,pattern.y),.03f);
                    }
                }
            }
        }
    }
}
