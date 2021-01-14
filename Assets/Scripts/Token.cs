using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour
{

    public Material chosenPathMaterial;
    public Material exploredAreaMaterial;
    public float surroundingFadeDuration = 5f;
    public float pathFadeDuration = 3f;
    Renderer rend;
    bool wasChosen;
    float fadeDuration;

    void Awake()
    {
        wasChosen = false;
        rend = GetComponent<Renderer>();
        rend.material = exploredAreaMaterial;

    }

    public void SetColorCode(float value, int min, int max)
    {
        //Debug.LogWarning("Value: " + value + ", Min: " + min + ", Max: " + max);
        int diff = (max - min) / 2;
        Color color;

        if (value < diff)
        {
            color = Color.Lerp(Color.yellow, Color.red, Mathf.InverseLerp(min, diff, value));
        }
        else
        {
            color = Color.Lerp(Color.red, Color.magenta, Mathf.InverseLerp(diff, max, value));
        }
        rend.material.color = color;

        /*
        Color[] colors = new Color[] { Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.red };

        //Debug.LogWarning("Color Value prior:" + value);
        value = value % (scalingFactor * colors.Length);
        int minindex = (int) Mathf.Floor(value / scalingFactor);
        int maxindex = (minindex + 1) % colors.Length;

        //Debug.LogWarning("Value: " + value + ", Minindex: " + minindex + ", (Value - Minindex * 100) / 100: " + (value - minindex*100)/100);
        Color color = Color.Lerp(colors[minindex], colors[maxindex], value / scalingFactor - minindex);
        rend.material.color = color;*/
    }

    public void SetAsChosenPath()
    {
        StopCoroutine("FadeAndDestroy");
        wasChosen = true;
        this.gameObject.SetActive(true); 
        rend.material = chosenPathMaterial;
    }

    public void Reset()
    {
        StopCoroutine("FadeAndDestroy");
        wasChosen = false;
        this.gameObject.SetActive(false);
        rend.material = exploredAreaMaterial;
    }

    public void Dissolve()
    {
        fadeDuration = pathFadeDuration;
        StartCoroutine("FadeAndDestroy");
    }

    public void DissolveSurrounding()
    {
        if (!wasChosen)
        {
            fadeDuration = surroundingFadeDuration;
            StartCoroutine("FadeAndDestroy");
        }
    }

    IEnumerator FadeAndDestroy() {
        Color color = rend.material.color;
        float startOpacity = exploredAreaMaterial.color.a;
        float targetOpacity = 0.0f;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float blend = Mathf.Clamp01(t / fadeDuration);

            color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);
            rend.material.color = color;
            yield return null;
        }

        this.gameObject.SetActive(false);
        color.a = 1;
        Reset();

    }

}
