﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Keylol.Models;
using Keylol.Models.DAL;
using Keylol.Provider.CachedDataProvider;
using Keylol.States.Shared;
using Keylol.StateTreeManager;
using Keylol.Utilities;

namespace Keylol.States.Entrance.Points
{
    /// <summary>
    /// 哨所据点列表
    /// </summary>
    public class OutpostPointList : List<OutpostPoint>
    {
        private OutpostPointList(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// 获取哨所据点列表
        /// </summary>
        /// <param name="page">分页页码</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="OutpostPointList"/></returns>
        public static async Task<OutpostPointList> Get(int page, [Injected] KeylolDbContext dbContext,
            [Injected] CachedDataProvider cachedData)
        {
            return await CreateAsync(StateTreeHelper.GetCurrentUserId(), page, 12, dbContext, cachedData);
        }

        /// <summary>
        /// 创建 <see cref="OutpostPointList"/>
        /// </summary>
        /// <param name="currentUserId">当前登录用户 ID</param>
        /// <param name="page">分页页码</param>
        /// <param name="recordsPerPage">每页数量</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <param name="cachedData"><see cref="CachedDataProvider"/></param>
        /// <returns><see cref="OutpostPointList"/></returns>
        public static async Task<OutpostPointList> CreateAsync(string currentUserId, int page, int recordsPerPage,
            KeylolDbContext dbContext, CachedDataProvider cachedData)
        {
            var queryResult = await (from feed in dbContext.Feeds
                where feed.StreamName == OutpostStream.Name
                join point in dbContext.Points on feed.Entry equals point.Id
                orderby feed.Id descending
                select new
                {
                    FeedId = feed.Id,
                    point.Id,
                    point.IdCode,
                    point.AvatarImage,
                    point.ChineseName,
                    point.EnglishName,
                    point.TitleCoverImage,
                    point.MultiPlayer,
                    point.SinglePlayer,
                    point.Coop,
                    point.CaptionsAvailable,
                    point.CommentaryAvailable,
                    point.IncludeLevelEditor,
                    point.Achievements,
                    point.Cloud,
                    point.LocalCoop,
                    point.SteamTradingCards,
                    point.SteamWorkshop,
                    point.InAppPurchases,
                    point.SteamAppId,
                    point.SteamPrice,
                    point.SteamDiscountedPrice,
                    point.SonkwoProductId,
                    point.SonkwoPrice,
                    point.SonkwoDiscountedPrice,
                    point.UplayLink,
                    point.UplayPrice,
                    point.XboxLink,
                    point.XboxPrice,
                    point.PlayStationLink,
                    point.PlayStationPrice,
                    point.OriginLink,
                    point.OriginPrice,
                    point.WindowsStoreLink,
                    point.WindowsStorePrice,
                    point.AppStoreLink,
                    point.AppStorePrice,
                    point.GooglePlayLink,
                    point.GooglePlayPrice,
                    point.GogLink,
                    point.GogPrice,
                    point.BattleNetLink,
                    point.BattleNetPrice,
                    Categories = (from relationship in dbContext.PointRelationships
                        where relationship.SourcePointId == point.Id &&
                              relationship.Relationship == PointRelationshipType.Tag
                        select new
                        {
                            relationship.TargetPoint.IdCode,
                            relationship.TargetPoint.ChineseName,
                            relationship.TargetPoint.EnglishName
                        }).ToList()
                })
                .TakePage(page, recordsPerPage)
                .ToListAsync();
            var result = new OutpostPointList(queryResult.Count);
            foreach (var p in queryResult)
            {
                result.Add(new OutpostPoint
                {
                    FeedId = p.FeedId,
                    Id = p.Id,
                    IdCode = p.IdCode,
                    AvatarImage = p.AvatarImage,
                    ChineseName = p.ChineseName,
                    EnglishName = p.EnglishName,
                    AverageRating = (await cachedData.Points.GetRatingsAsync(p.Id)).AverageRating,
                    TitleCoverImage = p.TitleCoverImage,
                    MultiPlayer = p.MultiPlayer ? true : (bool?) null,
                    SinglePlayer = p.SinglePlayer ? true : (bool?) null,
                    Coop = p.Coop ? true : (bool?) null,
                    CaptionsAvailable = p.CaptionsAvailable ? true : (bool?) null,
                    CommentaryAvailable = p.CommentaryAvailable ? true : (bool?) null,
                    IncludeLevelEditor = p.IncludeLevelEditor ? true : (bool?) null,
                    Achievements = p.Achievements ? true : (bool?) null,
                    Cloud = p.Cloud ? true : (bool?) null,
                    LocalCoop = p.LocalCoop ? true : (bool?) null,
                    SteamTradingCards = p.SteamTradingCards ? true : (bool?) null,
                    SteamWorkshop = p.SteamWorkshop ? true : (bool?) null,
                    InAppPurchases = p.InAppPurchases ? true : (bool?) null,
                    SteamAppId = p.SteamAppId,
                    SteamPrice = p.SteamPrice,
                    SteamDiscountedPrice = p.SteamDiscountedPrice,
                    SonkwoProductId = p.SonkwoProductId,
                    SonkwoPrice = p.SonkwoPrice,
                    SonkwoDiscountedPrice = p.SonkwoDiscountedPrice,
                    UplayLink = p.UplayLink,
                    UplayPrice = p.UplayPrice,
                    XboxLink = p.XboxLink,
                    XboxPrice = p.XboxPrice,
                    PlayStationLink = p.PlayStationLink,
                    PlayStationPrice = p.PlayStationPrice,
                    OriginLink = p.OriginLink,
                    OriginPrice = p.OriginPrice,
                    WindowsStoreLink = p.WindowsStoreLink,
                    WindowsStorePrice = p.WindowsStorePrice,
                    AppStoreLink = p.AppStoreLink,
                    AppStorePrice = p.AppStorePrice,
                    GooglePlayLink = p.GooglePlayLink,
                    GooglePlayPrice = p.GooglePlayPrice,
                    GogLink = p.GogLink,
                    GogPrice = p.GogPrice,
                    BattleNetLink = p.BattleNetLink,
                    BattleNetPrice = p.BattleNetPrice,
                    Categories = p.Categories.Select(c => new PointBasicInfo
                    {
                        IdCode = c.IdCode,
                        ChineseName = string.IsNullOrWhiteSpace(c.ChineseName) ? c.EnglishName : c.ChineseName
                    }).ToList(),
                    Subscribed = string.IsNullOrWhiteSpace(currentUserId)
                        ? (bool?) null
                        : await cachedData.Subscriptions.IsSubscribedAsync(currentUserId, p.Id,
                            SubscriptionTargetType.Point),
                    InLibrary = string.IsNullOrWhiteSpace(currentUserId) || p.SteamAppId == null
                        ? (bool?) null
                        : await cachedData.Users.IsSteamAppInLibraryAsync(currentUserId, p.SteamAppId.Value)
                });
            }
            return result;
        }
    }

    /// <summary>
    /// 哨所据点
    /// </summary>
    public class OutpostPoint
    {
        /// <summary>
        /// Feed ID
        /// </summary>
        public long? FeedId { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 识别码
        /// </summary>
        public string IdCode { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }

        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 平均评分
        /// </summary>
        public double? AverageRating { get; set; }

        /// <summary>
        /// 标题封面
        /// </summary>
        public string TitleCoverImage { get; set; }

        #region 特性属性

        /// <summary>
        /// 多人游戏
        /// </summary>
        public bool? MultiPlayer { get; set; }

        /// <summary>
        /// 单人游戏
        /// </summary>
        public bool? SinglePlayer { get; set; }

        /// <summary>
        /// 合作
        /// </summary>
        public bool? Coop { get; set; }

        /// <summary>
        /// 视听字幕
        /// </summary>
        public bool? CaptionsAvailable { get; set; }

        /// <summary>
        /// 旁白解说
        /// </summary>
        public bool? CommentaryAvailable { get; set; }

        /// <summary>
        /// 关卡客制化
        /// </summary>
        public bool? IncludeLevelEditor { get; set; }

        /// <summary>
        /// 成就系统
        /// </summary>
        public bool? Achievements { get; set; }

        /// <summary>
        /// 云存档
        /// </summary>
        public bool? Cloud { get; set; }

        /// <summary>
        /// 本地多人
        /// </summary>
        public bool? LocalCoop { get; set; }

        /// <summary>
        /// Steam 卡牌
        /// </summary>
        public bool? SteamTradingCards { get; set; }

        /// <summary>
        /// Steam 创意工坊
        /// </summary>
        public bool? SteamWorkshop { get; set; }

        /// <summary>
        /// 内购
        /// </summary>
        public bool? InAppPurchases { get; set; }

        #endregion

        #region 商店信息

        /// <summary>
        /// Steam App ID
        /// </summary>
        public int? SteamAppId { get; set; }

        /// <summary>
        /// Steam 价格
        /// </summary>
        public double? SteamPrice { get; set; }

        /// <summary>
        /// Steam 折后价格
        /// </summary>
        public double? SteamDiscountedPrice { get; set; }

        /// <summary>
        /// 杉果 Product ID
        /// </summary>
        public int? SonkwoProductId { get; set; }

        /// <summary>
        /// 杉果价格
        /// </summary>
        public double? SonkwoPrice { get; set; }

        /// <summary>
        /// 杉果折后价格
        /// </summary>
        public double? SonkwoDiscountedPrice { get; set; }

        /// <summary>
        /// Uplay 链接
        /// </summary>
        public string UplayLink { get; set; }

        /// <summary>
        /// Uplay 价格
        /// </summary>
        public string UplayPrice { get; set; }

        /// <summary>
        /// Xbox 链接
        /// </summary>
        public string XboxLink { get; set; }

        /// <summary>
        /// Xbox 价格
        /// </summary>
        public string XboxPrice { get; set; }

        /// <summary>
        /// PlayStation 链接
        /// </summary>
        public string PlayStationLink { get; set; }

        /// <summary>
        /// PlayStation 价格
        /// </summary>
        public string PlayStationPrice { get; set; }

        /// <summary>
        /// Origin 链接
        /// </summary>
        public string OriginLink { get; set; }

        /// <summary>
        /// Origin 价格
        /// </summary>
        public string OriginPrice { get; set; }

        /// <summary>
        /// Windows Store 链接
        /// </summary>
        public string WindowsStoreLink { get; set; }

        /// <summary>
        /// Windows Store 价格
        /// </summary>
        public string WindowsStorePrice { get; set; }

        /// <summary>
        /// App Store 链接
        /// </summary>
        public string AppStoreLink { get; set; }

        /// <summary>
        /// App Store 价格
        /// </summary>
        public string AppStorePrice { get; set; }

        /// <summary>
        /// Google Play 链接
        /// </summary>
        public string GooglePlayLink { get; set; }

        /// <summary>
        /// Google Play 价格
        /// </summary>
        public string GooglePlayPrice { get; set; }

        /// <summary>
        /// Gog 链接
        /// </summary>
        public string GogLink { get; set; }

        /// <summary>
        /// GOG 价格
        /// </summary>
        public string GogPrice { get; set; }

        /// <summary>
        /// 战网链接
        /// </summary>
        public string BattleNetLink { get; set; }

        /// <summary>
        /// 战网价格
        /// </summary>
        public string BattleNetPrice { get; set; }

        #endregion

        /// <summary>
        /// 类型
        /// </summary>
        public List<PointBasicInfo> Categories { get; set; }

        /// <summary>
        /// 是否已订阅
        /// </summary>
        public bool? Subscribed { get; set; }

        /// <summary>
        /// 是否已入库
        /// </summary>
        public bool? InLibrary { get; set; }
    }
}