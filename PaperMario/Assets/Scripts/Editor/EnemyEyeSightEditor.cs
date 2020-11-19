using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyController))]
public class EnemyEyeSightEditor : Editor {

    void OnSceneGUI()
    {
        //Radius
        EnemyController eC = (EnemyController)target;
        Handles.color = Color.black;
        Handles.DrawWireArc(eC.transform.position, Vector3.up, Vector3.forward, 360, eC.eyeSightRadius);

        //Angle
        Vector3 viewAngleLeft = eC.CurrentEyeSightAngle(-eC.eyeSightAngle/2);
        Vector3 viewAngleRight = eC.CurrentEyeSightAngle(eC.eyeSightAngle/2);

        Handles.color = Color.red;
        Handles.DrawLine(eC.transform.position, eC.transform.position + viewAngleLeft * eC.eyeSightRadius);
        Handles.DrawLine(eC.transform.position, eC.transform.position + viewAngleRight * eC.eyeSightRadius);
    }
}
