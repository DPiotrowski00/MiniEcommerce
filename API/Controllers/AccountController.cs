using API.DataModels;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(IAccountSqlService userSqlService) : ControllerBase
    {
        private readonly IAccountSqlService _userSqlService = userSqlService;

        //[HttpGet]
        //public ActionResult<AccountInformation> GetAccountInformation()
        //{

        //}

        //[HttpGet]
        //[Route("/account/addresses")]
        //public ActionResult<List<AddressModel>> GetAddresses()
        //{

        //}
    }
}
