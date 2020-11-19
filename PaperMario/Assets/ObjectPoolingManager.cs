using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolingManager : MonoBehaviour
{

    //#region Object Manager Singleton/Awake

    //public static ObjectPoolingManager _instance;//Reference

    //void Awake()
    //{
    //    if (instance == null)
    //    {
    //        instance = this;
    //    }
    //    else if (instance != null)
    //    {
    //        Destroy(gameObject);
    //    }

    //    DontDestroyOnLoad(gameObject);
    //}

    //public static ObjectPoolingManager instance
    //{
    //    get { return _instance; }
    //    set { _instance = value; }
    //}

    //#endregion

    //GameObject temp;//used for everything pretty much in object pooling
    //GameManager GM;

    //public GameObject player;

    ////Gold Pooling
    //[Header("Gold")]
    //public int goldPoolingAmount;

    ////Standard Pooling
    //public Dictionary<string, GameObject> itemCache = new Dictionary<string, GameObject>();

    ////Temp
    //Transform playerCurrentLocation;

    //void Start()
    //{
    //    player = GameObject.Find("Unit");

    //    #region Pooling Creation Amount Checker

    //    if (goldPoolingAmount <= 0)
    //    {
    //        Debug.LogWarning("goldPoolingAmount is not set, setting to 3");
    //        goldPoolingAmount = 10;
    //    }

    //    #endregion

    //    #region All Items Pooling Creation

    //    itemCache = Resources.LoadAll<GameObject>("Items").ToDictionary(item => item.name, item => item);

    //    for (int i = 0; i < itemCache.Count; i++)
    //    {

    //        if (itemCache.ElementAt(i).Value.GetComponent<ItemPickup>() == null)
    //        {
    //            Debug.LogWarning(itemCache.ElementAt(i).Value.name + "Error, Item Pickup Is not implimented onto this object");
    //        }
    //        if (itemCache.ElementAt(i).Value.GetComponent<ItemPickup>().item == null)
    //        {
    //            Debug.LogWarning(itemCache.ElementAt(i).Value.name + "Error, The Item in Item Pickup component is null on this prefab object");
    //        }
    //        if (itemCache.ElementAt(i).Value.GetComponent<ItemPickup>().item.itemName == "")
    //        {
    //            Debug.LogWarning(itemCache.ElementAt(i).Value.name + "Error, The Item in Item Pickup Scriptable Object name is null on this prefab object");
    //        }
    //        if (transform.GetChild(i).GetComponent<ItemPickup>().item.itemGameObject == null)
    //        {
    //            Debug.LogWarning(transform.GetChild(i).name + "Error, The Item Game Object in Item Pickup Scriptable Object name is null on this prefab object");
    //        }

    //        //This resets all starting values to 0 just incase
    //        itemCache.ElementAt(i).Value.GetComponent<ItemPickup>().item.count = 0;
    //    }

    //    //This cleans up everything and makes the location in the dictionary have a number

    //    itemCache = new Dictionary<string, GameObject>();

    //    int counterAidGold = 0;

    //    for (int i = 0; i < this.transform.childCount; i++)
    //    {
    //        if (this.transform.GetChild(i).GetComponent<ItemPickup>().item.itemType == ItemType.Gold) //|| this.transform.GetChild(i).GetComponent<ItemPickup>().item.itemName != waterPrefab.name)
    //        {
    //            itemCache.Add(this.transform.GetChild(i).GetComponent<ItemPickup>().item.itemName + counterAidGold.ToString(), this.transform.GetChild(i).gameObject);
    //            counterAidGold++;
    //        }
    //        else
    //        {
    //            //This will add one of each item in the scene and give it a key reference in the object pool with a number
    //            if (!itemCache.ContainsKey(this.transform.GetChild(i).GetComponent<ItemPickup>().item.itemName + "0"))
    //                itemCache.Add(this.transform.GetChild(i).GetComponent<ItemPickup>().item.itemName + "0", this.transform.GetChild(i).gameObject);
    //        }
    //    }

    //    Debug.Log(itemCache.Count + "How many items in the Object Pool");

    //    #endregion

    //    GM = GameManager.instance;
    //    GM.objPoolManager = this;
    //}

    //public void sendit()
    //{
    //    itemCache = new Dictionary<string, GameObject>();

    //    // int counterAidGold = 0;

    //    for (int i = 0; i < this.transform.childCount; i++)
    //    {
    //        ObjectPoolAddition(transform.GetChild(i).gameObject);
    //    }
    //}

    //#region Add To Object Pool

    //public void ObjectPoolAddition(GameObject newItem)//Function to be called when picking up a item
    //{
    //    int counterAssistForDictionary = 0;
    //    bool exists = false;
    //    // Debug.Log(newItem.name, newItem);
    //    for (int i = 0; i < 99; i++)
    //    {
    //        if (newItem.GetComponent<ItemPickup>() == null)
    //        {
    //            Debug.Log(newItem.name + "Error, Item Pickup Is not implimented onto this object");
    //        }
    //        if (newItem.GetComponent<ItemPickup>().item == null)
    //        {
    //            Debug.Log(transform.GetChild(i).name + "Error, The Item in Item Pickup component is null on this prefab object");
    //        }

    //        if (itemCache.ContainsValue(newItem))
    //        {
    //            if (newItem.GetComponent<ItemPickup>().madeFromScratch)
    //            {
    //                itemmanager.tester.objsToInstantiate.Remove(newItem.name);
    //            }
    //            else
    //            {
    //                if (!itemmanager.tester.pool.Contains(newItem.name))
    //                    itemmanager.addToPool(newItem);
    //            }

    //            /*     if(!itemmanager.tester.pool.Contains(newItem.name))
    //                 itemmanager.addToPool(newItem);
    //                 if (itemmanager.tester.objsToInstantiate.Contains(newItem.name))
    //                 {
    //                     itemmanager.tester.objsToInstantiate.Remove(newItem.name);
    //                 }*/

    //            newItem.transform.parent = this.transform;
    //            newItem.SetActive(false);
    //            Debug.Log("Item is already part of the item cache, Readding" + newItem.GetComponent<ItemPickup>().item.itemName + i);
    //            break;
    //        }
    //        else
    //        {
    //            exists = itemCache.ContainsKey(newItem.GetComponent<ItemPickup>().item.itemName + counterAssistForDictionary.ToString());
    //            //This next line is to help identify if there are errors with the itemcache
    //            //Debug.Log(newItem.GetComponent<ItemPickup>().item.itemName + counterAssistForDictionary.ToString());
    //            //exists = itemCache.Any(x => (x.Value.transform.GetComponent<ItemPickup>().item.itemName + counterAssistForDictionary.ToString() != null));
    //            if (exists)
    //            {
    //                counterAssistForDictionary++;
    //            }
    //            else
    //            {
    //                itemCache.Add(newItem.GetComponent<ItemPickup>().item.itemName + counterAssistForDictionary.ToString(), newItem);

    //                /*    if (!itemmanager.tester.pool.Contains(newItem.name))
    //                        itemmanager.addToPool(newItem);
    //                    if (itemmanager.tester.objsToInstantiate.Contains(newItem.name))
    //                    {
    //                        itemmanager.tester.objsToInstantiate.Remove(newItem.name);
    //                    }*/

    //                if (newItem.GetComponent<ItemPickup>().madeFromScratch)
    //                {
    //                    itemmanager.tester.objsToInstantiate.Remove(newItem.name);
    //                }
    //                else
    //                {
    //                    if (!itemmanager.tester.pool.Contains(newItem.name))
    //                        itemmanager.addToPool(newItem);
    //                }

    //                if (newItem.transform.parent != this.transform)
    //                    newItem.transform.parent = this.transform;
    //                newItem.SetActive(false);
    //                Debug.Log("Item being added to the item cache, adding " + newItem.GetComponent<ItemPickup>().item.itemName + i);
    //                counterAssistForDictionary = 0;
    //                break;
    //            }

    //        }
    //    }

    //}

    //#endregion

    //#region Removing Object From Object Pool


    //public void ObjectPoolPlayerDrop(GameObject[] droppingItem)
    //{

    //    for (int i = 0; i < 99; i++)
    //    {
    //        if (itemCache.ContainsKey(droppingItem.GetComponent<ItemPickup>().item.itemName + i))
    //        {
    //            temp = itemCache[droppingItem.GetComponent<ItemPickup>().item.itemName + i];
    //            if (!temp.activeInHierarchy)
    //            {
    //                temp.transform.position = player.transform.position;
    //                temp.GetComponent<ItemPickup>().count = droppingHowMany;
    //                temp.SetActive(true);
    //                //  temp.transform.localPosition = new Vector3(0, 0, -1);
    //                if (!temp.GetComponent<ItemPickup>().madeFromScratch)
    //                {
    //                    itemmanager.removeFromPool(temp);
    //                }
    //                else
    //                {
    //                    //    itemmanager.tester.pool.Remove(temp.name);
    //                    itemmanager.tester.objsToInstantiate.Add(temp.name);
    //                }
    //                break;
    //            }
    //            else
    //            {
    //                Debug.Log("This item is active" + droppingItem.GetComponent<ItemPickup>().item.itemName + i);
    //            }

    //        }
    //        else
    //        {
    //            temp = Instantiate(droppingItem.GetComponent<ItemPickup>().item.itemPrefab, Vector3.zero, Quaternion.identity);
    //            temp.transform.position = player.transform.position;
    //            temp.name += itemmanager.tester.objsToInstantiate.Count;
    //            itemmanager.items.Add(temp);
    //            itemmanager.tester.objsToInstantiate.Add(temp.name);
    //            //   temp.transform.localPosition = new Vector3(0, 0, -1);//This is to make the item pop up on the Z axis
    //            temp.GetComponent<ItemPickup>().count = droppingHowMany;
    //            temp.GetComponent<ItemPickup>().madeFromScratch = true;
    //            break;
    //        }
    //    }

    //}

    //public GameObject getReward(GameObject droppingItem, Vector3 pos)
    //{


    //    for (int i = 0; i < 99; i++)
    //    {
    //        if (itemCache.ContainsKey(droppingItem.GetComponent<ItemPickup>().item.itemName + i))
    //        {
    //            temp = itemCache[droppingItem.GetComponent<ItemPickup>().item.itemName + i];
    //            if (!temp.activeInHierarchy)
    //            {

    //                /* if (!temp.GetComponent<ItemPickup>().madeFromScratch)
    //                     itemmanager.removeFromPool(temp);
    //                 else
    //                 {
    //                     itemmanager.tester.objsToInstantiate.Add(temp.name);
    //                 }*/
    //            }
    //            else
    //            {
    //                continue;
    //            }

    //            break;
    //        }
    //        else
    //        {
    //            temp = Instantiate(droppingItem, Vector3.zero, Quaternion.identity);
    //            Debug.Log(temp + "made");
    //            temp.transform.position = pos;

    //            temp.name += itemmanager.tester.objsToInstantiate.Count;
    //            itemmanager.items.Add(temp);
    //            itemmanager.tester.objsToInstantiate.Add(temp.name);
    //            temp.SetActive(true);
    //            temp.GetComponent<ItemPickup>().madeFromScratch = true;
    //            //   temp.transform.localPosition = new Vector3(0, 0, -1);//This is to make the item pop up on the Z axis
    //            //     temp.GetComponent<ItemPickup>().count = droppingHowMany;
    //            break;
    //        }
    //    }

    //    return temp;
    //}


    //public void DropTable(GameObject droppingItem, Vector3 pos)
    //{

    //    for (int i = 0; i < 99; i++)
    //    {
    //        if (itemCache.ContainsKey(droppingItem.GetComponent<ItemPickup>().item.itemName + i))
    //        {
    //            temp = itemCache[droppingItem.GetComponent<ItemPickup>().item.itemName + i];
    //            if (!temp.activeInHierarchy)
    //            {
    //                //  temp.transform.localPosition = new Vector3(0, 0, -1);
    //                //   temp.transform.position = player.transform.position;

    //                temp.transform.position = pos;
    //                temp.SetActive(true);
    //                if (!temp.GetComponent<ItemPickup>().madeFromScratch)
    //                    itemmanager.removeFromPool(temp);
    //                else
    //                {
    //                    //     itemmanager.tester.pool.Remove(temp.name);
    //                    itemmanager.tester.objsToInstantiate.Add(temp.name);
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("This item is active" + droppingItem.GetComponent<ItemPickup>().item.itemName + i);
    //                continue;
    //            }

    //            break;

    //        }
    //        else
    //        {
    //            temp = Instantiate(droppingItem, Vector3.zero, Quaternion.identity);
    //            Debug.Log(temp + "made");
    //            temp.transform.position = pos;
    //            temp.name += itemmanager.tester.objsToInstantiate.Count;
    //            temp.GetComponent<ItemPickup>().madeFromScratch = true;
    //            itemmanager.items.Add(temp);
    //            itemmanager.tester.objsToInstantiate.Add(temp.name);
    //            temp.SetActive(true);
    //            //   temp.transform.localPosition = new Vector3(0, 0, -1);//This is to make the item pop up on the Z axis
    //            //     temp.GetComponent<ItemPickup>().count = droppingHowMany;
    //            break;
    //        }
    //    }



    //}


    /*
        public GameObject ObjectPoolFindNReturn(GameObject droppingItem, int droppingHowMany, Vector3 location)
        {

            for (int i = 0; i < Mathf.Infinity; i++)
            {
                if (itemCache.ContainsKey(droppingItem.GetComponent<ItemPickup>().item.itemName + i))
                {
                    temp = itemCache[droppingItem.GetComponent<ItemPickup>().item.itemName + i];
                    if (!temp.activeInHierarchy)
                    {
                        temp.transform.position = location;
                        temp.GetComponent<ItemPickup>().count = droppingHowMany;
                        temp.transform.parent = null;
                        temp.SetActive(true);
                        Debug.Log("Found inactive object ", temp);
                        return temp;
                    }
                }
                else
                {
                    temp = Instantiate(droppingItem, location, Quaternion.identity);
                    temp.GetComponent<ItemPickup>().count += droppingHowMany;
                    temp.transform.parent = null;
                    Debug.Log("Inactive object not Found, Creating New Object", temp);
                    return temp;
                }
            }
            Debug.Log("Unreachable Code Detected, fucked up somewhere in object pooling");
            return null;
        }
        */

    //#endregion

}