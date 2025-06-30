using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.XR;
using Unity.XR.Oculus;
using Anaglyph.XRTemplate.DepthKit; // Custom library for accessing depth textures from headset

/// <summary>
/// This script uses a compute shader to convert depth texture data into 3D positions.
/// These positions are stored in a GPU buffer (GraphicsBuffer) and optionally rendered as meshes.
/// </summary>
public class PositionProvider : MonoBehaviour
{
    // Set by Hand In editor or if possible in start 
    [SerializeField] private ComputeShader _PositionProvider; // Compute shader used to generate 3D positions on the GPU
    [SerializeField] private Material material;               // Material for rendering instances (e.g., colored spheres) Needs the PositionDisplayShader
    [SerializeField] private Mesh mesh;                       // Mesh to instance per point (e.g., quad or cube)
    [SerializeField] private int amount = 64;                 // Total number of points (should be a square number ideally)
    [SerializeField] private TMP_Text _text;                  // UI text to display debug info
    
    [SerializeField] private  int pointBounds = 15;            // World bounds for instancing the points needed for visualizing points with  Graphics.RenderMeshPrimitives
    private  Vector4[] _points;                                // Optional CPU-side copy of points
    
    private GraphicsBuffer _graphicsBufferPoints;             // GPU-side buffer for storing positions
    private int _kernelID;                                    // Compute shader kernel ID

    private AsyncGPUReadbackRequest _request;                 // Readback handle for copying GPU -> CPU
    private XRDisplaySubsystem _xrDisplay;                    // Oculus XR subsystem (used to get depth texture)
    private int _texSize = 512;                              

    private static readonly int GraphicsBufferPointsId = Shader.PropertyToID("_graphicsBufferPoints");

    // Link the compute buffer to the compute shader
    void SetShaderProperties() 
    {
        _kernelID = _PositionProvider.FindKernel("ProvidePositions"); // Locate kernel in compute shader
        _PositionProvider.SetBuffer(_kernelID, GraphicsBufferPointsId, _graphicsBufferPoints);
    }

    private void Start()
    {
        
        _points = new Vector4[amount]; // Allocate CPU-side buffer
        
        
        
        SetShaderProperties(); // Initialize compute shader references
        
        
        //(move thread amount setup here when not needed dynamic, should do that actually)
    }

    void OnEnable()
    {
        // Allocate a GPU structured buffer with room for `amount` Vector4s (4 floats per point)
        // IMPORTANT: This must match the data type used in the compute shader (float4)!
        _graphicsBufferPoints = new GraphicsBuffer(GraphicsBuffer.Target.Structured, amount, sizeof(float) * 4);
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
        //if (!DepthKitDriver.DepthAvailable) return;

        
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
        int side = Mathf.CeilToInt(Mathf.Sqrt(amount));  // Grid width/height in thread units
        int threadGroupsX = Mathf.CeilToInt(side / 8.0f); // Divide by group size
        int threadGroupsY = Mathf.CeilToInt(side / 8.0f);
        _PositionProvider.SetInt("side", side);

        _PositionProvider.Dispatch(_kernelID, threadGroupsX, threadGroupsY, 1);

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

        var data = req.GetData<Vector4>();
        int count = data.Length;

        // Show sample values in UI for debugging
        _text.text = $"First: {data[0]}, Mid: {data[count / 2]}, Last: {data[count - 1]}";

        // Optionally copy to CPU-side array to use 
        data.CopyTo(_points);
    }
}
