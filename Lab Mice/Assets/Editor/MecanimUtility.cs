using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class MecanimUtility
{
	[MenuItem("Mecanim/Build Controllers")]
	public static void DoBuildControllers()
	{
		UnityEditorInternal.AnimatorController[] controllers = UnityEngine.Object.FindObjectsOfTypeAll(typeof(UnityEditorInternal.AnimatorController)) as UnityEditorInternal.AnimatorController[];
		Debug.Log("Building controller list ...");
		foreach (UnityEditorInternal.AnimatorController controller in controllers)
		{
			Debug.Log("Building " + controller.name);
			
			string name = controller.name;
			
			// By changing a serialized property we are forcing the asset to rebuild itseft.
			SerializedObject obj = new SerializedObject(controller);
			SerializedProperty nameProp = obj.FindProperty("m_Name");
			nameProp.stringValue = "temp";
			obj.ApplyModifiedProperties();
			
			nameProp.stringValue = name;
			obj.ApplyModifiedProperties();
		}
	}
}