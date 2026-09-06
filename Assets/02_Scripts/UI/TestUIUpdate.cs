// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class TestUIUpdate : MonoBehaviour
// {
//     [SerializeField] private Button fameUpButton;
//     [SerializeField] private Button fameDownButton;
//     [SerializeField] private Button moneyUpButton;
//     [SerializeField] private Button moneyDownButton;
//     [SerializeField] private EconomyManager _economyManager;
//     [SerializeField] private Button addQuestButton;
//     [SerializeField] private Button removeQuestButton;
//     [SerializeField] private QuestGenerator generator;
//     [SerializeField] private QuestAcceptBar acceptBar;
//     [SerializeField] private Button addQuestUI;
//     
//     List<Quest> quests = new List<Quest>();
//
//     void Awake()
//     {
//         fameDownButton.onClick.AddListener(() => _economyManager.AddFame(-1));
//         fameUpButton.onClick.AddListener(() => _economyManager.AddFame(1));
//         moneyUpButton.onClick.AddListener(() => _economyManager.AddMoney(1));
//         moneyDownButton.onClick.AddListener(() => _economyManager.AddMoney(-1));
//         addQuestButton.onClick.AddListener(() => TestQuestCreate());
//         removeQuestButton.onClick.AddListener(() => QuestManager.Instance.EraseQuestFromActive(quests[0]));
//         
//     }
//
//     void TestQuestCreate()
//     {
//         Debug.Log("TestQuestCreate");
//         Quest newQuest = generator.GenerateRandomQuest();
//         acceptBar.Init(newQuest);
//         quests.Add(newQuest);
//     }
// }
