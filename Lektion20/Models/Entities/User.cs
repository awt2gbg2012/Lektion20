using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lektion20.Models.Entities.Abstract;

namespace Lektion20.Models.Entities
{
    public class User : IEntity
    {
        public Guid ID { get; set; }
        public string FullName { get; set; }
        public long FacebookID { get; set; }
        public string LongTermAccessToken { get; set; }
    }
}