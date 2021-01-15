﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleArea : PostEffectBase<CameraEffect_DepthCircleArea, PostEffectParam_DepthCirCleArea>
    {
    }
    [Serializable]
    public struct PostEffectParam_DepthCirCleArea
    {
        public Vector3 m_Origin;
        public float m_Radius;
        public float m_SqrOutline;
        public Color m_FillColor;
        public Color m_EdgeColor;
        public Texture2D m_FillTexure;
        [RangeVector(-5,5)] public Vector2 m_FillTextureFlow;
        public float m_FillTextureScale;

        public static readonly PostEffectParam_DepthCirCleArea m_Default = new PostEffectParam_DepthCirCleArea()
        {
            m_Origin = Vector3.zero,
            m_Radius = 5f,
            m_SqrOutline = 1f,
            m_FillColor=Color.white,
            m_EdgeColor=Color.black,
            m_FillTextureFlow=Vector2.one,
            m_FillTextureScale=1f,
        };
    }

    public class CameraEffect_DepthCircleArea:ImageEffectBase<PostEffectParam_DepthCirCleArea>
    {
        readonly int ID_Origin = Shader.PropertyToID("_Origin");
        readonly int ID_FillColor = Shader.PropertyToID("_FillColor");
        readonly int ID_FillTexture = Shader.PropertyToID("_FillTexture");
        readonly int ID_FillTextureScale = Shader.PropertyToID("_TextureScale");
        readonly int ID_FillTextureFlow = Shader.PropertyToID("_TextureFlow");
        readonly int ID_EdgeColor = Shader.PropertyToID("_EdgeColor");
        readonly int ID_SqrEdgeMin = Shader.PropertyToID("_SqrEdgeMin");
        readonly int ID_SqrEdgeMax = Shader.PropertyToID("_SqrEdgeMax");

        #region ShaderProperties
        #endregion
        protected override void OnValidate(PostEffectParam_DepthCirCleArea _params, Material _material)
        {
            base.OnValidate(_params, _material);
            float sqrEdgeMin = _params.m_Radius;
            float sqrEdgeMax = _params.m_Radius + _params.m_SqrOutline;
            _material.SetFloat(ID_SqrEdgeMax, sqrEdgeMax * sqrEdgeMax);
            _material.SetFloat(ID_SqrEdgeMin, sqrEdgeMin * sqrEdgeMin);
            _material.SetVector(ID_Origin, _params.m_Origin);
            _material.SetColor(ID_FillColor, _params.m_FillColor);
            _material.SetColor(ID_EdgeColor, _params.m_EdgeColor);
            _material.SetTexture(ID_FillTexture, _params.m_FillTexure);
            _material.SetFloat(ID_FillTextureScale, _params.m_FillTextureScale);
            _material.SetVector(ID_FillTextureFlow, _params.m_FillTextureFlow);
        }
    }
}