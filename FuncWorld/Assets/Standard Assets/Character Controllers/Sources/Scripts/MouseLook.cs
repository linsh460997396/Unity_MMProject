using UnityEngine;

/// MouseLook rotates the transform based on the mouse delta.�������������ת�任
/// Minimum and Maximum values can be used to constrain the possible rotation.����ʹ����Сֵ�����ֵ�����ƿ��ܵ���ת

/// To make an FPS style character������һ��FPS���Ľ�ɫ��:
/// - Create a capsule.����һ������
/// - Add the MouseLook script to the capsule.���MouseLook�ű�������
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it).������������ʹ��LookX��ע����ʮ��ͼ��������(��ֻ��Ҫת����ɫ��������б��ɫ)
/// - Add FPSInputController script to the capsule.���FPS����������ű�������
///   -> A CharacterMotor and a CharacterController component will be automatically added.һ��CharacterMotor��һ����ɫ��������������Զ����

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.����һ������ͷ���������Ϊ���ҵ���������������ı任
/// - Add a MouseLook script to the camera.���һ��MouseLook�ű��������
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)����������ʹ��LookY(�����������ͷһ��������б����ɫ�Ѿ�ת����)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	float rotationY = 0F;

	void Update ()
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}
	
	void Start ()
	{
        // Make the rigid body not change rotation.ʹ���岻�ı���ת
        if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}
}