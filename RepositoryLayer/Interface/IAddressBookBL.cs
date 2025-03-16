using System.Collections.Generic;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IAddressBookRL
    {
        ResponseModel<string> AddEntry(AddressBookDTO requestModel, int userId);
        AddressBookEntity GetEntryById(int id);
        List<ResponseContactListDTO> GetAllEntries(int userId);
        AddressBookEntity UpdateEntry(UpdateRequest requestModel, int userId);
        AddressBookEntity DeleteEntry(int id, int userId);

        List<AddressBookEntity> GetAllContacts();
        bool AdminDeleteEntry(int userId, int contactId);
    }
}
