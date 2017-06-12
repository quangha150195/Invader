using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;
using UnityEngine.Events;
using System.Security.Cryptography;
using System.Text;
using System.IO;
namespace external
{
    public class UIUtils
    {
        public static Rect getRectPixel(RectTransform rectTransform, Canvas canvas)
        {

            Vector3[] corners = new Vector3[4];
            Vector3[] screenCorners = new Vector3[2];

            rectTransform.GetWorldCorners(corners);

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
            }
            else
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
            }

            screenCorners[0].y = Screen.height - screenCorners[0].y;
            screenCorners[1].y = Screen.height - screenCorners[1].y;

            return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
        }
        public static Canvas getRootCanvas(Transform obj)
        {
            Transform resultGo = obj;
            while (resultGo != null)
            {
                Canvas resultCanvas = resultGo.GetComponent<Canvas>();
                if (resultCanvas != null && resultCanvas.isRootCanvas)
                    return resultCanvas;
                resultGo = resultGo.parent;
            }
            return null;
        }
    }
}