using UnityEngine;

// inherit from this class if you don'transform want to use the default events

namespace CellSpace
{

    public class CellEvents : MonoBehaviour
    {

        public virtual void OnMouseDown(int mouseButton, CellInfo cellInfo)
        {

        }

        public virtual void OnMouseUp(int mouseButton, CellInfo cellInfo)
        {

        }

        public virtual void OnMouseHold(int mouseButton, CellInfo cellInfo)
        {

        }

        public virtual void OnLook(CellInfo cellInfo)
        {

        }

        public virtual void OnBlockPlace(CellInfo cellInfo)
        {

        }
        public virtual void OnBlockPlaceMultiplayer(CellInfo cellInfo, NetworkPlayer sender)
        {

        }
        public virtual void OnBlockPlaceMultiplayerWebSocket(CellInfo cellInfo, string senderId)
        {

        }

        public virtual void OnBlockDestroy(CellInfo cellInfo)
        {

        }
        public virtual void OnBlockDestroyMultiplayer(CellInfo cellInfo, NetworkPlayer sender)
        {

        }
        public virtual void OnBlockDestroyMultiplayerWebSocket(CellInfo cellInfo, string senderId)
        {

        }

        public virtual void OnBlockChange(CellInfo cellInfo)
        {

        }
        public virtual void OnBlockChangeMultiplayer(CellInfo cellInfo, NetworkPlayer sender)
        {

        }
        public virtual void OnBlockChangeMultiplayerWebSocket(CellInfo cellInfo, string senderId)
        {

        }

        public virtual void OnBlockEnter(GameObject enteringObject, CellInfo cellInfo)
        {

        }

        public virtual void OnBlockStay(GameObject stayingObject, CellInfo cellInfo)
        {

        }
    }

}