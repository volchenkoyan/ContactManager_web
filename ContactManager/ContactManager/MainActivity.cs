using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Net;
using Android.Util;
using Android.Database;
using System.Net;
using System.Threading.Tasks;
using System.Json;
using Newtonsoft.Json;
using Android.Media;
using System.Drawing;

namespace ContactManager
{
	[Activity (Label = "Contact Manager", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		List<Contact> contacts = new List<Contact> (); //list of Contact
		ListView list_contacts;
		string json_str; //string that stores serialized JSON objectes
		string show_contacts_url = "http://172.17.72.6:8000/data"; //URL for showing contacts
		string delete_contact_url = "http://172.17.72.6:8000/delete"; // URL for deliting objects

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.Main);
			LoadContacts(show_contacts_url);
		}
		/**
		 * Loading Contacts
		 * jsonValue - stores response result
		 * obj - stores converted to string jsonValue as JsonArray
		 * loadedContacts - colects all the contacts as JSON
		 * 
		 * */
		private async void LoadContacts(string url) 
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (url);
			request.ContentType = "application/json";
			request.Method = "GET";
			contacts.Clear ();
			using (WebResponse response = await request.GetResponseAsync ()) {
				using (System.IO.Stream stream = response.GetResponseStream ()) {
					var jsonValue = await Task.Run (() => JsonObject.Load (stream));
					JsonArray obj = (JsonArray)JsonObject.Parse(jsonValue.ToString());
					var loadedContacts = from contact in obj
					               select contact;
					//Adding to Contact list Json values
					foreach (JsonObject con in loadedContacts) {
						contacts.Add(new Contact(con["ID"], con["FirstName"] + " "+con["LastName"], con["Phone"], con["Photo"])); 
					}
					//Shows listview if at least one contact exists
					if (contacts.Count > 0) {
						SetContentView (Resource.Layout.Main);
						list_contacts = FindViewById<ListView> (Resource.Id.list_contacts);
						list_contacts.Adapter = new ContactAdapter (this, contacts);
						list_contacts.ChoiceMode = ChoiceMode.Multiple;	

					}
					//shows layout with 'there is no contacts' message
					else {
						SetContentView (Resource.Layout.EmptyList);
					}
				}
			}
		}

		/**
		 * deletes contact
		 * var checked_contacts - HashSet<int> stores id's of elements for delete
		 */
		public void DeleteContact(string url)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (url);
			request.ContentType = "application/json";
			request.Method = "POST";
			request.Credentials = CredentialCache.DefaultCredentials;

			//list of objectes that checked for deleting
			List<JSONAdapter> contacts_for_delete = new List<JSONAdapter>();

			//send request only if contacts exist
			if (contacts.Count > 0) {
				var checked_contacts = (list_contacts.Adapter as ContactAdapter).CheckedItems;
				//continue only if at least one contact checked 
				if (checked_contacts.Count > 0) {
					//adding each id (as JSONAdapter object) to list
					foreach(int elem in checked_contacts)
					{
						var str = elem.ToString();
						contacts_for_delete.Add (new JSONAdapter (str));
					}
					//serialize list of objects to string
					json_str = JsonConvert.SerializeObject (contacts_for_delete);
					//writing JSON string in stream
					using (var streamWriter = new StreamWriter (request.GetRequestStream ())) {
						streamWriter.Write (json_str);
					}
						
					var response = (HttpWebResponse)request.GetResponse ();
					using (var streamReader = new StreamReader (response.GetResponseStream ())) {
						var responseText = streamReader.ReadToEnd ();
					}

					contacts.Clear ();   //clear list of contacts after deleting
					LoadContacts (show_contacts_url);  //load updated list of contacts
				}
				} 
			}

		//creating menu
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.menu_main, menu);
			return base.OnCreateOptionsMenu (menu);
		}


		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			//Add User option - loads AddUser Activity
			if (item.ItemId == Resource.Id.action_add_contact) {
				StartActivity (typeof(AddUser));
				return true;
			}
			//Delete button - deletes all  the checked contacts
			if (item.ItemId == Resource.Id.action_delete_contact) {
				DeleteContact(delete_contact_url);
			}
			//Refresh option - updates list of contacts
			if (item.ItemId == Resource.Id.refresh_contacts) {
				LoadContacts (show_contacts_url);
				Android.Widget.Toast.MakeText (this, "Refreshed", ToastLength.Short).Show ();
			}
			return base.OnOptionsItemSelected (item);
		}

		//If button back pressed in MainActivity - you will exit from app, not back to the last activity
		public override void OnBackPressed ()
		{
			base.OnBackPressed ();
		}


	}
		
	/**
	 * Contact Adapter - class that implements custom view of the Contacts list view
	 */
	public class ContactAdapter : BaseAdapter<Contact>
	{
		List<Contact> contacts; //list of Contact

		Activity context;
		HashSet<int> checked_items = new HashSet<int>(); //HashSet that stores id's of checked list's items
		public HashSet<int> CheckedItems { get { return checked_items; } }

		public ContactAdapter (Activity context, List<Contact> contacts) : base ()
		{
			this.context = context;
			this.contacts = contacts;

		}
			
		//Returns Item's id
		public override long GetItemId (int position)
		{
			return position;
		}

		public override Contact this [int position] { get { return contacts [position]; } }

		public override int Count { get { return contacts.Count; } }

		/**
		 * Method converts base64 string to image and returns it
		 * byte[] imageBytes - array that stores bytes of base64_string
		 * Bitmap decodedImage - bitmap received after decoding imageBytes array
		 */
		public Bitmap StringToImage(string base64_string)
		{
			byte[] imageBytes = System.Convert.FromBase64String(base64_string);
			Bitmap decodedImage = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
			return decodedImage;
		}
			
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null) {
				view = context.LayoutInflater.Inflate (Resource.Layout.listview, null);
				//action on any list item
				view.Click += (object sender, System.EventArgs e) => {	
					bool isChecked = view.FindViewById<CheckBox> (Resource.Id.check).Checked; //getting current status of item's checkbox
					view.FindViewById<CheckBox> (Resource.Id.check).Checked = !isChecked;  //changing to opposite
					//adding to checked_items list if checked
					if(!isChecked)
					{
						checked_items.Add(System.Convert.ToInt32(view.Tag));
					}
					//removing if unchecked
					else
					{
						checked_items.Remove(System.Convert.ToInt32(view.Tag));
					}
				};
			}
			Contact item = this [position];
			//setting values to item's TextViews
			view.FindViewById<TextView> (Resource.Id.label).Text = item.name;
			view.FindViewById<TextView> (Resource.Id.phone).Text = item.phone;
			//Tag for any item that shores id of contact in database
			view.Tag = item.id;
			//If photo exists - clear ImageView and set converted base 64 string as image
			if (item.photo != null) {
				view.FindViewById<ImageView> (Resource.Id.photo).SetImageResource (0);
				view.FindViewById<ImageView> (Resource.Id.photo).SetImageBitmap (StringToImage (item.photo));
			}
			return view;
		}
	}

	public static class ObjectHelper
	{
		public static T Cast<T> (this Java.Lang.Object obj) where T : class
		{
			var PropertyInfo = obj.GetType ().GetProperty ("Instance");
			return PropertyInfo == null ? null : PropertyInfo.GetValue (obj, null) as T;
		}
	}
}
