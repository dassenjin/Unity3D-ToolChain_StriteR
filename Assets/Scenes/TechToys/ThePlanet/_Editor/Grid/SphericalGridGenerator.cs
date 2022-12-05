using System;
using Geometry;
using Geometry.Explicit;
using Geometry.Explicit.Mesh;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace PCG.Grid
{
    [Serializable]
    public class SphericalGridGenerator : IGridGenerator
    {
        public Transform transform { get; set; }
        public int resolution;
        private GridChunkData m_ChunkData;
        public void Setup()
        {
        }

        public void Tick(float _deltaTime)
        {
        }

        public void Clear()
        {
        }

        public void OnSceneGUI(SceneView _sceneView)
        {
        }

        public void OnGizmos()
        {
            float r = resolution;

            for (int k = 0; k < KGeometryMesh.kCubeFacingAxisCount; k++)
            {
                var _axis = KGeometryMesh.GetCubeFacingAxis(k);
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var iTR = USphereExplicit.GetCubeSphereIndex(i + 1, j + 1, resolution, _axis.index);
                    var iTL = USphereExplicit.GetCubeSphereIndex(i, j + 1, resolution, _axis.index);
                    var iBR = USphereExplicit.GetCubeSphereIndex(i + 1, j, resolution, _axis.index);
                    var iBL = USphereExplicit.GetCubeSphereIndex(i, j, resolution, _axis.index);
                    
                    var vTR =  UGeometryMesh.CubeToSphere(_axis.origin + (i + 1)/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vTL =  UGeometryMesh.CubeToSphere(_axis.origin + i/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vBR =  UGeometryMesh.CubeToSphere(_axis.origin + (i + 1)/r * _axis.uDir + j/r * _axis.vDir);
                    var vBL =  UGeometryMesh.CubeToSphere(_axis.origin + i/r * _axis.uDir + j/r * _axis.vDir);
                    
                    Gizmos.DrawWireSphere(vTR,.1f);
                    Gizmos.DrawWireSphere(vTL,.1f);
                    Gizmos.DrawWireSphere(vBR,.1f);
                    Gizmos.DrawWireSphere(vBL,.1f);
                }
            }
            
        }

        public void Output(GridCollection _collection)
        {         
            float r = resolution;
            _collection.vertices = new GridVertexData[USphereExplicit.GetCubeSphereVertexCount(resolution)];
            _collection.chunks = new GridChunkData[] {new GridChunkData(){ quads =new GridQuadData[USphereExplicit.GetCubeSphereQuadCount(resolution)]}};
            int quadIndex = 0;
            for (int k = 0; k < KGeometryMesh.kCubeFacingAxisCount; k++)
            {
                var _axis = KGeometryMesh.GetCubeFacingAxis(k);
                for (int j = 0; j < resolution; j++)
                for (int i = 0; i < resolution; i++)
                {
                    var iTR = USphereExplicit.GetCubeSphereIndex(i + 1, j + 1, resolution, _axis.index);
                    var iTL = USphereExplicit.GetCubeSphereIndex(i, j + 1, resolution, _axis.index);
                    var iBR = USphereExplicit.GetCubeSphereIndex(i + 1, j, resolution, _axis.index);
                    var iBL = USphereExplicit.GetCubeSphereIndex(i, j, resolution, _axis.index);
                    
                    var vTR =  UGeometryMesh.CubeToSphere(_axis.origin + (i + 1)/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vTL =  UGeometryMesh.CubeToSphere(_axis.origin + i/r * _axis.uDir + (j+1)/r * _axis.vDir);
                    var vBR =  UGeometryMesh.CubeToSphere(_axis.origin + (i + 1)/r * _axis.uDir + j/r * _axis.vDir);
                    var vBL =  UGeometryMesh.CubeToSphere(_axis.origin + i/r * _axis.uDir + j/r * _axis.vDir);

                    _collection.vertices[iTR] = new GridVertexData() {position = vTR,normal = math.normalize(vTR) , invalid = true};
                    _collection.vertices[iTL] = new GridVertexData() {position = vTL,normal = math.normalize(vTL) , invalid = true};
                    _collection.vertices[iBR] = new GridVertexData() {position = vBR,normal = math.normalize(vBR) , invalid = true};
                    _collection.vertices[iBL] = new GridVertexData() {position = vBL,normal = math.normalize(vBL) , invalid = true};
                    _collection.chunks[0].quads[quadIndex++] = new GridQuadData() {vertices = new PQuad(iBL,iTL,iTR,iBR)};
                }
            }
        }

    }
    
}