using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

// Following copied from KittopiaTech (also an MIT license)
// Original code:  https://github.com/Kopernicus/KittopiaTech/blob/1a7256734b5f0dd25795c68c47638a919a716446/src/RuntimePreviewGenerator.cs

namespace ResonantOrbitCalculator
{
    // Source: https://github.com/yasirkula/UnityRuntimePreviewGenerator
    public static class RuntimePreviewGenerator
    {
        // Source: https://github.com/MattRix/UnityDecompiled/blob/master/UnityEngine/UnityEngine/Plane.cs
        private struct ProjectionPlane
        {
            private readonly Vector3 m_Normal;
            private readonly Single m_Distance;

            public ProjectionPlane(Vector3 inNormal, Vector3 inPoint)
            {
                m_Normal = Vector3.Normalize(inNormal);
                m_Distance = -Vector3.Dot(inNormal, inPoint);
            }

            public Vector3 ClosestPointOnPlane(Vector3 point)
            {
                Single d = Vector3.Dot(m_Normal, point) + m_Distance;
                return point - m_Normal * d;
            }

            public Single GetDistanceToPoint(Vector3 point)
            {
                Single signedDistance = Vector3.Dot(m_Normal, point) + m_Distance;
                if (signedDistance < 0f)
                    signedDistance = -signedDistance;

                return signedDistance;
            }
        }

        private class CameraSetup
        {
            private Vector3 position;
            private Quaternion rotation;

            private RenderTexture targetTexture;

            private Color backgroundColor;
            private Boolean orthographic;
            private Single orthographicSize;
            private Single nearClipPlane;
            private Single farClipPlane;
            private Single aspect;
            private CameraClearFlags clearFlags;

            public void GetSetup(Camera camera)
            {
                position = camera.transform.position;
                rotation = camera.transform.rotation;

                targetTexture = camera.targetTexture;

                backgroundColor = camera.backgroundColor;
                orthographic = camera.orthographic;
                orthographicSize = camera.orthographicSize;
                nearClipPlane = camera.nearClipPlane;
                farClipPlane = camera.farClipPlane;
                aspect = camera.aspect;
                clearFlags = camera.clearFlags;
            }

            public void ApplySetup(Camera camera)
            {
                camera.transform.position = position;
                camera.transform.rotation = rotation;

                camera.targetTexture = targetTexture;

                camera.backgroundColor = backgroundColor;
                camera.orthographic = orthographic;
                camera.orthographicSize = orthographicSize;
                camera.nearClipPlane = nearClipPlane;
                camera.farClipPlane = farClipPlane;
                camera.aspect = aspect;
                camera.clearFlags = clearFlags;

                targetTexture = null;
            }
        }

        private const Int32 PREVIEW_LAYER = 22;
        private static Vector3 PREVIEW_POSITION = new Vector3(-9245f, 9899f, -9356f);

        private static Camera renderCamera;
        private static CameraSetup cameraSetup = new CameraSetup();

        private static List<Renderer> renderersList = new List<Renderer>(64);
        private static List<Int32> layersList = new List<Int32>(64);

        private static Single aspect;
        private static Single minX, maxX, minY, maxY;
        private static Single maxDistance;

        private static Vector3 boundsCenter;
        private static ProjectionPlane projectionPlaneHorizontal, projectionPlaneVertical;

        private static Camera m_internalCamera;

        private static Camera InternalCamera
        {
            get
            {
                if (m_internalCamera == null)
                {
                    m_internalCamera = new GameObject("ModelPreviewGeneratorCamera").AddComponent<Camera>();
                    m_internalCamera.enabled = false;
                    m_internalCamera.nearClipPlane = 0.01f;
                    m_internalCamera.cullingMask = 1 << PREVIEW_LAYER;
                    m_internalCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }

                return m_internalCamera;
            }
        }

        private static Camera m_previewRenderCamera;

        public static Camera PreviewRenderCamera
        {
            get { return m_previewRenderCamera; }
            set { m_previewRenderCamera = value; }
        }

        private static Vector3 m_previewDirection;

        public static Vector3 PreviewDirection
        {
            get { return m_previewDirection; }
            set { m_previewDirection = value.normalized; }
        }

        private static Single m_padding;

        public static Single Padding
        {
            get { return m_padding; }
            set { m_padding = Mathf.Clamp(value, -0.25f, 0.25f); }
        }

        private static Color m_backgroundColor;

        public static Color BackgroundColor
        {
            get { return m_backgroundColor; }
            set { m_backgroundColor = value; }
        }

        private static Boolean m_orthographicMode;

        public static Boolean OrthographicMode
        {
            get { return m_orthographicMode; }
            set { m_orthographicMode = value; }
        }

        private static Boolean m_transparentBackground;

        public static Boolean TransparentBackground
        {
            get { return m_transparentBackground; }
            set { m_transparentBackground = value; }
        }

        private static Boolean m_adjustLighting;

        public static Boolean AdjustLighting
        {
            get { return m_adjustLighting; }
            set { m_adjustLighting = value; }
        }


        static RuntimePreviewGenerator()
        {
            PreviewRenderCamera = null;
            PreviewDirection = new Vector3(-1f, -1f, -1f);
            Padding = 0f;
            BackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            OrthographicMode = false;
            TransparentBackground = false;
        }

        public static Texture2D GenerateMaterialPreview(Material material, PrimitiveType previewObject, Int32 width = 64,
         Int32 height = 64, Vector3 planetRotation = new Vector3(), Vector2 lightDirection = new Vector2(), Color? lightColor = null)
        {
            return GenerateMaterialPreviewWithShader(material, previewObject, null, null, width, height, planetRotation, lightDirection, lightColor);
        }

        public static Texture2D GenerateMaterialPreviewWithShader(Material material, PrimitiveType previewPrimitive,
         Shader shader, String replacementTag, Int32 width = 64, Int32 height = 64, Vector3 planetRotation = new Vector3(), Vector2 lightDirection = new Vector2(), Color? lightColor = null)
        {
            GameObject previewModel = GameObject.CreatePrimitive(previewPrimitive);
            previewModel.gameObject.hideFlags = HideFlags.HideAndDontSave;
            previewModel.GetComponent<Renderer>().sharedMaterial = material;

            try
            {
                return GenerateModelPreviewWithShader(previewModel.transform, shader, replacementTag, width, height, false, planetRotation, lightDirection, lightColor);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Object.DestroyImmediate(previewModel);
            }

            return null;
        }

        public static Texture2D GenerateModelPreview(Transform model, Int32 width = 64, Int32 height = 64,
         Boolean shouldCloneModel = false, Vector3 planetRotation = new Vector3(), Vector2 lightDirection = new Vector2(), Color? lightColor = null)
        {
            return GenerateModelPreviewWithShader(model, null, null, width, height, shouldCloneModel, planetRotation, lightDirection, lightColor);
        }

        public static Texture2D GenerateModelPreviewWithShader(Transform model, Shader shader, String replacementTag,
         Int32 width = 64, Int32 height = 64, Boolean shouldCloneModel = false, Vector3 planetRotation = new Vector3(), Vector2 lightDirection = new Vector2(), Color? lightColor = null)
        {
            if (model == null || model.Equals(null))
                return null;

            GameObject lightObject = new GameObject();
            Light light = lightObject.AddOrGetComponent<Light>();
            lightObject.transform.position = model.position + new Vector3(0, 0, -36);
            light.intensity = 1.5f;
            if (AdjustLighting)
            {
                light.intensity = 1.0f;
                AdjustLighting = false;
            }
            light.shadowBias = 0.047f;
            light.shadows = LightShadows.Soft;
            light.type = LightType.Directional;
            light.renderMode = LightRenderMode.ForceVertex; //removes the "spotlight" like reflection on the planets

            light.color = lightColor ?? Color.white;
            lightObject.transform.RotateAround(model.position, Vector3.right, lightDirection.x);
            lightObject.transform.RotateAround(model.position, Vector3.down, lightDirection.y);
            lightObject.transform.rotation = Quaternion.LookRotation(model.position - lightObject.transform.position, lightObject.transform.up);

            Texture2D result = null;

            if (!model.gameObject.scene.IsValid() || !model.gameObject.scene.isLoaded)
                shouldCloneModel = true;

            Transform previewObject;
            if (shouldCloneModel)
            {
                previewObject = Object.Instantiate(model, null, false);
                previewObject.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            else
            {
                previewObject = model;

                layersList.Clear();
                GetLayerRecursively(previewObject);
            }

            Boolean isStatic = IsStatic(model);
            Boolean wasActive = previewObject.gameObject.activeSelf;
            Vector3 prevPos = previewObject.position;
            Quaternion prevRot = previewObject.rotation;

            try
            {
                SetupCamera();
                SetLayerRecursively(previewObject);

                if (!isStatic)
                {
                    previewObject.position = PREVIEW_POSITION;
                    previewObject.rotation = Quaternion.identity;
                }

                if (!wasActive)
                    previewObject.gameObject.SetActive(true);

                Vector3 previewDir = previewObject.rotation * m_previewDirection;

                renderersList.Clear();
                previewObject.GetComponentsInChildren(renderersList);

                Bounds previewBounds = new Bounds();
                Boolean init = false;
                for (Int32 i = 0; i < renderersList.Count; i++)
                {
                    if (!renderersList[i].enabled)
                        continue;

                    if (!init)
                    {
                        previewBounds = renderersList[i].bounds;
                        init = true;
                    }
                    else
                        previewBounds.Encapsulate(renderersList[i].bounds);
                }

                if (!init)
                {
                    Object.DestroyImmediate(lightObject);
                    return null;
                }

                boundsCenter = previewBounds.center;
                Vector3 boundsExtents = previewBounds.extents;
                Vector3 boundsSize = 2f * boundsExtents;
                Vector3 up = previewObject.up;

                previewObject.eulerAngles = previewObject.eulerAngles + planetRotation;

                aspect = (Single)width / height;
                renderCamera.aspect = aspect;
                renderCamera.transform.rotation = Quaternion.LookRotation(previewDir, up);

                Single distance;
                if (m_orthographicMode)
                {
                    renderCamera.transform.position = boundsCenter;

                    minX = minY = Mathf.Infinity;
                    maxX = maxY = Mathf.NegativeInfinity;

                    Vector3 point = boundsCenter + boundsExtents;
                    ProjectBoundingBoxMinMax(point);
                    point.x -= boundsSize.x;
                    ProjectBoundingBoxMinMax(point);
                    point.y -= boundsSize.y;
                    ProjectBoundingBoxMinMax(point);
                    point.x += boundsSize.x;
                    ProjectBoundingBoxMinMax(point);
                    point.z -= boundsSize.z;
                    ProjectBoundingBoxMinMax(point);
                    point.x -= boundsSize.x;
                    ProjectBoundingBoxMinMax(point);
                    point.y += boundsSize.y;
                    ProjectBoundingBoxMinMax(point);
                    point.x += boundsSize.x;
                    ProjectBoundingBoxMinMax(point);

                    distance = boundsExtents.magnitude + 1f;
                    renderCamera.orthographicSize = (1f + m_padding * 2f) * Mathf.Max(maxY - minY, (maxX - minX) / aspect) * 0.5f;
                }
                else
                {
                    projectionPlaneHorizontal = new ProjectionPlane(renderCamera.transform.up, boundsCenter);
                    projectionPlaneVertical = new ProjectionPlane(renderCamera.transform.right, boundsCenter);

                    maxDistance = Mathf.NegativeInfinity;

                    Vector3 point = boundsCenter + boundsExtents;
                    CalculateMaxDistance(point);
                    point.x -= boundsSize.x;
                    CalculateMaxDistance(point);
                    point.y -= boundsSize.y;
                    CalculateMaxDistance(point);
                    point.x += boundsSize.x;
                    CalculateMaxDistance(point);
                    point.z -= boundsSize.z;
                    CalculateMaxDistance(point);
                    point.x -= boundsSize.x;
                    CalculateMaxDistance(point);
                    point.y += boundsSize.y;
                    CalculateMaxDistance(point);
                    point.x += boundsSize.x;
                    CalculateMaxDistance(point);

                    distance = (1f + m_padding * 2f) * Mathf.Sqrt(maxDistance);
                }

                renderCamera.transform.position = boundsCenter - previewDir * distance;
                renderCamera.farClipPlane = distance * 4f;

                RenderTexture temp = RenderTexture.active;
                RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 16);
                RenderTexture.active = renderTex;
                if (m_transparentBackground)
                    GL.Clear(false, true, Color.clear);

                renderCamera.targetTexture = renderTex;

                if (shader == null)
                    renderCamera.Render();
                else
                    renderCamera.RenderWithShader(shader, replacementTag ?? String.Empty);

                renderCamera.targetTexture = null;

                result = new Texture2D(width, height, m_transparentBackground ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                result.Apply(false, false);

                RenderTexture.active = temp;
                RenderTexture.ReleaseTemporary(renderTex);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (shouldCloneModel)
                    Object.DestroyImmediate(previewObject.gameObject);
                else
                {
                    if (!wasActive)
                        previewObject.gameObject.SetActive(false);

                    if (!isStatic)
                    {
                        previewObject.position = prevPos;
                        previewObject.rotation = prevRot;
                    }

                    Int32 index = 0;
                    SetLayerRecursively(previewObject, ref index);
                }

                if (renderCamera == m_previewRenderCamera)
                    cameraSetup.ApplySetup(renderCamera);
            }

            UnityEngine.Object.DestroyImmediate(lightObject);
            return result;
        }

        private static void SetupCamera()
        {
            if (m_previewRenderCamera != null && !m_previewRenderCamera.Equals(null))
            {
                cameraSetup.GetSetup(m_previewRenderCamera);

                renderCamera = m_previewRenderCamera;
                renderCamera.nearClipPlane = 0.01f;
            }
            else
                renderCamera = InternalCamera;

            renderCamera.backgroundColor = m_backgroundColor;
            renderCamera.orthographic = m_orthographicMode;
            renderCamera.clearFlags = m_transparentBackground ? CameraClearFlags.Depth : CameraClearFlags.Color;
        }

        private static void ProjectBoundingBoxMinMax(Vector3 point)
        {
            Vector3 localPoint = renderCamera.transform.InverseTransformPoint(point);
            if (localPoint.x < minX)
                minX = localPoint.x;
            if (localPoint.x > maxX)
                maxX = localPoint.x;
            if (localPoint.y < minY)
                minY = localPoint.y;
            if (localPoint.y > maxY)
                maxY = localPoint.y;
        }

        private static void CalculateMaxDistance(Vector3 point)
        {
            Vector3 intersectionPoint = projectionPlaneHorizontal.ClosestPointOnPlane(point);

            Single horizontalDistance = projectionPlaneHorizontal.GetDistanceToPoint(point);
            Single verticalDistance = projectionPlaneVertical.GetDistanceToPoint(point);

            // Credit: https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
            Single halfFrustumHeight = Mathf.Max(verticalDistance, horizontalDistance / aspect);
            Single distance = halfFrustumHeight / Mathf.Tan(renderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

            Single distanceToCenter = (intersectionPoint - m_previewDirection * distance - boundsCenter).sqrMagnitude;
            if (distanceToCenter > maxDistance)
                maxDistance = distanceToCenter;
        }

        private static Boolean IsStatic(Transform obj)
        {
            if (obj.gameObject.isStatic)
                return true;

            for (Int32 i = 0; i < obj.childCount; i++)
            {
                if (IsStatic(obj.GetChild(i)))
                    return true;
            }

            return false;
        }

        private static void SetLayerRecursively(Transform obj)
        {
            obj.gameObject.layer = PREVIEW_LAYER;
            for (Int32 i = 0; i < obj.childCount; i++)
                SetLayerRecursively(obj.GetChild(i));
        }

        private static void GetLayerRecursively(Transform obj)
        {
            layersList.Add(obj.gameObject.layer);
            for (Int32 i = 0; i < obj.childCount; i++)
                GetLayerRecursively(obj.GetChild(i));
        }

        private static void SetLayerRecursively(Transform obj, ref Int32 index)
        {
            obj.gameObject.layer = layersList[index++];
            for (Int32 i = 0; i < obj.childCount; i++)
                SetLayerRecursively(obj.GetChild(i), ref index);
        }
    }

    
}