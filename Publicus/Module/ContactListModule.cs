using System.Collections.Generic;
using System.Linq;
using System;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json;

namespace Publicus
{
    public class ContactListItemViewModel
    {
        public bool ShowOrganization;
        public bool ShowName;
        public bool ShowStreet;
        public bool ShowPlace;
        public bool ShowState;
        public bool ShowMail;
        public bool ShowPhone;

        public string Id;
        public string Organization;
        public string LastName;
        public string FirstNames;
        public string MailAddress;
        public string PhoneNumber;
        public string Street;
        public string Place;
        public string State;

        public ContactListItemViewModel(Translator translator, Contact contact, ContactListDataViewModel parent, Session session)
        {
            ShowOrganization = parent.ShowOrganization;
            ShowName = parent.ShowName;
            ShowStreet = parent.ShowStreet;
            ShowPlace = parent.ShowPlace;
            ShowState = parent.ShowState;
            ShowMail = parent.ShowMail;
            ShowPhone = parent.ShowPhone;

            var contactAccess = session.HasAccess(contact, PartAccess.Contact, AccessRight.Read);

            Id = contact.Id.ToString();
            Organization = contact.Organization.Value.EscapeHtml();
            LastName = contactAccess ?
                contact.LastName.Value.EscapeHtml() :
                string.Empty;
            FirstNames = contactAccess ? 
                contact.ShortTitleAndNames.EscapeHtml() :
                string.Empty;
            MailAddress = contactAccess ? 
                contact.PrimaryMailAddress.EscapeHtml() :
                string.Empty;
            PhoneNumber = contactAccess ? 
                contact.PrimaryPhoneNumber.EscapeHtml() :
                string.Empty;
            Place = contactAccess ?
                contact.PostalAddresses
                .OrderBy(p => p.Precedence.Value)
                .Select(p => p.PlaceWithPostalCode.EscapeHtml())
                .FirstOrDefault() ?? string.Empty :
                string.Empty;
            Street = contactAccess ?
                contact.PostalAddresses
                .OrderBy(p => p.Precedence.Value)
                .Select(p => p.StreetOrPostOfficeBox.EscapeHtml())
                .FirstOrDefault() ?? string.Empty :
                string.Empty;
            State = contactAccess ?
                contact.PostalAddresses
                .OrderBy(p => p.Precedence.Value)
                .Select(p => p.StateOrCountry(translator).EscapeHtml())
                .FirstOrDefault() ?? string.Empty :
                string.Empty;
        }
    }

    public class ContactListDataViewModel
    {
        public bool ShowOrganization;
        public bool ShowName;
        public bool ShowStreet;
        public bool ShowPlace;
        public bool ShowState;
        public bool ShowMail;
        public bool ShowPhone;
        public List<ContactListItemViewModel> List;

        public string PhraseHeaderOrganization = string.Empty;
        public string PhraseHeaderLastName = string.Empty;
        public string PhraseHeaderFirstNames = string.Empty;
        public string PhraseHeaderStreet = string.Empty;
        public string PhraseHeaderPlace = string.Empty;
        public string PhraseHeaderState = string.Empty;
        public string PhraseHeaderMail = string.Empty;
        public string PhraseHeaderPhone = string.Empty;

        public ContactListDataViewModel(Translator translator, IEnumerable<Contact> contacts, SearchSettings settings, Session session)
        {
            ShowOrganization = settings.ShowOrganization;
            ShowName = settings.ShowName;
            ShowStreet = settings.ShowStreet;
            ShowPlace = settings.ShowPlace;
            ShowState = settings.ShowState;
            ShowMail = settings.ShowMail;
            ShowPhone = settings.ShowPhone;
            List = new List<ContactListItemViewModel>(contacts.Select(p => new ContactListItemViewModel(translator, p, this, session)));

            PhraseHeaderOrganization = translator.Get("Contact.List.Header.Organization", "Column 'Organization' in the contact list page", "Organization").EscapeHtml();
            PhraseHeaderLastName = translator.Get("Contact.List.Header.LastName", "Column 'Last name' in the contact list page", "Last name").EscapeHtml();
            PhraseHeaderFirstNames = translator.Get("Contact.List.Header.FirstNames", "Column 'First names' in the contact list page", "First names").EscapeHtml();
            PhraseHeaderStreet = translator.Get("Contact.List.Header.Street", "Column 'Street' in the contact list page", "Street").EscapeHtml();
            PhraseHeaderPlace = translator.Get("Contact.List.Header.Place", "Column 'Place' in the contact list page", "Place").EscapeHtml();
            PhraseHeaderState = translator.Get("Contact.List.Header.State", "Column 'State' in the contact list page", "State/Country").EscapeHtml();
            PhraseHeaderMail = translator.Get("Contact.List.Header.Mail", "Column 'E-Mail' in the contact list page", "E-Mail").EscapeHtml();
            PhraseHeaderPhone = translator.Get("Contact.List.Header.Phone", "Column 'Phone' in the contact list page", "Phone").EscapeHtml();
        }
    }

    public class ContactPageViewModel
    {
        public string Index;
        public string Number;
        public string State;

        public ContactPageViewModel(int index, bool active)
        {
            Index = index.ToString();
            Number = (index + 1).ToString();
            State = active ? "active" : string.Empty;
        }
    }

    public class ContactItemsPerPageViewModel
    {
        public string Count;
        public string State;

        public ContactItemsPerPageViewModel(int count, bool active)
        {
            Count = count.ToString();
            State = active ? "active" : string.Empty;
        }
    }

    public class ContactPagesViewModel
    {
        public string CurrentPageNumber;
        public string CurrentItemsPerPage;
        public List<ContactPageViewModel> Pages;
        public List<ContactItemsPerPageViewModel> ItemsPerPage;

        public string PhrasePage = string.Empty;
        public string PhrasePerPage = string.Empty;

        public ContactPagesViewModel(Translator translator, int pageCount, SearchSettings settings)
        {
            CurrentPageNumber = (settings.CurrentPage + 1).ToString();
            Pages = new List<ContactPageViewModel>();
            for (int i = 0; i < pageCount; i++)
            {
                Pages.Add(new ContactPageViewModel(i, settings.CurrentPage == i));
            }
            CurrentItemsPerPage = settings.ItemsPerPage.ToString();
            ItemsPerPage = new List<ContactItemsPerPageViewModel>();
            foreach (int i in new int[] { 10, 15, 20, 25, 30, 40, 50 })
            {
                ItemsPerPage.Add(new ContactItemsPerPageViewModel(i, settings.ItemsPerPage == i));
            }

            PhrasePage = translator.Get("Contact.List.Pages.Page", "In paging of the contact list page", "Page").EscapeHtml();
            PhrasePerPage = translator.Get("Contact.List.Pages.PerPage", "In paging of the contact list page", "per Page").EscapeHtml();
        }
    }

    public class SearchSettings
    {
        public string FilterText;
        public int ItemsPerPage;
        public int CurrentPage;
        public bool ShowOrganization;
        public bool ShowName;
        public bool ShowStreet;
        public bool ShowPlace;
        public bool ShowState;
        public bool ShowMail;
        public bool ShowPhone;
    }

    public class ContactListViewModel : MasterViewModel
    {
        public string PhraseShowOrganization = string.Empty;
        public string PhraseShowName = string.Empty;
        public string PhraseShowStreet = string.Empty;
        public string PhraseShowPlace = string.Empty;
        public string PhraseShowState = string.Empty;
        public string PhraseShowMail = string.Empty;
        public string PhraseShowPhone = string.Empty;
        public string PhraseShowColumns = string.Empty;
        public string PhraseSearch = string.Empty;
        public string PhraseFilter = string.Empty;

        public ContactListViewModel(Translator translator, Session session)
            : base(translator, 
                   translator.Get("Contact.List.Title", "Title of the contact list page", "Liste"), 
                   session)
        {
            PhraseShowOrganization = translator.Get("Contact.List.Show.Organization", "Show column 'Organization' in the contact list page", "Organization").EscapeHtml();
            PhraseShowName = translator.Get("Contact.List.Show.Name", "Show columns 'Name' in the contact list page", "Name").EscapeHtml();
            PhraseShowStreet = translator.Get("Contact.List.Show.Street", "Show column 'Street' in the contact list page", "Street").EscapeHtml();
            PhraseShowPlace = translator.Get("Contact.List.Show.Place", "Show column 'Place' in the contact list page", "Place").EscapeHtml();
            PhraseShowState = translator.Get("Contact.List.Show.State", "Show column 'State/Country' in the contact list page", "State/Country").EscapeHtml();
            PhraseShowMail = translator.Get("Contact.List.Show.Mail", "Show column 'E-Mail' in the contact list page", "E-Mail").EscapeHtml();
            PhraseShowPhone = translator.Get("Contact.List.Show.Phone", "Show column 'Phone' in the contact list page", "Phone").EscapeHtml();
            PhraseShowColumns = translator.Get("Contact.List.Show.Columns", "Dropdown 'Columns' in the contact list page", "Columns").EscapeHtml();
            PhraseSearch = translator.Get("Contact.List.Search", "Hint in the search box of the contact list page", "Search").EscapeHtml();
            PhraseFilter = translator.Get("Contact.List.Filter", "Button 'Filter' on the contact list page", "Filter").EscapeHtml();
        }
    }

    public class ContactListModule : PublicusModule
    {
        private bool Filter(Contact contact, SearchSettings settings)
        {
            var textFilter =
                contact.Organization.Value.Contains(settings.FilterText) ||
                contact.FullName.Contains(settings.FilterText) ||
                contact.ServiceAddresses.Any(a => a.Address.Value.Contains(settings.FilterText)) ||
                contact.PostalAddresses.Any(a => a.Text(Translator).Contains(settings.FilterText));
            var accessFilter = HasAccess(contact, PartAccess.Anonymous, AccessRight.Read);
            return textFilter && accessFilter;
        }

        public ContactListModule()
        {
            this.RequiresAuthentication();

            Get["/contact/list"] = parameters =>
            {
                return View["View/contactlist.sshtml", new ContactListViewModel(Translator, CurrentSession)];
            };
            Post["/contact/list/data"] = parameters =>
            {
                var settings = JsonConvert.DeserializeObject<SearchSettings>(ReadBody());
                var page = Database.Query<Contact>()
                    .Where(p => Filter(p, settings))
                    .OrderBy(p => p.LastName.Value)
                    .Skip(settings.ItemsPerPage * settings.CurrentPage)
                    .Take(settings.ItemsPerPage);
                return View["View/contactlist_data.sshtml", new ContactListDataViewModel(Translator, page, settings, CurrentSession)];
            };
            Post["/contact/list/pages"] = parameters =>
            {
                var settings = JsonConvert.DeserializeObject<SearchSettings>(ReadBody());
                var contactCount = Database.Query<Contact>()
                    .Where(p => Filter(p, settings))
                    .Count();
                var pageCount = (contactCount / settings.ItemsPerPage) + Math.Min(contactCount % settings.ItemsPerPage, 1);
                return View["View/contactlist_pages.sshtml", new ContactPagesViewModel(Translator, pageCount, settings)];
            };
        }
    }
}
