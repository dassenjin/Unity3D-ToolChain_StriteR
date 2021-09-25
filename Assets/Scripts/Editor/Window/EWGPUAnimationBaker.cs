﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Rendering.Optimize;

namespace TEditor
{
    public class GPUAnimationBaker : EditorWindow
    {
        GameObject m_TargetPrefab;
        SerializedObject m_SerializedWindow;
        [SerializeField] AnimationClip[] m_TargetAnimations;
        SerializedProperty m_AnimationProperty;
        string m_BoneExposeRegex="";
        EGPUAnimationMode m_Mode= EGPUAnimationMode.Transform;

        void OnEnable()
        {
            m_TargetAnimations = null;
            m_SerializedWindow = new SerializedObject(this);
            m_AnimationProperty = m_SerializedWindow.FindProperty(nameof(m_TargetAnimations));
            EditorApplication.update += Tick;
        }
        void OnDisable()
        {
            m_TargetPrefab = null;
            m_TargetAnimations = null;
            m_SerializedWindow.Dispose();
            m_AnimationProperty.Dispose();
            EditorApplication.update -= Tick;
        }
        private void Tick()
        {
            EditorUtility.SetDirty(this);
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DrawGUI();
            EditorGUILayout.EndVertical();
        }
        void DrawGUI()
        {
            //Select Prefab
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select FBX&Animation Datas");
            m_TargetPrefab = (GameObject)EditorGUILayout.ObjectField(m_TargetPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_AnimationProperty, true);
            m_SerializedWindow.ApplyModifiedProperties();
            //Select AnimationClipAsset  
            List<AnimationClip> clip = new List<AnimationClip>();
            foreach (Object obj in Selection.objects)
            {
                if ((obj as AnimationClip) != null && AssetDatabase.IsMainAsset(obj))
                    clip.Add(obj as AnimationClip);
                else if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj)) as ModelImporter != null)
                    m_TargetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(obj));
            }
            if(clip.Count>0)
            {
                m_TargetAnimations = clip.ToArray();
                m_SerializedWindow.Update();
            }

            ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_TargetPrefab)) as ModelImporter;
            if (m_TargetPrefab == null || importer == null || m_TargetAnimations==null || m_TargetAnimations.Length==0)
            {
                EditorGUILayout.LabelField("<Color=#FF0000>Select FBX Model & Animations</Color>", UEGUIStyle_Window.m_ErrorLabel);
                return;
            }
            
            //Generate Animation 
            if (m_TargetAnimations == null || m_TargetAnimations.Length == 0 || m_TargetAnimations.Any(p => p == null))
                return;

            EditorGUILayout.BeginHorizontal();
            m_Mode = (EGPUAnimationMode)EditorGUILayout.EnumPopup("Typs:",m_Mode);
            EditorGUILayout.EndHorizontal();

            if(m_Mode==EGPUAnimationMode.Vertex)
            {
                if (GUILayout.Button("Generate"))
                    GenerateVertexTexture(m_TargetPrefab, m_TargetAnimations);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Expose Transform Regex:");
                m_BoneExposeRegex = GUILayout.TextArea(m_BoneExposeRegex);
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Generate"))
                {
                    if (importer.optimizeGameObjects)
                    {
                        EditorUtility.DisplayDialog("Error!", "Can't Bake Optimized FBX With Bone Instance", "Confirm");
                        return;
                    }
                    GenerateBoneInstanceMeshAndTexture(m_TargetPrefab, m_TargetAnimations, m_BoneExposeRegex);
                }
            }
        }

        static int GetInstanceParams(AnimationClip[] _clips, out AnimationTickerClip[] instanceParams)
        {
            int totalHeight = 0;
            instanceParams = new AnimationTickerClip[_clips.Length];
            for (int i = 0; i < _clips.Length; i++)
            {
                AnimationClip clip = _clips[i];

                AnimationTickerEvent[] instanceEvents = new AnimationTickerEvent[clip.events.Length];
                for (int j = 0; j < clip.events.Length; j++)
                    instanceEvents[j] = new AnimationTickerEvent(clip.events[j], clip.frameRate);
                int frameCount = (int)(clip.length * clip.frameRate);
                instanceParams[i] = new AnimationTickerClip(clip.name, totalHeight, clip.frameRate, clip.length, clip.isLooping, instanceEvents.ToArray());
                totalHeight += frameCount;
            }
            return totalHeight;
        }
        void GenerateVertexTexture(GameObject _targetFBX, AnimationClip[] _clips)
        {
            if (!UEAsset.SelectDirectory(_targetFBX, out string savePath, out string meshName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }
            GameObject instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            SkinnedMeshRenderer skinnedMeshRenderer = instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            #region Bake Animation Atlas
            int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;
            int totalVertexRecord = vertexCount * 2;
            int totalFrame = GetInstanceParams(_clips, out AnimationTickerClip[] instanceParams);

            Texture2D atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalVertexRecord), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false);
            atlasTexture.filterMode = FilterMode.Point;
            atlasTexture.wrapModeU = TextureWrapMode.Clamp;
            atlasTexture.wrapModeV = TextureWrapMode.Repeat;
            UBoundsChecker.Begin();
            for (int i = 0; i < _clips.Length; i++)
            {
                AnimationClip clip = _clips[i];
                Mesh vertexBakeMesh = new Mesh();
                float length = clip.length;
                float frameRate = clip.frameRate;
                int frameCount = (int)(length * frameRate);
                int startFrame = instanceParams[i].frameBegin;
                for (int j = 0; j < frameCount; j++)
                {
                    clip.SampleAnimation(instantiatedObj, length * j / frameCount);
                    skinnedMeshRenderer.BakeMesh(vertexBakeMesh);
                    Vector3[] vertices = vertexBakeMesh.vertices;
                    Vector3[] normals = vertexBakeMesh.normals;
                    for (int k = 0; k < vertexCount; k++)
                    {
                        UBoundsChecker.CheckBounds(vertices[k]);
                        atlasTexture.SetPixel(k * 2, startFrame + j, UColor.VectorToColor(vertices[k]));
                        atlasTexture.SetPixel(k * 2 + 1, startFrame + j, UColor.VectorToColor(normals[k]));
                    }
                }
                vertexBakeMesh.Clear();
            }
            atlasTexture.Apply();
            #endregion

            #region Bake Mesh
            Mesh instanceMesh = skinnedMeshRenderer.sharedMesh.Copy();
            instanceMesh.normals = null;
            instanceMesh.tangents = null;
            instanceMesh.boneWeights = null;
            instanceMesh.bindposes = null;
            instanceMesh.bounds = UBoundsChecker.CalculateBounds();
            #endregion
            DestroyImmediate(instantiatedObj);

            GPUAnimationData data = ScriptableObject.CreateInstance<GPUAnimationData>();
            data.m_Mode = EGPUAnimationMode.Vertex;
            data.m_AnimationClips = instanceParams;
            data=UEAsset.CreateAssetCombination(savePath + meshName + "_GPU_Vertex.asset", data, new KeyValuePair< string, Object>(meshName + "_AnimationAtlas", atlasTexture),new KeyValuePair<string,Object>( meshName + "_InstanceMesh",instanceMesh));
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(data));
            foreach (var asset in assets)
            {
                Texture2D atlas = asset as Texture2D;
                Mesh mesh = asset as Mesh;
                if (atlas)
                    data.m_BakeTexture = atlas;
                if (mesh)
                    data.m_BakedMesh = mesh;
            }
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        void GenerateBoneInstanceMeshAndTexture(GameObject _targetFBX, AnimationClip[] _clips, string exposeBones)
        {
            if (!UEAsset.SelectDirectory(_targetFBX, out string savePath, out string meshName))
            {
                Debug.LogWarning("Invalid Folder Selected");
                return;
            }
            GameObject _instantiatedObj = GameObject.Instantiate(m_TargetPrefab);
            SkinnedMeshRenderer _skinnedMeshRenderer = _instantiatedObj.GetComponentInChildren<SkinnedMeshRenderer>();
            try
            {
                Matrix4x4[] bindPoses = _skinnedMeshRenderer.sharedMesh.bindposes;
                Transform[] bones = _skinnedMeshRenderer.bones;
                #region Record Expose Bone
                List<GPUAnimationExposeBone> exposeTransformParam = new List<GPUAnimationExposeBone>();
                if (exposeBones != "")
                {
                    Transform[] activeTransforms = _instantiatedObj.GetComponentsInChildren<Transform>();
                    for (int i = 0; i < activeTransforms.Length; i++)
                    {
                        if (!System.Text.RegularExpressions.Regex.Match(activeTransforms[i].name, exposeBones).Success)
                            continue;
                        int relativeBoneIndex = -1;
                        Transform relativeBone = activeTransforms[i];
                        while (relativeBone != null)
                        {
                            relativeBoneIndex = System.Array.FindIndex( bones,p => p == relativeBone);
                            if (relativeBoneIndex != -1)
                                break;
                            relativeBone = relativeBone.parent;
                        }
                        if (relativeBoneIndex == -1)
                            continue;

                        Matrix4x4 rootWorldToLocal = _skinnedMeshRenderer.transform.worldToLocalMatrix;

                        exposeTransformParam.Add(new GPUAnimationExposeBone()
                        {
                            index = relativeBoneIndex,
                            name = activeTransforms[i].name,
                            position = rootWorldToLocal.MultiplyPoint(activeTransforms[i].transform.position),
                            direction = rootWorldToLocal.MultiplyVector(activeTransforms[i].transform.forward)
                        });
                    }
                }
                #endregion
                #region Bake Animation Atlas
                int transformCount = _skinnedMeshRenderer.sharedMesh.bindposes.Length;
                int totalWdith = transformCount * 3;
                int totalFrame = GetInstanceParams(_clips, out AnimationTickerClip[] instanceParams);
                List<AnimationTickerEvent> instanceEvents = new List<AnimationTickerEvent>();

                Texture2D atlasTexture = new Texture2D(Mathf.NextPowerOfTwo(totalWdith), Mathf.NextPowerOfTwo(totalFrame), TextureFormat.RGBAHalf, false);
                atlasTexture.filterMode = FilterMode.Point;
                atlasTexture.wrapModeU = TextureWrapMode.Clamp;
                atlasTexture.wrapModeV = TextureWrapMode.Repeat;
                UBoundsChecker.Begin();
                for (int i = 0; i < _clips.Length; i++)
                {
                    AnimationClip clip = _clips[i];
                    float length = clip.length;
                    float frameRate = clip.frameRate;
                    int frameCount = (int)(length * frameRate);
                    int startFrame = instanceParams[i].frameBegin;
                    for (int j = 0; j < frameCount; j++)
                    {
                        clip.SampleAnimation(_instantiatedObj, length * j / frameCount);
                        for (int k = 0; k < transformCount; k++)
                        {
                            Matrix4x4 curFrameTransformMatrix = bones[k].localToWorldMatrix * bindPoses[k];
                            atlasTexture.SetPixel(k * 3, startFrame + j, UColor.VectorToColor(curFrameTransformMatrix.GetRow(0)));
                            atlasTexture.SetPixel(k * 3 + 1, startFrame + j, UColor.VectorToColor(curFrameTransformMatrix.GetRow(1)));
                            atlasTexture.SetPixel(k * 3 + 2, startFrame + j, UColor.VectorToColor( curFrameTransformMatrix.GetRow(2)));
                        }

                        Mesh boundsCheckMesh = new Mesh();
                        _skinnedMeshRenderer.BakeMesh(boundsCheckMesh);
                        Vector3[] verticies = boundsCheckMesh.vertices;
                        for (int k = 0; k < verticies.Length; k++)
                            UBoundsChecker.CheckBounds(verticies[k].Divide(_skinnedMeshRenderer.transform.localScale));

                        boundsCheckMesh.Clear();
                    }
                }
                atlasTexture.Apply();
                #endregion
                #region Bake Mesh
                Mesh instanceMesh = _skinnedMeshRenderer.sharedMesh.Copy();
                BoneWeight[] transformWeights = instanceMesh.boneWeights;
                Vector4[] uv1 = new Vector4[transformWeights.Length];
                Vector4[] uv2 = new Vector4[transformWeights.Length];
                for (int i = 0; i < transformWeights.Length; i++)
                {
                    uv1[i] = new Vector4(transformWeights[i].boneIndex0, transformWeights[i].boneIndex1, transformWeights[i].boneIndex2, transformWeights[i].boneIndex3);
                    uv2[i] = new Vector4(transformWeights[i].weight0, transformWeights[i].weight1, transformWeights[i].weight2, transformWeights[i].weight3);
                }
                instanceMesh.SetUVs(1, uv1);
                instanceMesh.SetUVs(2, uv2);
                instanceMesh.boneWeights = null;
                instanceMesh.bindposes = null;
                instanceMesh.bounds = UBoundsChecker.CalculateBounds();
                #endregion
                DestroyImmediate(_instantiatedObj);

                GPUAnimationData data = ScriptableObject.CreateInstance<GPUAnimationData>();
                data.m_Mode = EGPUAnimationMode.Transform;
                data.m_AnimationClips = instanceParams;
                data.m_ExposeTransforms = exposeTransformParam.ToArray();

                data = UEAsset.CreateAssetCombination(savePath + meshName + "_GPU_Transform.asset",data, new KeyValuePair<string,Object>(meshName + "_AnimationAtlas",atlasTexture), new KeyValuePair<string,Object>(meshName + "_InstanceMesh",instanceMesh));
                Object[] assets=AssetDatabase.LoadAllAssetsAtPath( AssetDatabase.GetAssetPath(data));
                foreach(var asset in assets)
                {
                    Texture2D atlas = asset as Texture2D;
                    Mesh mesh = asset as Mesh;
                    if (atlas)
                        data.m_BakeTexture = atlas;
                    if (mesh)
                        data.m_BakedMesh = mesh;
                }
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Generate Failed:" + e.Message);
                DestroyImmediate(_instantiatedObj);
            }
        }
    }
}