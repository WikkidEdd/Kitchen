//@author		Lozano Suárez, Roberto
//@email		robloz87@gmail.com
//@version		1.0.0
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utiles {
	
	public enum Axis{X,Y,Z};
	
	static public Vector3 PointTransform(Vector3 v, Transform transform){
		
		Vector3 v1 = v;
		
		v1.Scale(transform.localScale);
			
		v1 = Quaternion.Euler(transform.rotation.eulerAngles)*v1;
			
		v1 = v1 + transform.localPosition;
		
		return v1;
	}
	
	static public Vector3 CurveLienalBezier(Vector3 p0, Vector3 p1, float t){
		
		float _1_less_t = 1-t;
		Vector3 p1_less_p0 = p1-p0;

		
		return p0 +(p1_less_p0*_1_less_t);
	}

	static public Vector3 CurveLienalBezier(Vector2 p0, Vector2 p1, float t){
		
		float _1_less_t = 1-t;
		Vector2 p1_less_p0 = p1-p0;
		
		return p0 +(p1_less_p0*_1_less_t);
	}
	
	static public Vector3 GetPointRectaParametrica(Vector3 p1, Vector3 p2, float valor, Axis axis){
		
		Vector3 v = p2-p1;
		float lamba = (valor-p1[(int)axis])/v[(int)axis];

		
		return new Vector3(p1.x+lamba*v.x, p1.y+lamba*v.y, p1.z+lamba*v.z);
	}
	
	static public List<MeshFilter> GetAllMesh(GameObject obj){
		
		List<MeshFilter> l = new List<MeshFilter>();
		
		 foreach (MeshFilter mesh  in obj.GetComponentsInChildren(typeof(MeshFilter)) )
	    {
	   		if(mesh!=null)
	    		l.Add(mesh);
	    }
		
		return l;
		
	}
}
