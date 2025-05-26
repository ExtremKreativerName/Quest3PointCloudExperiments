using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.XR;
using Unity.XR.Oculus;

using Anaglyph.XRTemplate.DepthKit;
public class PositionProviderPoint : MonoBehaviour
{
    [SerializeField] private ComputeShader _PositionProvider;
    [SerializeField] private Material pointMaterial;
    [SerializeField] private int amount = 64;
    [SerializeField] private TMP_Text _text;
    
    public Vector3[] _points;
    
    private GraphicsBuffer _graphicsBufferPoints;
    private int _kernelID;
    
    private AsyncGPUReadbackRequest _request;
    private XRDisplaySubsystem _xrDisplay;
    private int _texSize = 512; //get actual size at start

    private static readonly int
        graphicsBufferPointsId = Shader.PropertyToID("_graphicsBufferPoints");

    // Method to set compute shader properties
    void SetShaderProperties()
    {
        _PositionProvider.SetBuffer(0, graphicsBufferPointsId, _graphicsBufferPoints);
        pointMaterial.SetBuffer(graphicsBufferPointsId, _graphicsBufferPoints);
    }

    private void Start()
    {
        // Setup Compute Shader properties
        SetShaderProperties();
        
        _xrDisplay = OVRManager.GetCurrentDisplaySubsystem();

        _points = new Vector3[amount];

        _kernelID = _PositionProvider.FindKernel("ProvidePositions");
        
       uint id = 0;
        if (Utils.GetEnvironmentDepthTextureId(ref id) && _xrDisplay != null && _xrDisplay.running)

        {

            var rt = _xrDisplay.GetRenderTexture(id);

            if (rt != null)

            {
                _texSize = rt.width;
            }
        }
    }
    void OnEnable()
    {  
        //  (float == 4)
        _graphicsBufferPoints = new GraphicsBuffer(GraphicsBuffer.Target.Structured,amount, sizeof(float)*3 ); // makes the assumption that amount is not a stupid number, just dont be an idiot
    }

    void OnDisable()
    {
        // Release the buffer when disabling the component
        if (_graphicsBufferPoints != null)
        {
            _graphicsBufferPoints.Release();
        }
        
    }
    void OnRenderObject()
    {
        pointMaterial.SetPass(0);
        // 6 vertices per quad (2 triangles)
        Graphics.DrawProceduralNow(MeshTopology.Points, amount );
    }
    void Update()
    {
        if (!DepthKitDriver.DepthAvailable)
            return;

        _PositionProvider.SetTexture(_kernelID, DepthKitDriver.agDepthTex_ID,
            Shader.GetGlobalTexture(DepthKitDriver.agDepthTex_ID));
        
        // Dispatch the compute shader to generate positions
        int side = Mathf.CeilToInt(Mathf.Sqrt(amount));
        int threadGroupsX = Mathf.CeilToInt(side / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(side / 8.0f);
        _PositionProvider.SetInt("side",side);
        _PositionProvider.Dispatch(_kernelID, threadGroupsX, threadGroupsY, 1);

       
        
         readbackTimer += Time.deltaTime;
                if (readbackTimer >= readbackInterval && _request.done)
                {
                    readbackTimer = 0f;
                    _request = AsyncGPUReadback.Request(_graphicsBufferPoints, OnReadback);
                }
    }
   
    private float readbackTimer = 0f;
    private const float readbackInterval = 3f;

    
    void OnReadback(AsyncGPUReadbackRequest req)
    {
        if (req.hasError) return;

        var data = req.GetData<Vector3>();

        int count = data.Length;
        _text.text=($"First: {data[0]}, Mid: {data[count / 2]}, Last: {data[count - 1]}");

        // Copy to _points if needed
        data.CopyTo(_points);
    }
}
