﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using JetBrains.Annotations;
using Keylol.Identity;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.CouponGift
{
    public partial class CouponGiftController
    {
        /// <summary>
        ///     创建一个文券礼品
        /// </summary>
        /// <param name="requestDto">请求 DTO</param>
        [Authorize(Roles = KeylolRoles.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(CouponGiftDto))]
        public async Task<IHttpActionResult> CreateOne([NotNull] CouponGiftCreateOneRequestDto requestDto)
        {
            var gift = _dbContext.CouponGifts.Create();
            gift.Name = requestDto.Name;
            gift.Descriptions = JsonConvert.SerializeObject(requestDto.Descriptions);
            gift.ThumbnailImage = requestDto.ThumbnailImage;
            gift.PreviewImage = requestDto.PreviewImage;
            gift.AcceptedFields = JsonConvert.SerializeObject(requestDto.AcceptedFields);
            gift.Price = requestDto.Price;
            gift.EndTime = requestDto.EndTime;
            _dbContext.CouponGifts.Add(gift);
            await _dbContext.SaveChangesAsync();
            return Created($"coupon-gift/{gift.Id}", new CouponGiftDto(gift));
        }
    }

    /// <summary>
    ///     请求 DTO
    /// </summary>
    public class CouponGiftCreateOneRequestDto
    {
        /// <summary>
        ///     名称
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        ///     描述
        /// </summary>
        [Required]
        public List<string> Descriptions { get; set; }

        /// <summary>
        ///     缩略图
        /// </summary>
        [Required]
        public string ThumbnailImage { get; set; }

        /// <summary>
        ///     预览图片
        /// </summary>
        [Required]
        public string PreviewImage { get; set; }

        /// <summary>
        ///     接受的用户输入字段
        /// </summary>
        [Required]
        public List<CouponGiftAcceptedFieldDto> AcceptedFields { get; set; }

        /// <summary>
        ///     价格
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        ///     下架日期
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}