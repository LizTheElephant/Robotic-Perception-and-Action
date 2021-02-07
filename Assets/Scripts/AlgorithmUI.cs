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
	private PathRequestManager pathRequestManager;

	private PathRequestManager.Algorithm active = PathRequestManager.Algorithm.BreadthFirstSearch;
	private Dictionary<PathRequestManager.Algorithm, GameObject> algDict;


    public void Start()
    {
		algDict = new Dictionary<PathRequestManager.Algorithm, GameObject>
		{
			{ PathRequestManager.Algorithm.BreadthFirstSearch, breadthFirst },
			{ PathRequestManager.Algorithm.Dijkstra, dijkstra },
			{ PathRequestManager.Algorithm.AStar, aStar }
		};
		foreach(var i in algDict){
			i.Value.GetComponent<Button>().onClick.AddListener(() => ActivateMode(i.Key));
		}

        if (hoverTargets == null)
            hoverTargets = GameObject.FindGameObjectsWithTag("AlgorithmHover");
        
		avatarScript =  GameObject.Find("Fox").GetComponent<Avatar>();
		pathRequestManager =  GameObject.Find("Grid").GetComponent<PathRequestManager>();

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

	private void ActivateMode(PathRequestManager.Algorithm option)
	{
		Debug.Log("Clicked " + option);
		active = option;
		pathRequestManager.algorithm = option;
		// avatarScript.Stop();
		hidePaused();
	}

    
    public void QuitGame()
    {
        Application.Quit();
    }
}
