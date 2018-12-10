using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour
{
    public Material chosenPathMaterial;
    public Material exploredAreaMaterial;
    Renderer rend;
    float fadeDuration = 5f;
    bool wasChosen;

    void Start()
    {
        wasChosen = false;
        rend = GetComponent<Renderer>();
        rend.material = exploredAreaMaterial;
        //rend.material = Resources.Load("Materials/Explored.mat", typeof(Material)) as Material;

    }

    public void SetAsChosenPath()
    {
        wasChosen = true;
        rend.material = chosenPathMaterial;
    }


    public void Dissolve()
    {
        if (!wasChosen)
        {
            StartCoroutine("FadeAndDestroy");
        }
    }

    IEnumerator FadeAndDestroy() {
        /*while (ValSlice>0.0f)
        {
            Debug.Log("Fading...");
            ValSlice -= Time.deltaTime;
            rend.material.SetFloat("_SliceAmount", ValSlice);

            yield return new WaitForSeconds(0.1f);
        }
        */

        // Cache the current color of the material, and its initiql opacity.
        Color color = exploredAreaMaterial.color;
        float startOpacity = exploredAreaMaterial.color.a;
        float targetOpacity = 0.0f;

        // Track how many seconds we've been fading.
        float t = 0;

        while (t < fadeDuration)
        {
            // Step the fade forward one frame.
            t += Time.deltaTime;
            // Turn the time into an interpolation factor between 0 and 1.
            float blend = Mathf.Clamp01(t / fadeDuration);

            // Blend to the corresponding opacity between start & target.
            color.a = Mathf.Lerp(startOpacity, targetOpacity, blend);

            // Apply the resulting color to the material.
            exploredAreaMaterial.color = color;

            // Wait one frame, and repeat.
            yield return null;
        }

        this.gameObject.SetActive(false);
        color.a = 1;
        exploredAreaMaterial.color = color;

    }

}
