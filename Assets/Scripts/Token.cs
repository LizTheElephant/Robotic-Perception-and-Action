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

    void Start()
    {
        wasChosen = false;
        rend = GetComponent<Renderer>();
        rend.material = exploredAreaMaterial;

    }

    public void SetAsChosenPath()
    {
        wasChosen = true;
        rend.material = chosenPathMaterial;
    }

    public void Reset()
    {
        StopCoroutine("FadeAndDestroy");
        wasChosen = false;
        this.gameObject.SetActive(false);
        if (rend != null)
        {
            rend.material = exploredAreaMaterial;
        }
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
