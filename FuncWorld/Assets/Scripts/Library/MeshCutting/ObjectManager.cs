using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeshCutting
{
    public class ObjectManager : MonoBehaviour
    {
        public List<MeshRenderer> objects;

        public Dropdown dropdown;

        public Transform ObjectContainer;

        private CameraOrbit cameraOrbit;

        // Start is called before the first frame update
        void Start()
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(objects.Select(r => r.gameObject.name).ToList());

            cameraOrbit = Camera.main.GetComponent<CameraOrbit>();
        }

        public void LoadObject()
        {
            if (dropdown.value >= objects.Count)
                throw new UnityException("Error: selected object is out of range");

            var SelectedObject = objects[dropdown.value];

            // Clear children
            foreach (Transform child in ObjectContainer)
                Destroy(child.gameObject);

            // Load new object in container and set to camera orbit
            cameraOrbit.target = Instantiate(SelectedObject, ObjectContainer).transform;
        }
    }
}