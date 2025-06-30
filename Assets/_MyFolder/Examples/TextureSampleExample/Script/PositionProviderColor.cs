using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.XR;
using Unity.XR.Oculus;
using Anaglyph.XRTemplate.DepthKit;
using PassthroughCameraSamples;
using UnityEngine.UI; 
using Meta.XR.EnvironmentDepth;

/// <summary>
/// This script uses a compute shader to convert depth texture data into 3D positions.
/// These positions are stored in a GPU buffer (GraphicsBuffer) and optionally rendered as meshes.
/// </summary>
public class PositionProviderColor : MonoBehaviour
{
    // Set by Hand In editor or if possible in start 
    [SerializeField] private ComputeShader _PositionProvider; // Compute shader used to generate 3D positions on the GPU
    [SerializeField] private Material material;               // Material for rendering instances (e.g., colored spheres) Needs the PositionDisplayColor Shader
    [SerializeField] private Mesh mesh;                       // Mesh to instance per point (e.g., quad or cube)
    [SerializeField] private int amount = 64;                 // Total number of points (should be a square number ideally)
    [SerializeField] private TMP_Text _text;                  // UI text to display debug info
    
    [SerializeField] private  int pointBounds = 15;            // World bounds for instancing the points needed for visualizing points with  Graphics.RenderMeshPrimitives
   
    [SerializeField] private WebCamTextureManager webCamTextureManager; // For RGB texture
    
    [SerializeField] private RawImage image;//debug

    private WebCamTexture _webTex;                             // check out WebTexDebug.cs for an example on how to read the tex or metas brightness evaluation in their samples
    private bool initialized = false;                          //the texture needs some time to be available and only runs on 30fps
    
    private  PointData[] _points;                              // Optional CPU-side copy of points
    
    private GraphicsBuffer _graphicsBufferPoints;             // GPU-side buffer for storing positions
    private int _kernelID;                                    // Compute shader kernel ID

    private AsyncGPUReadbackRequest _request;                 // Readback handle for copying GPU -> CPU
    private XRDisplaySubsystem _xrDisplay;                    // Oculus XR subsystem (used to get depth texture)
    private int _texSize = 320;                               

    private Texture2D _texture;                                //A texture to safe the web texture into before binding it to the ComputeShader, check out https://github.com/xrdevrob/QuestCameraKit they do it prettier
    
    private static readonly int GraphicsBufferPointsId = Shader.PropertyToID("_graphicsBufferPoints");//String to ID for faster access
    private static readonly int ColorTexId = Shader.PropertyToID("_colorTex");
     
    struct PointData //custom for returning the information needs to be the same as in Assets/_MyFolder/Examples/TextureSampleExample/Script/PointDataStruct.hlsl you can add more as needed
    {
        public Vector4 position;
        public Vector4 color;
    }


    // Link the compute buffer to the compute shader
    void SetShaderProperties() 
    {
        _kernelID = _PositionProvider.FindKernel("ProvidePositions"); // Locate kernel in compute shader
        _PositionProvider.SetBuffer(_kernelID, GraphicsBufferPointsId, _graphicsBufferPoints);
    }

    private void Start()
    {
        
        _points = new PointData[amount]; // Allocate CPU-side buffer

        
        SetShaderProperties(); // Initialize compute shader references
        
        //(move thread amount setup here when not needed dynamic, should do that actually)
        
        StartCoroutine(InitWhenReady());
    }
    
    private IEnumerator InitWhenReady()
    {
        // Wait until WebCamTexture is available
        while (webCamTextureManager.WebCamTexture == null)
            yield return null;

        _webTex = webCamTextureManager.WebCamTexture;//Both tex and webCamTextureManager.WebCamTexture point to the same live texture, aka automatic Update, if you change the WebCamTexture to right eye you might have to reassign
        //_text.text = "webTex:WH"+_webTex.width.ToString()+ _webTex.height.ToString();
        _texture=  new Texture2D(_webTex.width,  _webTex.height, TextureFormat.RGBA32, false); // i got a bit lazy i should check the web textures size and then send it to the ComputeShader

        image.texture =_webTex;
        
        initialized = true;
    }

    void OnEnable()
    {
        // Allocate a GPU structured buffer with room for `amount` size of PointData
        // IMPORTANT: This must match the data type used in the compute shader (PointData)!
        _graphicsBufferPoints = new GraphicsBuffer(GraphicsBuffer.Target.Structured, amount,  Marshal.SizeOf(typeof(PointData)));
    }

    void OnDisable()
    {
        // Always release GPU memory when done
        if (_graphicsBufferPoints != null)
        {
            _graphicsBufferPoints.Release();
        }
    }

    void Update()
    {
        //Texture depthTex = Shader.GetGlobalTexture("_EnvironmentDepthTexture");
        //_text.text = "depthTexWH:"+depthTex.width.ToString() + depthTex.height.ToString();//h:320 w:320
        
        //if (!DepthKitDriver.DepthAvailable) return;

        if (!initialized || _webTex == null)
            return;

        // Wait until a new frame has been captured
        if (_webTex.didUpdateThisFrame)
        {

            _texture.SetPixels(_webTex.GetPixels()); //sets pixel data for the texture in CPU memory
            _texture.Apply(); //upload the changed pixels to the GPU
            //!!! the texture might not exist on GPU because this was needed to make it work

            _PositionProvider.SetTexture(_kernelID, ColorTexId, _texture);
            // --- Dispatch Size Explanation ---
            // Our compute shader processes pixels in a 2D grid.
            // Each thread group is 8x8 threads (defined in HLSL: [numthreads(8,8,1)])
            //
            // To cover `amount` total points, we:
            // 1. Take sqrt(amount) to get a grid size.
            // 2. Divide that by 8 to get how many thread *groups* we need per dimension.
            //
            // ⚠️ DANGER: If `amount` isn't divisible by 64 (8x8), we might launch more threads than we have buffer space.
            // → Always make sure (side * side) == amount, and side is divisible by 8.
            int side = Mathf.CeilToInt(Mathf.Sqrt(amount)); // Grid width/height in thread units
            int threadGroupsX = Mathf.CeilToInt(side / 8.0f); // Divide by group size
            int threadGroupsY = Mathf.CeilToInt(side / 8.0f);
            _PositionProvider.SetInt("side", side);

            _PositionProvider.Dispatch(_kernelID, threadGroupsX, threadGroupsY, 1);
            
            
        }

        // Render points using instancing if the GPU buffer exists
        // This is one example on how to visualize, it is dependent on a custom shader using the buffer
        if (_graphicsBufferPoints != null)
        {
            RenderParams rp = new RenderParams(material);
            rp.worldBounds = new Bounds(Vector3.zero, Vector3.one * (pointBounds + 5));
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetBuffer(GraphicsBufferPointsId, _graphicsBufferPoints);

            Graphics.RenderMeshPrimitives(rp, mesh, 0, amount);
        }

        // Schedule GPU readback periodically it's just an example so every 3 seconds is enough 
        readbackTimer += Time.deltaTime;
        if (readbackTimer >= readbackInterval && _request.done)
        {
            readbackTimer = 0f;
            _request = AsyncGPUReadback.Request(_graphicsBufferPoints, OnReadback);// calls this method when done and returns the request
        }
    }

    private float readbackTimer = 0f;
    private const float readbackInterval = 3f;

    void OnReadback(AsyncGPUReadbackRequest req)
    {
        if (req.hasError) return;

        var data = req.GetData<PointData>();
        int count = data.Length;

        // Show sample values in UI for debugging
        _text.text = $"First: {data[0].color}, Mid: {data[count / 2].position}, Last: {data[count - 1]}";

        // Optionally copy to CPU-side array to use 
        data.CopyTo(_points);
    }
}
