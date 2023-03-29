using Common.Models;
using Common.Tables;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService
{
    public class TableHelper
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public TableHelper()
        {
            //_storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            //CloudTableClient tableClient = new CloudTableClient(new
            //Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("ReservationTable");
            _table.CreateIfNotExistsAsync();
        }


     

        public async Task AddReservation(Reservation res)
        {
            _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            //CloudTableClient tableClient = new CloudTableClient(new
            //Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("ReservationTable");
            _table.CreateIfNotExistsAsync();
            TableOperation add = TableOperation.Insert(new ReservationEntity(res.Id.ToString(), res.EstateId, res.EstateName, res.EstatePlace, res.EstateRentingPrice, res.DateFrom, res.DateTo));
            await _table.ExecuteAsync(add);
        }

        public async Task<List<Reservation>> GetReservations()
        {
            Reservation temp = new Reservation();
            List<Reservation> res = new List<Reservation>();
            var tableQuery = new TableQuery<ReservationEntity>();
            var reservations = await _table.ExecuteQuerySegmentedAsync(tableQuery, null);

            foreach(ReservationEntity r in reservations)
            {
                temp.Id = Int32.Parse(r.ID);
                temp.EstateId = r.EstateId;
                temp.EstateName = r.EstateName;
                temp.EstatePlace = r.EstatePlace;
                temp.EstateRentingPrice = r.EstateRentingPrice;
                temp.DateFrom = r.DateFrom;
                temp.DateTo = r.DateTo;

                res.Add(temp);
            }

            return res;

        }

        //public List<ReservationEntity> GetAllReservations()
        //{
        //    IQueryable<ReservationEntity> requests = from g in table.CreateQuery<ReservationEntity>()
        //                                where g.PartitionKey == "Reservation"
        //                                select g;

        //    return requests.ToList(); ;
        //}

    }
}
