﻿using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.InvitationCode
{
    public partial class InvitationCodeController
    {
        /// <summary>
        ///     验证一个邀请码是否正确
        /// </summary>
        /// <param name="code">邀请码</param>
        [AllowAnonymous]
        [Route("{code}")]
        [HttpGet]
        [ResponseType(typeof (InvitationCodeDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "邀请码无效")]
        public async Task<IHttpActionResult> GetOneByCode(string code)
        {
            var c = await DbContext.InvitationCodes.FindAsync(code);
            if (c == null || c.UsedByUser != null)
                return NotFound();
            return Ok(new InvitationCodeDTO(c));
        }
    }
}