﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     据点关系
        /// </summary>
        public enum PointRelationship
        {
            /// <summary>
            ///     开发商
            /// </summary>
            Developer,

            /// <summary>
            ///     发行商
            /// </summary>
            Publisher,

            /// <summary>
            ///     系列
            /// </summary>
            Series,

            /// <summary>
            ///     流派
            /// </summary>
            Genre,

            /// <summary>
            ///     特性
            /// </summary>
            Tag
        }

        /// <summary>
        ///     取得指定厂商或类型据点旗下的游戏据点
        /// </summary>
        /// <param name="id">据点 ID</param>
        /// <param name="relationship">该厂商或类型据点与想要获取游戏据点的关系</param>
        /// <param name="stats">是否包含据点的读者数、文章数、订阅状态，默认 false</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，最大 50，默认 9</param>
        [Route("{id}/games")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof(List<NormalPointDto>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在或者不是厂商或类型据点")]
        public async Task<IHttpActionResult> GetListOfRelatedGames(string id, PointRelationship relationship,
            bool stats = false, NormalPointIdentityType idType = NormalPointIdentityType.Id, int skip = 0, int take = 9)
        {
            if (take > 50)
                take = 50;

            var point = await _dbContext.NormalPoints
                .SingleOrDefaultAsync(p => idType == NormalPointIdentityType.IdCode ? p.IdCode == id : p.Id == id);
            if (point == null || (point.Type != NormalPointType.Manufacturer && point.Type != NormalPointType.Genre))
                return NotFound();

            var queryBase = _dbContext.NormalPoints.Where(p => p.Id == point.Id);
            IQueryable<Models.NormalPoint> queryNext;
            switch (relationship)
            {
                case PointRelationship.Developer:
                    queryNext = queryBase.SelectMany(p => p.DeveloperForPoints);
                    break;

                case PointRelationship.Publisher:
                    queryNext = queryBase.SelectMany(p => p.PublisherForPoints);
                    break;

                case PointRelationship.Series:
                    queryNext = queryBase.SelectMany(p => p.SeriesForPoints);
                    break;

                case PointRelationship.Genre:
                    queryNext = queryBase.SelectMany(p => p.GenreForPoints);
                    break;

                case PointRelationship.Tag:
                    queryNext = queryBase.SelectMany(p => p.TagForPoints);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null);
            }
            var points = await queryNext.OrderByDescending(p => p.ReleaseDate)
                .Skip(() => skip).Take(() => take).ToListAsync();
            var userId = User.Identity.GetUserId();
            return Ok(points.Select(p =>
            {
                var dto = new NormalPointDto(p)
                {
                    ReleaseDate = p.ReleaseDate
                };
                if (stats)
                {
                    dto.SubscriberCount = _dbContext.NormalPoints.Where(np => np.Id == p.Id)
                        .Select(np => np.Subscribers.Count)
                        .Single();
                    dto.ArticleCount = _dbContext.NormalPoints.Where(np => np.Id == p.Id)
                        .Select(np => np.Articles.Count)
                        .Single();
                    dto.Subscribed = _dbContext.NormalPoints.Where(np => np.Id == p.Id)
                        .SelectMany(np => np.Subscribers)
                        .Select(u => u.Id)
                        .Contains(userId);
                }
                return dto;
            }));
        }
    }
}