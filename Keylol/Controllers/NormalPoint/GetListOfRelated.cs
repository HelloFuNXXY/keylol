﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Provider;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     取得指定游戏据点的相关据点（开发商、发行商、系列、流派、特性），这些据点在发表文章时会自动推送
        /// </summary>
        /// <param name="id">游戏据点 ID</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}/related")]
        [HttpGet]
        [ResponseType(typeof (List<GetListOfRelatedEntryDTO>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定据点不存在或者不是游戏据点")]
        public async Task<IHttpActionResult> GetListOfRelated(string id, IdType idType = IdType.Id)
        {
            var point = await DbContext.NormalPoints
                .SingleOrDefaultAsync(p => idType == IdType.IdCode ? p.IdCode == id : p.Id == id);
            if (point == null || point.Type != NormalPointType.Game)
                return NotFound();

            var cacheKey = $"point:{point.Id}:related";
            var cache = await RedisProvider.Get(cacheKey);
            if (cache.HasValue)
                return Ok(RedisProvider.Deserialize(cache, true));

            var result = point.DeveloperPoints
                .Concat(point.PublisherPoints)
                .Concat(point.SeriesPoints)
                .Concat(point.GenrePoints)
                .Concat(point.TagPoints)
                .Distinct()
                .Select(p => new GetListOfRelatedEntryDTO
                {
                    Id = p.Id,
                    IdCode = p.IdCode,
                    Name = GetPreferredName(p)
                }).ToList();
            await RedisProvider.Set(cacheKey, RedisProvider.Serialize(result), TimeSpan.FromDays(30));
            return Ok(result);
        }

        public class GetListOfRelatedEntryDTO
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string IdCode { get; set; }
        }
    }
}