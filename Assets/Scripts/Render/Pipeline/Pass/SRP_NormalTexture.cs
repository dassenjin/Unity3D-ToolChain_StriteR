using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using static KRenderTextures;
    
    public class SRP_NormalTexture : ScriptableRenderPass, ISRPBase
    {
        PassiveInstance<Material> m_NormalMaterial=new PassiveInstance<Material>(()=>new Material( RenderResources.FindInclude("Hidden/NormalsFromDepth"))  {hideFlags = HideFlags.HideAndDontSave},GameObject.DestroyImmediate);

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(kCameraNormalTex, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            ConfigureTarget(kRTCameraNormalTex);
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(kCameraNormalTex);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Normal Texture");
            cmd.Blit(null, kRTCameraNormalTex, m_NormalMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        public void Dispose()
        {
            m_NormalMaterial.Dispose();
        }
    }
}