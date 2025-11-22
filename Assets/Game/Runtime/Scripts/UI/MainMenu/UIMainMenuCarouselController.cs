using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Audio;
using Game.Data;
using Game.Model;
using WanFramework.UI.DataComponent;

namespace Game.UI.MainMenu
{
    /// <summary>
    /// 按钮轮播
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIMainMenuCarouselController : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("所有要轮播的按钮（按想要的顺序添加）")]
        [SerializeField] private List<Button> carouselButtons;
        [Header("三个停放位置的 RectTransform：上/中/下")]
        [SerializeField] private RectTransform topAnchor;
        [SerializeField] private RectTransform middleAnchor;
        [SerializeField] private RectTransform bottomAnchor;
        [Header("动画相关参数")]
        [SerializeField] private float moveDuration = 0.3f; // 插值时长
        [SerializeField] private float fadeFactor = 0.5f;   // 非中间按钮透明度
        [SerializeField] private float dragThreshold = 50f; // 拖拽多少像素代表一个索引单位

        // 使用连续浮点数表示当前滚动位置（可无限增加或减少）
        private float currentPos = 0f;
        private float targetPos = 0f;
        private bool isLerping = false;
        private float lerpTimer = 0f;

        private bool isDragging = false;
        private Vector2 dragStartPos;
        private float dragStartPosIndex = 0f;

        //微型屎山，用来控制一下滚动时候音效连播的间隔
        private float audioScrollInterval = 0.2f;
        private float audioScrollTimer = 0;

        private void OnEnable()
        {
            if (carouselButtons.Count == 0) return;

            // 根据条件设置初始按钮索引
            //需要保证按钮索引 0-NewGame 1-Continue 2-Exit 3-Tutorial
            int targetIndex = 0;// 默认值


            //没有进过教程停留在教程
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Begin))
            {
                targetIndex = 3;
            }
            //进过教程但存档不存在
            else if (!SaveUtils.IsGameSaveExist() && DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Begin))
            {
                targetIndex = 0;
            }
            //进过教程且存档存在
            else if (SaveUtils.IsGameSaveExist() && DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Begin))
            {
                targetIndex = 1;
            }

            currentPos = targetIndex; // 直接定位到目标索引
            targetPos = targetIndex;
            UpdatePositions(currentPos); // 立即更新位置
            UpdateInteractability();
        }

        private void Update()
        {
            if (isLerping)
            {
                lerpTimer += Time.deltaTime;
                float t = lerpTimer / moveDuration;
                if (t >= 1f)
                {
                    t = 1f;
                    isLerping = false;
                }
                float newPos = Mathf.Lerp(currentPos, targetPos, t);
                UpdatePositions(newPos);
                if (!isLerping)
                {
                    currentPos = newPos;
                    UpdateInteractability();
                }
            }

            audioScrollTimer += Time.deltaTime;
        }

        /// <summary>
        /// 根据连续位置 pos 更新所有按钮的位置与透明度。
        /// pos 通过对按钮总数取模映射到 [0, count) 内，再计算每个按钮与中心的相对差值。
        /// </summary>
        private void UpdatePositions(float pos)
        {
            int count = carouselButtons.Count;
            // 映射到 [0, count) 范围内
            float centerPos = pos % count;
            if (centerPos < 0) centerPos += count;

            for (int i = 0; i < count; i++)
            {
                // 计算按钮 i 与中心的差值 diff，并做环绕处理
                float diff = i - centerPos;
                if (diff > count / 2f) diff -= count;
                if (diff < -count / 2f) diff += count;

                // 仅显示 diff 在 [-1.5, 1.5] 范围内的按钮，其它隐藏
                if (diff < -1.5f || diff > 1.5f)
                {
                    carouselButtons[i].gameObject.SetActive(false);
                }
                else
                {
                    carouselButtons[i].gameObject.SetActive(true);
                    RectTransform rt = carouselButtons[i].GetComponent<RectTransform>();
                    Vector2 finalPos;
                    if (diff <= 0f)
                    {
                        // 当 diff 在 [-1,0]时，从 topAnchor 到 middleAnchor 插值
                        float t = Mathf.InverseLerp(-1f, 0f, diff);
                        finalPos = Vector2.Lerp(topAnchor.anchoredPosition, middleAnchor.anchoredPosition, t);
                    }
                    else
                    {
                        // 当 diff 在 [0,1]时，从 middleAnchor 到 bottomAnchor 插值
                        float t = Mathf.InverseLerp(0f, 1f, diff);
                        finalPos = Vector2.Lerp(middleAnchor.anchoredPosition, bottomAnchor.anchoredPosition, t);
                    }
                    rt.anchoredPosition = finalPos;
                    // 中心按钮透明度为1，其它为 fadeFactor
                    float alpha = (Mathf.Abs(diff) < 0.01f) ? 1f : fadeFactor;
                    CanvasGroup cg = carouselButtons[i].GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = carouselButtons[i].gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = alpha;
                }
            }
        }

        /// <summary>
        /// 更新交互状态：仅让当前中心按钮可交互。
        /// </summary>
        private void UpdateInteractability()
        {
            int count = carouselButtons.Count;
            int centerIndex = Mathf.RoundToInt(currentPos % count);
            if (centerIndex < 0) centerIndex += count;
            for (int i = 0; i < count; i++)
            {
                carouselButtons[i].interactable = (i == centerIndex);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (isLerping) return; // 动画播放时不响应新输入

            // 修改滚轮方向：滚动向上时增加currentPos，向下则减少
            float delta = (eventData.scrollDelta.y > 0) ? 1f : -1f; // 反转delta符号
            targetPos = currentPos + delta;
            lerpTimer = 0f;
            isLerping = true;
            AudioSystem.Instance.SendEvent(AudioIds.UI_Mainmenu_Roll);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isLerping) return;
            isDragging = true;
            dragStartPos = eventData.position;
            dragStartPosIndex = currentPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || isLerping) return;

            float deltaY = eventData.position.y - dragStartPos.y;
            float indexOffset = deltaY / dragThreshold;
            // 调整拖拽方向：向上拖拽增加currentPos
            float newPos = dragStartPosIndex + indexOffset; // 减号改为加号
            UpdatePositions(newPos);
            if (audioScrollTimer >= audioScrollInterval){
                    AudioSystem.Instance.SendEvent(AudioIds.UI_Mainmenu_Roll);
                    audioScrollTimer = 0f;
                }

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isLerping) return;
            isDragging = false;

            float deltaY = eventData.position.y - dragStartPos.y;
            float indexOffset = deltaY / dragThreshold;
            float newPos = dragStartPosIndex + indexOffset;

            currentPos = Mathf.Round(newPos);
            targetPos = currentPos; // 保持目标位置与当前位置一致
            UpdatePositions(currentPos);
            UpdateInteractability();
        }
    }
}
