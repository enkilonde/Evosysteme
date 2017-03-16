//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(Monster))]
//public class MonsterManagerEditor : Editor
//{

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        //Show(serializedObject.FindProperty("winners"));

//    }


//    public static void Show(SerializedProperty list)
//    {
//        EditorGUILayout.PropertyField(list);
//        EditorGUI.indentLevel += 1;
//        if (list.isExpanded)
//        {
//            for (int i = 0; i < list.arraySize; i++)
//            {
//                //EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));

//                //EditorGUILayout.LabelField("Walk Speed =  " + i.ToString(), list.GetArrayElementAtIndex(i).fi("walkSpeed").GetArrayElementAtIndex(0).floatValue.ToString());


//                //EditorGUILayout.LabelField("bite " + i.ToString(), list.GetArrayElementAtIndex(i).floatValue.ToString());
//                //EditorGUILayout.LabelField("bite " + i.ToString(), list.GetArrayElementAtIndex(i).FindPropertyRelative.ToString());

//            }
//        }
//        EditorGUI.indentLevel -= 1;
//    }

//}
