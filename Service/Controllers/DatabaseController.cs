using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DatabaseWebService.Services;

namespace DatabaseWebService.Controllers
{
    public class DatabaseController : ApiController
    {
        private DatabaseService _dbService = new DatabaseService();

        // GET api/database/test
        [HttpGet]
        [Route("api/database/test")]
        public IHttpActionResult Test()
        {
            return Ok(new { message = "Сервис работает!", time = DateTime.Now });
        }

        // POST api/database/connect
        [HttpPost]
        [Route("api/database/connect")]
        public IHttpActionResult Connect([FromBody] string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest("Строка подключения не может быть пустой");
            }

            string sessionId;
            string result = _dbService.Connect(connectionString, out sessionId);

            if (result.StartsWith("Ошибка"))
            {
                return BadRequest(result);
            }

            return Ok(new
            {
                message = result,
                sessionId = sessionId
            });
        }

        // GET api/database/version?sessionId=xxx
        [HttpGet]
        [Route("api/database/version")]
        public IHttpActionResult GetVersion(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Не передан sessionId");
            }

            string version = _dbService.GetVersion(sessionId);

            if (version.StartsWith("Ошибка") || version.StartsWith("Сессия не найдена"))
            {
                return BadRequest(version);
            }

            return Ok(new { version = version });
        }

        // POST api/database/disconnect
        [HttpPost]
        [Route("api/database/disconnect")]
        public IHttpActionResult Disconnect([FromBody] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Не передан sessionId");
            }

            string result = _dbService.Disconnect(sessionId);
            return Ok(new { message = result });
        }
    }
}