using System;
using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public interface IUserData : ITrackablePoco
    {
        [ProtoMember(1)]
        string Name { get; set; }
        [ProtoMember(2)]
        string EncPhotoUrl { get; set; }
        [ProtoMember(3)]
        UserPermission Permission { get; set; }
        [ProtoMember(4)]
        int Inviter { get; set; }
        [ProtoMember(5)]
        int LastItemId { get; set; }
        [ProtoMember(6)]
        int LastPostId { get; set; }
        [ProtoMember(7)]
        short Stamina { get; set; }
        [ProtoMember(8)]
        DateTime StaminaRefillTime { get; set; }
        [ProtoMember(9)]
        short RaidStamina { get; set; }
        [ProtoMember(10)]
        DateTime RaidStaminaRefillTime { get; set; }
        [ProtoMember(11)]
        int Gold { get; set; }
        [ProtoMember(12)]
        int RubyCash { get; set; }
        [ProtoMember(13)]
        int RubyEvent { get; set; }
        [ProtoMember(14)]
        int Fame { get; set; }
        [ProtoMember(15)]
        int Medal { get; set; }
        [ProtoMember(16)]
        int LuckyClover { get; set; }
        [ProtoMember(17)]
        int JackpotKey { get; set; }
        [ProtoMember(18)]
        short Rank { get; set; }
        [ProtoMember(19)]
        short VipLevel { get; set; }
        [ProtoMember(20)]
        int TotalMoneySpent { get; set; }
        [ProtoMember(21)]
        byte TankArmor { get; set; }
        [ProtoMember(22)]
        byte TankEngine { get; set; }
        [ProtoMember(23)]
        byte TankGoldBonus { get; set; }
        [ProtoMember(24)]
        byte TankSeats { get; set; }
        [ProtoMember(25)]
        int MaxStage { get; set; }
        [ProtoMember(26)]
        int MainStage { get; set; }
        [ProtoMember(27)]
        int MainItemId { get; set; }
        [ProtoMember(28)]
        int MainTankId { get; set; }
        [ProtoMember(29)]
        DateTime DayChangeTime { get; set; }
        [ProtoMember(30)]
        byte MissionReplaceCount { get; set; }
        [ProtoMember(31)]
        byte FestivalVisitProgress { get; set; }
        [ProtoMember(32)]
        int FestivalPreRegistrationId { get; set; }
        [ProtoMember(33)]
        byte FestivalPreRegistrationProgress { get; set; }
        [ProtoMember(34)]
        byte FestivalNewUserSupportProgress { get; set; }
        [ProtoMember(35)]
        int FestivalInviteFriendsId { get; set; }
        [ProtoMember(36)]
        byte FestivalInvitedFriendRankRewardProgress { get; set; }
        [ProtoMember(37)]
        byte FestivalReturnUserSupportProgress { get; set; }
        [ProtoMember(38)]
        int HordeMaxStage { get; set; }
        [ProtoMember(39)]
        int EndlessLastLeagueId { get; set; }
        [ProtoMember(40)]
        DateTime EndlessLastPlayTime { get; set; }
        [ProtoMember(41)]
        int EndlessMaxDistance { get; set; }
        [ProtoMember(42)]
        int EndlessMaxDistanceRewardIndex { get; set; }
        [ProtoMember(43)]
        UserTutorial Tutorial { get; set; }
        [ProtoMember(44)]
        UserFlag UserFlag { get; set; }
        [ProtoMember(45)]
        UserConfigFlag UserConfigFlag { get; set; }
        [ProtoMember(46)]
        int InvitedFriendCount { get; set; }
        [ProtoMember(47)]
        int RemainingQuickPlayCount { get; set; }
        [ProtoMember(48)]
        string Comment { get; set; }
        [ProtoMember(49)]
        DateTime CommentModifyTime { get; set; }
        [ProtoMember(50)]
        short InventoryExtraSize { get; set; }
        [ProtoMember(51)]
        byte GameFriendExtraSize { get; set; }
        [ProtoMember(52)]
        byte GroupExtraSize { get; set; }
        [ProtoMember(53)]
        byte GroupPostBoxExtraSize { get; set; }
        [ProtoMember(54)]
        byte GroupPostBoxExtraTime { get; set; }
    }
}
