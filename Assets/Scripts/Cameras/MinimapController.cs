using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Credit: https://medium.com/@alessandrovalcepina/creating-a-rts-like-minimap-with-unity-9cd578dc4522
*/
public class MinimapController : MonoBehaviour
{
    public Material cameraBoxMaterial;
    public Camera minimap;
    public float lineWidth;
    public Collider mapCollider;

    private Vector3 GetCameraFrustumPoint(Vector3 position)
    {
        Ray positionRay = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        Vector3 result = mapCollider.Raycast(positionRay, out hit, Camera.main.transform.position.y * 10) ? hit.point : new Vector3();
        return result;
    }

    public void OnPostRender()
    {
        Vector3 minViewportPoint = minimap.WorldToViewportPoint(GetCameraFrustumPoint(new Vector3(0f, 0f)));
        Vector3 maxViewportPoint = minimap.WorldToViewportPoint(GetCameraFrustumPoint(new Vector3(Screen.width, Screen.height)));

        float minX = minViewportPoint.x;
        float minY = minViewportPoint.y;

        float maxX = maxViewportPoint.x;
        float maxY = maxViewportPoint.y;

        GL.PushMatrix();
        {
            cameraBoxMaterial.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(Color.yellow);
            {
                GL.Vertex(new Vector3(minX, minY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY + lineWidth, 0));

                GL.Vertex(new Vector3(minX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(minX + lineWidth, maxY, 0));

                GL.Vertex(new Vector3(minX, maxY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY + lineWidth, 0));

                GL.Vertex(new Vector3(maxX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(maxX + lineWidth, maxY, 0));
            }
            GL.End();
        }
        GL.PopMatrix();
    }
}
