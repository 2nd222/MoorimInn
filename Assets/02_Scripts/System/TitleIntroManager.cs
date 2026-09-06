using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TitleIntroManager : MonoBehaviour
{
    [SerializeField] private GameObject teamLogo;

    [SerializeField] private TitleMenuAnimator menuAnimator;

    [SerializeField] private float logoDuration = 2f;
    [SerializeField] private float fadeDuration = 1f;
    
    private Coroutine introRoutine;
    private bool skipped;
    
    private IEnumerator Start()
    {
        yield return introRoutine = StartCoroutine(PlaySequence());
    }
    
    private void Update()
    {
        if (skipped)
            return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            skipped = true;

            if (introRoutine != null)
                StopCoroutine(introRoutine);

            SkipIntro();
        }
    }
    
    private IEnumerator PlaySequence()
    {
        // 처음부터 화면을 검게
        LoadUIManager.Instance.SetFadeImmediate(true);

        // 팀 로고
        teamLogo.SetActive(true);

        yield return new WaitForSeconds(logoDuration);

        // 로고 제거
        teamLogo.SetActive(false);

        // 검은 화면 제거
        yield return LoadUIManager.Instance.FadeIn(fadeDuration);

        // 메인메뉴 애니메이션
        menuAnimator.Play();
    }
    
    private void SkipIntro()
    {
        DOTween.Kill(this);

        teamLogo.SetActive(false);

        LoadUIManager.Instance.SetFadeImmediate(false);

        menuAnimator.ShowImmediate();
    }
}
