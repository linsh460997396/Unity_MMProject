using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    //��һ�˳ƿ�����
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField][Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        /// <summary>
        /// ��Ծ״̬
        /// </summary>
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>(); //��ȡ��ɫ������ʵ��
            m_Camera = Camera.main;//��ȡ�������
            m_OriginalCameraPosition = m_Camera.transform.localPosition;//��ȡ��������ı�������
            m_FovKick.Setup(m_Camera);//��Ұ��Field of View, FOV���ġ��߶�����仯Ч�����������Ϸ��������Ҫģ����������������Ϸ�У�FOV�߶���һ�ֳ����ļ���������ģ����������ʱ���ߵ��𶯻�ƫ�ơ�
            m_HeadBob.Setup(m_Camera, m_StepInterval);//��·ʱ��ͷ���ζ�Ч��
            m_StepCycle = 0f;//���ڸ���ͷ���ζ������ڻ����
            m_NextStep = m_StepCycle / 2f;//����ȷ����һ�λζ���ʱ����������Ϊ m_StepCycle ��һ���������Ϊ����ĳ��ʱ��㴥���ζ�Ч�������統ǰ�ζ����ڵ�һ��ʱ����� m_StepCycle ���������ٵ�ǰ�ζ����ڵĻ�����ô m_NextStep ��������ȷ����ʱ��ʼ��һ���ζ�����
            m_Jumping = false;//��Ծ״̬
            m_AudioSource = GetComponent<AudioSource>();//��ȡ��Ƶ�����ʵ��
            m_MouseLook.Init(transform, m_Camera.transform);//���ע�ӹ��ܵĳ�ʼ��
        }


        /// <summary>
        /// �����ɫ����Ծ����½�Ϳ����ƶ���Ϊ��ȷ����ɫ�ڲ�ͬ��״̬������ȷ����Ӧ�Ͷ���
        /// </summary>
        private void Update()
        {
            RotateView(); //��ת����½�ɫ���ӽ�

            // the jump state needs to read here to make sure it is not missed

            //���m_Jump�����Ƿ�Ϊfalse������ǣ�����ʹ��CrossPlatformInputManager���������Ƿ����ˡ�Jump����ť�������Ұ�������Ծ��ť��m_Jump�ᱻ����Ϊtrue
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
            //������½���
            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            { //����ɫ�Ƿ�ոմӷ���½״̬��Ϊ��½״̬������ǣ�����ִ�����¶���
                StartCoroutine(m_JumpBob.DoBobCycle()); //ʹ��Э��������һ����Ծ��Ķ�������
                PlayLandingSound();//������½��Ч
                m_MoveDir.y = 0f;//��m_MoveDir.y����Ϊ0f��������ζ�Ž�ɫ����½ʱֹͣ��ֱ�ƶ�
                m_Jumping = false;//��m_Jumping����Ϊfalse����ʾ��ɫ��ǰ������Ծ״̬
            }
            //�����ɫ�ڿ���ʱ���ƶ�
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                //����ɫ�Ƿ��ڿ��У�������½״̬������û������Ծ����֮ǰ������½״̬����������Ὣm_MoveDir.y����Ϊ0f����ζ�Ž�ɫ�ڿ���ʱ������д�ֱ�ƶ�
                m_MoveDir.y = 0f;
            }
            //������½״̬��m_PreviouslyGrounded����ʹ����m_CharacterController.isGrounded��ֵ��ͬ
            //m_CharacterController.isGrounded�Ǳ�ʾ��ɫ�Ƿ���½�Ĳ���ֵ������һ��Update����ʱ��m_PreviouslyGrounded��������һ��Updateʱ����½״̬
            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }

        // ��δ�����һ����Ϊ FixedUpdate ��˽�з�����ͨ���� Unity ����Ϸ����������������µĹ̶�Ƶ�ʵ��á��Ӵ���Ľṹ���߼����������������Ҫ���ڿ���һ����ɫ���ƶ�����Ծ��Ϊ�����������ӽǺ���ײ״̬�������ǶԴ������ϸ�����

        // ��ʼ������:

        // speed: ��ɫ���ƶ��ٶȡ�
        // desiredMove: �������������������ƶ�����
        // ��ȡ����:

        // ���� GetInput ������ȡ��ҵ����룬��������洢�� speed �����С�
        // �����������ƶ�����:

        // ����������ͽ�ɫ�ĳ��򣬼����һ���������ƶ����� desiredMove��
        // RaycastHit ��ȡ������Ϣ:

        // ʹ�� Physics.SphereCast ����һ���������ߣ�����ɫ�·��Ƿ��п���ײ�ı��档
        // �����⵽���棬�� desiredMove ͶӰ����������ϣ�ȷ����ɫ������������ƶ���
        // �����ƶ�����:

        // ����ͶӰ��� desiredMove ����ҵ��ƶ��ٶ� speed������ m_MoveDir������ɫʵ�ʵ��ƶ����򣩡�
        // �������״̬����Ծ:

        // �����ɫվ���ڵ����� (m_CharacterController.isGrounded Ϊ��)��
        // ����ʩ��һ����С���� (m_StickToGroundForce)��ʹ��ɫ�������档
        // �����Ұ�����Ծ�� (m_Jump Ϊ��)�����������ϵ���Ծ����������Ծ��������������Ծ״̬��
        // �����ɫ���ڵ����ϣ���������Ӱ�죬�������µ�����
        // �ƶ���ɫ��������ײ:

        // ʹ�� m_CharacterController.Move �����ƶ���ɫ��������ʱ�䲽�� (Time.fixedDeltaTime)��
        // ��ȡ���洢��ײ��Ϣ (m_CollisionFlags)��
        // ��������״̬:

        // ���� ProgressStepCycle �������������ڸ��½�ɫ�Ĳ����򶯻�״̬��
        // ���� UpdateCameraPosition ���������½�ɫ���ӽǻ����λ�á�
        // ���� m_MouseLook.UpdateCursorLock �����������������״̬���������ڵ�һ�˳��ӽǵĿ��ơ�
        // ������ԣ���� FixedUpdate �����ǿ��ƽ�ɫ�ƶ�����Ծ��Ϊ�ĺ��ģ�ͬʱ�����뻷�������������Ӿ�������


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }

        /// <summary>
        /// ��ת�ӽ�
        /// </summary>
        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}

// ��δ�����Unity�е�һ�˳ƿ�������First Person Controller����ʵ�֣����ڴ�����ҵ��ƶ�����Ծ���ӽ���ת�Լ�����һЩ����������ʹ����Unity�����������API��
// �����ǶԴ�����Ҫ���ֵĽ��ͣ�
// �����ռ䣨Namespace����UnityStandardAssets.Characters.FirstPerson ��ʾ����ű�����Unity��׼��Դ���еĵ�һ�˳ƽ�ɫ��������
// �ඨ�壨Class Definition����FirstPersonController ��̳��� MonoBehaviour������ζ������һ��Unity��������Ը��ӵ���Ϸ�����ϡ�
// ���ԣ�Properties����
// m_IsWalking������Ƿ��������ߡ�
// m_WalkSpeed �� m_RunSpeed��������ߺͱ��ܵ��ٶȡ�
// m_RunstepLenghten������ʱÿһ���ĳ��ȡ�
// m_JumpSpeed�������Ծ�ĳ��ٶȡ�
// m_StickToGroundForce��������������������ɫ��Ծʱ����ֵʹ�������أ�����б��������ʱͨ���������ֵ��ʹ��ɫ����������б�»�����
// m_GravityMultiplier���������������ڵ�������ܵ�������Ӱ�졣
// m_MouseLook�����ڴ�������ӽ���ת�������
// m_UseFovKick �� m_FovKick���Ƿ�ʹ����Ұ����Ч�����Լ���Ұ�����Ĳ�����
// m_UseHeadBob��m_HeadBob �� m_JumpBob���Ƿ�ʹ��ͷ������Ч�����Լ�ͷ�������Ĳ�����
// m_StepInterval��ÿһ��֮���ʱ������
// m_FootstepSounds���Ų������飬���ѡ�񲥷š�
// m_JumpSound �� m_LandSound����Ծ�����ʱ��������
// ˽�б�����Private Variables����
// m_Camera����ҵ������
// m_Jump��һ����ǣ���ʾ����Ƿ�Ӧ����Ծ��
// m_YRotation����ҵ�Y����ת��ͨ�������ӽǣ���
// m_Input��������루���磬���̺�������룩��
// m_MoveDir����ҵ��ƶ�����
// m_CharacterController��CharacterController ��������ڴ���������ײ���ƶ���
// m_CollisionFlags����ײ��־�����ڼ���뻷������ײ��
// m_PreviouslyGrounded��һ����ǣ���ʾ�����һ֡�Ƿ��ڵ����ϡ�
// m_OriginalCameraPosition�����ԭʼλ�ã����ڴ���ͷ��������
// m_StepCycle �� m_NextStep�����ڴ��������ڵı�����
// m_Jumping��һ����ǣ���ʾ��ҵ�ǰ�Ƿ�����Ծ��
// m_AudioSource����ƵԴ��������ڲ���������