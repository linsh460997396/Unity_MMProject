//������һ��JavaScript

private var motor: CharacterMotor;

// Use this for initialization.ʹ�ñ��ű�����ɳ�ʼ��
function Awake() {
	//motor���ͨ�������ڽű��п��ƽ�ɫ���ƶ�����������
	motor = GetComponent(CharacterMotor);
}

// Update is called once per frame
function Update () {
	// Get the input vector from kayboard or analog stick.�Ӽ��̻�ģ��ҡ�˻������ʸ��
	var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	
	if (directionVector != Vector3.zero) {
		// Get the length of the directon vector and then normalize it.�õ����������ĳ��ȣ�Ȼ�����һ��
		// Dividing by the length is cheaper than normalizing when we already have the length anyway.�������Ѿ��г���ʱ�����Գ��ȱȹ�һ��������
		var directionLength = directionVector.magnitude;
		directionVector = directionVector / directionLength;
		
		// Make sure the length is no bigger than 1.ȷ�����Ȳ�����1
		directionLength = Mathf.Min(1, directionLength);
		
		// Make the input vector more sensitive towards the extremes and less sensitive in the middle.ʹ���������Լ�ֵ�����У������м��������
		// This makes it easier to control slow speeds when using analog sticks.��ʹ��ʹ��ģ��ҡ��ʱ�����׿�������
		directionLength = directionLength * directionLength;
		
		// Multiply the normalized direction vector by the modified length.����һ���������������޸ĺ�ĳ���
		directionVector = directionVector * directionLength;
	}
	
	// Apply the direction to the CharacterMotor.������Ӧ�õ�CharacterMotor
	motor.inputMoveDirection = transform.rotation * directionVector;
	motor.inputJump = Input.GetButton("Jump");
}

// ���� (Update ����):

// ���봦��:
// ʹ�� Input.GetAxis("Horizontal") �� Input.GetAxis("Vertical") �Ӽ��̻�ģ��ҡ�˻�ȡ������������Щֵ��ʾ�����ˮƽ�ʹ�ֱ�����ϵ����루���磬�����ƶ���ǰ���ƶ�����
// �������������Ϊ�㣨����������ƶ�������ִ��һϵ�в����������ͱ�׼������������ʹ����ʺ���Ϸ���ơ�

// ��������:
// �������������ĳ��ȣ�directionLength����
// ��׼������������ʹ�䳤��Ϊ1��
// ȷ�������ĳ��Ȳ�����1��
// ͨ��������ƽ����ʹ�����ڼ���ֵ�ϸ����У������м�ֵ�ϲ�̫���С�����������ʹ��ģ��ҡ��ʱ�����׿��������ƶ���
// ��������ĳ������׼���ķ���������ˣ��õ����յ�����������

// Ӧ������:
// �������������������directionVector�����ɫ����ת��transform.rotation�����ϣ��õ����յ��ƶ�����
// ������ƶ�����ֵ�� motor.inputMoveDirection���Ը��� CharacterMotor �����ɫӦ������ƶ���
// ʹ�� Input.GetButton("Jump") �������Ƿ�������Ծ��ť�����������ֵ�� motor.inputJump���Ը��� CharacterMotor ����Ƿ�Ӧ���ý�ɫ��Ծ��


// Require a character controller to be attached to the same game object.Ҫ���ɫ���������ӵ���ͬ����Ϸ����

//ע��ȷ���˽ű����ӵ���Ϸ��������һ�� CharacterMotor �������û�У�Unity���Զ����һ��
@script RequireComponent(CharacterMotor)
//ע�⽫�˽ű���ӵ�Unity������˵��У�ʹ�������ͨ���༭����ӵ���Ϸ������
@script AddComponentMenu ("Character/FPS Input Controller")
