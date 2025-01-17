﻿using OpenTK;
using System.Collections.Generic;

namespace Rasterizer
{
    public abstract class GameObject
    {
        public Vector3 position;
        public Vector3 rotationInAngle;
        public Vector3 scale;
        public Mesh mesh;
        public SurfaceTexture texture;

        public Matrix4 globalTransform;

        public GameObject parent;
        public List<GameObject> children;

        public Vector3 previousPosition;
        public Matrix4 previousGlobalTransform;

        public GameObject(Mesh mesh, SurfaceTexture texture, Vector3 position, Vector3 rotationInAngle, Vector3 scale)
        {
            this.position = position;
            this.rotationInAngle = rotationInAngle;
            this.scale = scale;
            this.mesh = mesh;
            this.texture = texture;

            children = new List<GameObject>();
        }

        public void AddChild(GameObject gameObject)
        {
            children.Add(gameObject);
            gameObject.parent = this;
        }

        public void RemoveChild(GameObject gameObject)
        {
            children.Remove(gameObject);
            gameObject.parent = null;
        }

        public void RenderToScene(ModelShader shader)
        {
            if (mesh != null)
            {
                mesh.RenderToScene(shader, this);
            }
        }

        public void RenderToDepth(DepthShader shader, Matrix4 viewProjMatrix, Vector3 lightPosition)
        {
            if (mesh != null)
            {
                mesh.RenderToDepth(shader, this, viewProjMatrix, lightPosition);
            }
        }

        public virtual void Update()
        {
            globalTransform = GetGlobalTransformationMatrix();
        }

        public virtual void EndUpdate()
        {
            previousGlobalTransform = globalTransform;
            previousPosition = globalTransform.ExtractTranslation();
        }

        private Matrix4 GetGlobalTransformationMatrix()
        {
            Matrix4 globalTransform = GetLocalTransformationMatrix();
            GameObject gameObject = this;
            while (gameObject.parent != null)
            {
                globalTransform *= gameObject.parent.GetLocalTransformationMatrix();
                gameObject = gameObject.parent;
            }
            return globalTransform;
        }

        public Matrix4 GetLocalTransformationMatrix()
        {
            return GetScaleMatrix() * GetRotationMatrix() * GetTranslationMatrix();
        }

        public Matrix4 GetScaleMatrix()
        {
            return Matrix4.CreateScale(scale);
        }

        public Matrix4 GetRotationMatrix()
        {
            return Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotationInAngle.X)) *
                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotationInAngle.Y)) *
                Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotationInAngle.Z));
        }

        public Matrix4 GetTranslationMatrix()
        {
            return Matrix4.CreateTranslation(position);
        }
    }
}
