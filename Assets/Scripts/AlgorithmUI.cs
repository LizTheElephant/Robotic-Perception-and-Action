using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class AlgorithmUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {


    public GameObject breadthFirst;
	public GameObject dijkstra;
	public GameObject aStar;

    private GameObject[] hoverTargets;
	private Avatar avatarScript;
	private Pathfinding pathfinding;

	private Pathfinding.Algorithm active = Pathfinding.Algorithm.BreadthFirstSearch;
	private Dictionary<Pathfinding.Algorithm, GameObject> algDict;


    public void Start()
    {
		algDict = new Dictionary<Pathfinding.Algorithm, GameObject>
		{
			{ Pathfinding.Algorithm.BreadthFirstSearch, breadthFirst },
			{ Pathfinding.Algorithm.Dijkstra, dijkstra },
			{ Pathfinding.Algorithm.AStar, aStar }
		};
		foreach(var i in algDict){
			i.Value.GetComponent<Button>().onClick.AddListener(() => ActivateMode(i.Key));
		}

        if (hoverTargets == null)
            hoverTargets = GameObject.FindGameObjectsWithTag("AlgorithmHover");
        
		avatarScript =  GameObject.Find("Fox").GetComponent<Avatar>();
		pathfinding =  GameObject.Find("Grid").GetComponent<Pathfinding>();

		hidePaused();
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
		foreach(GameObject i in hoverTargets){
			i.SetActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
    {
		hidePaused();
	}

    //hides objects with ShowOnPause tag
	public void hidePaused()
	{
		foreach(GameObject i in hoverTargets){
			i.SetActive(i == algDict[active]);
		}
	}

	private void ActivateMode(Pathfinding.Algorithm option)
	{
		Debug.Log("Clicked " + option);
		active = option;
		pathfinding.algorithm = option;
		// avatarScript.Stop();
		hidePaused();
	}

    
    public void QuitGame()
    {
        Application.Quit();
    }
}
