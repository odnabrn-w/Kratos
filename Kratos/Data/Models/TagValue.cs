﻿using System.ComponentModel.DataAnnotations;

namespace Kratos.Data
{
    public class TagValue
    {
        [Key]
        public int Key { get; set; }

        public string Tag { get; set; }

        public string Value { get; set; }

        public ulong CreatedAt { get; set; }

        public ulong CreatedBy { get; set; }

        public int TimesInvoked { get; set; }
    }
}
