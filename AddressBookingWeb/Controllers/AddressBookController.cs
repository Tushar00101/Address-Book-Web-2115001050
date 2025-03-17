using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using NLog;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Middleware.GlobalExceptionHandler;

namespace AddressBookApplication.Controllers
{
    [ApiController]
    [Route("api/addressbook")]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _addressBookBL;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AddressBookController(IAddressBookBL addressBookBL)
        {
            _addressBookBL = addressBookBL;
        }

        

        [Authorize]
        [HttpGet]
        public IActionResult GetAllEntries()
        {
            try
            {
                logger.Info("GET request received to fetch all address book entries.");
                int userId = GetUserId();
                var response = _addressBookBL.GetAllEntries(userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in GetAllEntries");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetEntryById(int id)
        {
            try
            {
                logger.Info($"GET request received for address book entry with ID: {id}");
                var response = _addressBookBL.GetEntryById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in GetEntryById");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddEntry([FromBody] AddressBookDTO requestModel)
        {
            try
            {
                int userId = GetUserId();
                logger.Info($"POST request received to add address book entry: {requestModel.Name}");
                var response = _addressBookBL.AddEntry(requestModel, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in AddEntry");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateEntry([FromBody] UpdateRequest updateRequest,int id)
        {
            try
            {
                int userId = GetUserId();
                logger.Info($"PUT request received to update address book entry: {updateRequest.Name}");
                updateRequest.Id = id;
                var response = _addressBookBL.UpdateEntry(updateRequest, userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in UpdateEntry");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteEntry(int id)
        {
            try
            {
                int userId = GetUserId();
                logger.Info($"DELETE request received to remove address book entry with ID: {id}");
                var response = _addressBookBL.DeleteEntry(id, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in DeleteEntry");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }

        private int GetUserId()
        {
            try
            {
                var userIdClaims = User.Claims
                    .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    .Select(c => c.Value)
                    .ToList();

                if (!userIdClaims.Any())
                {
                    throw new Exception("User ID claim is missing in the JWT.");
                }

                foreach (var claim in userIdClaims)
                {
                    if (int.TryParse(claim, out int userId))
                    {
                        return userId;
                    }
                }
                throw new FormatException($"No valid integer User ID found in claims: {string.Join(", ", userIdClaims)}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in GetUserId");
                throw;
            }
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("All")]
        public IActionResult GetAllContacts()
        {
            try
            {
                logger.Info("GET request received to fetch all address book entries (Admin Only)");
                var response = _addressBookBL.GetAllContacts();
                if (response == null)
                {
                    return Ok(new ResponseModel<Object> { Success = false, Message = "No contact found", Data = response });
                }

                return Ok(new ResponseModel<Object> { Success = true, Message = "All Contacts", Data = response });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in GetAllContacts");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }



        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}/{contactId}")]
        public IActionResult DeleteEntry(int userId, int contactId)
        {
            try
            {
                logger.Info($"DELETE request received from Admin to remove contact ID: {contactId} of User ID: {userId}");
                var response = _addressBookBL.AdminDeleteEntry(userId, contactId);
                if (response == true)
                {
                    return Ok(new ResponseModel<Object> { Success = true, Message = "Contact is deleted", Data = response });
                }
                return Ok(new ResponseModel<Object> { Success = false, Message = "No contact is found", Data = response });

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in DeleteEntry");
                return BadRequest(ExceptionHandler.CreateErrorResponse(ex));
            }
        }


    }
}
