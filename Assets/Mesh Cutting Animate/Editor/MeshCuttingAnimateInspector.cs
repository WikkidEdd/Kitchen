//@author		Lozano Suárez, Roberto
//@email		robloz87@gmail.com
//@version		1.0.0
using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(MeshCuttingAnimate))]
public class MeshCuttingAnimateInspector : Editor {
	

	private SerializedObject cddd;
	
	MeshCuttingAnimate dce;

	// This function is called when the object is loaded.
	void OnEnable () {
		
		cddd = new SerializedObject(target);
	}



	public void ShowCajaEnvolvente ()
	{
		Handles.color = Color.yellow;
		
		// top square
		Handles.DrawLine (dce.p1_trans, dce.p2_trans);
		Handles.DrawLine (dce.p1_trans, dce.p3_trans);	
		Handles.DrawLine (dce.p4_trans, dce.p2_trans);
		Handles.DrawLine (dce.p4_trans, dce.p3_trans);
		// buttom square
		Handles.DrawLine (dce.p5_trans, dce.p6_trans);
		Handles.DrawLine (dce.p5_trans, dce.p7_trans);
		Handles.DrawLine (dce.p8_trans, dce.p6_trans);
		Handles.DrawLine (dce.p8_trans, dce.p7_trans);
		// verticales
		Handles.DrawLine (dce.p1_trans, dce.p5_trans);
		Handles.DrawLine (dce.p2_trans, dce.p6_trans);
		Handles.DrawLine (dce.p3_trans, dce.p7_trans);
		Handles.DrawLine (dce.p4_trans, dce.p8_trans);
	}

	public void ShowPlane ()
	{
		Handles.color = Color.red;
		Handles.DrawLine (Utiles.PointTransform(dce.plane.p1, dce.transform), Utiles.PointTransform(dce.plane.p2, dce.transform));
		Handles.DrawLine (Utiles.PointTransform(dce.plane.p2, dce.transform), Utiles.PointTransform(dce.plane.p3, dce.transform));
		Handles.DrawLine (Utiles.PointTransform(dce.plane.p3, dce.transform), Utiles.PointTransform(dce.plane.p4, dce.transform));
		Handles.DrawLine (Utiles.PointTransform(dce.plane.p4, dce.transform), Utiles.PointTransform(dce.plane.p1, dce.transform));
	}
	
	
	void OnSceneGUI(){
		
		dce = (MeshCuttingAnimate)target;
		
		
		if(dce.ChangeTransform(dce.transform)){
			dce.SetPointsBoundBoxTranform ();
			SceneView.RepaintAll(); // refresh 
		}
		
		dce.GeneratePlane();
		
		if(dce.isInit==true){
			ShowCajaEnvolvente ();
			ShowPlane ();
		}
		
	}
	
	public override void OnInspectorGUI () {

		float slider_value_aux;
		
		cddd.Update();
		
		
		dce = (MeshCuttingAnimate)target;
	
		if(Application.isPlaying==false){	
				
			if(dce.isInit==false){
				// Show button
				if(GUILayout.Button("Init")){
					dce.DrawBoundBoxReset();
					SceneView.RepaintAll(); // refresh 
				}
				// Show Axis
				dce.axis = (Utiles.Axis) EditorGUILayout.EnumPopup("Axis", dce.axis);

					
			}else{
				// Show button
				 if(GUILayout.Button("Reset")){
					dce.isInit = false;
					SceneView.RepaintAll(); // refresh 
				}
				// Show Axis
				EditorGUILayout.LabelField("Axis: "+dce.axis.ToString());
				// show GUI
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Show GUI");
				dce.isActiveGui = EditorGUILayout.Toggle(dce.isActiveGui);
				EditorGUILayout.EndHorizontal();
				// Show slider
				slider_value_aux = EditorGUILayout.Slider(dce.PercentageCut, 0.0f, 1.0f);

					
				if(dce.PercentageCut!=slider_value_aux ){
					dce.PercentageCut = slider_value_aux;
					dce.GeneratePlane();
					SceneView.RepaintAll(); // refresh 
				}
			}
			
		}else{
			
			if(dce.isInit==false)
				EditorGUILayout.HelpBox("Not Init", MessageType.Error);
			else{
				// Show Axis
				EditorGUILayout.LabelField("Axis: "+dce.axis.ToString());
				// show GUI
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Show GUI");
				dce.isActiveGui = EditorGUILayout.Toggle(dce.isActiveGui);
				EditorGUILayout.EndHorizontal();
				// Show slider
				slider_value_aux = EditorGUILayout.Slider(dce.PercentageCut, 0.0f, 1.0f);
				
				if(dce.PercentageCut!=slider_value_aux ){
					dce.PercentageCut = slider_value_aux;
					dce.GeneratePlane();
					dce.ExecuteCutMesh();
					SceneView.RepaintAll(); // refresh 
					
				}
			}
			
		}
		
	}
	
	
}
