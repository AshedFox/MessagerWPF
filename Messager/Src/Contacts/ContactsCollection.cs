using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public class ContactsCollection: ObservableCollection<Contact>
    {
        public void AddContact(long id, string name)
        {
            Add(new Contact { Id = id, Name = name });
        }

        public void RemoveContact(long id)
        {
            Remove(this.First(item => item.Id == id));
        }
    }
}
