using System;

namespace ContactManager
{
	/**
	 * class have members and constructors for implentimg contacts as JSON objects
	 */
	public class JSONAdapter
	{
		public string id { get; set; }
		public string firstname { get; set;}
		public string lastname {get; set;}
		public string phone {get; set;}
		public string photo {get; set;}

		public JSONAdapter(string firstname, string id, string lastname, string phone, string photo)
		{
			this.firstname = firstname;
			this.id = id;
			this.lastname = lastname;
			this.phone = phone;
			this.photo = photo;
		}

		public JSONAdapter(string firstname, string id, string lastname, string phone)
		{
			this.firstname = firstname;
			this.id = id;
			this.lastname = lastname;
			this.phone = phone;
		}

		public JSONAdapter(string id)
		{
			this.id = id;
		}
	}
}

