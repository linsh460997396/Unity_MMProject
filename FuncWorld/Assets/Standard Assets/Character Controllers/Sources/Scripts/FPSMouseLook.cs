using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class FPSMouseLook : MonoBehaviour
{
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationY = 0F;

    void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }
    }

    void Start()
    {
        // Make the rigid body not change rotation.使刚体不改变旋转
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }
}

/// FPSMouseLook rotates the transform based on the mouse delta.根据鼠标增量旋转变换
/// Minimum and Maximum values can be used to constrain the possible rotation.可以使用最小值和最大值来限制可能的旋转

/// To make an FPS style character（创建一个FPS风格的角色):
/// - Create a capsule.创建一个胶囊
/// - Add the FPSMouseLook script to the capsule.添加MouseLook脚本到胶囊
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it).设置鼠标外观以使用LookX,注视用十字图标点击对象(你只需要转动角色而不是倾斜角色)
/// - Add FPSInputController script to the capsule.添加FPS输入控制器脚本到胶囊
///   -> A CharacterMotor and a CharacterController component will be automatically added.一个CharacterMotor和一个角色控制器组件将被自动添加

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.创建一个摄像头将相机设置为胶囊的子组件并重置它的变换
/// - Add a FPSMouseLook script to the camera.添加一个MouseLook脚本到摄像机
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)设置鼠标外观使用LookY(你想让相机像头一样上下倾斜,角色已经转身了)