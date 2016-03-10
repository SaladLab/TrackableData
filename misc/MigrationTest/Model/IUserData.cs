using System;
using ProtoBuf;
using TrackableData;

namespace Model
{
    [ProtoContract]
    public interface IUserData : ITrackablePoco<IUserData>
    {
        [ProtoMember(1)] string Name { get; set; }
        [ProtoMember(2)] string EncPhotoUrl { get; set; }
        [ProtoMember(3)] UserPermission Permission { get; set; }
        [ProtoMember(4)] int LastItemId { get; set; }
        [ProtoMember(5)] int LastPostId { get; set; }
        [ProtoMember(6)] short Stamina { get; set; }
        [ProtoMember(7)] DateTime StaminaRefillTime { get; set; }
        [ProtoMember(8)] short RaidStamina { get; set; }
        [ProtoMember(9)] DateTime RaidStaminaRefillTime { get; set; }
        [ProtoMember(10)] int Gold { get; set; }
        [ProtoMember(11)] int RubyCash { get; set; }
        [ProtoMember(12)] int RubyEvent { get; set; }
        [ProtoMember(13)] int Fame { get; set; }
        [ProtoMember(14)] int Medal { get; set; }
        [ProtoMember(15)] int JackpotKey { get; set; }
        [ProtoMember(16)] short Rank { get; set; }
        [ProtoMember(17)] int Level { get; set; }
        [ProtoMember(18)] int Exp { get; set; }
        [ProtoMember(19)] short VipLevel { get; set; }
        [ProtoMember(20)] int VipPoint { get; set; }
        [ProtoMember(21)] int TotalMoneySpent { get; set; }
        [ProtoMember(22)] int MaxStage { get; set; }
        [ProtoMember(23)] int SuperiorMaxStage { get; set; }
        [ProtoMember(24)] int MainStage { get; set; }
        [ProtoMember(25)] byte MainTeamId { get; set; }
        [ProtoMember(26)] int MainItemId { get; set; }
        [ProtoMember(27)] int SecondaryItemId { get; set; }
        [ProtoMember(28)] int MainTankId { get; set; }
        [ProtoMember(29)] DateTime LastRubyMadeTime { get; set; }
        [ProtoMember(30)] DateTime DayChangeTime { get; set; }
        [ProtoMember(31)] byte MissionReplaceCount { get; set; }
        [ProtoMember(32)] byte FestivalVisitProgress { get; set; }
        [ProtoMember(33)] byte FestivalCustomVisitProgress { get; set; }
        [ProtoMember(34)] byte FestivalNewUserSupportProgress { get; set; }
        [ProtoMember(35)] byte FestivalReturnUserSupportProgress { get; set; }
        [ProtoMember(36)] short GrowPackageRewardProgress { get; set; }
        [ProtoMember(37)] int HordeMaxStage { get; set; }
        [ProtoMember(38)] int EndlessLastLeagueId { get; set; }
        [ProtoMember(39)] DateTime EndlessLastPlayTime { get; set; }
        [ProtoMember(40)] int EndlessMaxDistance { get; set; }
        [ProtoMember(41)] int EndlessMaxDistanceRewardIndex { get; set; }
        [ProtoMember(42)] int Tutorial { get; set; }
        [ProtoMember(43)] int UserFlag { get; set; }
        [ProtoMember(44)] int UserConfigFlag { get; set; }
        [ProtoMember(45)] int Locale { get; set; }
        [ProtoMember(46)] int InvitedFriendCount { get; set; }
        [ProtoMember(47)] int RemainingQuickPlayCount { get; set; }
        [ProtoMember(48)] short SpecialFriendRewardSendCount { get; set; }
        [ProtoMember(49)] int DesiredFriendRewardSetting { get; set; }
        [ProtoMember(50)] string Comment { get; set; }
        [ProtoMember(51)] DateTime CommentModifyTime { get; set; }
        [ProtoMember(52)] short InventoryExtraSize { get; set; }
        [ProtoMember(53)] byte GroupExtraSize { get; set; }
        [ProtoMember(54)] byte GroupPostBoxExtraSize { get; set; }
        [ProtoMember(55)] byte GroupPostBoxExtraTime { get; set; }
        [ProtoMember(56)] int GuildId { get; set; }
        [ProtoMember(57), TrackableProperty("sql.ignore")] short GuildLevel { get; set; }
        [ProtoMember(58)] byte FacebookFriendCountRewardIndex { get; set; }
        [ProtoMember(59)] byte FirstSignInBonusReceived { get; set; }
        [ProtoMember(60)] string RegisterCountry { get; set; }
        [ProtoMember(61)] string EncFacebookId { get; set; }
        [ProtoMember(62)] byte AccountType { get; set; }
        [ProtoMember(63)] string PlatformCode { get; set; }
    }
}
