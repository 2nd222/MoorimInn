using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class SlotPreset
{
    public Vector2 position;
    public float scale;
    public int sortingOrder;
    
    [Header("연출용")]
    public float focusScale;   // 강조 시 크기
}

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("슬롯들")]
    [SerializeField] private List<CharacterSlot> slots;
    [SerializeField] private List<SlotPreset> presets;

    [Header("캐릭터 데이터")]
    [SerializeField] private List<Character> characterDatas;
    
    [Header("Inner Thought")]
    [SerializeField] private GameObject dimObject;

    private Dictionary<int, Character> dataDict; // 캐릭터 id -> 객체

    private Dictionary<int, int> currentSlotMap = new(); // characterID → slotIndex
    
    public bool DuringMotion
    {
        get
        {
            foreach (var slot in slots)
            {
                if (slot != null && slot.IsPlayingMotion)
                    return true;
            }

            return false;
        }
    }
    
    private bool isInnerThought;
    private int innerThoughtCharacterID = -1;
    
    private void Awake()
    {
        dataDict = new Dictionary<int, Character>();

        foreach (var data in characterDatas)
        {
            dataDict[data.characterID] = data;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private void OnEnable()
    {
        StoryManager.Instance.SetCharacterUI(this);
    }
    
    public void Show()
    {
        
    }

    public void Hide()
    {
        // 혹시 남아있는 상태 초기화
        foreach (var slot in slots)
        {
            slot.ClearImmediate();
            slot.gameObject.SetActive(false);
        }
        currentSlotMap.Clear();
    }

    public void SetupScene(List<CharacterInScene> newChars)
    {
        if (newChars == null)
            return;

        #region SetupScene 처리 기준

        /*
         * ================================================================
         * SetupScene의 기본 처리 기준
         * ================================================================
         *
         * SetupScene은 "현재 스토리 씬에 맞게 캐릭터 구성을 변경하고
         * 씬 시작에 필요한 기본 상태를 적용"하는 역할이다.
         *
         *
         * [1. 기존 캐릭터 + 동일 슬롯]
         * ----------------------------------------------------------------
         * 같은 캐릭터가 이전 Scene과 현재 Scene에서 같은 슬롯을 사용하는 경우.
         *
         * 예:
         *
         * 이전 Scene
         *   Character A → Slot 0
         *
         * 현재 Scene
         *   Character A → Slot 0
         *
         * → 기존 CharacterSlot을 그대로 유지한다.
         *
         * 기존 캐릭터와 슬롯을 유지하기 때문에 다음 상태가 이어진다.
         *
         *   - CharacterSlot 자체
         *   - 현재 캐릭터 데이터
         *   - 현재 Sprite
         *   - 현재 표정
         *   - MotionXRoot 위치
         *   - MotionYRoot 위치
         *
         * 특히 MotionXRoot / MotionYRoot는
         * SetupScene에서 위치를 초기화하지 않는다.
         *
         * 따라서 이전 Scene에서 Motion에 의해 이동한 상태라면
         * 그 위치가 다음 Scene에서도 그대로 유지된다.
         *
         * 단, 현재 진행 중인 Motion의 Coroutine / DOTween은
         * StopSlotMotion()으로 정리한다.
         *
         * 즉,
         *
         *   "진행 중인 Motion은 종료하지만,
         *    MotionRoot의 현재 위치는 유지한다."
         *
         *
         * 반대로 다음 상태는 현재 Scene 기준으로 다시 적용한다.
         *
         *   - SlotRoot 위치
         *   - BaseScale
         *   - Flip
         *   - SortingOrder
         *
         * 따라서 SlotRoot가 직접 이동한 경우에는
         * 현재 Scene의 preset.position으로 되돌아간다.
         *
         *
         * [2. 기존 캐릭터 + 다른 슬롯]
         * ----------------------------------------------------------------
         * 같은 캐릭터지만 이전 Scene과 현재 Scene에서 사용하는 슬롯이
         * 달라진 경우.
         *
         * 예:
         *
         * 이전 Scene
         *   Character A → Slot 0
         *
         * 현재 Scene
         *   Character A → Slot 2
         *
         * → 이전 Slot 0을 ResetSlot()으로 정리한다.
         * → 새로운 Slot 2를 PrepareSlot()으로 초기화한다.
         * → Character A를 Slot 2에 SetCharacter()로 배치한다.
         *
         * 기존 슬롯을 완전히 정리하므로 이전 Slot의 상태를 이어받지 않는다.
         *
         * 따라서 새 슬롯에서는 캐릭터의 기본 상태로 시작한다.
         *
         *
         * [3. 완전히 새로운 캐릭터]
         * ----------------------------------------------------------------
         * 이전 Scene에는 없었던 캐릭터.
         *
         * 예:
         *
         * 이전 Scene
         *   Character A
         *
         * 현재 Scene
         *   Character A
         *   Character B
         *
         * → Character B는 새로운 캐릭터로 처리한다.
         *
         * → 대상 Slot을 PrepareSlot()으로 초기화한다.
         *
         * → startVisible == true라면
         *      Appear()로 등장시킨다.
         *
         * → startVisible == false라면
         *      SetHidden()으로 숨긴다.
         *
         * 새로운 캐릭터이므로 이전 Scene의 상태를 이어받지 않는다.
         *
         *
         * [4. 기존 Slot에 다른 캐릭터가 들어오는 경우]
         * ----------------------------------------------------------------
         * 예:
         *
         * 이전 Scene
         *   Slot 0 → Character A
         *
         * 현재 Scene
         *   Slot 0 → Character B
         *
         * → Character A가 새 Scene에 없다면
         *   캐릭터 제거 단계에서 Slot 0이 ResetSlot()된다.
         *
         * → Character B는 existedBefore == false이므로
         *   새로운 캐릭터처럼 처리된다.
         *
         * 따라서 Character A의 상태가 Character B에게 남지 않는다.
         *
         *
         * [5. 새 Scene에 존재하지 않는 기존 캐릭터]
         * ----------------------------------------------------------------
         * 이전 Scene에는 있었지만 현재 Scene에는 없는 캐릭터.
         *
         * → ResetSlot()으로 Slot을 완전히 정리한다.
         *
         * ResetSlot()에서는 기존 캐릭터의 상태를 제거한다.
         *
         *
         * [6. startVisible]
         * ----------------------------------------------------------------
         *
         * startVisible == true
         *
         *   [동일 캐릭터 + 동일 슬롯]
         *       → 기존 CharacterSlot 유지
         *       → MotionXRoot / MotionYRoot 위치 유지
         *       → 현재 진행 중인 Motion은 StopSlotMotion()으로 종료
         *       → 캐릭터가 비활성 상태라면 SetCharacter()
         *       → 활성 상태라면 resetExpression에 따라
         *         표정을 초기화하거나 기존 표정 유지
         *
         *   [다른 슬롯으로 이동한 기존 캐릭터]
         *       → 기존 슬롯 Reset
         *       → 새 슬롯에 SetCharacter()
         *
         *   [완전히 새로운 캐릭터]
         *       → Appear()
         *
         *
         * startVisible == false
         *
         *   → SetHidden()
         *
         *   → resetExpression 값에 따라
         *     표정 상태를 초기화하거나 유지한다.
         *
         *
         * [7. resetExpression]
         * ----------------------------------------------------------------
         * 동일 캐릭터 + 동일 슬롯에서
         * 이전 Scene의 표정을 유지할지 결정한다.
         *
         * resetExpression == true
         *
         *   → ResetExpression()을 통해
         *     씬 기본 표정으로 초기화한다.
         *
         * resetExpression == false
         *
         *   → 기존 표정 상태를 유지한다.
         *
         * 즉, resetExpression은
         * "같은 캐릭터 + 같은 슬롯에서 표정 상태를 유지할지"
         * 결정하는 옵션이다.
         *
         * MotionXRoot / MotionYRoot의 위치와는 별개의 설정이다.
         *
         *
         * [8. Motion 상태]
         * ----------------------------------------------------------------
         * Motion 상태는 캐릭터의 종류와 슬롯 변경 여부에 따라 다르게 처리한다.
         *
         *
         * [동일 캐릭터 + 동일 슬롯]
         *
         *   → 기존 CharacterSlot 유지
         *   → MotionXRoot 위치 유지
         *   → MotionYRoot 위치 유지
         *
         *   → 단, 현재 실행 중인 Coroutine / DOTween은
         *     StopSlotMotion()으로 종료한다.
         *
         * 따라서 이전 Scene에서
         *
         *   MotionXRoot = (100, 0)
         *   MotionYRoot = (0, 20)
         *
         * 상태였다면,
         *
         * SetupScene 이후에도 해당 위치는 유지된다.
         *
         *
         * [다른 슬롯으로 이동]
         *
         *   → 기존 슬롯 Reset
         *   → 새 슬롯 Prepare
         *
         *   → 이전 MotionRoot 상태를 이어받지 않는다.
         *
         *
         * [새로운 캐릭터]
         *
         *   → PrepareSlot()으로 새 슬롯을 초기화하므로
         *     이전 Motion 상태를 이어받지 않는다.
         *
         *
         * ※ SlotRoot는 MotionRoot와 별개로 처리된다.
         *
         * SetupScene에서
         *
         *   targetSlot.SlotRoot.anchoredPosition = finalPos;
         *
         * 를 수행하기 때문에 SlotRoot 위치는
         * 현재 Scene의 preset 기준 위치로 다시 설정된다.
         *
         *
         * [9. 기본 위치 / 크기]
         * ----------------------------------------------------------------
         * 모든 캐릭터의 SlotRoot 위치와 기본 크기는
         * 현재 Scene의 SlotPreset을 기준으로 적용한다.
         *
         * 위치:
         *
         *   preset.position
         *   + character.visual.offset
         *
         * 크기:
         *
         *   preset.scale
         *   × character.visual.baseScale
         *
         * 따라서 같은 캐릭터 + 같은 슬롯이라도
         * SlotRoot 위치와 BaseScale은 현재 Scene 기준으로 갱신된다.
         *
         * 단, MotionXRoot / MotionYRoot의 위치는 별도로 변경하지 않는다.
         *
         *
         * [10. Flip]
         * ----------------------------------------------------------------
         * 현재 Scene의 startFlip 값을 항상 적용한다.
         *
         * 따라서 이전 Scene의 Flip 상태는
         * 현재 Scene의 startFlip으로 덮어쓴다.
         *
         *
         * [11. SortingOrder]
         * ----------------------------------------------------------------
         * 현재 Scene의
         *
         *   preset.sortingOrder
         *
         * 를 적용한다.
         *
         * 따라서 이전 Scene에서 변경된 SiblingOrder는
         * 현재 Scene의 기본값으로 다시 설정된다.
         *
         * InnerThought 등 별도의 연출에서
         * 추가로 SiblingOrder를 변경하는 경우에는
         * SetupScene 이후 별도로 처리한다.
         *
         *
         * [12. currentSlotMap]
         * ----------------------------------------------------------------
         * SetupScene 시작 시 기존 currentSlotMap을 백업한다.
         *
         * 이후 currentSlotMap을 Clear하고
         * 현재 Scene 기준으로 다시 작성한다.
         *
         * SetupScene이 끝난 뒤에는
         *
         *   characterID → 현재 slotIndex
         *
         * 형태가 된다.
         *
         *
         * [13. 전체 처리 요약]
         * ----------------------------------------------------------------
         *
         * 동일 캐릭터 + 동일 슬롯
         *   → CharacterSlot 유지
         *   → MotionXRoot / MotionYRoot 위치 유지
         *   → 기존 표정 유지 가능
         *   → 현재 Motion의 Coroutine / DOTween은 종료
         *   → SlotRoot 위치는 현재 Scene preset으로 갱신
         *   → BaseScale 갱신
         *   → Flip 갱신
         *   → SortingOrder 갱신
         *
         *
         * 동일 캐릭터 + 다른 슬롯
         *   → 이전 Slot Reset
         *   → 새 Slot Prepare
         *   → 새 슬롯에 SetCharacter
         *   → 이전 MotionRoot 상태를 이어받지 않음
         *
         *
         * 새로운 캐릭터
         *   → 새 Slot Prepare
         *   → startVisible이면 Appear
         *   → 아니면 SetHidden
         *
         *
         * 현재 Scene에 없는 캐릭터
         *   → ResetSlot
         *
         * ================================================================
         */

        #endregion

        // --------------------------------------------------
        // 1. 기존 캐릭터의 슬롯 위치 백업
        // --------------------------------------------------
        Dictionary<int, int> oldMap = new(currentSlotMap);

        // 새 씬 기준으로 다시 작성
        currentSlotMap.Clear();

        // --------------------------------------------------
        // 2. 새 씬에 존재하는 캐릭터 ID 수집
        // --------------------------------------------------
        HashSet<int> newIDs = new();

        foreach (var info in newChars)
        {
            if (info == null || !info.useSlotUI)
                continue;

            newIDs.Add(info.characterID);
        }

        // --------------------------------------------------
        // 3. 새 씬에 없는 기존 캐릭터 제거
        // --------------------------------------------------
        foreach (var slot in slots)
        {
            if (slot == null)
                continue;

            int currentID = slot.CurrentCharacterID;

            if (currentID == -1)
                continue;

            // 새 씬에서도 사용하는 캐릭터라면 유지
            if (newIDs.Contains(currentID))
                continue;

            ResetSlot(slot);
        }

        // --------------------------------------------------
        // 4. 새 씬 캐릭터 세팅
        // --------------------------------------------------
        foreach (var info in newChars)
        {
            if (info == null || !info.useSlotUI)
                continue;

            // 슬롯 번호 검증
            if (!IsValidSlotIndex(info.slotIndex))
            {
                Debug.LogWarning(
                    $"[CharacterUI] Invalid slotIndex. " +
                    $"characterID:{info.characterID}, slotIndex:{info.slotIndex}"
                );

                continue;
            }

            // 캐릭터 데이터 확인
            if (!dataDict.TryGetValue(info.characterID, out var data))
            {
                Debug.LogWarning(
                    $"[CharacterUI] Character data not found. " +
                    $"characterID:{info.characterID}"
                );

                continue;
            }

            CharacterSlot targetSlot = slots[info.slotIndex];
            SlotPreset preset = presets[info.slotIndex];

            if (targetSlot == null)
                continue;

            // --------------------------------------------------
            // 기존 캐릭터가 어디 있었는지 확인
            // --------------------------------------------------
            bool existedBefore = oldMap.TryGetValue(info.characterID, out int oldSlotIndex);
            bool sameSlot = existedBefore && oldSlotIndex == info.slotIndex;

            // --------------------------------------------------
            // 기존 캐릭터가 다른 슬롯으로 이동한 경우
            // --------------------------------------------------
            if (existedBefore && !sameSlot)
            {
                if (oldSlotIndex != info.slotIndex && IsValidSlotIndex(oldSlotIndex))
                {
                    CharacterSlot oldSlot = slots[oldSlotIndex];
                    if (oldSlot != null && oldSlot != targetSlot)
                    {
                        ResetSlot(oldSlot);
                    }
                }
            }

            // --------------------------------------------------
            // 대상 슬롯 정리
            //
            // 같은 캐릭터 + 같은 슬롯이면 기존 오브젝트를
            // 완전히 지우지 않는다.
            // --------------------------------------------------
            if (!sameSlot)
            {
                PrepareSlot(targetSlot);
            }
            else
            {
                // 같은 슬롯이어도 현재 진행 중인 DOTween이나
                // 코루틴은 정리
                StopSlotMotion(targetSlot);
            }

            // --------------------------------------------------
            // 기본 위치 / 크기
            // --------------------------------------------------
            Vector2 finalPos = preset.position;
            float finalScale = preset.scale;

            if (data.visual != null)
            {
                finalPos += data.visual.offset;
                finalScale *= data.visual.baseScale;
            }

            // --------------------------------------------------
            // 슬롯 기본 상태 적용
            // --------------------------------------------------
            targetSlot.SlotRoot.anchoredPosition = finalPos;
            targetSlot.SetBaseScale(finalScale);
            targetSlot.SetFlip(info.startFlip);

            // 여기서 기존 sortingOrder를 복구
            //
            // 단, InnerThought 자체의 특별한 순서는 건드리지 않는다.
            // SetupScene은 "씬 초기화"의 역할만 담당.
            targetSlot.transform.SetSiblingIndex(preset.sortingOrder);

            // --------------------------------------------------
            // 캐릭터 표시 상태
            // --------------------------------------------------
            if (info.startVisible)
            {
                if (sameSlot)
                {
                    // 같은 캐릭터 + 같은 슬롯
                    //
                    // 기존 캐릭터를 유지하되,
                    // resetExpression이 true라면 씬 기본 표정으로 초기화한다.
                    if (!targetSlot.gameObject.activeSelf)
                    {
                        targetSlot.SetCharacter(data, data.characterImage, info.resetExpression);
                    }
                    else if (info.resetExpression)
                    {
                        targetSlot.ResetExpression();
                    }
                }
                else if (existedBefore)
                {
                    // 다른 슬롯에서 이동
                    targetSlot.SetCharacter(data, data.characterImage, true);
                }
                else
                {
                    // 완전히 새로운 캐릭터
                    targetSlot.Appear(data, data.characterImage, finalPos);
                }
            }
            else
            {
                targetSlot.SetHidden(data, data.characterImage, info.resetExpression);
            }

            // --------------------------------------------------
            // 새 슬롯 맵 등록
            // --------------------------------------------------
            currentSlotMap[info.characterID] = info.slotIndex;

            Debug.Log(
                $"[CharacterUI] Setup " +
                $"character:{info.characterID} " +
                $"slot:{info.slotIndex} " +
                $"sameSlot:{sameSlot} " +
                $"existedBefore:{existedBefore}"
            );
        }

        Debug.Log("[CharacterUI] SetupScene Complete");
    }

    private bool IsValidSlotIndex(int index)
    {
        return index >= 0 &&
               index < slots.Count &&
               index < presets.Count;
    }
    
    private void StopSlotMotion(CharacterSlot slot)
    {
        if (slot == null)
            return;

        slot.StopAllCoroutines();

        slot.transform.DOKill();

        if (slot.SlotRoot != null)
            slot.SlotRoot.DOKill();
    }
    
    private void PrepareSlot(CharacterSlot slot)
    {
        if (slot == null)
            return;

        StopSlotMotion(slot);

        slot.ClearImmediate();
    }
    
    private void ResetSlot(CharacterSlot slot)
    {
        if (slot == null)
            return;

        StopSlotMotion(slot);

        slot.ClearImmediate();
    }
    
    public void SetExpression(int characterID, ExpressionData expression)
    {
        CharacterSlot slot = FindSlot(characterID);

        if (slot == null)
            return;

        slot.SetExpression(expression);
    }
    
    public void SetUnknown(int characterID, bool isUnknown)
    {
        var slot = FindSlot(characterID);
        if (slot == null) return;

        slot.SetUnknown(isUnknown);
    }

    public void Focus(int speakerID)
    {
        foreach (var slot in slots)
        {
            if (slot.IsPlayingMotion)
                continue;
            
            int id = slot.CurrentCharacterID;
            if (id == -1) continue;
            
            int slotIndex = currentSlotMap[id];
            var preset = presets[slotIndex];
            
            RectTransform rt = slot.GetComponent<RectTransform>();
            
            if (id == speakerID)
            {
                slot.SetFocusScale(preset.focusScale, 0.2f);
                slot.SetFocused(true);
            }
            else
            {
                slot.SetFocusScale(preset.scale, 0.2f);
                slot.SetFocused(false);
            }
        }
    }

    public void ApplyInnerThoughtFocus(int thinkerID, bool enable)
    {
        isInnerThought = enable;
        innerThoughtCharacterID = enable ? thinkerID : -1;
        
        if (dimObject == null)
            return;

        dimObject.SetActive(enable);

        if (!enable)
        {
            dimObject.transform.SetAsLastSibling();
            return;
        }

        foreach (var slot in slots)
        {
            if (slot.CurrentCharacterID == -1)
                continue;

            int id = slot.CurrentCharacterID;

            if (!currentSlotMap.TryGetValue(id, out int slotIndex))
                continue;

            var preset = presets[slotIndex];

            if (enable)
            {
                if (id == thinkerID)
                {
                    // dim보다 위
                    slot.transform.SetAsLastSibling();
                }
                else
                {
                    // dim보다 아래
                    slot.transform.SetSiblingIndex(
                        Mathf.Min(preset.sortingOrder, dimObject.transform.GetSiblingIndex() - 1)
                    );
                }
            }
        }
    }
    
    private void SetMotionSiblingOrder(CharacterSlot slot)
    {
        if (slot == null)
            return;

        // InnerThought가 아니면 기존 동작 그대로
        if (!isInnerThought)
        {
            slot.transform.SetAsLastSibling();
            return;
        }

        // InnerThought 상태에서는
        // 속마음 화자만 맨 앞으로 보낸다.
        if (slot.CurrentCharacterID == innerThoughtCharacterID)
        {
            slot.transform.SetAsLastSibling();
            return;
        }

        // 그 외 캐릭터는 현재 순서를 그대로 유지한다.
    }
    
    public void SetFlip(int characterID, bool flip)
    {
        var slot = FindSlot(characterID);

        if (slot == null)
            return;

        slot.SetFlip(flip);
    }
    
    public void BringCharacterToFront(int characterID)
    {
        var slot = FindSlot(characterID);

        if (slot == null)
            return;

        slot.transform.SetAsLastSibling();
    }
    
    public IEnumerator PlayMotionRoutine(int characterID, MotionData motion, bool isUnknown)
    {
        var slot = FindSlot(characterID);
        
        if (slot == null || motion == null)
            yield break;

        if (motion.useFlip)
        {
            slot.SetFlip(motion.flip);
        }
        
        var preset = presets[slot.slotIndex];

        Vector2 finalPos = preset.position;
        if (dataDict[characterID] != null && dataDict[characterID].visual != null)
        {
            finalPos += dataDict[characterID].visual.offset;
        }
        
        Debug.Log($"{slot.slotIndex}, {slot.SlotPos} , {finalPos}");
        
        switch (motion.type)
        {
            case MotionType.MoveLeft:
                SetMotionSiblingOrder(slot);
                yield return slot.MoveRoutine(Vector2.left, motion.value, motion.duration);
                break;

            case MotionType.MoveRight:
                SetMotionSiblingOrder(slot);
                yield return slot.MoveRoutine(Vector2.right, motion.value, motion.duration);
                break;
            
            case MotionType.MoveToTarget:
            {
                if (motion.targetSlotIndex < 0 ||
                    motion.targetSlotIndex >= slots.Count)
                    yield break;
                
                SetMotionSiblingOrder(slot);
                var target = slots[motion.targetSlotIndex];

                Vector2 targetPos = target.SlotPos;
                targetPos.y = slot.SlotPos.y; // 현재 높이 유지
                Vector2 dir = GetDirection(slot, target);

                Vector2 destination = targetPos - dir * motion.stopDistance;
                
                yield return slot.MoveToPosition(destination, motion.duration);

                break;
            }
            
            case MotionType.ReturnSlot:
            {
                Vector2 originalPos = presets[slot.slotIndex].position;

                if (dataDict[characterID] != null && dataDict[characterID].visual != null)
                {
                    originalPos += dataDict[characterID].visual.offset;
                }
                yield return slot.MoveToPosition(originalPos, motion.duration); 

                if (isInnerThought)
                {
                    // 속마음 화자는 항상 Dim보다 위
                    if (slot.CurrentCharacterID == innerThoughtCharacterID)
                    {
                        slot.transform.SetAsLastSibling();
                    }
                }
                else
                {
                    // 기존 동작
                    slot.transform.SetSiblingIndex(preset.sortingOrder);
                }
                break;
            }

            case MotionType.Jump:
                yield return slot.JumpRoutine(motion.value, motion.duration);
                break;
            
            case MotionType.ExitLeft:
                SetMotionSiblingOrder(slot);
                yield return slot.ExitRoutine(Vector2.left, motion.value, motion.duration);
                break;

            case MotionType.ExitRight:
                SetMotionSiblingOrder(slot);
                yield return slot.ExitRoutine(Vector2.right, motion.value, motion.duration);
                break;

            case MotionType.EnterLeft:
                SetMotionSiblingOrder(slot);
                yield return slot.EnterRoutine(finalPos, Vector2.left, motion.value, motion.duration, isUnknown);
                break;

            case MotionType.EnterRight:
                SetMotionSiblingOrder(slot);
                yield return slot.EnterRoutine(finalPos, Vector2.right, motion.value, motion.duration, isUnknown);
                break;

            case MotionType.Shake:
                yield return slot.Shake(motion.duration, motion.value);
                break;

            case MotionType.Emphasis:
                SetMotionSiblingOrder(slot);
                yield return slot.EmphasisRoutine();
                break;

            case MotionType.Nervous:
                yield return slot.NervousRoutine();
                break;
            
            case MotionType.Sit:
                yield return slot.SitRoutine(motion.value, motion.duration);
                break;

            case MotionType.Stand:
                SetMotionSiblingOrder(slot);
                yield return slot.StandRoutine(motion.duration);
                break;
            
            case MotionType.Greet:
                SetMotionSiblingOrder(slot);
                yield return slot.GreetRoutine(motion.value, motion.targetValue, motion.duration);
                break;
            
            case MotionType.Attack:
            {
                SetMotionSiblingOrder(slot);
                var target = FindSlot(motion.targetSlotIndex);
                Vector2 dir = target != null ? GetDirection(slot, target) : Vector2.right;
                if (motion.value >= 50f)
                {
                    StoryManager.Instance.ScreenEffectUI.PlayImpact(motion.value, motion.duration, false);
                }
                yield return slot.PlayAttack(dir, motion.value, motion.duration * 0.5f);
                break;
            }
            
            case MotionType.EnterMove:
            {
                SetMotionSiblingOrder(slot);
                Vector2 targetPos = new Vector2(motion.targetValue, slot.SlotRoot.anchoredPosition.y);
                yield return slot.EnterMoveRoutine(motion.value, targetPos, motion.duration, isUnknown);
                break;
            }
        }
    }
    
    public IEnumerator PlayMotionRoutine(MotionCommand cmd, bool isUnknown)
    {
        var actor = FindSlot(cmd.characterID);

        if (actor == null)
            yield break;

        var motion = cmd.motion;

        if (motion.useFlip)
        {
            actor.SetFlip(motion.flip);
        }
        
        switch (motion.type)
        {
            case MotionType.Attack:
            {
                var target = FindSlot(cmd.targetID);
                Vector2 dir = target != null ? GetDirection(actor, target) : Vector2.right;
                // 공격 먼저
                yield return actor.PlayAttack(dir, motion.value, motion.duration * 0.5f);
                // 피격
                if (target != null && cmd.targetID != cmd.characterID)
                {
                    yield return target.PlayHit(-dir, motion.value * 0.5f, motion.duration * 0.5f);
                }
                break;
            }

            case MotionType.Hit:
            {
                var target = FindSlot(cmd.targetID);
                Vector2 dir = target != null ? GetDirection(target, actor) : Vector2.left;
                yield return actor.PlayHit(dir, motion.value, motion.duration);
                break;
            }

            default:
            {
                yield return PlayMotionRoutine(cmd.characterID, motion, isUnknown);
                break;
            }
        }
    }
    
    
    public IEnumerator PlayMotionsParallel(List<MotionCommand> motions, bool isUnknown)
    {
        if (motions == null || motions.Count == 0)
            yield break;

        int completeCount = 0;

        foreach (var cmd in motions)
        {
            StartCoroutine(ParallelMotionRoutine(cmd, isUnknown, () =>
            {
                completeCount++;
            }));
        }

        yield return new WaitUntil(() => completeCount >= motions.Count);
    }

    private IEnumerator ParallelMotionRoutine(MotionCommand cmd, bool isUnknown, Action onComplete)
    {
        yield return StartCoroutine(PlayMotionRoutine(cmd, isUnknown));
        onComplete?.Invoke();
    }
    
    private Vector2 GetDirection(CharacterSlot from, CharacterSlot to)
    {
        if (from == to)
            return Vector2.right;
        
        RectTransform a = from.GetComponent<RectTransform>();
        RectTransform b = to.GetComponent<RectTransform>();

        Vector2 result = (b.anchoredPosition - a.anchoredPosition);
        result.y = 0;
        
        return result.normalized;
    }
    
    public CharacterSlot FindSlot(int characterID)
    {
        if (!currentSlotMap.TryGetValue(characterID, out int slotIndex))
            return null;

        if (slotIndex < 0 || slotIndex >= slots.Count)
            return null;

        return slots[slotIndex];
    }
    
    public void ShowEmotionIcon(int characterID, Emotion emotion)
    {
        var slot = FindSlot(characterID);
        if (slot == null) return;

        if (!dataDict.TryGetValue(characterID, out var data)) return;

        var icon = data.emotionIcons.Find(e => e.emotion == emotion);

        if (icon != null)
        {
            slot.PlayEmotionIcon(emotion);
        }
    }
    
    public Character GetCharacterData(int characterID)
    {
        if (dataDict.TryGetValue(characterID, out var data))
            return data;

        return null;
    }
    
    public IEnumerator FadeOut(float duration)
    {
        canvasGroup.DOKill();

        yield return canvasGroup.DOFade(0f, duration).SetEase(Ease.InQuad).WaitForCompletion();
    }
    
    public void ResetFade()
    {
        canvasGroup.DOKill();
        canvasGroup.alpha = 1f;
    }
}
