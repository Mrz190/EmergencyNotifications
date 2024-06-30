using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class RedisController : BaseApiController
    {
        private readonly IRedisService _redisService;
        public RedisController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        //[HttpGet("key")]
        //public async Task<IActionResult> GetValue(string key)
        //{
        //    var value = await _redisService.GetValueAsync(key);
        //    return Ok(value);
        //}

        //[HttpPost]
        //public async Task<IActionResult> SetValue(KeyValuePair<string, string> pair)
        //{
        //    await _redisService.SetValueAsync(pair.Key, pair.Value);
        //    return Ok();
        //}
    }
}
