using OpenTK;
using System;
using System.Collections.Generic;

namespace Template
{
    class Consts
    {
        public static int LightsCount = 2;
    }

    class MyApplication
    {
        public Surface screen;                  // background surface for printing etc.
        Model dragon, teapot2, teapot3, box;
        Model floorBottom, floorLeft, floorRight, floorTop, floorFront, floorBack;
        float a = 0;
        RenderTarget target;                    // intermediate render target
        ScreenQuad quad;                        // screen filling quad for post processing
        public static Camera camera;
        SceneGraph sceneGraph;
        bool useRenderTarget = true;
        DepthMap depthMap;
        CubeTexture skyboxTexture;
        Skybox skybox;
        PointLight light1, light2;
        CubeDepthMap[] cubeDepthMaps = new CubeDepthMap[Consts.LightsCount];

        //Shaders
        private DepthShader depthShader = new DepthShader();
        private ModelShader modelShader = new ModelShader();
        private PostProcessingShader postProcessingShader = new PostProcessingShader();
        private SkyboxShader skyboxShader = new SkyboxShader();

        //Assets
        private List<Mesh> meshesAsset = new List<Mesh>();
        private List<SurfaceTexture> texturesAsset = new List<SurfaceTexture>();

        // initialize
        public void Init()
        {
            meshesAsset.Add(new Mesh("../../assets/dragon.obj"));
            meshesAsset.Add(new Mesh("../../assets/teapot.obj"));
            meshesAsset.Add(new Mesh("../../assets/floor.obj"));
            meshesAsset.Add(new Mesh("../../assets/sphere.obj"));
            meshesAsset.Add(new Mesh("../../assets/cube.obj"));

            texturesAsset.Add(new SurfaceTexture(new Texture("../../assets/wood.jpg"), null)); 
            texturesAsset.Add(new SurfaceTexture(new Texture("../../assets/diffuseGray.png"), null));
            texturesAsset.Add(new SurfaceTexture(new Texture("../../assets/floor.png"), new Texture("../../assets/floorNormal.png")));

            dragon = new Model(meshesAsset[0], texturesAsset[1], new Vector3(0, 25, 0), Vector3.Zero, new Vector3(7));
            teapot2 = new Model(meshesAsset[1], texturesAsset[0], new Vector3(15, 125, 15), Vector3.Zero, Vector3.One);
            teapot3 = new Model(meshesAsset[1], texturesAsset[0], new Vector3(0, 5, 10), Vector3.Zero, new Vector3(0.25F, 0.25F, 0.25F));
            box = new Model(meshesAsset[4], texturesAsset[0], new Vector3(50, 5, 10), new Vector3(45, 0, 45), new Vector3(35));

            floorBottom = new Model(meshesAsset[2], texturesAsset[2], new Vector3(0, 13, 0), Vector3.Zero, new Vector3(20, 20, 20));
            floorTop = new Model(meshesAsset[2], texturesAsset[2], new Vector3(0, 225, 0), Vector3.Zero, new Vector3(20, 20, 20));
            floorLeft = new Model(meshesAsset[2], texturesAsset[2], new Vector3(0, 40, 125), Vector3.Zero, new Vector3(20, 20, 20));
            floorRight = new Model(meshesAsset[2], texturesAsset[2], new Vector3(0, 40, -125), Vector3.Zero, new Vector3(20, 20, 20));
            floorFront = new Model(meshesAsset[2], texturesAsset[2], new Vector3(125, 40, 0), Vector3.Zero, new Vector3(20, 20, 20));
            floorBack = new Model(meshesAsset[2], texturesAsset[2], new Vector3(-125, 40, 0), Vector3.Zero, new Vector3(20, 20, 20));

            light1 = new PointLight(meshesAsset[3], texturesAsset[1], new Vector3(0, 45, 100), Vector3.Zero, Vector3.One);
            light1.color = new Vector3(1, 1, 1); light1.brightness = 6000;
            cubeDepthMaps[0] = new CubeDepthMap(256, 256);
            light1.CreateDepth(cubeDepthMaps[0]);

            light2 = new PointLight(meshesAsset[3], texturesAsset[1], new Vector3(0, 105, 0), Vector3.Zero, Vector3.One);
            light2.color = new Vector3(0.5F, 0, 1); light2.brightness = 3000;
            cubeDepthMaps[1] = new CubeDepthMap(256, 256);
            light2.CreateDepth(cubeDepthMaps[1]);


            // create the render target
            target = new RenderTarget(screen.width, screen.height);
            quad = new ScreenQuad();
            camera = new FPSCamera(new Vector3(0, -15, 0));
            sceneGraph = new SceneGraph();

            dragon.AddChild(teapot3);

            sceneGraph.gameObjects.Add(dragon);
            sceneGraph.gameObjects.Add(light1);
            sceneGraph.gameObjects.Add(light2);
            sceneGraph.gameObjects.Add(teapot2);
            sceneGraph.gameObjects.Add(box);

            sceneGraph.gameObjects.Add(floorBottom); 
            sceneGraph.gameObjects.Add(floorTop); floorTop.rotationInAngle.X = 180;
            sceneGraph.gameObjects.Add(floorLeft); floorLeft.rotationInAngle.X = -90;
            sceneGraph.gameObjects.Add(floorRight); floorRight.rotationInAngle.X = 90;
            sceneGraph.gameObjects.Add(floorFront); floorFront.rotationInAngle.Z = 90;
            sceneGraph.gameObjects.Add(floorBack); floorBack.rotationInAngle.Z = -90;

            sceneGraph.gameObjects.Add(teapot3);
            sceneGraph.AddLight(light1);
            sceneGraph.AddLight(light2);

            //depthMap = new DepthMap(screen.width, screen.height);

            skyboxTexture = new CubeTexture(new string[]{ "../../assets/right.png", "../../assets/left.png", "../../assets/top.png",
                "../../assets/bottom.png", "../../assets/front.png", "../../assets/back.png" });
            skybox = new Skybox();
        }

        public void OnWindowResize(int width, int height)
        {
            target.width = width;
            target.height = height;
        }

        public void Tick(OpenTKApp app, float deltaTime)
        {

        }

        // tick for OpenGL rendering code
        public void RenderGL(OpenTKApp app, float deltaTime)
        {
            camera.ProcessInput(app, deltaTime);

            // update rotation
            a += 50 * deltaTime;
            if (a > 360) { a -= 360; }

           // dragon.rotationInAngle.Y += (float)(1 * Math.Cos(MathHelper.DegreesToRadians(a)));

            light1.position.X = (float)(125 * Math.Cos(MathHelper.DegreesToRadians(a)));
            light1.position.Z = (float)(125 * Math.Sin(MathHelper.DegreesToRadians(a)));

            camera.CalculateFrustumPlanes();
            sceneGraph.UpdateScene(camera);

            if (useRenderTarget)
            {
                sceneGraph.RenderDepthMap(camera, depthShader);

                target.Bind();
                Matrix4 viewProjMatrix = camera.GetViewMatrix().ClearTranslation() * camera.GetProjectionMatrix();
                skybox.Render(skyboxShader, skyboxTexture.id, viewProjMatrix);
                sceneGraph.RenderScene(camera, modelShader, cubeDepthMaps);
                target.Unbind();

                quad.Render(postProcessingShader, target.GetTextureID());
            } else
            {
                // render scene directly to the screen
                depthMap.Bind();
                sceneGraph.RenderDepthMap(camera, depthShader);
                depthMap.Unbind();

                Matrix4 viewProjMatrix = camera.GetViewMatrix() * camera.GetProjectionMatrix();
                skybox.Render(skyboxShader, skyboxTexture.id, viewProjMatrix);
                sceneGraph.RenderScene(camera, modelShader, cubeDepthMaps);
            }
        }
    }
}