//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 气泡对话文本系统.
    /// 在单位头顶创建带打字机效果与淡入淡出的对话气泡,同一单位同时仅保留一个气泡(新气泡会打断旧气泡).
    /// 提供Start/Break/End三个生命周期事件,气泡句柄与代际计数器通过DataTable存取.
    /// 定位采用Screen Space Overlay画布 + 每帧WorldToScreenPoint投影,气泡挂在UGUITemplate.Dialog_GameUI画布上.
    /// </summary>
    public static class BubbleTalk
    {
        /// <summary>
        /// 气泡生命周期信息.随三个生命周期事件传递给订阅者.
        /// </summary>
        public sealed class Info
        {
            /// <summary>气泡跟随的单位Transform</summary>
            public Transform followTarget;
            /// <summary>说话者名字</summary>
            public string name;
            /// <summary>对话内容</summary>
            public string text;
            /// <summary>气泡根GameObject</summary>
            public GameObject dialog;
            /// <summary>气泡面板RectTransform</summary>
            public RectTransform panel;
            /// <summary>背景图</summary>
            public Image image;
            /// <summary>文字标签</summary>
            public TextMeshProUGUI label;
            /// <summary>挂点名(仅进阶版)</summary>
            public string refPoint;
            /// <summary>屏幕X偏移(仅进阶版)</summary>
            public int x;
            /// <summary>屏幕Y偏移(仅进阶版)</summary>
            public int y;
            /// <summary>气泡是否被打断(Break时为true,End时为false)</summary>
            public bool interrupted;

            // ===== 内部跟踪字段(事件订阅者无需关注) =====
            internal CanvasGroup canvasGroup;
            internal Transform trackTarget;
            internal GameObject maJia;
            internal Camera worldCamera;
            internal RectTransform canvasRect;
            internal int totalChars;
            /// <summary>代龄计数器在DataTable中的完整key(预构建,协程每帧用Load0读取,避免Load1每帧ThreadStringBuilder.Concat产生GC)</summary>
            internal string tagKey;
            /// <summary>画布是否为Screen Space Overlay(预解析,UpdatePosition每帧复用,避免每帧读Control_GameUICanvas属性)</summary>
            internal bool isOverlay;
        }

        // ===== 生命周期事件 =====
        /// <summary>气泡开始显示时触发</summary>
        public static event Action<Info> OnEventStart;
        /// <summary>气泡被打断时触发(单位失效或被新气泡替换)</summary>
        public static event Action<Info> OnEventBreak;
        /// <summary>气泡正常播完并消失时触发</summary>
        public static event Action<Info> OnEventEnd;

        // ===== 配置常量 =====
        /// <summary>气泡句柄在DataTable中的key前缀</summary>
        private const string DataKey = "MM_BubbleTalk";
        /// <summary>代际计数器在DataTable中的key</summary>
        private const string DataKeyTag = "MM_BubbleTalkTag";
        /// <summary>淡入淡出时长(秒),默认0.25</summary>
        private const float FadeDuration = 0.25f;
        /// <summary>基础版默认气泡宽度</summary>
        private const int DefaultWidth = 600;
        /// <summary>基础版默认气泡高度</summary>
        private const int DefaultHeight = 1200;
        /// <summary>生成气泡的圆角半径(像素,同时作为9宫格边框宽度)</summary>
        private const int DefaultCornerRadius = 16;
        /// <summary>生成气泡的边框线宽(像素,0则无边框)</summary>
        private const int DefaultBorderWidth = 2;
        /// <summary>生成气泡的正方形贴图边长(像素)</summary>
        private const int DefaultTextureSize = 128;
        /// <summary>默认气泡底色(半透明深空黑)</summary>
        private static readonly Color DefaultBubbleBgColor = new Color(0.05f, 0.05f, 0.08f, 0.85f);
        /// <summary>默认气泡边框色(略亮的青灰,提供轮廓定义)</summary>
        private static readonly Color DefaultBubbleBorderColor = new Color(0.4f, 0.45f, 0.5f, 0.9f);
        /// <summary>生成气泡Sprite的缓存(key=底色+边框色+圆角+边框+尺寸),避免同参数重复逐像素生成</summary>
        private static readonly Dictionary<(Color, Color, int, int, int), Sprite> _bubbleSpriteCache = new Dictionary<(Color, Color, int, int, int), Sprite>();

        /// <summary>
        /// 创建气泡对话文本(基础版).固定尺寸600x1200、C#生成圆角气泡、气泡底部居中于单位头顶上方offSet像素处.
        /// </summary>
        /// <param name="followTarget">气泡跟随的单位Transform(对应Galaxy的lp_unit)</param>
        /// <param name="name">说话者名字(为空且nameShow=false时只显示text)</param>
        /// <param name="text">对话内容</param>
        /// <param name="offSet">气泡距单位头顶的Y轴像素偏移(向上为正)</param>
        /// <param name="count">预留参数(Galaxy原版未使用,保持API对齐)</param>
        /// <param name="whiteTime">打字机逐字显示总时长(秒,对应lp_whiteTime)</param>
        /// <param name="waitTime">气泡停留总时长(秒,含淡入,对应lp_waitTime)</param>
        /// <param name="hidden">是否隐藏背景图(true=纯文字,对应lp_hidden)</param>
        /// <param name="nameShow">是否显示"名字：内容"格式(对应lp_nameShow)</param>
        public static void CreateText(Transform followTarget, string name, string text, int offSet, int count, float whiteTime, float waitTime, bool hidden, bool nameShow)
        {
            // 基础版以进阶版实现:固定尺寸/C#生成气泡(默认底色)/无挂点/底部居中定位/Y偏移=offSet/不启用马甲
            // bgColor传null由进阶版走默认底色分支
            CreateTextAdv(followTarget, name, text, count, whiteTime, waitTime, hidden, nameShow,
                null, DefaultWidth, DefaultHeight, 0, offSet, null, false, null);
        }

        /// <summary>
        /// 创建气泡对话文本(进阶版).支持自定义尺寸、贴图、挂点、屏幕偏移与马甲单位.
        /// </summary>
        /// <param name="followTarget">气泡跟随的单位Transform</param>
        /// <param name="name">说话者名字</param>
        /// <param name="text">对话内容</param>
        /// <param name="count">预留参数(Galaxy原版未使用,保持API对齐)</param>
        /// <param name="whiteTime">打字机逐字显示总时长(秒)</param>
        /// <param name="waitTime">气泡停留总时长(秒,含淡入)</param>
        /// <param name="hidden">是否隐藏背景图</param>
        /// <param name="nameShow">是否显示"名字：内容"格式</param>
        /// <param name="refPoint">挂点子物体名称(如"Head"),null或空则跟随单位根Transform</param>
        /// <param name="width">气泡宽度(像素)</param>
        /// <param name="height">气泡高度(像素)</param>
        /// <param name="x">屏幕X偏移(像素)</param>
        /// <param name="y">屏幕Y偏移(像素)</param>
        /// <param name="texturePath">外部背景贴图Resources路径(null或空则用C#生成的圆角气泡,无需任何外部资源)</param>
        /// <param name="enableMaJia">是否启用马甲单位(true=创建中间标记物体)</param>
        /// <param name="bgColor">生成气泡的底色(仅texturePath为空时生效;null使用默认底色;非null时按所给Color生成,允许alpha=0全透明底色)</param>
        public static void CreateTextAdv(Transform followTarget, string name, string text, int count, float whiteTime, float waitTime, bool hidden, bool nameShow, string refPoint, int width, int height, int x, int y, string texturePath, bool enableMaJia, Color? bgColor = null)
        {
            // 1. 合法性校验(对应Galaxy的 UnitIsValid 检查)
            if (followTarget == null) return;

            // 2. 单位唯一标识(对应Galaxy的 lv_unitTag = UnitGetTag(lp_unit))
            //    使用GetInstanceID作为key,即使单位被销毁,int仍可用于DataTable查表
            int unitTag = followTarget.GetInstanceID();

            // 3. 清理该单位已有的气泡(对应Galaxy的 DataTableValueExists + DialogDestroy)
            //    保证同一单位同时仅有一个气泡
            GameObject oldBubble = DataTable<GameObject>.Load1(true, DataKey, unitTag);
            if (oldBubble != null)
            {
                UnityEngine.Object.Destroy(oldBubble);
            }

            // 4. 代际计数器自增(对应Galaxy的 SaveInt1("BubbleTalkTag", tag, Load+1))
            //    新气泡会使旧气泡的tag失效,旧气泡在下次帧检测时走Break分支
            //    预构建完整key(格式与Load1内部ThreadStringBuilder.Concat一致: "BubbleTalk_<unitTag>"),
            //    协程每帧用Load0读取此key,避免Load1每帧重复拼字符串产生GC
            string tagKey = DataKeyTag + "_" + unitTag;
            int tag = DataTable<int>.Load0(true, tagKey) + 1;
            DataTable<int>.Save0(true, tagKey, tag);

            // 5. 解析挂点(对应Galaxy的 lp_ref / Ref_Overhead)
            //    在单位子物体中按名称查找挂点(如"Head"/"Overhead"),找不到则用单位根Transform
            Transform attach = followTarget;
            if (!string.IsNullOrEmpty(refPoint))
            {
                Transform child = followTarget.Find(refPoint);
                if (child != null) attach = child;
            }

            // 6. 马甲单位(对应Galaxy的 DialogLocationMaJia,可选)
            //    Unity中可直接跟随任意Transform,马甲仅作API对齐与显式标记物体之用,销毁时一并清理(对应UnitRemove)
            GameObject maJia = null;
            Transform trackTarget = attach;
            if (enableMaJia)
            {
                maJia = new GameObject("BubbleVest_" + unitTag);
                maJia.transform.SetParent(attach, false);
                maJia.transform.localPosition = Vector3.zero;
                trackTarget = maJia.transform;
            }

            // 7. 构建气泡UI(对应Galaxy的 DialogCreate + DialogControlCreateFromTemplate)
            //    气泡根挂在UGUITemplate.Dialog_GameUI画布(Screen Space Overlay)上
            GameObject bubbleGO = new GameObject("BubbleTalk_" + unitTag);
            bubbleGO.transform.SetParent(UGUITemplate.Dialog_GameUI.transform, false);
            RectTransform bubbleRect = bubbleGO.AddComponent<RectTransform>();
            // 底部居中轴心(0.5,0):气泡向画布上方延伸,底部对齐单位头顶
            bubbleRect.pivot = new Vector2(0.5f, 0f);
            bubbleRect.sizeDelta = new Vector2(width, height);
            bubbleRect.anchoredPosition = Vector2.zero;

            // CanvasGroup:统一控制整体透明度(对应Galaxy的DialogControlFadeTransparency作用于panel)
            CanvasGroup canvasGroup = bubbleGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;             // 起始透明,由协程淡入
            canvasGroup.blocksRaycasts = false; // 气泡不拦截UI交互
            canvasGroup.interactable = false;

            // 7.1 背景图(对应Galaxy的 TestImage + SetDialogItemImage)
            GameObject imageObj = new GameObject("TestImage");
            imageObj.transform.SetParent(bubbleGO.transform, false);
            RectTransform imageRect = imageObj.AddComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero; // 拉伸填充面板
            Image image = imageObj.AddComponent<Image>();
            // 解析背景Sprite:texturePath非空则加载外部Sprite(失败回退到生成),否则用C#逐像素生成9宫格圆角气泡
            Sprite bgSprite = ResolveBubbleSprite(texturePath, bgColor);
            image.sprite = bgSprite;
            // 有九宫格边框用Sliced(对应Galaxy的ImageTypeBorder),否则Simple
            // 注:Sprite.border为Vector4类型(x=左,y=下,z=右,w=上),非Rect
            image.type = (bgSprite.border != Vector4.zero) ? Image.Type.Sliced : Image.Type.Simple;
            if (hidden)
            {
                // 隐藏背景图(对应Galaxy的 lp_hidden → 背景瞬间透明)
                image.enabled = false;
            }

            // 7.2 文字标签(对应Galaxy的 TestLabel + SetDialogItemStyle/Text/TextWriteoutDuration)
            GameObject labelObj = new GameObject("TestLabel");
            labelObj.transform.SetParent(bubbleGO.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 10f);
            labelRect.offsetMax = new Vector2(-10f, -10f); // 内边距
            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.font = UGUITemplate.DefaultFont;
            label.fontSize = 36f;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            // 组装文本(对应Galaxy的 TextExpressionAssemble:nameShow时为"名字：内容")
            label.text = (nameShow && !string.IsNullOrEmpty(name)) ? string.Concat(name, "：", text) : text;
            label.ForceMeshUpdate();
            int totalChars = label.textInfo.characterCount;
            label.maxVisibleCharacters = 0; // 起始0字,由协程逐字推进(对应SetDialogItemTextWriteoutDuration)

            // 8. 存入DataTable(对应Galaxy的 DataTableSetDialog)
            DataTable<GameObject>.Save1(true, DataKey, unitTag, bubbleGO);

            // 9. 解析画布与相机(Screen Space Overlay画布的worldCamera为null,但投影世界点仍需主相机)
            Canvas canvas = UGUITemplate.Control_GameUICanvas;
            RectTransform canvasRect = canvas.transform as RectTransform;
            // 预解析renderMode(Screen Space Overlay时ScreenPointToLocalPointInRectangle的camera参数传null),
            // UpdatePosition每帧复用此bool,避免每帧穿越UGUITemplate.Control_GameUICanvas属性
            bool isOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
            Camera worldCamera = Camera.main;
            if (worldCamera == null)
            {
                // 兜底:通过UKit.MainCamera获取(其getter会确保创建带Camera的物体)
                worldCamera = UKit.MainCamera.GetComponent<Camera>();
            }

            Info info = new Info
            {
                followTarget = followTarget,
                name = name,
                text = text,
                dialog = bubbleGO,
                panel = bubbleRect,
                image = image,
                label = label,
                refPoint = refPoint,
                x = x,
                y = y,
                interrupted = false,
                canvasGroup = canvasGroup,
                trackTarget = trackTarget,
                maJia = maJia,
                worldCamera = worldCamera,
                canvasRect = canvasRect,
                totalChars = totalChars,
                tagKey = tagKey,
                isOverlay = isOverlay
            };

            // 10. 立即定位一次,避免首帧出现在画布原点(协程由MainThreadDispatcher调度,有1帧延迟)
            UpdatePosition(info);

            // 11. 派发BubbleTalkStart事件(对应Galaxy的 TriggerSendEvent("BubbleTalkStart"))
            //     Galaxy在淡入触发后立即派发Start,此处同样在协程启动前派发
            OnEventStart?.Invoke(info);

            // 12. 启动生命周期协程(对应Galaxy的 TriggerExecute 触发的 _TriggerFunc)
            //     使用MainThreadDispatcher.Call以对齐UTime.Wait的静态协程调度约定
            MainThreadDispatcher.Call(LifecycleCoroutine(info, unitTag, tag, whiteTime, waitTime));
        }

        /// <summary>
        /// 气泡生命周期协程.依次执行:淡入→停留(含打字机)→淡出,期间每帧更新定位并检测打断.
        /// </summary>
        /// <param name="info">气泡信息</param>
        /// <param name="unitTag">单位标识(用于DataTable查表)</param>
        /// <param name="tag">本气泡的代龄(用于打断检测)</param>
        /// <param name="whiteTime">打字机总时长(秒,<=0则无逐字效果直接全文显示)</param>
        /// <param name="waitTime">气泡停留总时长(秒,含淡入)</param>
        private static IEnumerator LifecycleCoroutine(Info info, int unitTag, int tag, float whiteTime, float waitTime)
        {
            CanvasGroup cg = info.canvasGroup;
            TextMeshProUGUI label = info.label;
            int totalChars = info.totalChars;

            // waitTime<=0时无逐字效果,直接显示全文
            if (whiteTime <= 0f)
            {
                label.maxVisibleCharacters = totalChars;
            }

            // 时间节点:淡入结束点 = FadeDuration;停留结束点 = max(FadeDuration, waitTime);淡出结束点 = 停留结束点 + FadeDuration
            // waitTime含淡入时间(对齐Galaxy的 Wait(waitTime - 0.25) 语义)
            float holdEnd = Mathf.Max(FadeDuration, waitTime);
            float outEnd = holdEnd + FadeDuration;
            float elapsed = 0f;
            bool interrupted = false;

            while (elapsed < outEnd)
            {
                // 打断检测(对应Galaxy的 lv_dialog==invalid || tag!=lv_tag):
                //   单位失效 / 气泡被新气泡销毁 / 代际计数器失效(新气泡已接管)
                //   用预构建的tagKey走Load0,避免每帧Load1内部ThreadStringBuilder.Concat产生GC字符串
                if (info.followTarget == null || info.dialog == null ||
                    DataTable<int>.Load0(true, info.tagKey) != tag)
                {
                    interrupted = true;
                    break;
                }

                elapsed += Time.deltaTime;

                // 每帧定位(对应Galaxy的 DialogSetPositionRelativeToUnit,由协程持续跟随)
                UpdatePosition(info);

                // 透明度:淡入→停留→淡出
                if (elapsed < FadeDuration)
                {
                    cg.alpha = Mathf.Clamp01(elapsed / FadeDuration);
                }
                else if (elapsed < holdEnd)
                {
                    cg.alpha = 1f;
                }
                else
                {
                    cg.alpha = Mathf.Clamp01(1f - (elapsed - holdEnd) / FadeDuration);
                }

                // 打字机推进(对应SetDialogItemTextWriteoutDuration的逐字效果)
                if (whiteTime > 0f)
                {
                    float writeElapsed = Mathf.Min(elapsed, whiteTime);
                    label.maxVisibleCharacters = Mathf.Min(totalChars, Mathf.FloorToInt(writeElapsed / whiteTime * totalChars));
                }

                yield return null;
            }

            if (interrupted)
            {
                // Break分支:派发BubbleTalkBreak → 销毁(不清DataTable,新气泡已接管)
                // 对应Galaxy的打断路径:销毁控件 + TriggerSendEvent("BubbleTalkBreak")
                info.interrupted = true;
                OnEventBreak?.Invoke(info);
                DestroyBubble(info, unitTag, tag, clearData: false);
            }
            else
            {
                // 正常End分支:补全文字 → 派发BubbleTalkEnd → 清理(清DataTable)
                // 对应Galaxy的正常收尾路径:ClearValue1 + DialogDestroy + TriggerSendEvent("BubbleTalkEnd")
                label.maxVisibleCharacters = totalChars;
                OnEventEnd?.Invoke(info);
                DestroyBubble(info, unitTag, tag, clearData: true);
            }
        }

        /// <summary>
        /// 更新气泡屏幕定位.将trackTarget的世界坐标投影到画布坐标系并应用像素偏移.
        /// 对应Galaxy的 DialogSetPositionRelativeToUnit.
        /// </summary>
        private static void UpdatePosition(Info info)
        {
            if (info.trackTarget == null || info.worldCamera == null || info.canvasRect == null) return;

            // 世界→屏幕(投影世界点到屏幕像素坐标)
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(info.worldCamera, info.trackTarget.position);
            // 屏幕像素偏移(对应Galaxy的 lp_x/lp_y 或基础版的 offSet)
            screenPos.x += info.x;
            screenPos.y += info.y;

            // 屏幕→画布本地坐标(Screen Space Overlay画布的camera参数传null,非Overlay传worldCamera)
            // isOverlay在CreateTextAdv中预解析,避免每帧穿越UGUITemplate.Control_GameUICanvas属性读renderMode
            Camera camForLocal = info.isOverlay ? null : info.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(info.canvasRect, screenPos, camForLocal, out Vector2 localPos))
            {
                info.panel.anchoredPosition = localPos;
            }
        }

        /// <summary>
        /// 销毁气泡及其马甲单位,并按需清理DataTable中的句柄与代龄计数器.
        /// </summary>
        /// <param name="info">气泡信息</param>
        /// <param name="unitTag">单位标识</param>
        /// <param name="tag">本气泡的代龄</param>
        /// <param name="clearData">true=正常结束清DataTable;false=被打断不清(新气泡已接管)</param>
        private static void DestroyBubble(Info info, int unitTag, int tag, bool clearData)
        {
            if (clearData)
            {
                // 仅当代龄仍属于本气泡时清理(对应Galaxy的 lv_dialog有效 && tag匹配 判断)
                // tag计数器用预构建的tagKey走Load0/Clear0,与协程及CreateTextAdv保持一致(零GC)
                if (info.followTarget != null && DataTable<int>.Load0(true, info.tagKey) == tag)
                {
                    DataTable<GameObject>.Clear1(true, DataKey, unitTag);
                    DataTable<int>.Clear0(true, info.tagKey);
                }
            }
            if (info.maJia != null)
            {
                UnityEngine.Object.Destroy(info.maJia); // 对应Galaxy的 UnitRemove(lv_u)
            }
            if (info.dialog != null)
            {
                UnityEngine.Object.Destroy(info.dialog); // 销毁气泡根(子物体Image/Label随之一并销毁)
            }
        }

        /// <summary>
        /// 生成9宫格圆角气泡背景Sprite(纯C#逐像素绘制,无需任何外部贴图资源).
        /// 四角为固定圆角,四边与中心可自由拉伸(Sliced);底色与边框色均可配置,边缘带1像素抗锯齿.
        /// 结果按(底色,边框色,圆角,边框,尺寸)缓存,同参数重复调用直接复用,不重复逐像素生成.
        /// </summary>
        /// <param name="bgColor">底色(支持透明)</param>
        /// <param name="borderColor">边框色(与底色相同时视觉上无边框区分)</param>
        /// <param name="cornerRadius">圆角半径(像素,同时作为9宫格边框宽度,默认16)</param>
        /// <param name="borderWidth">边框线宽(像素,0则无边框,默认2)</param>
        /// <param name="textureSize">正方形贴图边长(像素,默认128)</param>
        /// <returns>带9宫格边框(border=cornerRadius)的圆角Sprite,可直接用于Image.Type.Sliced</returns>
        public static Sprite GenerateSprite(Color bgColor, Color borderColor, int cornerRadius = 16, int borderWidth = 2, int textureSize = 128)
        {
            // 参数钳制:圆角不超过半边长,边框线宽不超过圆角
            cornerRadius = Mathf.Clamp(cornerRadius, 0, textureSize / 2);
            borderWidth = Mathf.Clamp(borderWidth, 0, cornerRadius);

            // 缓存查找(同参数组合直接复用)
            var cacheKey = (bgColor, borderColor, cornerRadius, borderWidth, textureSize);
            if (_bubbleSpriteCache.TryGetValue(cacheKey, out Sprite cached)) return cached;

            int size = textureSize;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];

            // 圆角矩形SDF参数:画布中心为原点,半边长halfW=halfH=(size-1)/2,圆角半径r
            float cx = (size - 1) * 0.5f;
            float cy = (size - 1) * 0.5f;
            float halfW = (size - 1) * 0.5f;
            float halfH = (size - 1) * 0.5f;
            float r = cornerRadius;
            float bw = borderWidth;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 像素坐标转为中心原点
                    float px = x - cx;
                    float py = y - cy;
                    // 圆角矩形SDF(signed distance field):负值在内,0在边界,正值在外
                    // 公式:q = abs(p) - (half - r); sdf = min(max(q.x,q.y),0) + length(max(q,0)) - r
                    float dx = Mathf.Abs(px) - (halfW - r);
                    float dy = Mathf.Abs(py) - (halfH - r);
                    float ax = Mathf.Max(dx, 0f);
                    float ay = Mathf.Max(dy, 0f);
                    float sdf = Mathf.Min(Mathf.Max(dx, dy), 0f) + Mathf.Sqrt(ax * ax + ay * ay) - r;

                    // 外缘覆盖率(整体形状,1px抗锯齿):sdf<=0为内部,sdf>0为外部
                    float outerCov = Mathf.Clamp01(0.5f - sdf);
                    // 填充覆盖率(边框内侧):sdf <= -bw 为纯填充
                    float fillCov = Mathf.Clamp01(0.5f - sdf - bw);
                    // 边框带覆盖率 = 外缘 - 填充(填充区与边框带不重叠)
                    float borderCov = outerCov - fillCov;

                    // 非重叠覆盖的straight-alpha合成(填充区用bgColor,边框带用borderColor)
                    float alpha = bgColor.a * fillCov + borderColor.a * borderCov;
                    Color c;
                    if (alpha <= 0f)
                    {
                        c = Color.clear;
                    }
                    else
                    {
                        c.a = alpha;
                        c.r = (bgColor.r * bgColor.a * fillCov + borderColor.r * borderColor.a * borderCov) / alpha;
                        c.g = (bgColor.g * bgColor.a * fillCov + borderColor.g * borderColor.a * borderCov) / alpha;
                        c.b = (bgColor.b * bgColor.a * fillCov + borderColor.b * borderColor.a * borderCov) / alpha;
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(false, true); // 上传GPU并释放CPU端像素内存(生成后无需回读)

            // 创建带9宫格边框的Sprite:border=Vector4(左,下,右,上),均=cornerRadius
            // MeshType必须FullRect以支持Image.Type.Sliced的9宫格拉伸
            Vector4 border = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);
            Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, border);

            _bubbleSpriteCache[cacheKey] = sprite;
            return sprite;
        }

        /// <summary>
        /// 清空已缓存的气泡Sprite及其底层Texture2D.在场景切换或确定不再需要已生成的气泡样式时调用,释放原生纹理内存.
        /// 注意:调用前应确保没有正在显示的气泡仍引用缓存中的Sprite,否则会导致该气泡背景贴图失效(纹理被销毁而Sprite仍被Image引用).
        /// </summary>
        public static void ClearSpriteCache()
        {
            foreach (var kv in _bubbleSpriteCache)
            {
                if (kv.Value != null)
                {
                    // Sprite.Create产生的Sprite与Texture2D均为原生对象,需显式销毁,否则即使字典清空也不会释放显存
                    Texture2D tex = kv.Value.texture;
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(kv.Value);
                        if (tex != null) UnityEngine.Object.Destroy(tex);
                    }
                    else
                    {
                        // 编辑器非播放模式下Destroy会告警,改用DestroyImmediate立即释放
                        UnityEngine.Object.DestroyImmediate(kv.Value);
                        if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                    }
                }
            }
            _bubbleSpriteCache.Clear();
        }

        /// <summary>
        /// 解析气泡背景Sprite:texturePath非空时加载外部Sprite(失败回退到生成),否则用C#生成.
        /// </summary>
        /// <param name="texturePath">外部贴图Resources路径(null或空则生成)</param>
        /// <param name="bgColor">生成时的底色(null使用默认底色;非null按所给Color生成,允许alpha=0全透明)</param>
        /// <returns>背景Sprite(永不为null)</returns>
        private static Sprite ResolveBubbleSprite(string texturePath, Color? bgColor)
        {
            if (!string.IsNullOrEmpty(texturePath))
            {
                Sprite external = Resources.Load<Sprite>(texturePath);
                if (external != null) return external;
                Debug.LogWarning($"[BubbleTalk] 外部贴图加载失败,回退到C#生成气泡: {texturePath}");
            }
            // null走默认底色;非null按调用方显式指定的Color生成(包括alpha=0全透明)
            Color bg = bgColor ?? DefaultBubbleBgColor;
            return GenerateSprite(bg, DefaultBubbleBorderColor, DefaultCornerRadius, DefaultBorderWidth, DefaultTextureSize);
        }
    }
}

// ===== 用法示范 =====
// 基础版:在殖民者头顶显示一句对话,打字机2秒,停留5秒,显示"名字：内容"(背景由C#生成,默认深色)
// BubbleTalk.CreateText(pawn.transform, pawn.name, "今天天气不错", 80, 0, 2f, 5f, false, true);
//
// 进阶版:自定义尺寸/C#生成气泡(指定红色底色)/挂到"Head"子物体/屏幕偏移(0,100)
// BubbleTalk.CreateTextAdv(pawn.transform, pawn.name, "收到指令", 0, 1.5f, 4f, false, true,
//     "Head", 500, 1000, 0, 100, null, false, new Color(0.6f, 0.1f, 0.1f, 0.85f));
//
// 进阶版:显式传入全透明底色(只显示边框,内填充透明)——nullable语义下可表达,旧版alpha=0哨兵无法区分
// BubbleTalk.CreateTextAdv(pawn.transform, pawn.name, "收到指令", 0, 1.5f, 4f, false, true,
//     "Head", 500, 1000, 0, 100, null, false, new Color(0f, 0f, 0f, 0f));
//
// 进阶版:使用外部贴图(传入Resources路径,加载失败自动回退到C#生成)
// BubbleTalk.CreateTextAdv(pawn.transform, pawn.name, "收到指令", 0, 1.5f, 4f, false, true,
//     "Head", 500, 1000, 0, 100, "UI/Box/MyBubble", false);
//
// 单独获取C#生成的气泡Sprite(可用于任意Image组件):
// Sprite sp = BubbleTalk.GenerateSprite(new Color(0.1f, 0.2f, 0.4f, 0.9f), Color.white, 20, 3, 128);
//
// 场景切换或不再需要已生成气泡样式时,清空Sprite缓存释放原生纹理内存(确保此时无气泡在用缓存Sprite):
// BubbleTalk.ClearSpriteCache();
//
// 订阅生命周期事件:
// BubbleTalk.OnEventStart += info => Debug.Log($"{info.name} 开始说话");
// BubbleTalk.OnEventBreak += info => Debug.Log($"{info.name} 说话被打断");
// BubbleTalk.OnEventEnd   += info => Debug.Log($"{info.name} 说话结束");
#endif
