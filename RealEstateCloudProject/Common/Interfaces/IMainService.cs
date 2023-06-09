﻿using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IMainService : IService
    {
        Task<bool> AddNewEstate(RealEstate realEstate);
        Task<List<RealEstate>> GetEstates();

        Task<bool> AddNewReservation(Reservation reservation);
        Task<List<Reservation>> GetReservations();
        Task<List<Reservation>> GetReservationsFromTable();
    }
}
