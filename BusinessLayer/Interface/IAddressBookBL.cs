using System.Collections.Generic;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IAddressBookBL
    {
        ResponseModel<AddressBookEntity> GetEntryById(int id);
        ResponseModel<List<ResponseContactListDTO>> GetAllEntries(int userId);
        ResponseModel<string> AddEntry(AddressBookDTO requestModel, int userId);
        ResponseModel<AddressBookEntity> UpdateEntry(UpdateRequest requestModel, int userId);
        ResponseModel<AddressBookEntity> DeleteEntry(int id, int userId);

        List<AddressBookEntity> GetAllContacts();
        bool AdminDeleteEntry(int userId, int contactId);
    }
}
