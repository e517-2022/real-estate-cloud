using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Client.Controllers
{
    public class ReservationController : Controller
    {

        List<RealEstate> estates = new List<RealEstate>();
        

        [HttpGet]
        [Route("/Reservation/Add")]
        public async Task<IActionResult> Add()
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

                    estates = await proxy.GetEstates();

                    index++;
                }


                ViewBag.Estates = estates;
                return View();



            }
            catch
            {
                ViewData["Error"] = "Adding reservation failed!";
                return View();
            }
        }

        [HttpPost]
        [Route("/Reservation/AddReservation")]
        public async Task<IActionResult> AddReservation(Reservation reservation)
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

                    result = await proxy.AddNewReservation(reservation);

                    index++;
                }

                if (result == false)
                {
                    
                    bool r = true;
                    FabricClient fabricClient2 = new System.Fabric.FabricClient();
                    int partitionsNumber2 = (await fabricClient2.QueryManager.GetPartitionListAsync(new Uri("fabric:/RealEstateCloudProject/MainService"))).Count;
                    int index2 = 0;

                    for (int i = 0; i < partitionsNumber2; i++)
                    {
                        var proxy = ServiceProxy.Create<IMainService>(
                        new Uri("fabric:/RealEstateCloudProject/MainService"),
                        new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index2 % partitionsNumber2)
                        );

                        estates = await proxy.GetEstates();

                        index2++;
                    }

                    ViewData["Error"] = "Please check your dates! This estate may be already reserved!";
                    ViewBag.Estates = estates;
                    return View("Add");

                }

                

                return RedirectToAction("ShowAll");
            }
            catch
            {
                ViewData["Error"] = "Please check your dates! This estate may be already reserved!";
                return View("Add");
            }
        }

        [HttpGet]
        [Route("/Reservation/ShowAll")]
        public async Task<IActionResult> ShowAll()
        {

            List<Reservation> reservations = new List<Reservation>();

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

                    reservations = await proxy.GetReservations();

                    index++;
                }


                ViewBag.Reservations = reservations;
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
