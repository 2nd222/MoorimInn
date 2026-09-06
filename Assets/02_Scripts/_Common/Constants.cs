public class Constants
{
    public enum InventoryType
    {
       Fridge,   // 냉장고 (퀘스트 기준)
       Chef,      // 조리사 (임시 저장소)
       Server,
       MealTable,
       Ingredients,
       IngredientsButMeatOnly,
       BondItem,
       Guest
    }
    
    public enum IngredientRarity
    {
        Normal,
        Epic,
    }
    public enum RecipeClass
    {
        Normal,
        Event,
    }
    public enum IngredientType
    {
        Vegetable, //양파,당근,호박 같은 농산물
        Meats, // 돼지고기, 소고기, 닭고기, 계란같은 축산물
        Dairy, // 우유, 치즈 같은 유제품
        Spices, // 소금, 후추 같은 조미료
        Processed, // 두부, 어묵, 밀가루, 오일 같은 가공식재료
        
        Buff, // 가게 버프 아이템
        Recipe, // 레시피
    }
    public enum FoodType
    {
        MainDish,
        SideDish,
        Drink,
        Dessert
    }

    public enum PlayerType
    {
        Myungwol, Sowol
    }

    public enum GuestState 
    { Enter, Waiting, WaitOrder, WaitFood, Eat, Pay, Exit }

    public enum GuestMood 
    { VeryBad, Bad, Neutral, Good, VeryGood }

    public enum GuestType 
    { Normal, Rich, Thief }

    public enum GuestAppearance 
    { NormalMaleA, NormalMaleB, NormalFemaleA, NormalFemaleB, RichMan, ThiefGuy, BadGuest, ViolenceGuest }

    public enum FoodQuality 
    { Normal, Fine, Good }
    
    public enum FreshState
    { Good, Normal, Bad, None }
    
    public enum QuestCategory { Main, Sub, Repeat } // 메인, 서브, 반복

    public const float SPOIL_TIME = 30f;
    public const int MAX_FRIDGE_DAY = 3;
    public const int SERVINGSLOT_MAXCOUNT = 8;
    public const int FRIDGE_SIZE = 4;

    // 씬 이름
    public const string SPLASHSCREEN = "01.SplashScreen";
    public const string MAINMENU = "02.MainMenu";
    public const string GAME = "03.Game";
    public const string DIALOG = "04.Dialog";
}