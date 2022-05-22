using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CustomPostProcessURP
{
    public class CustomPostProcessRenderFeature : ScriptableRendererFeature
    {
        public RenderPassEvent renderPassEvent;
        public bool enableCameraParam;
        public Shader shader;
        
        
        private PostProcessPass m_PostProcessPass;

        public override void Create()
        {
            if (m_PostProcessPass == null)
            {
                m_PostProcessPass = new PostProcessPass();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_PostProcessPass != null)
            {
                m_PostProcessPass.Setup(renderPassEvent,enableCameraParam,this.name,shader);

                renderer.EnqueuePass(m_PostProcessPass);
            }
        }
    }
}