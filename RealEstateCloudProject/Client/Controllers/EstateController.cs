﻿using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Client.Controllers
{
    public class EstateController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("/Estate/Show")]
        public async Task<IActionResult> Show()
        {
            
            List<RealEstate> estates = new List<RealEstate>();

            try
            {
                bool result = true;
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/RealEstateCloudProject/PubSub"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IPubSub>(
                    new Uri("fabric:/RealEstateCloudProject/PubSub"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    estates = await proxy.GetEstatesPS();

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
