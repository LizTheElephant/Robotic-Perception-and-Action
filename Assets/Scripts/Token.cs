using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour
{

    public Material chosenPathMaterial;
    public Material exploredAreaMaterial;
    public float exploredFaceDuration = 5f;
    public float pathFadeDuration = 3f;
    public float scalingFactor = 10f;
    
    private Renderer rend;

    private bool wasChosen;
    private float fadeDuration;
    private Color[] colors = new Color[] { Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.red };


    private void Start()
    {
        // Draw token mesh as triangle
        Mesh m = new  Mesh();
        m.vertices = new [] {
            new Vector3(0,0,-1),
            new Vector3(-1,0,0),
            new Vector3(1,0,0)
        };
        m.triangles = new int[] {0, 1, 2};

        MeshFilter mf = (MeshFilter)gameObject.GetComponent("MeshFilter");
        mf.mesh = m;
    }

    void Awake()
    {
        wasChosen = false;
        rend = GetComponent<Renderer>();
        rend.material = exploredAreaMaterial;
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
        float startOpacity = exploredAreaMaterial.color.a;
        float targetOpacity = 0.0f;
        float t = 0;

            
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(startOpacity, targetOpacity, Mathf.Clamp01(t / fadeDuration));
            rend.material.color = color;
            yield return null;
        }

        this.gameObject.SetActive(false);
        color.a = 1;
    }

}
