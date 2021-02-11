using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Token : MonoBehaviour
{

    private class TokenMesh
    {
        public static readonly Mesh trianguleMesh;
        public static readonly Mesh circleMesh;

        static TokenMesh()
        {
            trianguleMesh = new Mesh();
            trianguleMesh.vertices = new [] {
                new Vector3(0,0,-1),
                new Vector3(-1,0,0),
                new Vector3(1,0,0)
            };
            trianguleMesh.triangles = new int[] {0, 1, 2};

            int n = 12;
            float radius = 1f;
            circleMesh = new Mesh();
            circleMesh.vertices = CircleVerticies(n, radius);
            circleMesh.triangles = CircleTrigs(n);
        }

        private static int[] CircleTrigs(int n)
        {
            List<int> trianglesList = new List<int>();
            for(int i = 0; i < (n-2); i++)
            {
                trianglesList.Add(0);
                trianglesList.Add(i+1);
                trianglesList.Add(i+2);
            }
            return trianglesList.ToArray();
        }

        private static Vector3[] CircleVerticies(int n, float radius)
        {
            List<Vector3> verticiesList = new List<Vector3>();
            float x;
            float z;
            for (int i = 0; i < n; i ++)
            {
                x = radius * Mathf.Sin((2 * Mathf.PI * i) / n);
                z = radius * Mathf.Cos((2 * Mathf.PI * i) / n);
                verticiesList.Add(new Vector3(x, 0f, z));
            }
            return verticiesList.ToArray();
        }
    }

    public Material chosenPathMaterial;
    public Material exploredAreaMaterial;
    public float exploredFaceDuration = 5f;
    public float pathFadeDuration = 3f;
    public float scalingFactor = 10f;
    
    private Renderer rend;
    private MeshFilter mf;

    private bool wasChosen;
    private float fadeDuration;
    private Color[] colors = new Color[] { Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.red };

    void Awake()
    {
        // Draw token mesh as triangle
        mf = (MeshFilter)gameObject.GetComponent("MeshFilter");
        mf.mesh = TokenMesh.trianguleMesh;

        wasChosen = false;
        rend = GetComponent<Renderer>();
        rend.material = exploredAreaMaterial;
    }

    public void MarkTarget()
    {
        rend.material.color = Color.green;
        mf.mesh = TokenMesh.circleMesh;
        
    }

    public void SetColorCode(float value, int min, int max)
    {
        // color in a gradient representing the node's fCost relative to the global min and max fCost
        if (scalingFactor == 0)
        {
            int diff = (max - min) / 2;
            rend.material.color = value < diff ? 
                Color.Lerp(Color.yellow, Color.red, Mathf.InverseLerp(min, diff, value)) :
                Color.Lerp(Color.red, Color.magenta, Mathf.InverseLerp(diff, max, value));        
        } 
        else
        {
            value = value % (scalingFactor * colors.Length);
            int minindex = (int) Mathf.Floor(value / scalingFactor);
            int maxindex = (minindex + 1) % colors.Length;
            rend.material.color = Color.Lerp(colors[minindex], colors[maxindex], value / scalingFactor - minindex);
        }
    }

    public void SetInvalid()
    {
        rend.material.color = Color.red;
    }

    public void SetAsChosenPath()
    {
        StopCoroutine("FadeAndDestroy");
        wasChosen = true;
        this.gameObject.SetActive(true); 
        rend.material = chosenPathMaterial;
        transform.RotateAround(transform.position, Vector3.up, 180);
    }

    public void Reset()
    {
        StopCoroutine("FadeAndDestroy");
        wasChosen = false;
        this.gameObject.SetActive(false);
        rend.material = exploredAreaMaterial;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        mf.mesh = TokenMesh.trianguleMesh;
    }

    public void Dissolve()
    {
        if (this.gameObject.activeSelf)
        {
            fadeDuration = pathFadeDuration;
            StartCoroutine("FadeAndDestroy");
        }
    }

    public void DissolveSurrounding()
    {
        if (!wasChosen)
        {
            fadeDuration = exploredFaceDuration;
            StartCoroutine("FadeAndDestroy");
        }
    }

    public void RotateTowardsParent(GameObject parent) {
        var lookPos = parent.transform.position - transform.position;
        float angle = Mathf.Atan2(lookPos.x, lookPos.z) * Mathf.Rad2Deg;
        transform.RotateAround(transform.position, Vector3.up, angle);
    }

    IEnumerator FadeAndDestroy() {
        Color color = rend.material.color;
        float circletOpacity = exploredAreaMaterial.color.a;
        float targetOpacity = 0.0f;
        float t = 0;

            
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(circletOpacity, targetOpacity, Mathf.Clamp01(t / fadeDuration));
            rend.material.color = color;
            yield return null;
        }

        this.gameObject.SetActive(false);
        color.a = 1;
    }

}
