using UnityEngine;
using UnityEngine.VFX;

public class ExamplePointRendererVFX : MonoBehaviour
{
    public VisualEffect _visualEffect;
    public int pointCount = 1000000;
    [SerializeField] private float _sphereSize = 1f;
    private GraphicsBuffer positionBuffer;

    void Start()
    {
        Vector3[] positions = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            positions[i] = Random.insideUnitSphere * _sphereSize;
        }

        positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointCount, sizeof(float) * 3);

        positionBuffer.SetData(positions);

        _visualEffect.SetGraphicsBuffer("_graphicsBufferPoints", positionBuffer);
        _visualEffect.SetUInt("ParticleCount", (uint)pointCount);
    }

    void OnDestroy()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
    }
}