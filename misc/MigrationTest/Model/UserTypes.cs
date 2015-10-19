using System;

namespace Model
{
    public enum UserStatus : byte
    {
        Normal = 0,
        Unregistered = 1,
        Banned = 2,
    }

    public enum UserPermission : byte
    {
        Normal = 0,
        Tester = 2,
        Administrator = 3,
        Developer = 4,
        TestBot = 5,
    }

    [Flags]
    public enum UserTutorial
    {
        StartGame = 0x0001,
        Team = 0x0002,
        EvolvePrepare = 0x0004,
        Evolve = 0x0008,
        RubyGachaPrepare = 0x0010,
        RubyGacha = 0x0020,
        Raise = 0x0040,
        Tank = 0x0080,
        MaterialEvolvePrepare = 0x0100,
        MaterialEvolve = 0x0200,
        Horde = 0x0400,
        EvolveStage = 0x0800,
        RaiseAndGoldStage = 0x1000,
        AppReview = 0x2000,
        Tank2 = 0x4000,
        FriendRaid = 0x8000,
        AutoPlay = 0x10000,
        QuickPlay = 0x20000,
        Awaken = 0x40000,
        EndlessPlay = 0x80000,
        EndlessReward = 0x100000
    }

    [Flags]
    public enum UserFlag : short
    {
        GachaFirstPickAdvantage = 0x0001,
        RubySubscriptionReward = 0x0002,
        TeamElementFireReward = 0x0004,
        TeamElementWaterReward = 0x0008,
        TeamElementWoodReward = 0x0010,
        LuckyCloverActive = 0x0040,
    }

    [Flags]
    public enum UserConfigFlag : short
    {
        ReceivePushNotification = 1,
        ReceiveLocalNotificationStaminaRefill = 2,
        ReceiveLocalNotificationHordeRankingEnd = 4,
        ReceiveLocalNotificationTakeRubySubscription = 8,
        PlayMusic = 16,
        PlaySound = 32,
        AllowScreenAlwaysVisible = 64,
        ShowProfilePhotoToOthers = 128
    }

    public enum UserCardState
    {
        NeverCollected = 0,
        Collected,
        MaxLevelAchieved
    }

    public enum UserStageGrade
    {
        None,
        Cleared,
        C,
        B,
        A,
        S,
        Ss,
        Sss
    }

    public enum UserStageDifficulty
    {
        None = 0,
        Normal,
        Hard,
        Hell
    }
}
