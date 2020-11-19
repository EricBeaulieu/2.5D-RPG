using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    Entity[] allEntities;
    List<GameObject> allObjectsInScene;

    // Use this for initialization
    void Start () {
        allEntities = (Entity[])GameObject.FindObjectsOfType(typeof(Entity));
        GameManager.instance.currentLevel = this;
    }

    public void PauseEntitiesForBattle()
    {
        GameObject[] gO = GameObject.FindObjectsOfType<GameObject>();
        allObjectsInScene = new List<GameObject>();
        foreach (GameObject child in gO)
        {
            if (child.activeInHierarchy)
            {
                allObjectsInScene.Add(child);
                child.SetActive(false);
            }
        }

        //for (int i = 0; i < allEntities.Length; i++)
        //{
        //    allEntities[i].inBattle = true;
        //}
    }

    public void ResumeEntitiesFromBattle()
    {
        for (int i = 0; i < allObjectsInScene.Count; i++)
        {
            allObjectsInScene[i].SetActive(true);
        }
        //for (int i = 0; i < allEntities.Length; i++)
        //{
        //    allEntities[i].inBattle = false;
        //}
    }
}
