using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class AlgorithmUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


    public GameObject breadthFirst;
	public GameObject djiekstra;
	public GameObject aStar;
    private GameObject[] hoverTargets;

	private Avatar avatarScript;
	private WorldGrid worldGridScript;

	private WorldGrid.PathPlanningPriority active = WorldGrid.PathPlanningPriority.ShortestDistance;
	private Dictionary<WorldGrid.PathPlanningPriority, GameObject> algDict;


    public void Start()
    {
		algDict = new Dictionary<WorldGrid.PathPlanningPriority, GameObject>
		{
			{ WorldGrid.PathPlanningPriority.ShortestDistance, breadthFirst },
			{ WorldGrid.PathPlanningPriority.ShortestTime, djiekstra },
			{ WorldGrid.PathPlanningPriority.SmallestFuelConsumption, aStar },
			{ WorldGrid.PathPlanningPriority.SafeDistanceFromObstacles, aStar }
		};
		foreach(var i in algDict){
			i.Value.GetComponent<Button>().onClick.AddListener(() => ActivateMode(i.Key));
		}

        if (hoverTargets == null)
            hoverTargets = GameObject.FindGameObjectsWithTag("AlgorithmHover");
        
		avatarScript =  GameObject.Find("Fox").GetComponent<Avatar>();
		worldGridScript =  GameObject.Find("Grid").GetComponent<WorldGrid>();

		hidePaused();
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
		showPaused();
	}

	public void OnPointerExit(PointerEventData eventData)
    {
		hidePaused();
	}
    
    //shows objects with ShowOnPause tag
	public void showPaused()
	{
		foreach(GameObject i in hoverTargets){
			i.SetActive(true);
		}
	}

    //hides objects with ShowOnPause tag
	public void hidePaused()
	{
		foreach(GameObject i in hoverTargets){
			i.SetActive(i == algDict[active]);
		}
	}

	private void ActivateMode(WorldGrid.PathPlanningPriority option)
	{
		Debug.Log("Clicked " + option);
		active = option;
		worldGridScript.priority = option;
		avatarScript.Stop();
		hidePaused();
	}

    
    public void QuitGame()
    {
        Application.Quit();
    }
}
