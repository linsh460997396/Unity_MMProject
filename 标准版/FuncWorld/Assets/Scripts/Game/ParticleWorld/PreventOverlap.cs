using UnityEngine;

namespace ParticleWorld
{
    public class PreventOverlap : MonoBehaviour
    {
        //private Rigidbody2D a;

        void Start()
        {
            //a = GetComponent<Rigidbody2D>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // 获取碰撞的接触点
            ContactPoint2D[] contactPoints = collision.contacts;

            foreach (ContactPoint2D contact in contactPoints)
            {
                // 计算碰撞点到当前物体中心的距离
                Vector2 direction = contact.point - new Vector2(transform.position.x, transform.position.y);
                Vector3 direction3D = new Vector3(direction.x, direction.y, transform.position.z);
                // 将物体推开,使其不进入另一个物体的内部
                transform.position += direction3D;
            }
        }
    }
}