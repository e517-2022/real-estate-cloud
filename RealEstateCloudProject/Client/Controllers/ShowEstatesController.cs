using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Client.Controllers
{
    public class ShowEstatesController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/ShowEstates/Show")]
        public async Task<IActionResult> Show()
        {
            
            List<RealEstate> estates = new List<RealEstate>();

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

                    estates = await proxy.GetEstates();

                    index++;
                }

                
                ViewBag.Estates = estates; 
                return View();



            }
            catch
            {
                ViewData["Error"] = "Failed";
                return View();
            }
        }
    }
}
