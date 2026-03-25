using System.Collections.Generic;
using UnityEngine;

namespace MeshCutting
{
    public class ExploreSlice : MonoBehaviour
    {

        public GameObject plane;
        //public Transform ObjectContainer;

        // How far away from the slice do we separate resulting objects
        public float separation;

        // Do we draw a plane object associated with the slice
        private Plane slicePlane = new Plane();
        public bool drawPlane;

        // Reference to the line renderer
        //public ScreenLineRenderer lineRenderer;

        public MeshCutter meshCutter;
        private TempMesh biggerMesh, smallerMesh;
        private List<Transform> CutterMeshList = new List<Transform>();

        #region Utility Functions

        void DrawPlane(Vector3 start, Vector3 end, Vector3 normalVec)
        {
            Quaternion rotate = Quaternion.FromToRotation(Vector3.up, normalVec);

            plane.transform.localRotation = rotate;
            plane.transform.position = (end + start) / 2;
            plane.SetActive(true);
        }

        #endregion

        public ExploreSlice()
        {
            // Initialize a somewhat big array so that it doesn'transform resize
            meshCutter = new MeshCutter(256);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 获取鼠标点击的屏幕坐标
                Vector3 mousePos = Input.mousePosition;
                Vector3 rayOrigin = Camera.main.transform.position; // 射线的起点:摄像机位置
                Vector3 rayDirection = Camera.main.ScreenPointToRay(mousePos).direction; // 射线的方向:从摄像机到鼠标点击

                // 投射射线
                Ray ray = new Ray(rayOrigin, rayDirection);
                RaycastHit hit;
                float rayLength = 10000f; // 射线的长度,可以根据需要调整
                LayerMask layerMask = LayerMask.GetMask("Default"); // 射线投射的目标层

                if (Physics.Raycast(ray, out hit, rayLength, layerMask))
                {
                    // 射线击中了物体
                    Debug.Log("Hit object: " + hit.transform.name);
                    Explore(hit.transform.gameObject, ray.direction);
                }
                else
                {
                    // 射线没有击中任何物体
                    Debug.Log("No object hit");
                }
                // 使用Debug.DrawRay在Scene视图中绘制射线
                Debug.DrawRay(rayOrigin, rayDirection * rayLength, Color.red, 0.5f); // 红色射线,持续0.5秒
            }
        }



        public void Explore(GameObject obj, Vector3 dir)
        {
            CutterMeshList.Clear();
            CutterMeshList.Add(obj.transform);
            Vector3 center = obj.transform.position;
            for (int i = 0; i < 7; i++)
            {
                int count = CutterMeshList.Count;
                for (int j = 0; j < count; j++)
                {
                    Bounds bounds = CutterMeshList[j].GetComponent<MeshRenderer>().bounds;
                    Vector3 startPoint = Random.insideUnitSphere;
                    Vector3 endPoint = Random.insideUnitSphere;
                    OnLineDrawn(bounds.center + startPoint, bounds.center + endPoint, dir, CutterMeshList[j].gameObject);
                }
            }
            for (int i = 0; i < CutterMeshList.Count; ++i)
            {
                Rigidbody rigidbody = CutterMeshList[i].GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = CutterMeshList[i].gameObject.AddComponent<Rigidbody>();
                }
                float ExplosionX = Random.Range(-100, 100);
                float ExplosionY = Random.Range(-100, 100);
                float ExplosionZ = Random.Range(-100, 100);
                rigidbody.AddForce(new Vector3(ExplosionX, ExplosionY, ExplosionZ).normalized * 7, ForceMode.Impulse);
            }
        }

        //private void OnEnable()
        //{
        //    lineRenderer.OnLineDrawn += OnLineDrawn;
        //}

        //private void OnDisable()
        //{
        //    lineRenderer.OnLineDrawn -= OnLineDrawn;
        //}

        private void OnLineDrawn(Vector3 start, Vector3 end, Vector3 depth, GameObject obj)
        {
            var planeTangent = (end - start).normalized;

            // if we didn'transform drag, we set tangent to be on pixelX
            if (planeTangent == Vector3.zero)
                planeTangent = Vector3.right;

            var normalVec = Vector3.Cross(depth, planeTangent);

            if (drawPlane) DrawPlane(start, end, normalVec);

            SliceObjects(start, normalVec, obj);
        }


        void SliceObjects(Vector3 point, Vector3 normal, GameObject obj)
        {
            // Put results in positive and negative array so that we separate all meshes if there was a cut made
            List<Transform> positive = new List<Transform>(),
                negative = new List<Transform>();

            bool slicedAny = false;

            // We multiply by the inverse transpose of the worldToLocal Matrix, a.k.a the transpose of the localToWorld Matrix
            // Since this is how normal are transformed
            var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

            //Convert plane in object's local frame
            slicePlane.SetNormalAndPosition(
                transformedNormal,
                obj.transform.InverseTransformPoint(point));

            slicedAny = SliceObject(ref slicePlane, obj, positive, negative) || slicedAny;


            // Separate meshes if a slice was made
            if (slicedAny)
                SeparateMeshes(positive, negative, normal);
        }

        bool SliceObject(ref Plane slicePlane, GameObject obj, List<Transform> positiveObjects, List<Transform> negativeObjects)
        {
            var mesh = obj.GetComponent<MeshFilter>().mesh;

            if (!meshCutter.SliceMesh(mesh, ref slicePlane))
            {
                // Put object in the respective list
                if (slicePlane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
                    positiveObjects.Add(obj.transform);
                else
                    negativeObjects.Add(obj.transform);

                return false;
            }

            // TODO: Update center of mass

            // Silly condition that labels which mesh is bigger to keep the bigger mesh in the original gameobject
            bool posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
            if (posBigger)
            {
                biggerMesh = meshCutter.PositiveMesh;
                smallerMesh = meshCutter.NegativeMesh;
            }
            else
            {
                biggerMesh = meshCutter.NegativeMesh;
                smallerMesh = meshCutter.PositiveMesh;
            }

            // Create new Sliced object with the other mesh
            GameObject newObject = Instantiate(obj, null);
            newObject.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
            var newObjMesh = newObject.GetComponent<MeshFilter>().mesh;
            CutterMeshList.Add(newObject.transform);

            // Put the bigger mesh in the original object
            // TODO: Enable collider generation (either the exact mesh or compute smallest enclosing sphere)
            ReplaceMesh(mesh, biggerMesh);
            ReplaceMesh(newObjMesh, smallerMesh);

            (posBigger ? positiveObjects : negativeObjects).Add(obj.transform);
            (posBigger ? negativeObjects : positiveObjects).Add(newObject.transform);

            return true;
        }


        /// <summary>
        /// Replace the mesh with tempMesh.
        /// </summary>
        void ReplaceMesh(Mesh mesh, TempMesh tempMesh, MeshCollider collider = null)
        {
            mesh.Clear();
            mesh.SetVertices(tempMesh.vertices);
            mesh.SetTriangles(tempMesh.triangles, 0);
            mesh.SetNormals(tempMesh.normals);
            mesh.SetUVs(0, tempMesh.uvs);

            //mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            if (collider != null && collider.enabled)
            {
                collider.sharedMesh = mesh;
                collider.convex = true;
            }
        }

        void SeparateMeshes(Transform posTransform, Transform negTransform, Vector3 localPlaneNormal)
        {
            // Bring back normal in world space
            Vector3 worldNormal = ((Vector3)(posTransform.worldToLocalMatrix.transpose * localPlaneNormal)).normalized;

            Vector3 separationVec = worldNormal * separation;
            // Transform direction in world coordinates
            posTransform.position += separationVec;
            negTransform.position -= separationVec;
        }

        void SeparateMeshes(List<Transform> positives, List<Transform> negatives, Vector3 worldPlaneNormal)
        {
            int i;
            var separationVector = worldPlaneNormal * separation;

            for (i = 0; i < positives.Count; ++i)
                positives[i].transform.position += separationVector;

            for (i = 0; i < negatives.Count; ++i)
                negatives[i].transform.position -= separationVector;
        }
    }
}