using API.DataModels;
using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController(IItemsSqlService service) : ControllerBase
    {
        private readonly IItemsSqlService _sqlService = service;

        public class ItemWrapper
        {
            public required ItemModel Item { get; set; }
            public IFormFile? Image { get; set; }
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        public ActionResult Test()
        {
            return Ok();
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateItem([FromForm] ItemWrapper wrapper)
        {
            var token = Request.Cookies["JWT_Token"];
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims są null");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id jest null");

            var item = wrapper.Item;
            if (item == null) return BadRequest("Item jest null");

            item.CreatorID = id;

            var image = wrapper.Image;
            if (image != null)
            {
                if (!image.ContentType.StartsWith("image/") || image.Length == 0) return BadRequest("Zdjęcie jest puste");

                var extension = Path.GetExtension(image.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine("Uploads", fileName);
                if (!Directory.Exists("Uploads"))
                {
                    Directory.CreateDirectory("Uploads");
                }

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                item.Thumbnail = fileName;
                Console.WriteLine("Zapisano zdjęcie");
            }
            else
            {
                Console.WriteLine("Nie przesłano zdjęcia");
            }

            if (await _sqlService.AddItem(item))
            {
                return Ok("Udało się zapisać");
            }
            else
            {
                return BadRequest("Nie udało się zapisać");
            }
        }
    }
}
