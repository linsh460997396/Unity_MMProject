using UnityEngine;

namespace SimWorld
{
    partial class Main_SimWorld
    {

        // 处理玩家输入
        internal InputActions.PlayerActions iapa;           // 不能在这直接 new,会为空
        internal bool playerKBMovingUp;                     // 键盘 W/UP
        internal bool playerKBMovingDown;                   // 键盘 S/Down
        internal bool playerKBMovingLeft;                   // 键盘 A/Left
        internal bool playerKBMovingRight;                  // 键盘 D/Right
                                                            // 主要用下面这几个
        internal bool playerUsingKeyboard;                  // false: 手柄
        internal bool playerJumping;                        // 键盘 Space 或 手柄按钮 A / X
        internal bool playerMoving;                         // 是否正在移动( 键盘 ASDW 或 手柄左 joy 均能触发 )
        internal Vector2 playerMoveValue;                   // 归一化之后的移动方向( 读前先判断 playerMoving )
        internal Vector2 playerLastMoveValue = new(1, 0);   // 上一个非 0 移动值的备份( 当前值若为 0, 该值可供参考 )
        internal Vector2 playerDirection
        {                  // 获取玩家朝向
            get
            {
                if (playerMoving)
                {
                    return playerMoveValue;
                }
                else
                {
                    return playerLastMoveValue;
                }
            }
        }

        internal void InitInputAction()
        {
            var ia = new InputActions();
            iapa = ia.Player;
            iapa.Enable();

            // keyboard
            iapa.KBJump.started += c =>
            {
                playerUsingKeyboard = true;
                playerJumping = true;
            };
            iapa.KBJump.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerJumping = false;
            };

            iapa.KBMoveUp.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingUp = true;
            };
            iapa.KBMoveUp.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingUp = false;
            };

            iapa.KBMoveDown.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingDown = true;
            };
            iapa.KBMoveDown.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingDown = false;
            };

            iapa.KBMoveLeft.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingLeft = true;
            };
            iapa.KBMoveLeft.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingLeft = false;
            };

            iapa.KBMoveRight.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingRight = true;
            };
            iapa.KBMoveRight.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingRight = false;
            };

            // gamepad
            iapa.GPJump.started += c =>
            {
                playerUsingKeyboard = false;
                playerJumping = true;
            };
            iapa.GPJump.canceled += c =>
            {
                playerUsingKeyboard = false;
                playerJumping = false;
            };

            iapa.GPMove.started += c =>
            {
                playerUsingKeyboard = false;
                playerMoving = true;
            };
            iapa.GPMove.performed += c =>
            {
                playerUsingKeyboard = false;
                playerMoving = true;
            };
            iapa.GPMove.canceled += c =>
            {
                playerUsingKeyboard = false;
                playerMoving = false;
            };
        }

        internal void HandlePlayerInput()
        {
            if (playerUsingKeyboard)
            {      // 键盘需要每帧判断, 合并方向, 计算最终矢量
                if (!playerKBMovingUp && !playerKBMovingDown && !playerKBMovingLeft && !playerKBMovingRight
                    || playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 0f;
                    playerMoving = false;
                }
                else if (!playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 1f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && !playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = -1f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingDown && !playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 1f;
                    playerMoveValue.y = 0f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && !playerKBMovingRight)
                {
                    playerMoveValue.x = -1f;
                    playerMoveValue.y = 0f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingDown
                      || playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 0f;
                    playerMoving = false;
                }
                else if (playerKBMovingUp && playerKBMovingLeft)
                {
                    playerMoveValue.x = -sqrt2_1;
                    playerMoveValue.y = -sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingRight)
                {
                    playerMoveValue.x = sqrt2_1;
                    playerMoveValue.y = -sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown && playerKBMovingLeft)
                {
                    playerMoveValue.x = -sqrt2_1;
                    playerMoveValue.y = sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown && playerKBMovingRight)
                {
                    playerMoveValue.x = sqrt2_1;
                    playerMoveValue.y = sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingUp)
                {
                    playerMoveValue.x = 0;
                    playerMoveValue.y = -1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown)
                {
                    playerMoveValue.x = 0;
                    playerMoveValue.y = 1;
                    playerMoving = true;
                }
                else if (playerKBMovingLeft)
                {
                    playerMoveValue.x = -1;
                    playerMoveValue.y = 0;
                    playerMoving = true;
                }
                else if (playerKBMovingRight)
                {
                    playerMoveValue.x = 1;
                    playerMoveValue.y = 0;
                    playerMoving = true;
                }
                //if (playerMoving) {
                //    Debug.Log(playerKBMovingUp + " " + playerKBMovingDown + " " + playerKBMovingLeft + " " + playerKBMovingRight + " " + playerMoveValue);
                //}
            }
            else
            {    // 手柄不需要判断
                var v = iapa.GPMove.ReadValue<Vector2>();
                //v.Normalize();
                playerMoveValue.x = v.x;
                playerMoveValue.y = -v.y;
                // todo: playerMoving = 距离 > 死区长度 ?
            }
            if (playerMoving)
            {
                playerLastMoveValue = playerMoveValue;
            }
        }
    }
}