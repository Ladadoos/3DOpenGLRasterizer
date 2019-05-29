﻿using OpenTK;

namespace Template
{
    abstract class GameObject
    {
        public Vector3 position;
        public Vector3 rotationInAngle;
        public Vector3 scale;
        public Mesh mesh;

        public GameObject(string meshFile, Vector3 position, Vector3 rotationInAngle, Vector3 scale)
        {
            this.position = position;
            this.rotationInAngle = rotationInAngle;
            this.scale = scale;
            if(meshFile != string.Empty) { mesh = new Mesh(meshFile); }
        }

        public void RenderScene(ModelShader shader, Matrix4 transform, Matrix4 view, Matrix4 projection, 
            Matrix4 lightMatrix, Texture texture, DepthMap depthMap)
        {
            if(mesh != null) { mesh.RenderToScene(shader, transform, view, projection, lightMatrix, texture, depthMap); }
        }

        public void RenderDepth(DepthShader shader, Matrix4 transform, Matrix4 viewProjection)
        {
            if (mesh != null) { mesh.RenderToDepth(shader, transform, viewProjection); }
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
