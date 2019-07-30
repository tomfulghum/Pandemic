using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class DrawCircle : MonoBehaviour
{
    [Range(0, 50)]
    public int segments = 100;
    [Range(0, 5)]
    [HideInInspector] public float radius;
    LineRenderer line;

    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = segments + 1;
        line.useWorldSpace = true;
        //CreatePoints();
    }

    public void CreatePoints()
    {
        float x;
        float y;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, transform.position + new Vector3(x, y, 0));

            angle += (360f / segments);
        }
    }
}