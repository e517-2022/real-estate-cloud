using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tables
{
    public class ReservationEntity : TableEntity
    {
        public ReservationEntity()
        {
        }

        public ReservationEntity(string id, string estateid, string name, string place, int price, string from, string to)
        {
            PartitionKey = "Reservation";
            ID = id;
            RowKey = ID;
            EstateId = estateid;
            EstatePlace = place;
            EstateName = name;
            EstateRentingPrice = price;
            DateFrom = from;
            DateTo= to;


        }

            public string ID { get; set; }

            
            public string EstateId { get; set; }

            
            public string EstateName { get; set; }
            
            public string EstatePlace { get; set; }
            
            public int EstateRentingPrice { get; set; }
            
            public string DateFrom { get; set; }
            
            public string DateTo { get; set; }

          

     
        
    }
}
