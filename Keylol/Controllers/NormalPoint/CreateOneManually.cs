﻿using System.Data.Entity;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Keylol.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.NormalPoint
{
    public partial class NormalPointController
    {
        /// <summary>
        ///     创建一个据点
        /// </summary>
        /// <param name="vm">据点相关属性</param>
        [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
        [Route]
        [HttpPost]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof (NormalPointDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "存在无效的输入属性")]
        public async Task<IHttpActionResult> CreateOneManually(NormalPointVM vm)
        {
            if (vm == null)
            {
                ModelState.AddModelError("vm", "Invalid view model.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (vm.IdCode == null || !Regex.IsMatch(vm.IdCode, @"^[A-Z0-9]{5}$"))
            {
                ModelState.AddModelError("vm.IdCode", "识别码只允许使用 5 位数字或大写字母");
                return BadRequest(ModelState);
            }

            if (await DbContext.NormalPoints.AnyAsync(u => u.IdCode == vm.IdCode))
            {
                ModelState.AddModelError("vm.IdCode", "识别码已经被其他据点使用");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(vm.EnglishName))
            {
                ModelState.AddModelError("vm.EnglishName", "英文名称不能为空");
                return BadRequest(ModelState);
            }

            if (vm.PreferredName == null)
            {
                ModelState.AddModelError("vm.PreferredName", "名称语言偏好必填");
                return BadRequest(ModelState);
            }

            if (vm.Type == null)
            {
                ModelState.AddModelError("vm.PreferredName", "据点类型必填");
                return BadRequest(ModelState);
            }

            if (!vm.BackgroundImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.BackgroundImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            if (!vm.AvatarImage.IsTrustedUrl())
            {
                ModelState.AddModelError("vm.AvatarImage", "不允许使用可不信图片来源");
                return BadRequest(ModelState);
            }

            var normalPoint = DbContext.NormalPoints.Create();
            normalPoint.IdCode = vm.IdCode;
            normalPoint.BackgroundImage = vm.BackgroundImage;
            normalPoint.AvatarImage = vm.AvatarImage;
            normalPoint.ChineseName = vm.ChineseName;
            normalPoint.EnglishName = vm.EnglishName;
            normalPoint.PreferredName = vm.PreferredName.Value;
            normalPoint.ChineseAliases = vm.ChineseAliases;
            normalPoint.EnglishAliases = vm.EnglishAliases;
            normalPoint.Type = vm.Type.Value;
            normalPoint.Description = vm.Description;
            if (vm.Type.Value == NormalPointType.Genre || vm.Type.Value == NormalPointType.Manufacturer)
            {
                if (vm.NameInSteamStore == null)
                {
                    ModelState.AddModelError("vm.NameInSteamStore", "商店匹配名必填");
                    return BadRequest(ModelState);
                }
                normalPoint.NameInSteamStore = vm.NameInSteamStore;
            }
            if (normalPoint.Type == NormalPointType.Game &&
                !await PopulateGamePointAttributes(normalPoint, vm, StaffClaim.Operator))
            {
                return BadRequest(ModelState);
            }
            DbContext.NormalPoints.Add(normalPoint);
            await DbContext.SaveChangesAsync();

            return Created($"normal-point/{normalPoint.Id}", new NormalPointDTO(normalPoint));
        }
    }
}