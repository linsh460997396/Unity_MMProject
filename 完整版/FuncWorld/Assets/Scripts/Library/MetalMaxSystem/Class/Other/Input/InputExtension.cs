#if UNITY_EDITOR || UNITY_STANDALONE

using System;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

namespace MetalMaxSystem.Unity
{
    public partial class @InputSystem: IInputActionCollection2, IDisposable
    {
        //处理玩家输入

        /// <summary>
        /// 玩家动作输入.
        /// </summary>
        public InputSystem.PlayerActions iapa; //不能在这直接new(会被覆盖)
        /// <summary>
        /// 键盘 W/UP
        /// </summary>
        public bool playerKBMovingUp;
        /// <summary>
        /// 键盘 S/Down
        /// </summary>
        public bool playerKBMovingDown;
        /// <summary>
        /// 键盘 A/Left
        /// </summary>
        public bool playerKBMovingLeft;
        /// <summary>
        /// 键盘 D/Right
        /// </summary>
        public bool playerKBMovingRight;

        //主要用下面这几个

        /// <summary>
        /// 键盘=true,手柄=false
        /// </summary>
        public bool playerUsingKeyboard;
        /// <summary>
        /// 键盘Space或手柄按钮A/X
        /// </summary>
        public bool playerJumping;
        /// <summary>
        /// 是否正在移动(键盘ASDW或手柄左joy均能触发)
        /// </summary>
        public bool playerMoving;
        /// <summary>
        /// 归一化之后的移动方向(读前先判断playerMoving)
        /// </summary>
        public Vector2 playerMoveValue;
        /// <summary>
        /// 上一个非零移动值的备份(比如当前值为0而上次备份值为1时可供变化参考),初值=new(1, 0)
        /// </summary>
        public Vector2 playerLastMoveValue = new(1, 0);
        /// <summary>
        /// 获取玩家朝向.若移动中,获取归一化后的移动方向；不在移动,则返回上一个非零移动值的备份
        /// </summary>
        public Vector2 PlayerDirection
        {
            get
            {
                if (playerMoving)
                {//若移动中,获取归一化后的移动方向
                    return playerMoveValue;
                }
                else
                {//不在移动,则返回上一个非零移动值的备份
                    return playerLastMoveValue;
                }
            }
        }

        /// <summary>
        /// [常数]根号二大小
        /// </summary>
        public const float sqrt2 = 1.414213562373095f;
        /// <summary>
        /// [常数]根号二大小的一半
        /// </summary>
        public const float sqrt2_1 = 0.7071067811865475f;

        /// <summary>
        /// 初始化输入动作
        /// </summary>
        public void InitInputAction()
        {
            iapa = Player;//绑定输入动作玩家
            iapa.Enable();//启用

            //↓记录各种玩家操作事件下的状态值

            // keyboard键盘
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

            // gamepad游戏手柄
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
        /// <summary>
        /// 处理玩家输入(只是填充playerMoving等状态值)
        /// </summary>
        public void HandlePlayerInput()
        {
            if (playerUsingKeyboard)
            {//使用键盘情况:需每帧判断、合并方向,计算最终矢量
                if (!playerKBMovingUp && !playerKBMovingDown && !playerKBMovingLeft && !playerKBMovingRight
                    || playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 0f; //采用X-Z平面时此y用于z轴变化
                    playerMoving = false;
                }
                else if (!playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = -1f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && !playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 1f;
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
                    playerMoveValue.y = sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingRight)
                {
                    playerMoveValue.x = sqrt2_1;
                    playerMoveValue.y = sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown && playerKBMovingLeft)
                {
                    playerMoveValue.x = -sqrt2_1;
                    playerMoveValue.y = -sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown && playerKBMovingRight)
                {
                    playerMoveValue.x = sqrt2_1;
                    playerMoveValue.y = -sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingUp)
                {
                    playerMoveValue.x = 0;
                    playerMoveValue.y = 1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown)
                {
                    playerMoveValue.x = 0;
                    playerMoveValue.y = -1;
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
            }
            else
            {//手柄不需要判断
                var v = iapa.GPMove.ReadValue<Vector2>();
                //v.Normalize();//归一化
                playerMoveValue.x = v.x;
                playerMoveValue.y = v.y;
                //todo:playerMoving = 距离 > 死区长度 ？
            }
            if (playerMoving)
            {//若移动成功
             //记录最后一次移动方向
                playerLastMoveValue = playerMoveValue;
            }
        }
    }
}

#endif
