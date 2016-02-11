using System;

namespace ContactManager
{
	/**
	 * class helps at displaying contacts in MainActivity
	 * 
	 * int id - contact id
	 * string name - contact name (firstname + lastname)
	 * string phone - contact phone
	 * string photo - converted image to base64 string
	 */
	public class Contact
	{
		public int id { get; set; }
		public string name{ get; set; }
		public string phone{ get; set; }
		public string photo { get; set; }

		public Contact (int id, string name, string phone, string photo)
		{
			this.id = id;
			this.name = name;
			this.phone = phone;
			this.photo = photo;
		}
	}
}

