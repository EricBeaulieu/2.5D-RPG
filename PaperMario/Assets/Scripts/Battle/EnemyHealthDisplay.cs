using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EnemyHealthDisplay : MonoBehaviour {

    float healthBarMaximum;
    Transform healthBarLostParent;
    [SerializeField]
    GameObject healthBarLost;

    float fadeOutTime = 2.25f;

	void Start () {
        //enemyController = this.gameObject.GetComponent<EnemyController>();
        //healthBarPos = enemyController.healthBarTransform;
        //healthBar = Instantiate(GameManager.instance.healthBarPrefab, healthBarPos.position, Quaternion.identity, healthBarPos);
        healthBarMaximum = this.transform.Find("Canvas/Health").GetComponent<RectTransform>().rect.width;
        healthBarLostParent = this.transform.Find("Canvas/Health");
    }

    void OnTakeDamage(int damage,EnemyController currentEnemy)
    {
        GameObject healthLost = FindandReturnUnusedHealthBarLostGameobject();

        float healthBarLostPosition = ((float)currentEnemy.currHealth / (float)currentEnemy.getMaxHealth) * healthBarMaximum;

        healthLost.GetComponent<RectTransform>().localPosition= new Vector3(healthBarLostPosition,0,0);

        float healthLostSize;

        if (damage <= currentEnemy.currHealth)
        {
            healthLostSize = ((float)damage / (float)currentEnemy.getMaxHealth) * healthBarMaximum;
        }
        else
        {
            healthLostSize = ((float)currentEnemy.currHealth / (float)currentEnemy.getMaxHealth) * healthBarMaximum;
        }

        healthLost.GetComponent<RectTransform>().sizeDelta = new Vector2(healthLostSize, healthLost.GetComponent<RectTransform>().sizeDelta.y);

        healthLost.SetActive(true);
        healthLost.GetComponent<Rigidbody2D>().AddForce(Vector2.up);
        StartCoroutine(HealthLostFadeOut(healthLost));
    }

    GameObject FindandReturnUnusedHealthBarLostGameobject()
    {
        GameObject temp = null;

        for (int i = 0; i < healthBarLostParent.childCount; i++)
        {
            if(healthBarLostParent.GetChild(i).gameObject.activeInHierarchy == false)
            {
                temp = healthBarLostParent.GetChild(i).gameObject;
                return temp;
            }
        }

        temp = Instantiate(healthBarLost, healthBarLostParent);
        return temp;
    }

    IEnumerator HealthLostFadeOut(GameObject currentHealthBarLost)
    {
        Color temp = currentHealthBarLost.GetComponent<Image>().color;
        float t = 0;

        while(t<fadeOutTime)
        {
            temp.a = 1 - (t / fadeOutTime);
            yield return null;
            t += Time.deltaTime;
        }

        currentHealthBarLost.SetActive(false);
        currentHealthBarLost.transform.position = healthBarLostParent.transform.position;
    }
}
