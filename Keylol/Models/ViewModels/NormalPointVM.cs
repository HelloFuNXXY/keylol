﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Keylol.Models.ViewModels
{
    public class NormalPointVM
    {
        [Required(AllowEmptyStrings = true)]
        public string BackgroundImage { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string AvatarImage { get; set; }


        [Required(AllowEmptyStrings = true)]
        public string ChineseName { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string EnglishAliases { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string ChineseAliases { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Description { get; set; }

        #region Admin Editable

        public string EnglishName { get; set; }

        public string NameInSteamStore { get; set; }

        public PreferredNameType? PreferredName { get; set; }

        public string IdCode { get; set; }

        public NormalPointType? Type { get; set; }

        #endregion

        #region Game Point Only

        public int? SteamAppId { get; set; }

        public string DisplayAliases { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public string CoverImage { get; set; }

        public List<string> DeveloperPointsId { get; set; }

        public List<string> PublisherPointsId { get; set; }

        public List<string> GenrePointsId { get; set; }

        public List<string> TagPointsId { get; set; }

        public List<string> MajorPlatformPointsId { get; set; }

        public List<string> MinorPlatformPointsId { get; set; }

        public List<string> SeriesPointsId { get; set; }

        #endregion
    }
}