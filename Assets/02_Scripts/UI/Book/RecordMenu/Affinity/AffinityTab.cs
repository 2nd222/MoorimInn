using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq;
using System.Collections.Generic;

public class AffinityTab : BaseTab
{
    private List<NPCProfile> listNPCProfiles = new List<NPCProfile>();
    // ���� ������
    [SerializeField] private GameObject objNPCProfile;
    [SerializeField] private Transform tsParent;

    // ������ ������
    [SerializeField] private GameObject objRightPage;
    [SerializeField] private Image imgPortrait;                  // �ʻ�ȭ �̹���
    [SerializeField] private TextMeshProUGUI txtName;            // �̸�
    [SerializeField] private TextMeshProUGUI txtDescription;     // �����
    [SerializeField] private TextMeshProUGUI txtLikes;           // �����ϴ� ��
    [SerializeField] private TextMeshProUGUI txtDislikes;        // �Ⱦ��ϴ� ��

    [SerializeField] private GameObject progressPrefab;
    [SerializeField] private Transform progressParent;
    [SerializeField] private BriefStoryInfo briefStoryInfo;
    
    private readonly List<StoryProgressSlot> progressSlots = new();

    public override void SetupData()
    {
        List<AffinityNPCData> affinity = DataManager.Instance.GetAffinityNPCList();
        UpdateNPCList(affinity);
    }

    private void UpdateNPCList(List<AffinityNPCData> affinityList)
    {
        for (int i = 0; i < affinityList.Count; i++)
        {
            if (i >= this.listNPCProfiles.Count)
            {
                NPCProfile profile = Instantiate(this.objNPCProfile, this.tsParent).GetComponent<NPCProfile>();
                this.listNPCProfiles.Add(profile);
            }
            this.listNPCProfiles[i].gameObject.SetActive(true);
            this.listNPCProfiles[i].Init(affinityList[i], ShowGuestProfile);
        }

        for (int i = affinityList.Count; i < this.listNPCProfiles.Count; i++)
        {
            this.listNPCProfiles[i].gameObject.SetActive(false);
        }
    }

    private void ShowGuestProfile(AffinityNPCData data)
    {
        this.objRightPage.SetActive(true);

        this.imgPortrait.sprite = data.profile;
        this.txtName.text = data.myName;
        this.txtDescription.text = data.description;

        this.txtLikes.text = data.likes != null ? string.Join(", ", data.likes) : "없음";
        this.txtDislikes.text = data.dislikes != null ? string.Join(", ", data.dislikes) : "없음";
        
        RefreshStoryProgress(data);
    }

    public override void ResetSetting() 
    {
        this.objRightPage.SetActive(false);

        this.imgPortrait.sprite = null;
        this.txtName.text = "";
        this.txtDescription.text = "";
        this.txtLikes.text = "";
        this.txtDislikes.text = "";
    }
    
    private void RefreshStoryProgress(AffinityNPCData data)
    {
        if (data.storyTriggers == null)
            return;

        List<StoryTrigger> triggers = data.storyTriggers.triggers;

        // 부족하면 생성
        while (progressSlots.Count < triggers.Count)
        {
            StoryProgressSlot slot = Instantiate(progressPrefab, progressParent).GetComponent<StoryProgressSlot>();

            progressSlots.Add(slot);
        }

        // 사용
        for (int i = 0; i < triggers.Count; i++)
        {
            progressSlots[i].gameObject.SetActive(true);

            bool played = StoryManager.Instance.IsStoryPlayed(triggers[i].storyId);
            Story story = StoryManager.Instance.GetStory(triggers[i].storyId);
            
            progressSlots[i].Init(story, briefStoryInfo, played);
        }

        // 남는 슬롯 숨김
        for (int i = triggers.Count; i < progressSlots.Count; i++)
        {
            progressSlots[i].gameObject.SetActive(false);
        }
    }
}
