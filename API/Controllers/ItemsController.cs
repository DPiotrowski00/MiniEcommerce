using API.DataModels;
using API.Filters;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

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

        public class ItemResponse
        {
            public int ID { get; set; }
            public string? CreatorName { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public string? ThumbnailURL { get; set; }
            public DateTime? CreationTime { get; set; }
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetItems()
        {
            var items = await _sqlService.GetItems();
            List<ItemResponse> response = [];

            foreach (var i in items)
            {
                ItemResponse newItem = new()
                {
                    ID = i.ID,
                    CreatorName = i.CreatorName,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    ThumbnailURL = "/uploads/" + i.Thumbnail,
                    CreationTime = i.CreationTime
                };
                response.Add(newItem);
            }

            return Ok(response);
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetItemById(int id)
        {
            var item = await _sqlService.GetItemById(id);
            ItemResponse response = new()
            {
                ID = item.ID,
                CreatorName = item.CreatorName,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ThumbnailURL = "/uploads/" + item.Thumbnail,
                CreationTime = item.CreationTime
            };

            return Ok(response);
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpPut]
        [Route("/items/images")]
        public async Task<ActionResult> AddImages([FromForm] int ItemID, [FromForm] List<IFormFile> images)
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            var claims = jwt.Claims;
            if (claims == null) return BadRequest("Claims są null");

            int id = Convert.ToInt32(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value);
            if (id == 0) return BadRequest("Id jest null");

            if (id == 0) return BadRequest("ItemID jest null");

            List<string> fileNames = [];

            foreach (var image in images)
            {
                if (image != null)
                {
                    if (!image.ContentType.StartsWith("image/") || image.Length == 0) return BadRequest("Zdjęcie jest puste");

                    var extension = Path.GetExtension(image.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine("Uploads", fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    fileNames.Add(fileName);
                }
            }

            if (await _sqlService.AddImages(new ItemModel() { ID = ItemID }, fileNames))
            {
                return Ok();
            }
            else
            {
                foreach (var fileName in fileNames)
                {
                    var filePath = Path.Combine("Uploads", fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                return BadRequest("Nie udało się zapisać");
            }
        }

        [ServiceFilter(typeof(CsrfFilter))]
        [Authorize]
        [HttpPut]
        public async Task<ActionResult> CreateItem([FromForm] ItemWrapper wrapper)
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");
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

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                item.Thumbnail = fileName;
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
