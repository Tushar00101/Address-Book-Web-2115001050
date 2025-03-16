using System;
using System.Collections.Generic;
using BusinessLayer.Interface;
using ModelLayer.Model;
using NLog;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IAddressBookRL _addressBookRL;

        public AddressBookBL(IAddressBookRL addressBookRL)
        {
            _addressBookRL = addressBookRL;
        }

        /// <summary>
        /// Retrieves an address book entry by its unique ID.
        /// </summary>
        public ResponseModel<AddressBookEntity> GetEntryById(int id)
        {
            logger.Info($"Fetching address book entry with ID: {id}");
            var entry = _addressBookRL.GetEntryById(id);

            if (entry == null)
            {
                logger.Warn($"Address book entry with ID {id} not found.");
                return new ResponseModel<AddressBookEntity> { Success = false, Message = "Entry not found", Data = null };
            }

            logger.Info($"Address book entry with ID {id} found.");
            return new ResponseModel<AddressBookEntity> { Success = true, Message = "Entry found", Data = entry };
        }

        /// <summary>
        /// Retrieves all address book entries for a user.
        /// </summary>
        public ResponseModel<List<ResponseContactListDTO>> GetAllEntries(int userId)
        {
            logger.Info($"Fetching all address book entries for user ID: {userId}");
            var entries = _addressBookRL.GetAllEntries(userId);

            if (entries.Count == 0)
            {
                logger.Warn("No address book entries found.");
                return new ResponseModel<List<ResponseContactListDTO>> { Success = false, Message = "No Contact created yet!" };
            }

            logger.Info("All address book entries retrieved successfully.");
            return new ResponseModel<List<ResponseContactListDTO>> { Success = true, Message = "All contacts retrieved successfully", Data = entries };
        }

        /// <summary>
        /// Adds a new entry to the address book.
        /// </summary>
        public ResponseModel<string> AddEntry(AddressBookDTO requestModel, int userId)
        {
            logger.Info($"Adding new address book entry for user ID: {userId}");
            var response = _addressBookRL.AddEntry(requestModel, userId);

            if (response == null)
            {
                logger.Error("Failed to add address book entry.");
                return new ResponseModel<string> { Success = false, Message = "Failed to add entry" };
            }

            logger.Info("Address book entry added successfully.");
            return response;
        }

        /// <summary>
        /// Updates an existing address book entry.
        /// </summary>
        public ResponseModel<AddressBookEntity> UpdateEntry(UpdateRequest requestModel, int userId)
        {
            logger.Info($"Updating address book entry for user ID: {userId}");
            var updatedEntry = _addressBookRL.UpdateEntry(requestModel, userId);

            if (updatedEntry == null)
            {
                logger.Warn("Failed to update entry. Entry not found.");
                return new ResponseModel<AddressBookEntity> { Success = false, Message = "Entry update failed", Data = null };
            }

            logger.Info("Address book entry updated successfully.");
            return new ResponseModel<AddressBookEntity> { Success = true, Message = "Entry updated successfully", Data = updatedEntry };
        }

        /// <summary>
        /// Deletes an address book entry.
        /// </summary>
        public ResponseModel<AddressBookEntity> DeleteEntry(int id, int userId)
        {
            logger.Info($"Deleting address book entry with ID: {id}");
            var deletedEntry = _addressBookRL.DeleteEntry(id, userId);

            if (deletedEntry == null)
            {
                logger.Warn("Failed to delete entry. Entry not found.");
                return new ResponseModel<AddressBookEntity> { Success = false, Message = "Entry not found", Data = null };
            }

            logger.Info("Address book entry deleted successfully.");
            return new ResponseModel<AddressBookEntity> { Success = true, Message = "Entry deleted successfully", Data = deletedEntry };
        }


        public List<AddressBookEntity> GetAllContacts()
        {
            try
            {
                return _addressBookRL.GetAllContacts();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all contacts", ex);
            }
        }

        public bool AdminDeleteEntry(int userId, int contactId)
        {
            try
            {
                return _addressBookRL.AdminDeleteEntry(userId, contactId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting contact", ex);
            }
        }

    }
}
