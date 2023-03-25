using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Client.Controllers
{
    public class CreateNewController : Controller
    {
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Route("/CreateNew/Add")]
        public async Task<IActionResult> AddNewEstate(RealEstate realEstate)
        {
            try
            {
                bool result = true;
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/RealEstateCloudProject/MainService"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IMainService>(
                    new Uri("fabric:/RealEstateCloudProject/MainService"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    result = await proxy.AddNewEstate(realEstate);

                    index++;
                }

                //if (result)
                //{
                //    ViewData["Error"] = "";
                //}
                //else
                //{
                //    ViewData["Error"] = "Korisnik NIJE Dodat!";
                //}

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ViewData["Error"] = "Korisnik NIJE Dodat!";
                return View("Add");
            }

        }

    }
}
