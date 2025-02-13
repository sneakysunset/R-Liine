using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintArea : MonoBehaviour
{
    RenderTexture paintRT;
    public int textureResolution = 256;
    Material mat;
    // Start is called before the first frame update


    private void OnEnable()
    {
        // Cr�er la Texture
        mat = GetComponent<Renderer>().material;
        paintRT = RenderTexture.GetTemporary(textureResolution, textureResolution, 32, RenderTextureFormat.Default);
        mat.SetTexture("_Paint_Tex", paintRT);

        ClearOutRenderTexture(paintRT);
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(paintRT);
    }

    public void ClearOutRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        RenderTexture.active = null;
    }

    public void Paint(Vector2 uv, float brushWidth, Texture2D brushTex, bool erasing)
    {
        //Activate RT
        RenderTexture.active = paintRT;
        GL.PushMatrix();

        GL.LoadPixelMatrix(0, textureResolution, textureResolution, 0);

        //Setup UVs � la bonne scale
        uv.x *= textureResolution;
        uv.y = textureResolution * (1 - uv.y);

        //Scale la brush pour matcher avec le scale de l'objet dans la sc�ne
        brushWidth *= textureResolution;

        // Paint sur la RT
        Rect paintRect = new Rect(uv.x - brushWidth * 0.5f, uv.y - brushWidth * 0.5f, brushWidth, brushWidth);
        if(!erasing) Graphics.DrawTexture(paintRect, brushTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.white, null);
        if(erasing) Graphics.DrawTexture(paintRect, brushTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, Color.black, null);

        GL.PopMatrix();

        //Turn off RT
        RenderTexture.active = null;
    }

    private void Update()
    {
        mat.SetTexture("_PaintTex", paintRT);
    }
}
