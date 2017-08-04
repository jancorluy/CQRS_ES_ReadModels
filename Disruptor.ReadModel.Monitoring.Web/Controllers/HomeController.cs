using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Disruptor.ReadModel.Monitoring.Web.Models;
using Disruptor.ReadModel.Tests.Infrastructure.Repositories;

namespace Disruptor.ReadModel.Monitoring.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public async Task<ActionResult> Index()
        {
            var repository = new RedisReadModelRepository();
            var readModelHandlers = await repository.GetAll();
            return View(readModelHandlers.Select(s => new ReadModelState()
            {
                ReadmodelType = s.ReadmodelType,
                CommitPosition = s.CommitPosition,
                PreparePosition = s.PreparePosition,
                LastComittedPosition = s.LastComittedPosition
            }).ToList());
        }
    }
}