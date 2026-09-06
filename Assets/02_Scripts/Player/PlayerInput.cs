using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static Constants;

public class PlayerInput : MonoBehaviour
{
    private Camera mainCamera;

    [Header("클릭 시 나올 마우스 클릭")]
    public GameObject clickCircle;
    private ParticleSystem clickCircleParticles;
    private Coroutine clickCoroutine;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnsceneLoaded;
    }

    void Start()
    {
        mainCamera = Camera.main;
        clickCircleParticles = clickCircle.GetComponent<ParticleSystem>();
    }

    private void OnsceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnsceneLoaded;
    }

    void Update()
    {
        // 글로벌 UI 입력 (어떤 상태에서든 항상 처리)
        GlobalUIInput();
 
        // 캐릭터 전환
        PlayerSelection();
        
        // O키를 누르면 달리기
        if (Input.GetMouseButtonDown(2))
        {
            if (UpgradeManager.Instance.HasCharacterSkill(CharacterSkillLevel.Ghost) &&
                SkillUIManager.Instance.CanUseGhost())
            {
                SkillUIManager.Instance.UseGhostSkill();
            }
        }

        if (GameManager.Instance.CurrentState == GameFlowState.Gameplay)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("호출 인벤토리");
                InventoryManager.Instance.ToggleCommonInventory();
            }
        }
 
        // 마우스 클릭
        if (Input.GetMouseButtonDown(0))
            MouseClick();
    }

    private void GlobalUIInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (GameManager.Instance.CurrentState == GameFlowState.Story)
                return;
            
            Debug.Log(UIManager.Instance.IsBookActive);
            UIManager.Instance.ActiveBook(!UIManager.Instance.IsBookActive);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.CurrentState == GameFlowState.Story)
                return;
            
            if (CookingController.Instance != null && CookingController.Instance.IsCookingUIOpen)
            {
                CookingController.Instance.CloseCookingSystem();
                return;
            }
            
            if (UIManager.Instance.IsBookActive)
            {
                UIManager.Instance.ActiveBook(false);
                return;
            }

            if (UIManager.Instance.IsSaveLoadActive)
            {
                UIManager.Instance.ActiveSaveUI(false);
                return;
            }
            
            Debug.Log(!UIManager.Instance.IsSettingActive);
            UIManager.Instance.ActiveSetting(!UIManager.Instance.IsSettingActive);
        }
    }

    // 캐릭터 전환
    private void PlayerSelection()
    {
        if (PlayerManager.Instance.BlockSwap) return;

        var currentUnit = PlayerManager.Instance.SelectedUnit;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentUnit.PlayerType == PlayerType.Myungwol) 
                return;
            PlayerManager.Instance.SelectPlayer(PlayerType.Myungwol);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentUnit.PlayerType == PlayerType.Sowol) 
                return;
            PlayerManager.Instance.SelectPlayer(PlayerType.Sowol);
        }
    }

    // 마우스 클릭
    private void MouseClick()
    {
        // UI 위를 클릭했으면 게임, 월드 클릭 무시
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            return;
        
        Debug.Log($"Raycast Hit: {hit.collider.gameObject.name}", hit.collider.gameObject);

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
 
        if (interactable != null)
            HandleInteractableClick(interactable);
        else
            HandleGroundClick(hit.point);
    }

    // IInteractable 오브젝트 클릭
    // 이미 트리거 안에 있으면 → 이동 없이 바로 재실행 (ClickToInteract 모드)
    // 트리거 밖이면 → 예약 후 이동
    private void HandleInteractableClick(IInteractable interactable)
    {
        PlayerType currentPlayer = PlayerManager.Instance.CurrentPlayer;

        if (!interactable.CanInteract(currentPlayer))
            return;
        
        if (interactable.Mode == InteractMode.Immediate)
        {
            PlayerManager.Instance.BeginInteract(interactable);
            return;
        }
        
        // 이미 해당 대상의 트리거 안에 있으면 이동 없이 바로 상호작용
        if (PlayerManager.Instance.ActiveInteractable == interactable)
        {
            PlayerManager.Instance.ExecuteInteract(interactable);
            return;
        }

        //  P키를 누른 상태라면 해당 위치로 순간이동
        if (UpgradeManager.Instance.HasCharacterSkill(CharacterSkillLevel.Teleport) &&
            SkillUIManager.Instance.CanUseTeleport())
        {
            if (Input.GetMouseButton(1))
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 teleportPos = interactable.GetInteractPosition();
                    PlayerManager.Instance.TeleportSelectedUnit(teleportPos);
            
                    SkillUIManager.Instance.UseTeleportSkill();
                    // 순간이동 직후 바로 상호작용 실행되도록 설정
                    PlayerManager.Instance.SelectedUnit.ActiveInteractable = interactable;
                    PlayerManager.Instance.ExecuteInteract(interactable);
                    return;
                }
            }
        }
        

        PlayerManager.Instance.ReservedInteractable = interactable;
        PlayerManager.Instance.MoveSelectedUnitTo(interactable.GetInteractPosition(), () =>
        {
            // 도착했을 때 아직 이 대상이 예약 상태면 실행 (이동 중 다른 걸 클릭했거나 트리거 진입으로 이미 실행됐으면 스킵됨)
            if (PlayerManager.Instance.ReservedInteractable == interactable)
                PlayerManager.Instance.ExecuteInteract(interactable);
        });
    }

    // 명월 전용 / 빈 땅 클릭 시 이동
    private void HandleGroundClick(Vector3 point)
    {
        if (PlayerManager.Instance.CurrentPlayer == PlayerType.Sowol) return;

        PlayerManager.Instance.ReservedInteractable = null;

        bool teleported = false;

        // 우클릭 + 좌클릭 && 텔레포트 사용 가능하면 순간이동
        if (Input.GetMouseButton(1) &&
            UpgradeManager.Instance.HasCharacterSkill(CharacterSkillLevel.Teleport) &&
            SkillUIManager.Instance.CanUseTeleport())
        {
            PlayerManager.Instance.TeleportSelectedUnit(point);
            SkillUIManager.Instance.UseTeleportSkill();
            teleported = true;
        }

        if (!teleported)
            PlayerManager.Instance.MoveSelectedUnitTo(point);

        // 마우스 클릭 이펙트
        if (clickCoroutine != null)
            StopCoroutine(clickCoroutine);

        clickCoroutine = StartCoroutine(ClickEffect(point));
    }

    private IEnumerator ClickEffect(Vector3 point)
    {
        clickCircle.SetActive(true);
        clickCircle.transform.position = point;
        clickCircleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        clickCircleParticles.Play();
        yield return new WaitForSeconds(2f);
        clickCircle.SetActive(false);
    }
}