using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PassthroughCameraSamples;

public class WebTexDebug : MonoBehaviour
{
    [SerializeField] private WebCamTextureManager webCamTextureManager;
    [SerializeField] private Text debugText;
    [SerializeField] private RawImage image;

    private WebCamTexture tex;
    private bool initialized = false;
    private Color32[] pixelsBuffer;

    private void Start()
    {
        StartCoroutine(InitWhenReady());
    }

    private IEnumerator InitWhenReady()
    {
        // Wait until WebCamTexture is available
        while (webCamTextureManager.WebCamTexture == null)
            yield return null;

        tex = webCamTextureManager.WebCamTexture;
        image.texture = tex;

        // Allocate once for reuse
        pixelsBuffer = new Color32[tex.width * tex.height];
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || tex == null)
            return;

        // Wait until a new frame has been captured
        if (!tex.didUpdateThisFrame)
            return;

        // Get a pixel directly from GPU texture (note: this can be slow!)
        Color pixel = tex.GetPixel(1, 1);

        // Copy full frame to CPU buffer
        tex.GetPixels32(pixelsBuffer);

        // Display info
        debugText.text = $"Pixel(1,1): {pixel}\n";
        debugText.text += $"Is Readable: {tex.isReadable}\n";
        debugText.text += $"First CPU Pixel: {pixelsBuffer[0]}\n";
        debugText.text += $"Format: {tex.graphicsFormat}\n";
        debugText.text += $"height: {tex.height}\n";
    }
}