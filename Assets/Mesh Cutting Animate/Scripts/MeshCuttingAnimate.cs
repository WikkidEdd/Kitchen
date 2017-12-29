//@author		Lozano Suárez, Roberto
//@email		robloz87@gmail.com
//@version		1.0.0
using System.Collections.Generic;
using UnityEngine;


public class MeshCuttingAnimate : MonoBehaviour {

	
	public struct ObjectPlane{
		
		public Vector3 p1, p2, p3, p4;
		
	}
	
	public enum StateTriangle{ In, Out, Between}
	
	[System.Serializable]
	public class TriangleInfo{
		
		public float min_value;
		public float between_value;
		public float max_value;
		
		public int index_triangle;

		public int min_index_t, max_index_t, between_index_t;
		public int min_index_v, max_index_v, between_index_v; 
		
		public int index_other_list;

	}
	
	
	[System.Serializable]
	public class TriangleInfoList{
		
		public int[] l_triangles_show;
		public TriangleInfo[] l_triangleInfo;
		public int l_first_triangle_hide;

		public int[] l_sortBigger;
		public int l_sortBigger_count;
		
		public List<int> l_intersection;
		
	}
	
	

	// old transformation data
	private Vector3 old_position, old_scale;
	private Quaternion old_rotation;
	
	[SerializeField]
	private float percentageCut=1;

	public float PercentageCut {
		get {
			return this.percentageCut;
		}
		set {
			if(value>1.0f)
				percentageCut = 1.0f;
			else if(value<0.0f)
				percentageCut = 0.0f;
			else
				percentageCut = value;

		}
	}

	
	// Main Mesh Points
	public ObjectPlane plane;
	[SerializeField]
	private Vector3 p1,p2,p3,p4,p5,p6,p7,p8;
	public Vector3 p1_trans,p2_trans,p3_trans,p4_trans,p5_trans,p6_trans,p7_trans,p8_trans;
	public Vector3 pointCenterPlane;
	float pointCenterPlaneCoordinate;
	
	// Lists and Arrays
	public List<Vector3> listExtraVertexs;
	public List<Vector2> listExtraUVMapping;
	public List<MeshFilter> listMeshFilter;
	public List<TriangleInfoList> listTrianglesInfo;
	public Mesh[] arrayOriginalMesh;
	
	
	// others
	public Utiles.Axis axis;
	public bool isInit=false;
	int[] index_aux = new int[3];
	bool upPlane=false;
	float PercentageCutBefore=0.0f;
	public bool isActiveGui=false;
	
	public StateTriangle CheckTriangle(TriangleInfo t){
		
			if(t.min_value>pointCenterPlaneCoordinate){
				return StateTriangle.Out;
			}else if(t.max_value<pointCenterPlaneCoordinate){ 
				return StateTriangle.In;
			}else{
				return StateTriangle.Between;
			}
		
	}
	
	public bool ChangeTransform(Transform t_current){
	
		if(t_current.position!=old_position)
			return true;
		if(t_current.rotation!=old_rotation)
			return true;
		if(t_current.localScale!=old_scale)
			return true;
		
		return false;
		
	}
	
	public List<TriangleInfo> SortTriangles(Mesh mesh, TriangleInfoList triangleInfoList){
	
		float p1, p2, p3;
		List<TriangleInfo> l_triangles_sort_local = new List<TriangleInfo>(mesh.triangles.Length/3);

		
		for(int i=0; i<mesh.triangles.Length-2; i+=3){
			
			TriangleInfo triangleInfo = new TriangleInfo();

			p1 = mesh.vertices[mesh.triangles[i]][(int)axis];
			p2 = mesh.vertices[mesh.triangles[i+1]][(int)axis];
			p3 = mesh.vertices[mesh.triangles[i+2]][(int)axis];

			
			if(p1<p2 && p1<p3){
				 triangleInfo.min_value = p1;
				 triangleInfo.min_index_t = i;
				 if(p2<p3){
					triangleInfo.max_value = p3;
					triangleInfo.between_value = p2;
					triangleInfo.max_index_t = i+2;
					triangleInfo.between_index_t = i+1;
				}else{
					triangleInfo.max_value = p2;
					triangleInfo.between_value = p3;
					triangleInfo.max_index_t = i+1;
					triangleInfo.between_index_t = i+2;
				}
				
			}else if(p2<p3){
				 triangleInfo.min_value = p2;
				 triangleInfo.min_index_t = i+1;
				 if(p1<p3){
					triangleInfo.max_value = p3;
					triangleInfo.between_value = p1;
					triangleInfo.max_index_t = i+2;
					triangleInfo.between_index_t = i;
				}else{
					triangleInfo.max_value = p1;
					triangleInfo.between_value = p3;
					triangleInfo.max_index_t = i;
					triangleInfo.between_index_t = i+2;
				}
				
			}else {
				 triangleInfo.min_value = p3;
				 triangleInfo.min_index_t = i+2;
				 if(p1<p2){
					triangleInfo.max_value = p2;
					triangleInfo.between_value = p1;
					triangleInfo.max_index_t = i+1;
					triangleInfo.between_index_t = i;
				}else{
					triangleInfo.max_value = p1;
					triangleInfo.between_value = p2;
					triangleInfo.max_index_t = i;
					triangleInfo.between_index_t = i+1;
				}
			}
			
			triangleInfo.min_index_v     = mesh.triangles[triangleInfo.min_index_t];
			triangleInfo.max_index_v     = mesh.triangles[triangleInfo.max_index_t]; 
			triangleInfo.between_index_v = mesh.triangles[triangleInfo.between_index_t];
			
			triangleInfo.index_triangle = i;

			l_triangles_sort_local.Add(triangleInfo);
		}
		
	// 1st ordenation
	l_triangles_sort_local.Sort(
		
	
			delegate(TriangleInfo x, TriangleInfo y)
				   {
						if(x.max_value>y.max_value)
								return 1;
						else if(x.max_value<y.max_value)
							return -1;
						else{
							int dif = x.min_value.CompareTo(y.min_value);
							if(dif==0)
								return x.index_triangle.CompareTo(y.index_triangle);
							return dif;
						}
		});

		
		for(int i=0; i<l_triangles_sort_local.Count; i++){
			l_triangles_sort_local[i].index_other_list = i;	

		}
		// 2sd ordenation
		l_triangles_sort_local.Sort(
			delegate(TriangleInfo x, TriangleInfo y)
				   {

						if(x.min_value<y.min_value)
								return -1;
						else if(x.min_value>y.min_value)
							return 1;
						else{
							int dif = x.max_value.CompareTo(y.max_value);
							if(dif==0)
								return x.index_triangle.CompareTo(y.index_triangle);
							return dif;
						}

			});
		
		return l_triangles_sort_local;
		
	}

	public void SetPointsBoundBoxTranform ()
	{
		p1_trans = Utiles.PointTransform(p1, transform);
		p2_trans = Utiles.PointTransform(p2, transform);
		p3_trans = Utiles.PointTransform(p3, transform);
		p4_trans = Utiles.PointTransform(p4, transform);
		
		p5_trans = Utiles.PointTransform(p5, transform);
		p6_trans = Utiles.PointTransform(p6, transform);
		p7_trans = Utiles.PointTransform(p7, transform);
		p8_trans = Utiles.PointTransform(p8, transform);
		
		old_rotation = transform.rotation;
		old_position = transform.position;
		old_scale = transform.localScale;
		
	}
	
	public void GeneratePlane(){
	
		if(axis == Utiles.Axis.Z){
			plane.p1 = Utiles.CurveLienalBezier(p4, p8, percentageCut);
			plane.p2 = Utiles.CurveLienalBezier(p3, p7, percentageCut);
			plane.p3 = Utiles.CurveLienalBezier(p1, p5, percentageCut);
			plane.p4 = Utiles.CurveLienalBezier(p2, p6, percentageCut);
		
			pointCenterPlane = new Vector3((plane.p1.x+plane.p2.x)/2.0f,(plane.p1.y+plane.p4.y)/2.0f, plane.p1.z);
			pointCenterPlaneCoordinate = pointCenterPlane.z;

		}else if(axis == Utiles.Axis.Y){
			plane.p1 = Utiles.CurveLienalBezier(p1, p3, percentageCut);
			plane.p2 = Utiles.CurveLienalBezier(p5, p7, percentageCut);
			plane.p4 = Utiles.CurveLienalBezier(p2, p4, percentageCut);
			plane.p3 = Utiles.CurveLienalBezier(p6, p8, percentageCut);

			pointCenterPlane = new Vector3((plane.p2.x+plane.p3.x)/2.0f, plane.p1.y, (plane.p1.z+plane.p3.z)/2.0f);
			pointCenterPlaneCoordinate = pointCenterPlane.y;

		
		}else if(axis == Utiles.Axis.X){
			plane.p1 = Utiles.CurveLienalBezier(p1, p2, percentageCut);
			plane.p2 = Utiles.CurveLienalBezier(p5, p6, percentageCut);
			plane.p3 = Utiles.CurveLienalBezier(p7, p8, percentageCut);
			plane.p4 = Utiles.CurveLienalBezier(p3, p4, percentageCut);
                                                 
			pointCenterPlane = new Vector3(plane.p1.x, (plane.p1.y+plane.p4.y)/2.0f, (plane.p1.z+plane.p3.z)/2.0f);
			pointCenterPlaneCoordinate = pointCenterPlane.x;
		}
		
	}
	
	public void Init(){
		
		Bounds b = new Bounds();
		
		listTrianglesInfo = new List<TriangleInfoList>();
		
		listMeshFilter = Utiles.GetAllMesh(this.gameObject);
		
		arrayOriginalMesh = new Mesh[listMeshFilter.Count];
		
		
		for(int i=0; i<listMeshFilter.Count; i++){
			
			b.Encapsulate(listMeshFilter[i].sharedMesh.bounds);
			
			arrayOriginalMesh[i] = Mesh.Instantiate(listMeshFilter[i].sharedMesh) as Mesh;
			
			TriangleInfoList triangleInfoList = new TriangleInfoList();
			
			triangleInfoList.l_triangleInfo = SortTriangles(arrayOriginalMesh[i], triangleInfoList).ToArray();
			triangleInfoList.l_sortBigger = new int[triangleInfoList.l_triangleInfo.Length];
			triangleInfoList.l_intersection = new List<int>(triangleInfoList.l_sortBigger.Length);
			triangleInfoList.l_first_triangle_hide = 0;
			triangleInfoList.l_sortBigger_count = -1;
			
			for(int j=0; j<triangleInfoList.l_triangleInfo.Length; j++){			
				triangleInfoList.l_sortBigger[triangleInfoList.l_triangleInfo[j].index_other_list] = j;
			}
			
			triangleInfoList.l_triangles_show = new int[arrayOriginalMesh[i].triangles.Length];
			
			
			
			for(int z=0; z<triangleInfoList.l_triangles_show.Length; z++)
				triangleInfoList.l_triangles_show[z] = 0;
			
			listTrianglesInfo.Add(triangleInfoList);
			
		}
		
		// top square
		p1 = new Vector3(b.max.x, b.max.y, b.max.z);
		p2 = new Vector3(b.min.x, b.max.y, b.max.z);
		p3 = new Vector3(b.max.x, b.min.y, b.max.z);
		p4 = new Vector3(b.min.x, b.min.y, b.max.z);
		// buttom square
		p5 = new Vector3(b.max.x, b.max.y, b.min.z);
		p6 = new Vector3(b.min.x, b.max.y, b.min.z);
		p7 = new Vector3(b.max.x, b.min.y, b.min.z);
		p8 = new Vector3(b.min.x, b.min.y, b.min.z);
		
		GeneratePlane();
		SetPointsBoundBoxTranform();
		
		isInit = true;	
	}
	
	public void DrawBoundBoxReset(){

		listMeshFilter.Clear();
		arrayOriginalMesh = null;
		listExtraVertexs.Clear();
		listExtraUVMapping.Clear();
		isInit = false;
		
		Init();
	}
	
	public void IntersectionTriangles (Mesh mesh_input, List<int> l_triangles_add, TriangleInfo current_triangle)
	{
		
		index_aux[0] = current_triangle.index_triangle;
		index_aux[1] = current_triangle.index_triangle+1;
		index_aux[2] = current_triangle.index_triangle+2;		
		
		
		if(current_triangle.between_value<pointCenterPlaneCoordinate){
			
			int pos_firs_out = current_triangle.max_index_t-current_triangle.index_triangle;
			int index_pout = current_triangle.max_index_v;
			int index_pin1 = mesh_input.triangles[index_aux[(pos_firs_out+1)%3]];
			int index_pin2 = mesh_input.triangles[index_aux[(pos_firs_out+2)%3]];
			
			Vector3 pout = mesh_input.vertices[index_pout];
			Vector3 pin1 = mesh_input.vertices[index_pin1];
			Vector3 pin2 = mesh_input.vertices[index_pin2];

			
			listExtraVertexs.Add(Utiles.GetPointRectaParametrica(pin1, pout, pointCenterPlaneCoordinate, axis));
			listExtraVertexs.Add(Utiles.GetPointRectaParametrica(pin2, pout, pointCenterPlaneCoordinate, axis));
		
			// Insert new two triangles
			l_triangles_add.Add(index_pin2);
			l_triangles_add.Add(mesh_input.vertices.Length+listExtraVertexs.Count-1);
			l_triangles_add.Add(mesh_input.vertices.Length+listExtraVertexs.Count-2);
			
			l_triangles_add.Add(index_pin1);
			l_triangles_add.Add(index_pin2);
			l_triangles_add.Add(mesh_input.vertices.Length+listExtraVertexs.Count-2);
			
			// Insert new two uv mapping
			listExtraUVMapping.Add(mesh_input.uv[index_pout]);
			listExtraUVMapping.Add(mesh_input.uv[index_pout]);
		
		// There is a only one point in			
		}else{
			int pos_firs_in  = current_triangle.min_index_t-current_triangle.index_triangle;
			int index_pin   = current_triangle.min_index_v;
			int index_pout1 = mesh_input.triangles[index_aux[(pos_firs_in+1)%3]];
			int index_pout2 = mesh_input.triangles[index_aux[(pos_firs_in+2)%3]];
			
			Vector3 pin =   mesh_input.vertices[index_pin];
			Vector3 pout1 = mesh_input.vertices[index_pout1];
			Vector3 pout2 = mesh_input.vertices[index_pout2];

			
			// Insert two vertexs
			listExtraVertexs.Add(Utiles.GetPointRectaParametrica(pin, pout1, pointCenterPlaneCoordinate, axis));
			listExtraVertexs.Add(Utiles.GetPointRectaParametrica(pin, pout2, pointCenterPlaneCoordinate, axis));
		
			// Insert new triangle
			l_triangles_add.Add(mesh_input.vertices.Length+listExtraVertexs.Count-2);
			l_triangles_add.Add(mesh_input.vertices.Length+listExtraVertexs.Count-1);
			l_triangles_add.Add(index_pin);
			
			// Insert new two uv mapping
			listExtraUVMapping.Add(mesh_input.uv[index_pout1]);
			listExtraUVMapping.Add(mesh_input.uv[index_pout2]);
		}
	}

	public void HideTriangle (TriangleInfoList triangleInfoList,TriangleInfo triangle_actual)
	{
		triangleInfoList.l_triangles_show[triangle_actual.index_triangle]   = 0; 
		triangleInfoList.l_triangles_show[triangle_actual.index_triangle+1] = 0;
		triangleInfoList.l_triangles_show[triangle_actual.index_triangle+2] = 0;
	}

	public void ShowTriangle (Mesh mesh_input, TriangleInfoList triangleInfoList, TriangleInfo current_triangle)
	{
		triangleInfoList.l_triangles_show[current_triangle.index_triangle]   = mesh_input.triangles[current_triangle.index_triangle]; 
		triangleInfoList.l_triangles_show[current_triangle.index_triangle+1] = mesh_input.triangles[current_triangle.index_triangle+1];
		triangleInfoList.l_triangles_show[current_triangle.index_triangle+2] = mesh_input.triangles[current_triangle.index_triangle+2];
	}

	public void CheckIntersection (Mesh mesh_input, TriangleInfoList triangleInfoList, List<int> l_triangles_add)
	{
		
		int i=0;
		
		while(i<triangleInfoList.l_intersection.Count){
			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[triangleInfoList.l_intersection[i]];	
			StateTriangle stateTriangle =  CheckTriangle(current_triangle);
		
			if(stateTriangle==StateTriangle.In){
				ShowTriangle (mesh_input, triangleInfoList, current_triangle);

				triangleInfoList.l_sortBigger_count++;
				triangleInfoList.l_intersection.RemoveAt(i);
			}else if(stateTriangle==StateTriangle.Between){
				
				HideTriangle (triangleInfoList, current_triangle);

				IntersectionTriangles (mesh_input, l_triangles_add, current_triangle);
				i++;
			}else{

				HideTriangle (triangleInfoList, current_triangle);

				if(triangleInfoList.l_intersection[i]<triangleInfoList.l_first_triangle_hide)
					triangleInfoList.l_first_triangle_hide = triangleInfoList.l_intersection[i];
				
				triangleInfoList.l_intersection.RemoveAt(i);
			}
			
		}
	
	}
	
	public void CutMeshUp (Mesh mesh_input, TriangleInfoList triangleInfoList, List<int> l_triangles_add)
	{
		bool notfind = true;
		
		CheckIntersection (mesh_input, triangleInfoList, l_triangles_add);
		
		for(int i=triangleInfoList.l_first_triangle_hide; i<triangleInfoList.l_triangleInfo.Length && notfind; i++){
			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[i];
			
			StateTriangle stateTriangle =  CheckTriangle(current_triangle);
			
			if(stateTriangle==StateTriangle.Out){// don't show anything
				triangleInfoList.l_first_triangle_hide = i;
				notfind = false; 
			}else if(stateTriangle==StateTriangle.In){ // show all

				ShowTriangle (mesh_input, triangleInfoList, current_triangle);
				
				triangleInfoList.l_sortBigger_count++;
				triangleInfoList.l_first_triangle_hide = i+1;


			}else{
				
				HideTriangle (triangleInfoList, current_triangle);

				IntersectionTriangles (mesh_input, l_triangles_add, current_triangle);
				triangleInfoList.l_intersection.Add(i);
			}
			
		}
		
		if(notfind){
		 	triangleInfoList.l_first_triangle_hide = triangleInfoList.l_triangleInfo.Length;	
		}

		
	}

	public void CutMeshDown (Mesh mesh_input, TriangleInfoList triangleInfoList, List<int> l_triangles_add)
	{
		int index_start;
		bool goOn=true;
		
		CheckIntersection (mesh_input, triangleInfoList, l_triangles_add);

		index_start = triangleInfoList.l_sortBigger_count;

		for(int i=index_start; goOn && i>-1; i--){

			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[triangleInfoList.l_sortBigger[i]];

			StateTriangle estadotriangle = CheckTriangle(current_triangle);
			
			if(estadotriangle==StateTriangle.Out){// don't show anything
				
				HideTriangle (triangleInfoList, current_triangle);
		
				triangleInfoList.l_sortBigger_count--;
				
				if(triangleInfoList.l_sortBigger[i]<triangleInfoList.l_first_triangle_hide)
					triangleInfoList.l_first_triangle_hide = triangleInfoList.l_sortBigger[i];
				
			}else if(estadotriangle==StateTriangle.In){ // show all
				goOn = false;
				
			}else{
				
				triangleInfoList.l_sortBigger_count--;
				
				HideTriangle (triangleInfoList, current_triangle);

				IntersectionTriangles (mesh_input, l_triangles_add, current_triangle);
				triangleInfoList.l_intersection.Add(triangleInfoList.l_sortBigger[i]);
				
			}
			
		}


	}
	
	public void CutMesh (Mesh mesh_input, Mesh mesh_output, TriangleInfoList triangleInfoList)
	{
		List<Vector3> l_joinVertex;
		List<Vector2> l_joinUV;
		List<int> l_joinTriangles;
		List<int> l_triangles_add = new List<int>(100);
		listExtraVertexs = new List<Vector3>(100);
		listExtraUVMapping = new List<Vector2>(100);
		
		
		mesh_output.Clear();

		
		if(upPlane){
			CutMeshUp(mesh_input, triangleInfoList, l_triangles_add);
		}else{
			CutMeshDown(mesh_input, triangleInfoList, l_triangles_add);
		}
		
		l_joinVertex = new List<Vector3>(mesh_input.vertices.Length+listExtraVertexs.Count);
		l_joinUV = new List<Vector2>(mesh_input.uv.Length+listExtraUVMapping.Count);
		l_joinTriangles = new List<int>(triangleInfoList.l_triangles_show.Length+l_triangles_add.Count);
		
		l_joinVertex.AddRange(mesh_input.vertices);
		l_joinVertex.AddRange(listExtraVertexs);
		
		l_joinUV.AddRange(mesh_input.uv);
		l_joinUV.AddRange(listExtraUVMapping);
		
		l_joinTriangles.AddRange(triangleInfoList.l_triangles_show);
		l_joinTriangles.AddRange(l_triangles_add);
		
		mesh_output.vertices = l_joinVertex.ToArray(); 
		mesh_output.triangles = l_joinTriangles.ToArray();
		mesh_output.uv = l_joinUV.ToArray();

		
		mesh_output.RecalculateNormals();
		mesh_output.RecalculateBounds();
		
		l_joinVertex.Clear();
		l_joinTriangles.Clear();
		l_joinUV.Clear();
		
	}

	public void SetHideEveryTriangle (Mesh mesh_output, TriangleInfoList triangleInfoList)
	{
		for(int j=0; j<triangleInfoList.l_intersection.Count; j++){
			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[triangleInfoList.l_intersection[j]];
			HideTriangle (triangleInfoList, current_triangle);
		
		}

		for(int j=triangleInfoList.l_sortBigger_count; j>-1; j--){
			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[triangleInfoList.l_sortBigger[j]];
			HideTriangle (triangleInfoList, current_triangle);
		
		}
		
		mesh_output = null;	

		triangleInfoList.l_first_triangle_hide = 0;
		triangleInfoList.l_sortBigger_count = -1;
		
		triangleInfoList.l_intersection.Clear();

	}

	public void SetShowEveryTriangle (Mesh mesh_input, Mesh mesh_output, TriangleInfoList triangleInfoList)
	{

		for(int j=0; j<triangleInfoList.l_intersection.Count; j++){
			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[triangleInfoList.l_intersection[j]];
			
			ShowTriangle (mesh_input, triangleInfoList, current_triangle);
		}
		
		for(int j=triangleInfoList.l_first_triangle_hide; j<triangleInfoList.l_triangleInfo.Length; j++){
			TriangleInfo current_triangle = triangleInfoList.l_triangleInfo[j];
			
			ShowTriangle (mesh_input, triangleInfoList, current_triangle);
		}

		
		triangleInfoList.l_intersection.Clear();

		mesh_output = mesh_input; 
		triangleInfoList.l_first_triangle_hide = triangleInfoList.l_triangleInfo.Length;
		triangleInfoList.l_sortBigger_count = triangleInfoList.l_triangleInfo.Length-1;

	}	
	
	public void ExecuteCutMesh(){
	
		float b_min, b_max;
		
		if(PercentageCutBefore<PercentageCut){
			upPlane = true;
		}else{
			upPlane = false;
		}
		
		PercentageCutBefore = PercentageCut;
		
		for(int i=0; i<arrayOriginalMesh.Length; i++){
			
			
				b_max = arrayOriginalMesh[i].bounds.max[(int)axis];
				b_min = arrayOriginalMesh[i].bounds.min[(int)axis];
			
				if(percentageCut==0 || b_min>pointCenterPlaneCoordinate){

					if(listMeshFilter[i].mesh!=null){
						SetHideEveryTriangle (listMeshFilter[i].mesh, listTrianglesInfo[i]);
						listMeshFilter[i].mesh = null;
					}

				}else if(percentageCut==1 || b_max<pointCenterPlaneCoordinate){
				
					if(listMeshFilter[i].mesh != arrayOriginalMesh[i]){
						SetShowEveryTriangle (arrayOriginalMesh[i], listMeshFilter[i].mesh,listTrianglesInfo[i]);
						listMeshFilter[i].mesh = arrayOriginalMesh[i]; 
					}
				}else{

					CutMesh(arrayOriginalMesh[i], listMeshFilter[i].mesh, listTrianglesInfo[i]);
				
				}
		}
	}	
	
	// Use this for initialization
	void Start () {
		
		if(isInit){
			GeneratePlane();
			ExecuteCutMesh();
		}
	}


	void OnGUI(){

		if(isActiveGui){
			float slider = GUI.HorizontalSlider(new Rect(10.0f, 10.0f, 300.0f, 40.0f), this.percentageCut, 0.0f, 1.0f);
			if(slider!=this.percentageCut){
				this.percentageCut = slider;
				this.GeneratePlane();
				this.ExecuteCutMesh();
			}
		}
	}

}













