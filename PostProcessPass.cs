using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomPostProcessURP
{
    public class PostProcessPass : ScriptableRenderPass
    {
        protected Material m_BlitMaterial;
        protected string  m_ProfilerTag;
        protected Shader m_Shader;
        protected bool m_EnableCameraParameter;
        private static int _ClipToWorldMatrix = Shader.PropertyToID("_ClipToWorldMatrix");
        private static int _CameraForward = Shader.PropertyToID("_CameraForward");
        private static int _NearPlane = Shader.PropertyToID("_NearPlane");
        private static int _CameraWorldPos = Shader.PropertyToID("_CameraWorldPos");

        void CreateMaterial()
        {
            if (!m_BlitMaterial)
            {
                var shader = m_Shader;
                if (!shader)
                {
                    Debug.LogError($"Could not find shader");
                    return;
                }

                m_BlitMaterial = new Material(shader);
            }
        }

        public virtual void Setup(RenderPassEvent evt, bool enableCameraParam, string profilerTag, Shader shader)
        {
            m_ProfilerTag = profilerTag;
            m_Shader = shader;
            renderPassEvent = evt;
            m_EnableCameraParameter = enableCameraParam;
            CreateMaterial();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            if (isSceneViewCamera)
            {
                return;
            }

            if (m_BlitMaterial == null)
            {
                Debug.LogErrorFormat(
                    "Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.",
                    m_BlitMaterial, GetType().Name);
                return;
            }


            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);


            Camera camera = cameraData.camera;

            if (m_EnableCameraParameter)
            {
                m_BlitMaterial.SetVector(_CameraForward, camera.transform.forward);
                m_BlitMaterial.SetMatrix(_ClipToWorldMatrix,
                    camera.cameraToWorldMatrix * camera.projectionMatrix.inverse);
                m_BlitMaterial.SetFloat(_NearPlane, camera.nearClipPlane);
                m_BlitMaterial.SetVector(_CameraWorldPos, camera.transform.position);
            }


            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(camera.pixelRect);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);


            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}