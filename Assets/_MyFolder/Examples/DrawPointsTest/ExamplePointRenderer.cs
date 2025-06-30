using UnityEngine;

using TMPro;
public class ExamplePointRenderer : MonoBehaviour
{
    public Material pointMaterial;
    public int pointCount = 1000000;
    [SerializeField] private float _sphereSize= 1f;
    private ComputeBuffer positionBuffer;

    void Start()
    {
        Vector4[] positions = new Vector4[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            positions[i] = Random.insideUnitSphere * _sphereSize;
            positions[i].w = 1;
        }

        positionBuffer = new ComputeBuffer(pointCount, sizeof(float)*4); // 12 bytes = 3 floats
        positionBuffer.SetData(positions);
        pointMaterial.SetBuffer("_graphicsBufferPoints", positionBuffer);
    }

    void OnRenderObject()
    {
        pointMaterial.SetPass(0);
        // 6 vertices per quad (2 triangles)
        Graphics.DrawProceduralNow(MeshTopology.Triangles, pointCount );
    }

    void OnDestroy()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
    }
}

