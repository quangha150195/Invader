using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WebCamView : MonoBehaviour {
    private Texture m_Texture;
    private WebCamTexture m_Webcam;
    private Vector2 m_ScreenSize;
    [SerializeField]
    private Camera m_Camera;
    // Use this for initialization
    void Start() {

    }
    IEnumerator startCamera()
    {
        while (m_Webcam == null)
        {
            if (WebCamTexture.devices.Length > 0)
            {
                m_Webcam = new WebCamTexture(1280, 1280 * Screen.width / Screen.height, 60);
                if (m_Webcam != null)
                {
                    yield return new WaitWhile(() => { return m_Webcam.isPlaying; });
                    GetComponent<Renderer>().material.mainTexture = m_Webcam;
                    m_Texture = GetComponent<Renderer>().material.mainTexture;
                    m_Webcam.Play();
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
    void OnValidate()
    {
        if (m_Camera == null)
        {
            m_Camera = Camera.main;
        }
    }
    void Reset()
    {
        OnValidate();
    }
    void OnEnable()
    {
        StartCoroutine(startCamera());
    }
    public void OnDestroy()
    {
        if (m_Webcam != null)
        {
            m_Webcam.Stop();
        }
    }
    public Texture2D getCurrentTexture()
    {
        Texture2D texture2D = null;
        if (m_Texture != null)
        {
            texture2D = new Texture2D(m_Texture.width, m_Texture.height, TextureFormat.RGBA32, false);

            RenderTexture currentRT = RenderTexture.active;

            RenderTexture renderTexture = new RenderTexture(m_Texture.width, m_Texture.height, 32);
            Graphics.Blit(m_Texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            Color[] pixels = texture2D.GetPixels();


            RenderTexture.active = currentRT;
        }
        return texture2D;
    }
    void Update()
    {
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            m_ScreenSize.x = m_Camera.orthographicSize * 2;
            m_ScreenSize.y = Screen.width * m_ScreenSize.y / Screen.height;
        }
        else
        {
            m_ScreenSize.y = m_Camera.orthographicSize * 2;
            m_ScreenSize.x = Screen.width * m_ScreenSize.y / Screen.height;
        }
      
        updateRect();
    }
    private void updateRect()
    {
        if (m_Webcam != null && m_Webcam.isPlaying)
        {
           
            //if (mSizeWebCam.x != mWebCam.width)
            {
                Vector2 sizeWebCam = new Vector2(m_Webcam.width, m_Webcam.height);
                float fX = sizeWebCam.x / m_ScreenSize.x;
                float fY = sizeWebCam.y / m_ScreenSize.y;
                Vector3 scale = new Vector3(m_ScreenSize.x, m_ScreenSize.y, 1);
                if (fX > fY)
                {
                    scale.x = scale.y * sizeWebCam.x / sizeWebCam.y;
                }
                else
                {
                    scale.y = scale.x * sizeWebCam.y / sizeWebCam.x;
                }

                if (m_Webcam.videoVerticallyMirrored)
                {
                    scale.y = -scale.y;
                }
                transform.localScale = scale;
                if (m_Webcam.videoRotationAngle == 90)
                {
                    transform.localEulerAngles = new Vector3(0, 0, 270);
                }
                else
                {
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                }
            }
        }
    }
}
