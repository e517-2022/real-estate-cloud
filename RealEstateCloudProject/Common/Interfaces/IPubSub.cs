using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IPubSub :IService
    {
        Task<bool> NewEstatePublish(List<RealEstate> estates);
        Task<List<RealEstate>> GetEstatesPS();
    }
}
