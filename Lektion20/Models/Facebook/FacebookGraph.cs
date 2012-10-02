//-----------------------------------------------------------------------
// <copyright file="FacebookGraph.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock.Facebook {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using System.Text;

	[DataContract]
	public class FacebookGraph {
		private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(FacebookGraph));

		[DataMember(Name = "id")]
		public long Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

		[DataMember(Name = "link")]
		public Uri Link { get; set; }

		[DataMember(Name = "birthday")]
		public string Birthday { get; set; }

		public static FacebookGraph Deserialize(string json) {
			if (string.IsNullOrEmpty(json)) {
				throw new ArgumentNullException("json");
			}

			return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
		}

		public static FacebookGraph Deserialize(Stream jsonStream) {
			if (jsonStream == null) {
				throw new ArgumentNullException("jsonStream");
			}

			return (FacebookGraph)jsonSerializer.ReadObject(jsonStream);
		}
	}

    [DataContract]
    public class FacebookFeedGraph
    {
        private static DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(FacebookFeedGraph));

        [DataMember(Name="data")]
        public List<Post> Posts { get; set; }

        public static FacebookFeedGraph Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json");
            }

            return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        public static FacebookFeedGraph Deserialize(Stream jsonStream)
        {
            if (jsonStream == null)
            {
                throw new ArgumentNullException("jsonStream");
            }

            return (FacebookFeedGraph)jsonSerializer.ReadObject(jsonStream);
        }
    }

    [DataContract]
    public class Post
    {
        [DataMember(Name="from")]
        public NameIDPair From { get; set; }

        [DataMember(Name="message")]
        public string Message { get; set; }

        [DataMember(Name="picture")]
        public string PictureURL { get; set; }
    }

    [DataContract]
    public class NameIDPair
    {
        [DataMember(Name="name")]
        public string Name { get; set; }

        [DataMember(Name="id")]
        public string ID { get; set; }
    }
}
