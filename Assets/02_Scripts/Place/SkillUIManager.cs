using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIManager : Singleton<SkillUIManager>
{
    [Header("Ghost Skill UI")]
    [SerializeField] private Image ghostCover;
    [SerializeField] private TextMeshProUGUI ghostCooltimeText;
    public float ghostValidTime = 5f;
    public float ghostCooldown = 10f; 
    public bool isGhostCooldown = false; // 쿨타임 진행 여부 체크

    [Header("Teleport Skill UI")]
    [SerializeField] private Image teleportCover;
    [SerializeField] private TextMeshProUGUI teleportCooltimeText;
    public float teleportCooldown = 3f; 
    public bool isTeleportCooldown = false;

    private void Start()
    {
        // 시작 시 초기화
        ghostCover.fillAmount = 0f;
        ghostCover.transform.parent.gameObject.SetActive(false);
        ghostCooltimeText.gameObject.SetActive(false);

        teleportCover.fillAmount = 0f;
        teleportCover.transform.parent.gameObject.SetActive(false);
        teleportCooltimeText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        UpgradeManager.Instance.SelectedOptionUpgraded += RefreshSkillUnlock;
    }

    private void OnDisable()
    {
        if(UpgradeManager.Instance)
            UpgradeManager.Instance.SelectedOptionUpgraded -= RefreshSkillUnlock;
    }

    private void RefreshSkillUnlock(UpgradeType upgradeType = UpgradeType.Null)
    {
        ghostCover.transform.parent.gameObject.SetActive(UpgradeManager.Instance.HasCharacterSkill(CharacterSkillLevel.Ghost));
        teleportCover.transform.parent.gameObject.SetActive(UpgradeManager.Instance.HasCharacterSkill(CharacterSkillLevel.Teleport)); 
    }

    /// <summary>
    /// 고스트 스킬 사용 시 호출
    /// </summary>
    public void UseGhostSkill()
    {
        if (isGhostCooldown) return; 
        StartCoroutine(Co_GhostCooldown());
    }

    /// <summary>
    /// 텔레포트 스킬 사용 시 호출
    /// </summary>
    public void UseTeleportSkill()
    {
        if (isTeleportCooldown) return;
        StartCoroutine(Co_TeleportCooldown());
    }

    private IEnumerator Co_GhostCooldown()
    {
        isGhostCooldown = true;
        // 1. 스킬 즉시 발동 내가 직접 켠 경우만 기록
        if (!PlayerManager.Instance.IsRunMode)
        {
            PlayerManager.Instance.SetRunMode(true);
        }

        // 2. 쿨타임 UI 즉시 시작
        float remainingTime = ghostCooldown;
        ghostCooltimeText.gameObject.SetActive(true);

        bool isEffectActive = true; // 스킬 효과가 켜져 있는지 확인하는 플래그

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            
            // 흘러간 시간 계산 (전체 쿨타임 - 남은 시간)
            float elapsedTime = ghostCooldown - remainingTime;

            // 3. 스킬 효과 종료 (5초가 지났을 때 한 번만 실행)
            if (isEffectActive && elapsedTime >= ghostValidTime)
            {
                if (PlayerManager.Instance.IsRunMode)
                {
                    PlayerManager.Instance.SetRunMode(false);
                }
                isEffectActive = false; // 한 번 껐으니 플래그 닫기
            }

            // UI 업데이트 (1 -> 0 비율)
            ghostCover.fillAmount = remainingTime / ghostCooldown; 
            ghostCooltimeText.text = remainingTime.ToString("F1");

            yield return null; // 다음 프레임까지 대기
        }

        // 쿨타임 완전 종료 후 UI 끄기 및 초기화
        ghostCover.fillAmount = 0f;
        ghostCooltimeText.gameObject.SetActive(false);
        isGhostCooldown = false;
    }

    private IEnumerator Co_TeleportCooldown()
    {
        isTeleportCooldown = true;
        float remainingTime = teleportCooldown;

        teleportCooltimeText.gameObject.SetActive(true);

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            teleportCover.fillAmount = remainingTime / teleportCooldown;
            teleportCooltimeText.text = remainingTime.ToString("F1");

            yield return null;
        }

        teleportCover.fillAmount = 0f;
        teleportCooltimeText.gameObject.SetActive(false);
        isTeleportCooldown = false;
    }
    
    public bool CanUseTeleport()
    {
        return !isTeleportCooldown;
    }

    public bool CanUseGhost()
    {
        return !isGhostCooldown;
    }
}