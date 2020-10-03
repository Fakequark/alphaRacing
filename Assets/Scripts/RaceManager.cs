using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{

    
    public List<CarAi> participants; //you can add participants by dragging the gameobjects here from Unity's inspector, or add them in 
    public int pos = 1;
    void Start() {
        //You can initialize startPoint, participants here. Or do it in the inspector
    }

    void Update() {
        pos = 1;
        //Sort the list first. Check if the list is null first.. I will not do that here for clarity sake
        //participants.Sort((p1, p2) => p1.GetDistanceTravelled(startPoint).Value.CompareTo(p2.GetDistanceTravelled(startPoint).Value)); //you can sort easily with lambda expressions
        foreach(CarAi oponent in participants)
        {
            if (oponent.GetProgress() > (CarController.playerProgress + CarController.playerLap)) {
                pos++;
            }
            //objet.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alphaLevel);
        }
        //print the results here, you can iterate through the list and do a Debug.Log() or something
    }

    public void OnGUI () {
       GUI.Box(new Rect(0,0,10,10), pos.ToString());
        //GUI.Label(Rect(10,10,100,30), pos.ToString());
       
    }

}
